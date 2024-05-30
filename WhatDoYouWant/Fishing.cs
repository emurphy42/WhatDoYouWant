using StardewValley;
using StardewValley.GameData.Objects;
using StardewValley.Extensions;

namespace WhatDoYouWant
{
    internal class Fishing
    {
        public static void ShowFishingList(ModEntry modInstance, Farmer who)
        {
            var linesToDisplay = new List<string>();

            // adapted from base game logic to calculate fishing %
            //   TODO sort options: season (spring first), season (current first), alpha; mod items first, last
            //   seasons may vary by location, https://stardewvalleywiki.com/Modding:Location_data
            foreach (var parsedItemData in ItemRegistry.GetObjectTypeDefinition().GetAllData())
            {
                // Is it a fish?
                if (parsedItemData.ObjectType != "Fish")
                {
                    continue;
                }

                // Is it part of Master Angler?
                if (parsedItemData.RawData is ObjectData rawData && rawData.ExcludeFromFishingCollection)
                {
                    continue;
                }

                // Have they already caught it?
                if (who.fishCaught.ContainsKey(parsedItemData.QualifiedItemId))
                {
                    continue;
                }

                // Add it to the list
                var dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(parsedItemData.QualifiedItemId);
                var fishDescription = modInstance.Helper.Translation.Get("Fishing_Fish", new { fish = dataOrErrorItem.DisplayName });
                linesToDisplay.Add($"* {fishDescription}{ModEntry.LineBreak}");
            }

            if (linesToDisplay.Count == 0)
            {
                var completeDescription = modInstance.Helper.Translation.Get("Fishing_Complete", new { title = ModEntry.Title_Fishing });
                Game1.drawDialogueNoTyping(completeDescription);
                return;
            }

            modInstance.ShowLines(linesToDisplay, title: ModEntry.Title_Fishing);
        }
    }
}
