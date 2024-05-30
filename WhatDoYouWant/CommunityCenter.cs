using StardewValley;

namespace WhatDoYouWant
{
    internal class CommunityCenter
    {
        private const string CommunityCenter_Money = "-1";

        public static void ShowCommunityCenterList(ModEntry modInstance)
        {
            var linesToDisplay = new List<string>();

            if (Game1.player.mailReceived.Contains("ccIsComplete"))
            {
                var completeDescription = modInstance.Helper.Translation.Get("CommunityCenter_Complete", new { title = ModEntry.Title_CommunityCenter });
                Game1.drawDialogueNoTyping(completeDescription);
                return;
            }

            var bundleAreas = new Dictionary<int, string>();
            var bundleNames = new Dictionary<int, string>();
            var bundleOptions = new Dictionary<int, int>(); // total number of bundle options, whether or not already donated
            var bundleSlotsNeeded = new Dictionary<int, int>(); // e.g. Crab Pot Bundle has 10 options, but donating any 5 completes it
            var bundleItems = new Dictionary<int, List<string>>(); // list of options not already donated

            // adapted from base game logic for Community Center to refresh its bundle data
            var communityCenter = Game1.RequireLocation<StardewValley.Locations.CommunityCenter>("CommunityCenter");
            var bundleDictionary = communityCenter.bundlesDict();
            var bundleData = Game1.netWorldState.Value.BundleData;
            foreach (var keyValuePair in bundleData)
            {
                // e.g. key = "Pantry/0", value = "Spring Crops/O 465 20/24 1 0 188 1 0 190 1 0 192 1 0/0"
                // https://stardewvalleywiki.com/Modding:Bundles
                var keyArray = keyValuePair.Key.Split('/');

                var areaName = keyArray[0];
                // Missing Bundle is after Community Center
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
                        case Cooking.CookingIngredient_AnyMilk:
                            itemDescription = "Milk (any)"; // TODO i18n
                            break;
                        case Cooking.CookingIngredient_AnyEgg:
                            itemDescription = "Egg (any)";
                            break;
                        case Cooking.CookingIngredient_AnyFish:
                            itemDescription = "Fish (any)";
                            break;
                        default:
                            var dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(itemId);
                            itemDescription = dataOrErrorItem.DisplayName; // TODO distinguish e.g. Large Egg (white) / Large Egg (brown), Smoked Fish (any)
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
                                itemDescription += " (silver)"; // TODO i18n
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
                    bundleSlotPrefix = $"{bundleSlotsShort} of "; // TODO i18n
                }
                var bundleItemList = String.Join(", ", bundleItems[bundleId]);
                linesToDisplay.Add($"* {areaName} - {bundleName} Bundle - {bundleSlotPrefix}{bundleItemList}{ModEntry.LineBreak}"); // TODO i18n
            }

            if (linesToDisplay.Count == 0)
            {
                var completeDescription = modInstance.Helper.Translation.Get("CommunityCenter_Complete", new { title = ModEntry.Title_CommunityCenter });
                Game1.drawDialogueNoTyping(completeDescription);
                return;
            }

            modInstance.ShowLines(linesToDisplay, title: ModEntry.Title_CommunityCenter, longerLinesExpected: true);
        }
    }
}
