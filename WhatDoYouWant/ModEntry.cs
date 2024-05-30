using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.GameData.Crops;
using StardewValley.GameData.Objects;
using StardewValley.Locations;

namespace WhatDoYouWant
{
    public class ModEntry : Mod
    {
        public const string LineBreak = "^";
        private const int NumberShippedForPolyculture = 15;

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

        public const string Title_CommunityCenter = "Community Center";
        public const string Title_Walnuts = "Golden Walnuts";
        public const string Title_Shipping = "Full Shipment";
        public const string Title_Cooking = "Gourmet Chef";
        public const string Title_Crafting = "Craft Master";
        public const string Title_Fishing = "Master Angler";
        public const string Title_Museum = "A Complete Collection";
        public const string Title_Stardrops = "Stardrops";
        public const string Title_Polyculture = "Polyculture";

        private const string CommunityCenter_Money = "-1";

        private const string CookingIngredient_AnyMilk = "-6";
        private const string CookingIngredient_AnyEgg = "-5";
        private const string CookingIngredient_AnyFish = "-4";

        private const string WalnutType_MissingTheseNuts = "MissingTheseNuts";
        private const string WalnutType_MissingLimitedNutDrops = "MissingLimitedNutDrops";
        private const string WalnutType_GoldenCoconutCracked = "GoldenCoconutCracked";
        private const string WalnutType_GotBirdieReward = "GotBirdieReward";

        private static readonly List<List<string>> WalnutList = new()
        {
            // base game function name, token passed to it, hint text (Strings\\Locations:NutHint_*) or "none", [number of walnuts - 1 if not specified]
            new List<string>() { WalnutType_MissingTheseNuts, "Bush_IslandNorth_13_33", "VolcanoLava" },
            new List<string>() { WalnutType_MissingTheseNuts, "Bush_IslandNorth_5_30", "VolcanoLava" },
            new List<string>() { WalnutType_MissingTheseNuts, "Bush_IslandNorth_19_39", "BuriedArch" },
            new List<string>() { WalnutType_MissingTheseNuts, "Bush_IslandNorth_4_42", "NorthHidden" },
            new List<string>() { WalnutType_MissingTheseNuts, "Bush_IslandNorth_45_38", "NorthHidden" },
            new List<string>() { WalnutType_MissingTheseNuts, "Bush_IslandNorth_47_40", "NorthHidden" },
            new List<string>() { WalnutType_MissingTheseNuts, "IslandLeftPlantRestored", "Arch" },
            new List<string>() { WalnutType_MissingTheseNuts, "IslandRightPlantRestored", "Arch" },
            new List<string>() { WalnutType_MissingTheseNuts, "IslandBatRestored", "Arch" },
            new List<string>() { WalnutType_MissingTheseNuts, "IslandFrogRestored", "Arch" },
            new List<string>() { WalnutType_MissingTheseNuts, "IslandCenterSkeletonRestored", "Arch", "6" },
            new List<string>() { WalnutType_MissingTheseNuts, "IslandSnakeRestored", "Arch", "3" },
            new List<string>() { WalnutType_MissingTheseNuts, "Buried_IslandNorth_19_13", "NorthBuried" },
            new List<string>() { WalnutType_MissingTheseNuts, "Buried_IslandNorth_57_79", "NorthBuried" },
            new List<string>() { WalnutType_MissingTheseNuts, "Buried_IslandNorth_54_21", "NorthBuried" },
            new List<string>() { WalnutType_MissingTheseNuts, "Buried_IslandNorth_42_77", "NorthBuried" },
            new List<string>() { WalnutType_MissingTheseNuts, "Buried_IslandNorth_62_54", "NorthBuried" },
            new List<string>() { WalnutType_MissingTheseNuts, "Buried_IslandNorth_26_81", "NorthBuried" },
            new List<string>() { WalnutType_MissingTheseNuts, "Bush_IslandNorth_20_26", "NorthHidden" },
            new List<string>() { WalnutType_MissingTheseNuts, "Bush_IslandNorth_9_84", "NorthHidden" },
            new List<string>() { WalnutType_MissingTheseNuts, "Bush_IslandNorth_56_27", "NorthHidden" },
            new List<string>() { WalnutType_MissingTheseNuts, "Bush_IslandSouth_31_5", "NorthHidden" },
            new List<string>() { WalnutType_MissingTheseNuts, "TreeNut", "HutTree" },
            new List<string>() { WalnutType_MissingTheseNuts, "IslandWestCavePuzzle", "WestHidden", "3" },
            new List<string>() { WalnutType_MissingTheseNuts, "SandDuggy", "WestHidden" },
            new List<string>() { WalnutType_MissingLimitedNutDrops, "TigerSlimeNut", "TigerSlime" },
            new List<string>() { WalnutType_MissingTheseNuts, "Buried_IslandWest_21_81", "WestBuried" },
            new List<string>() { WalnutType_MissingTheseNuts, "Buried_IslandWest_62_76", "WestBuried" },
            new List<string>() { WalnutType_MissingTheseNuts, "Buried_IslandWest_39_24", "WestBuried" },
            new List<string>() { WalnutType_MissingTheseNuts, "Buried_IslandWest_88_14", "WestBuried" },
            new List<string>() { WalnutType_MissingTheseNuts, "Buried_IslandWest_43_74", "WestBuried" },
            new List<string>() { WalnutType_MissingTheseNuts, "Buried_IslandWest_30_75", "WestBuried" },
            new List<string>() { WalnutType_MissingLimitedNutDrops, "MusselStone", "MusselStone", "5" },
            new List<string>() { WalnutType_MissingLimitedNutDrops, "IslandFarming", "IslandFarming", "5" },
            new List<string>() { WalnutType_MissingTheseNuts, "Bush_IslandWest_104_3", "WestHidden" },
            new List<string>() { WalnutType_MissingTheseNuts, "Bush_IslandWest_31_24", "WestHidden" },
            new List<string>() { WalnutType_MissingTheseNuts, "Bush_IslandWest_38_56", "WestHidden" },
            new List<string>() { WalnutType_MissingTheseNuts, "Bush_IslandWest_75_29", "WestHidden" },
            new List<string>() { WalnutType_MissingTheseNuts, "Bush_IslandWest_64_30", "WestHidden" },
            new List<string>() { WalnutType_MissingTheseNuts, "Bush_IslandWest_54_18", "WestHidden" },
            new List<string>() { WalnutType_MissingTheseNuts, "Bush_IslandWest_25_30", "WestHidden" },
            new List<string>() { WalnutType_MissingTheseNuts, "Bush_IslandWest_15_3", "WestHidden" },
            new List<string>() { WalnutType_MissingLimitedNutDrops, "IslandFishing", "IslandFishing", "5" },
            new List<string>() { WalnutType_MissingLimitedNutDrops, "VolcanoNormalChest", "VolcanoTreasure" },
            new List<string>() { WalnutType_MissingLimitedNutDrops, "VolcanoRareChest", "VolcanoTreasure" },
            new List<string>() { WalnutType_MissingLimitedNutDrops, "VolcanoBarrel", "VolcanoBarrel", "5" },
            new List<string>() { WalnutType_MissingLimitedNutDrops, "VolcanoMining", "VolcanoMining", "5" },
            new List<string>() { WalnutType_MissingLimitedNutDrops, "VolcanoMonsterDrop", "VolcanoMonsters", "5" },
            new List<string>() { WalnutType_MissingLimitedNutDrops, "Island_N_BuriedTreasureNut", "Journal" },
            new List<string>() { WalnutType_MissingLimitedNutDrops, "Island_W_BuriedTreasureNut", "Journal" },
            new List<string>() { WalnutType_MissingLimitedNutDrops, "Island_W_BuriedTreasureNut2", "Journal" },
            new List<string>() { WalnutType_MissingTheseNuts, "Mermaid", "Journal", "5" },
            new List<string>() { WalnutType_MissingTheseNuts, "TreeNutShot", "Journal" },
            new List<string>() { WalnutType_MissingTheseNuts, "Buried_IslandSouthEastCave_36_26", "SouthEastBuried" },
            new List<string>() { WalnutType_MissingTheseNuts, "Buried_IslandSouthEast_25_17", "SouthEastBuried" },
            new List<string>() { WalnutType_MissingTheseNuts, "StardropPool", "StardropPool" },
            new List<string>() { WalnutType_MissingTheseNuts, "Bush_Caldera_28_36", "Caldera" },
            new List<string>() { WalnutType_MissingTheseNuts, "Bush_Caldera_9_34", "Caldera" },
            new List<string>() { WalnutType_MissingTheseNuts, "Bush_CaptainRoom_2_4", "WestHidden" }, // shipwreck
            new List<string>() { WalnutType_MissingTheseNuts, "BananaShrine", "none", "3" },
            new List<string>() { WalnutType_MissingTheseNuts, "Bush_IslandEast_17_37", "none" }, // out in the open near Leo's hut
            new List<string>() { WalnutType_MissingLimitedNutDrops, "Darts", "none", "3" },
            new List<string>() { WalnutType_MissingTheseNuts, "IslandGourmand1", "Gourmand", "5" },
            new List<string>() { WalnutType_MissingTheseNuts, "IslandGourmand2", "Gourmand", "5" },
            new List<string>() { WalnutType_MissingTheseNuts, "IslandGourmand3", "Gourmand", "5" },
            new List<string>() { WalnutType_MissingTheseNuts, "IslandShrinePuzzle", "IslandShrine", "5" },
            new List<string>() { WalnutType_MissingTheseNuts, "Bush_IslandShrine_23_34", "none" },
            new List<string>() { WalnutType_GoldenCoconutCracked, "", "GoldenCoconut" },
            new List<string>() { WalnutType_GotBirdieReward, "", "none", "5" } // Pirate's Wife
        };

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
                    ShowWalnutsList();
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
                case ResponseToken_Stardrops:
                    Stardrops.ShowStardropList(modInstance: this, who: who);
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

        public void ShowWalnutsList()
        {
            var linesToDisplay = new List<string>();

            // adapted from base game logic for hints in Leo's hut
            //   TODO sort by area
            //   TODO option to provide more detail (see Stardew Checker source)
            var hintDictionary = new Dictionary<string, int>();
            foreach (var walnut in WalnutList)
            {
                var walnutFunction = walnut[0];
                var walnutToken = walnut[1];
                var walnutHint = walnut[2];
                var walnutNumberTotal = (walnut.Count >= 4) ? Convert.ToInt32(walnut[3]) : 1;

                var walnutNumberMissing = 0;
                switch (walnutFunction)
                {
                    case WalnutType_MissingTheseNuts:
                        if (!Game1.player.team.collectedNutTracker.Contains(walnutToken))
                        {
                            walnutNumberMissing = walnutNumberTotal;
                        }
                        break;
                    case WalnutType_MissingLimitedNutDrops:
                        walnutNumberMissing = walnutNumberTotal - Math.Max(Game1.player.team.GetDroppedLimitedNutCount(walnutToken), 0);
                        break;
                    case WalnutType_GoldenCoconutCracked:
                        if (!Game1.netWorldState.Value.GoldenCoconutCracked)
                        {
                            walnutNumberMissing = walnutNumberTotal;
                        }
                        break;
                    case WalnutType_GotBirdieReward:
                        if (!Game1.MasterPlayer.hasOrWillReceiveMail("gotBirdieReward"))
                        {
                            walnutNumberMissing = walnutNumberTotal;
                        }
                        break;
                }

                if (walnutNumberMissing > 0)
                {
                    if (!hintDictionary.ContainsKey(walnutHint)) {
                        hintDictionary[walnutHint] = 0;
                    }
                    hintDictionary[walnutHint] += walnutNumberMissing;
                }
            }

            foreach (var hint in hintDictionary)
            {
                string hintText;
                if (hint.Key == "none")
                {
                    hintText = "Other";
                } else
                {
                    hintText = Game1.content.LoadString($"Strings\\Locations:NutHint_{hint.Key}");
                    if (hintText.StartsWith("{0} "))
                    {
                        hintText = hintText.Substring(4, 1).ToUpper() + hintText.Substring(5);
                    }
                }
                if (hint.Value > 1)
                {
                    hintText += $" ({hint.Value})";
                }
                linesToDisplay.Add($"* {hintText}{LineBreak}");
            }

            if (linesToDisplay.Count == 0)
            {
                Game1.drawDialogueNoTyping($"{Title_Walnuts} are complete!");
                return;
            }

            ShowLines(linesToDisplay, title: Title_Walnuts);
        }

        public void ShowFullShipmentList(Farmer who)
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
                Game1.drawDialogueNoTyping($"{Title_Shipping} is complete!");
                return;
            }

            ShowLines(linesToDisplay, title: Title_Shipping);
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

        public void ShowFishingList(Farmer who)
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
                Game1.drawDialogueNoTyping($"{Title_Fishing} is complete!");
                return;
            }

            ShowLines(linesToDisplay, title: Title_Fishing);
        }

        public void ShowMuseumList()
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
                Game1.drawDialogueNoTyping($"{Title_Museum} is complete!");
                return;
            }

            ShowLines(linesToDisplay, title: Title_Museum);
        }

        public void ShowPolycultureList()
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
                Game1.drawDialogueNoTyping($"{Title_Polyculture} is complete!");
                return;
            }

            ShowLines(linesToDisplay, title: Title_Polyculture);
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
