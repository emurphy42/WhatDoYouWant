using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.GameData.Crops;
using StardewValley.GameData.Shops;
using StardewValley.TokenizableStrings;
using System;

namespace WhatDoYouWant
{
    public class ModEntry : Mod
    {
        private const string LineBreak = "^";
        private const int NumberShippedForPolyculture = 15;

        private const string ResponseToken_CommunityCenter = "CommunityCenter";
        private const string ResponseToken_Shipping = "Shipping";
        private const string ResponseToken_Cooking = "Cooking";
        private const string ResponseToken_Crafting = "Crafting";
        private const string ResponseToken_Fishing = "Fishing";
        private const string ResponseToken_Museum = "Museum";
        private const string ResponseToken_Polyculture = "Polyculture";
        private const string ResponseToken_Cancel = "Cancel";

        private const string CookingIngredient_AnyMilk = "-6";
        private const string CookingIngredient_AnyEgg = "-5";
        private const string CookingIngredient_AnyFish = "-4";

        // TODO i18n

        public override void Entry(IModHelper helper)
        {
            Helper.Events.Input.ButtonsChanged += (_sender, _e) => buttonsChanged(_sender, _e);
        }

        private void buttonsChanged(object? _sender, ButtonsChangedEventArgs _e)
        {
            var key = KeybindList.Parse("F2"); // TODO make this a mod option, changeable via GMCM
            if (!key.JustPressed())
            {
                return;
            }

            var responseList = new List<Response>();
            responseList.Add(new Response(responseKey: ResponseToken_CommunityCenter, responseText: "Community Center"));
            responseList.Add(new Response(responseKey: ResponseToken_Shipping, responseText: "Full Shipment"));
            responseList.Add(new Response(responseKey: ResponseToken_Cooking, responseText: "Gourmet Chef"));
            responseList.Add(new Response(responseKey: ResponseToken_Crafting, responseText: "Craft Master"));
            responseList.Add(new Response(responseKey: ResponseToken_Fishing, responseText: "Master Angler"));
            responseList.Add(new Response(responseKey: ResponseToken_Museum, responseText: "A Complete Collection"));
            responseList.Add(new Response(responseKey: ResponseToken_Polyculture, responseText: "Polyculture"));
            responseList.Add(new Response(responseKey: ResponseToken_Cancel, responseText: "(Cancel)"));
            Game1.currentLocation.createQuestionDialogue(
              question: "Show items still needed for...",
              answerChoices: responseList.ToArray(),
              afterDialogueBehavior: new GameLocation.afterQuestionBehavior(this.gotResponse)
            );
        }

        public virtual void gotResponse(Farmer who, string answer)
        {
            switch (answer)
            {
                // TODO CommunityCenter
                case ResponseToken_Shipping:
                    showFullShipmentList(who: who, answer: answer);
                    break;
                case ResponseToken_Cooking:
                    showCookingList(who: who, answer: answer);
                    break;
                case ResponseToken_Crafting:
                    showCraftingList(who: who, answer: answer);
                    break;
                // TODO Utility.getFishCaughtPercent
                // TODO Museum
                case ResponseToken_Polyculture:
                    ShowPolycultureList(who: who, answer: answer);
                    break;
                case ResponseToken_Cancel:
                    break;
                default:
                    Game1.drawDialogueNoTyping("Not yet implemented");
                    break;
            }
        }

        public void showFullShipmentList(Farmer who, string answer)
        {
            var linesToDisplay = new List<string>();

            // adapted from base game logic to calculate full shipment %
            //   TODO sort options: alpha, collection tab order, category / type; mod items first, last
            foreach (var parsedItemData in ItemRegistry.GetObjectTypeDefinition().GetAllData())
            {
                switch (parsedItemData.Category)
                {
                    case StardewValley.Object.CookingCategory:
                    case StardewValley.Object.GemCategory:
                        continue;
                    default:
                        if (!StardewValley.Object.isPotentialBasicShipped(
                            itemId: parsedItemData.ItemId,
                            category: parsedItemData.Category,
                            objectType: parsedItemData.ObjectType
                        ))
                        {
                            continue;
                        }
                        if (who.basicShipped.ContainsKey(parsedItemData.ItemId))
                        {
                            continue;
                        }
                        linesToDisplay.Add($"* {parsedItemData.DisplayName}{LineBreak}");
                        break;
                }
            }

            if (linesToDisplay.Count == 0)
            {
                linesToDisplay.Add("Full Shipment is complete!");
            }

            showLines(linesToDisplay);
        }

        public void showCookingList(Farmer who, string answer)
        {
            var linesToDisplay = new List<string>();

            // adapted from base game logic to calculate cooking %
            //   TODO sort options: mod items first, last
            var dictionary = DataLoader.CookingRecipes(Game1.content);
            foreach (var keyValuePair in dictionary)
            {
                // keyValuePair = e.g. <"Fried Egg", "-5 1/10 10/194/default">
                // value = list of ingredient IDs and quantities / unused / item ID of cooked dish / unlock conditions
                var key1 = keyValuePair.Key;
                var key2 = ArgUtility.SplitBySpaceAndGet(ArgUtility.Get(keyValuePair.Value.Split('/'), 2), 0);
                var recipeLearned = who.cookingRecipes.ContainsKey(key1);
                var recipeCooked = who.recipesCooked.ContainsKey(key2);
                if (recipeLearned && recipeCooked)
                {
                    continue;
                }

                // TODO parse unlock conditions
                var learnedPrefix = recipeLearned ? "" : "not yet learned - ";

                var ingredients = ArgUtility.Get(keyValuePair.Value.Split('/'), 0);
                var ingredientText = GetIngredientText(ingredients);

                var dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(key2);
                linesToDisplay.Add($"* {dataOrErrorItem.DisplayName} - {learnedPrefix}{ingredientText}{LineBreak}");
            }

            if (linesToDisplay.Count == 0)
            {
                linesToDisplay.Add("Gourmet Chef is complete!");
            }

            showLines(linesToDisplay);
        }

        public void showCraftingList(Farmer who, string answer)
        {
            var linesToDisplay = new List<string>();

            // adapted from base game logic to calculate crafting %
            //   TODO sort options: mod items first, last
            var dictionary = DataLoader.CraftingRecipes(Game1.content);
            foreach (var keyValuePair in dictionary)
            {
                // keyValuePair = e.g. <"Wood Fence", "388 2/Field/322/false/l 0">
                // value = list of ingredient IDs and quantities / unused / item ID of crafted item / big craftable? / unlock conditions
                var key1 = keyValuePair.Key;
                var key2 = ArgUtility.SplitBySpaceAndGet(ArgUtility.Get(keyValuePair.Value.Split('/'), 2), 0);
                if (key1 == "Wedding Ring")
                {
                    continue;
                }
                int numberCrafted;
                who.craftingRecipes.TryGetValue(key1, out numberCrafted);
                if (numberCrafted > 0)
                {
                    continue;
                }

                // TODO parse unlock conditions

                var ingredients = ArgUtility.Get(keyValuePair.Value.Split('/'), 0);
                var ingredientText = GetIngredientText(ingredients);

                var isBigCraftable = ArgUtility.SplitBySpaceAndGet(ArgUtility.Get(keyValuePair.Value.Split('/'), 3), 0);
                var itemPrefix = (isBigCraftable == "true") ? "(BC)" : "(O)";
                var dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(itemPrefix + key2);
                linesToDisplay.Add($"* {dataOrErrorItem.DisplayName} - {ingredientText}{LineBreak}");
            }

            if (linesToDisplay.Count == 0)
            {
                linesToDisplay.Add("Craft Master is complete!");
            }

            showLines(linesToDisplay);
        }

        private static string GetIngredientText(string ingredients)
        {
            var ingredientList = ingredients.Trim().Split(' ');
            var ingredientTextList = new List<string>();
            for (var index = 0; index < ingredientList.Length; index += 2)
            {
                var ingredientId = ingredientList[index];
                string ingredientName;
                switch (ingredientId)
                {
                    case CookingIngredient_AnyMilk:
                        ingredientName = "Milk (any)";
                        break;
                    case CookingIngredient_AnyEgg:
                        ingredientName = "Egg (any)";
                        break;
                    case CookingIngredient_AnyFish:
                        ingredientName = "Fish (any)";
                        break;
                    default:
                        var ingredientDataOrErrorItem = ItemRegistry.GetDataOrErrorItem(ingredientId);
                        ingredientName = ingredientDataOrErrorItem.DisplayName;
                        break;
                }

                var ingredientQuantity = ingredientList[index + 1];
                if (ingredientQuantity != "1")
                {
                    ingredientName += $" x{ingredientQuantity}";
                }

                ingredientTextList.Add(ingredientName);
            }

            return String.Join(", ", ingredientTextList);
        }

        /* TODO museum
           TODO sort options: alpha, type (mineral / artifact); mod items first, last
            foreach (ParsedItemData parsedItemData in ItemRegistry.GetObjectTypeDefinition().GetAllData())
            {
              string qualifiedItemId = parsedItemData.QualifiedItemId;
              if (LibraryMuseum.IsItemSuitableForDonation(qualifiedItemId, checkDonatedItems: true))
              {
                HashSet<string> baseContextTags = ItemContextTagManager.GetBaseContextTags(itemId);
                baseContextTags.Contains("museum_donatable")
                baseContextTags.Contains("item_type_minerals")
                baseContextTags.Contains("item_type_arch")
                  what if an item has both minerals and arch? neither minerals nor arch?
              }
            }
         */

        public void ShowPolycultureList(Farmer who, string answer)
        {
            var linesToDisplay = new List<string>();

            // adapted from base game logic to award Polyculure achievement
            //   TODO sort options: alpha, season(s), number shipped; mod items first, last
            foreach (CropData cropData in (IEnumerable<CropData>)Game1.cropData.Values)
            {
                if (!cropData.CountForPolyculture)
                {
                    continue;
                }
                int numberShipped;
                Game1.player.basicShipped.TryGetValue(cropData.HarvestItemId, out numberShipped);
                if (numberShipped >= NumberShippedForPolyculture)
                {
                    continue;
                }
                var dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(cropData.HarvestItemId);
                linesToDisplay.Add($"* {dataOrErrorItem.DisplayName} - ship {NumberShippedForPolyculture - numberShipped}{LineBreak}");
            }

            if (linesToDisplay.Count == 0)
            {
                linesToDisplay.Add("Polyculture is complete!");
            }

            showLines(linesToDisplay);
        }

        private void showLines(List<string> linesToDisplay)
        {
            // adapted from base game logic to display Perfection Tracker output
            //   TODO reduce height a bit more, replace "..." with "Page N of M" and change "Count - 1" to "Count"
            var sectionsOfHeight = SpriteText.getStringBrokenIntoSectionsOfHeight(
                s: string.Concat(linesToDisplay),
                width: 9999,
                height: Game1.uiViewport.Height - 100
            );
            for (var index = 0; index < sectionsOfHeight.Count - 1; ++index)
            {
                sectionsOfHeight[index] += "...\n";
            }

            Game1.drawDialogueNoTyping(sectionsOfHeight);
        }

    }
}
