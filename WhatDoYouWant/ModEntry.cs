using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.GameData.Crops;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Shops;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;
using System;
using System.Collections.Generic;

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

        private const string CommunityCenter_Money = "-1";

        private const string CookingIngredient_AnyMilk = "-6";
        private const string CookingIngredient_AnyEgg = "-5";
        private const string CookingIngredient_AnyFish = "-4";

        // TODO i18n

        public override void Entry(IModHelper helper)
        {
            Helper.Events.Input.ButtonsChanged += (_sender, _e) => ButtonsChanged(_sender, _e);
        }

        private void ButtonsChanged(object? _sender, ButtonsChangedEventArgs _e)
        {
            var key = KeybindList.Parse("F2"); // TODO make this a mod option, changeable via GMCM
            if (!key.JustPressed())
            {
                return;
            }

            // TODO option to omit things already completed (check mail / achievement data)
            List<Response> responseList = new();
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
              afterDialogueBehavior: new GameLocation.afterQuestionBehavior(this.GotResponse)
            );
        }

        public virtual void GotResponse(Farmer who, string answer)
        {
            switch (answer)
            {
                case ResponseToken_CommunityCenter:
                    ShowCommunityCenterList();
                    break;
                case ResponseToken_Shipping:
                    ShowFullShipmentList(who);
                    break;
                case ResponseToken_Cooking:
                    ShowCookingList(who);
                    break;
                case ResponseToken_Crafting:
                    ShowCraftingList(who);
                    break;
                case ResponseToken_Fishing:
                    ShowFishingList(who);
                    break;
                case ResponseToken_Museum:
                    ShowMuseumList();
                    break;
                case ResponseToken_Polyculture:
                    ShowPolycultureList();
                    break;
                case ResponseToken_Cancel:
                    break;
                default:
                    Game1.drawDialogueNoTyping("Not yet implemented");
                    break;
            }
        }

        public void ShowCommunityCenterList()
        {
            var linesToDisplay = new List<string>();

            if (Game1.player.mailReceived.Contains("ccIsComplete"))
            {
                linesToDisplay.Add("Full Shipment is complete!");
                ShowLines(linesToDisplay);
                return;
            }

            var bundleAreas = new Dictionary<int, string>();
            var bundleNames = new Dictionary<int, string>();
            var bundleOptions = new Dictionary<int, int>(); // total number of bundle options, whether or not already donated
            var bundleSlotsNeeded = new Dictionary<int, int>(); // e.g. Crab Pot Bundle has 10 options, but donating any 5 completes it
            var bundleItems = new Dictionary<int, List<string>>(); // list of options not already donated

            // adapted from base game logic for Community Center to refresh its bundle data
            var communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
            var bundleDictionary = communityCenter.bundlesDict();
            var bundleData = Game1.netWorldState.Value.BundleData;
            foreach (var keyValuePair in bundleData)
            {
                // e.g. key = "Pantry/0", value = "Spring Crops/O 465 20/24 1 0 188 1 0 190 1 0 192 1 0/0"
                // https://stardewvalleywiki.com/Modding:Bundles
                var keyArray = keyValuePair.Key.Split('/');

                var areaName = keyArray[0];
                if (!communityCenter.shouldNoteAppearInArea(CommunityCenter.getAreaNumberFromName(areaName)))
                {
                    continue;
                }

                var bundleId = Convert.ToInt32(keyArray[1]);
                if (!bundleDictionary.ContainsKey(bundleId))
                {
                    continue;
                }

                var valueArray = keyValuePair.Value.Split('/');

                bundleAreas[bundleId] = areaName;
                bundleNames[bundleId] = valueArray[0];
                bundleSlotsNeeded[bundleId] = 0;
                if (valueArray.Length > 4 && Int32.TryParse(valueArray[4], out int slotsNeeded))
                {
                    bundleSlotsNeeded[bundleId] = slotsNeeded;
                }
                bundleItems[bundleId] = new List<string>();

                var itemsNeededList = ArgUtility.SplitBySpace(valueArray[2]);
                bundleOptions[bundleId] = itemsNeededList.Length / 3;
                for (var index = 0; index < itemsNeededList.Length; index += 3)
                {
                    if (bundleDictionary[bundleId][index / 3])
                    {
                        continue;
                    }

                    string itemId;
                    if (int.TryParse(itemsNeededList[index], out int result) && result < 0)
                    {
                        itemId = result.ToString();
                    }
                    else
                    {
                        var data = ItemRegistry.GetData(itemsNeededList[index]);
                        itemId = (data != null) ? data.QualifiedItemId : "(O)" + itemsNeededList[index];
                    }
                    var itemQuantityNeeded = Convert.ToInt32(itemsNeededList[index + 1]);
                    var minimumQuality = Convert.ToInt32(itemsNeededList[index + 2]);

                    string itemDescription;
                    switch (itemId)
                    {
                        case CommunityCenter_Money:
                            itemDescription = $"{itemQuantityNeeded}g";
                            break;
                        case CookingIngredient_AnyMilk:
                            itemDescription = "Milk (any)";
                            break;
                        case CookingIngredient_AnyEgg:
                            itemDescription = "Egg (any)";
                            break;
                        case CookingIngredient_AnyFish:
                            itemDescription = "Fish (any)";
                            break;
                        default:
                            var dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(itemId);
                            itemDescription = dataOrErrorItem.DisplayName;
                            break;
                    }
                    if (itemId != CommunityCenter_Money)
                    {
                        if (itemQuantityNeeded > 1)
                        {
                            itemDescription += $" x{itemQuantityNeeded}";
                        }
                        switch (minimumQuality)
                        {
                            case 1:
                                itemDescription += " (silver)";
                                break;
                            case 2:
                                itemDescription += " (gold)";
                                break;
                            case 3:
                            case 4:
                                itemDescription += " (iridium)";
                                break;
                        }
                    }
                    bundleItems[bundleId].Add(itemDescription);
                }
            }

            foreach (var keyValuePair in bundleItems.OrderBy(keyValuePair => keyValuePair.Key))
            {
                var bundleId = keyValuePair.Key;
                if (bundleItems[bundleId].Count == 0)
                {
                    continue;
                }
                var areaName = bundleAreas[bundleId];
                var bundleName = bundleNames[bundleId];
                var bundleSlotPrefix = "";
                if (bundleSlotsNeeded[bundleId] != 0 && bundleSlotsNeeded[bundleId] != bundleOptions[bundleId])
                {
                    var numberOptionsAlreadyDonated = bundleOptions[bundleId] - bundleItems[bundleId].Count;
                    var bundleSlotsShort = bundleSlotsNeeded[bundleId] - numberOptionsAlreadyDonated;
                    bundleSlotPrefix = $"{bundleSlotsShort} of ";
                }
                var bundleItemList = String.Join(", ", bundleItems[bundleId]);
                linesToDisplay.Add($"* {areaName} - {bundleName} Bundle - {bundleSlotPrefix}{bundleItemList}{LineBreak}");
            }

            if (linesToDisplay.Count == 0)
            {
                linesToDisplay.Add("Full Shipment is complete!");
            }

            ShowLines(linesToDisplay);
        }

        public static void ShowFullShipmentList(Farmer who)
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

            ShowLines(linesToDisplay);
        }

        public static void ShowCookingList(Farmer who)
        {
            var linesToDisplay = new List<string>();

            // adapted from base game logic to calculate cooking %
            //   TODO sort options: mod items first, last
            var cookingDictionary = DataLoader.CookingRecipes(Game1.content);
            foreach (var keyValuePair in cookingDictionary)
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

            ShowLines(linesToDisplay);
        }

        public static void ShowCraftingList(Farmer who)
        {
            var linesToDisplay = new List<string>();

            // adapted from base game logic to calculate crafting %
            //   TODO sort options: mod items first, last
            var craftingDictionary = DataLoader.CraftingRecipes(Game1.content);
            foreach (var keyValuePair in craftingDictionary)
            {
                // keyValuePair = e.g. <"Wood Fence", "388 2/Field/322/false/l 0">
                // value = list of ingredient IDs and quantities / unused / item ID of crafted item / big craftable? / unlock conditions
                var key1 = keyValuePair.Key;
                var key2 = ArgUtility.SplitBySpaceAndGet(ArgUtility.Get(keyValuePair.Value.Split('/'), 2), 0);
                if (key1 == "Wedding Ring")
                {
                    continue;
                }
                who.craftingRecipes.TryGetValue(key1, out int numberCrafted);
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

            ShowLines(linesToDisplay);
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

        public static void ShowFishingList(Farmer who)
        {
            var linesToDisplay = new List<string>();

            // adapted from base game logic to calculate fishing %
            //   TODO sort options: alpha, season (starting with current); mod items first, last
            foreach (var parsedItemData in ItemRegistry.GetObjectTypeDefinition().GetAllData())
            {
                if (parsedItemData.ObjectType != "Fish")
                {
                    continue;
                }
                if (parsedItemData.RawData is ObjectData rawData && rawData.ExcludeFromFishingCollection)
                {
                    continue;
                }
                if (who.fishCaught.ContainsKey(parsedItemData.QualifiedItemId))
                {
                    continue;
                }
                var dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(parsedItemData.QualifiedItemId);
                linesToDisplay.Add($"* {dataOrErrorItem.DisplayName}{LineBreak}");
            }

            if (linesToDisplay.Count == 0)
            {
                linesToDisplay.Add("Master Angler is complete!");
            }

            ShowLines(linesToDisplay);
        }

        public static void ShowMuseumList()
        {
            var linesToDisplay = new List<string>();

            // adapted from base game logic to award A Complete Collection achievement
            //   TODO sort options: alpha, type(mineral / artifact); mod items first, last
            foreach (var parsedItemData in ItemRegistry.GetObjectTypeDefinition().GetAllData())
            {
                var qualifiedItemId = parsedItemData.QualifiedItemId;
                if (!LibraryMuseum.IsItemSuitableForDonation(qualifiedItemId, checkDonatedItems: true))
                {
                    continue;
                }
                var baseContextTags = ItemContextTagManager.GetBaseContextTags(qualifiedItemId);
                if (
                    !baseContextTags.Contains("museum_donatable")
                        && !baseContextTags.Contains("item_type_minerals")
                        && !baseContextTags.Contains("item_type_arch")
                )
                {
                    continue;
                }
                var dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(qualifiedItemId);
                linesToDisplay.Add($"* {dataOrErrorItem.DisplayName}{LineBreak}");
            }

            if (linesToDisplay.Count == 0)
            {
                linesToDisplay.Add("A Complete Collection is complete!");
            }

            ShowLines(linesToDisplay);
        }

        public static void ShowPolycultureList()
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
                Game1.player.basicShipped.TryGetValue(cropData.HarvestItemId, out int numberShipped);
                if (numberShipped >= NumberShippedForPolyculture)
                {
                    continue;
                }
                var numberNeeded = NumberShippedForPolyculture - numberShipped;
                var dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(cropData.HarvestItemId);
                linesToDisplay.Add($"* {dataOrErrorItem.DisplayName} - ship {numberNeeded}{LineBreak}");
            }

            if (linesToDisplay.Count == 0)
            {
                linesToDisplay.Add("Polyculture is complete!");
            }

            ShowLines(linesToDisplay);
        }

        private static void ShowLines(List<string> linesToDisplay)
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
