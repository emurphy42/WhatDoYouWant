using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.Locations;

namespace WhatDoYouWant
{
    public class ModEntry : Mod
    {
        public const string LineBreak = "^";

        private const string ResponseToken_CommunityCenter = "CommunityCenter";
        private const string ResponseToken_Walnuts = "Walnuts";
        private const string ResponseToken_Shipping = "Shipping";
        private const string ResponseToken_Cooking = "Cooking";
        private const string ResponseToken_Crafting = "Crafting";
        private const string ResponseToken_Fishing = "Fishing";
        private const string ResponseToken_Museum = "Museum";
        private const string ResponseToken_Stardrops = "Stardrops";
        private const string ResponseToken_Polyculture = "Polyculture";
        private const string ResponseToken_Cancel = "Cancel";

        public const string Title_CommunityCenter = "Community Center"; // TODO replace these with functions pulling them from base game content
        public const string Title_Walnuts = "Golden Walnuts";
        public const string Title_Shipping = "Full Shipment";
        public const string Title_Cooking = "Gourmet Chef";
        public const string Title_Crafting = "Craft Master";
        public const string Title_Fishing = "Master Angler";
        public const string Title_Museum = "A Complete Collection";
        public const string Title_Stardrops = "Stardrops";
        public const string Title_Polyculture = "Polyculture";

        public static readonly List<Season> seasons = new()
        {
            Season.Spring,
            Season.Summer,
            Season.Fall,
            Season.Winter
        };

        private const string CommunityCenter_Money = "-1";

        private const string CookingIngredient_AnyMilk = "-6";
        private const string CookingIngredient_AnyEgg = "-5";
        private const string CookingIngredient_AnyFish = "-4";

        // TODO https://stardewcommunitywiki.com/Modding:Modder_Guide/APIs/Translation

        public override void Entry(IModHelper helper)
        {
            Helper.Events.Input.ButtonsChanged += (_sender, _e) => ButtonsChanged(_sender, _e);
        }

        private void ButtonsChanged(object? _sender, ButtonsChangedEventArgs _e)
        {
            if (!Game1.hasStartedDay)
            {
                return;
            }

            var key = KeybindList.Parse("F2"); // TODO make this a mod option, changeable via GMCM
            if (!key.JustPressed())
            {
                return;
            }

            // TODO default to omitting things already completed or unavailable (check mail / achievement data when possible), option to include them anyway
            //   * CC if already completed or bought Joja membership
            //   * walnuts if island not yet unlocked
            List<Response> responseList = new();
            responseList.Add(new Response(responseKey: ResponseToken_CommunityCenter, responseText: Title_CommunityCenter));
            responseList.Add(new Response(responseKey: ResponseToken_Walnuts, responseText: Title_Walnuts));
            responseList.Add(new Response(responseKey: ResponseToken_Shipping, responseText: Title_Shipping));
            responseList.Add(new Response(responseKey: ResponseToken_Cooking, responseText: Title_Cooking));
            responseList.Add(new Response(responseKey: ResponseToken_Crafting, responseText: Title_Crafting));
            responseList.Add(new Response(responseKey: ResponseToken_Fishing, responseText: Title_Fishing));
            responseList.Add(new Response(responseKey: ResponseToken_Museum, responseText: Title_Museum));
            responseList.Add(new Response(responseKey: ResponseToken_Stardrops, responseText: Title_Stardrops));
            responseList.Add(new Response(responseKey: ResponseToken_Polyculture, responseText: Title_Polyculture));
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
                case ResponseToken_Walnuts:
                    Walnuts.ShowWalnutsList(modInstance: this);
                    break;
                case ResponseToken_Shipping:
                    Shipping.ShowShippingList(modInstance: this, who: who);
                    break;
                case ResponseToken_Cooking:
                    ShowCookingList(who);
                    break;
                case ResponseToken_Crafting:
                    ShowCraftingList(who);
                    break;
                case ResponseToken_Fishing:
                    Fishing.ShowFishingList(modInstance: this, who: who);
                    break;
                case ResponseToken_Museum:
                    Museum.ShowMuseumList(modInstance: this);
                    break;
                case ResponseToken_Stardrops:
                    Stardrops.ShowStardropList(modInstance: this, who: who);
                    break;
                case ResponseToken_Polyculture:
                    Polyculture.ShowPolycultureList(modInstance: this);
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
                Game1.drawDialogueNoTyping($"{Title_CommunityCenter} is complete!");
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
                if (areaName == "Abandoned Joja Mart")
                {
                    continue;
                }
                // TODO option to only include unlocked bundles
                //if (!communityCenter.shouldNoteAppearInArea(CommunityCenter.getAreaNumberFromName(areaName)))
                //{
                //    continue;
                //}

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
                Game1.drawDialogueNoTyping($"{Title_CommunityCenter} is complete!");
                return;
            }

            ShowLines(linesToDisplay, title: Title_CommunityCenter, longLinesExpected: true);
        }

        public void ShowCookingList(Farmer who)
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
                Game1.drawDialogueNoTyping($"{Title_Cooking} is complete!");
                return;
            }

            ShowLines(linesToDisplay, title: Title_Cooking, longLinesExpected: true);
        }

        public void ShowCraftingList(Farmer who)
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
                Game1.drawDialogueNoTyping($"{Title_Crafting} is complete!");
                return;
            }

            ShowLines(linesToDisplay, title: Title_Crafting, longLinesExpected: true);
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

        public void ShowLines(List<string> linesToDisplay, string? title = null, bool longLinesExpected = false)
        {
            if (title != null)
            {
                Monitor.Log(title, LogLevel.Info);
                foreach (var line in linesToDisplay)
                {
                    Monitor.Log(line.Replace(LineBreak, ""), LogLevel.Info);
                }
            }

            // adapted from base game logic to display Perfection Tracker output
            var sectionsOfHeight = SpriteText.getStringBrokenIntoSectionsOfHeight(
                s: string.Concat(linesToDisplay),
                width: 9999,
                height: longLinesExpected
                    ? Game1.uiViewport.Height / 2
                    : Game1.uiViewport.Height - 100
            );
            if (sectionsOfHeight.Count > 1)
            {
                for (var index = 0; index < sectionsOfHeight.Count; ++index)
                {
                    sectionsOfHeight[index] += $"(part {index + 1} of {sectionsOfHeight.Count})\n";
                }
            }

            Game1.drawDialogueNoTyping(sectionsOfHeight);
        }

    }
}
