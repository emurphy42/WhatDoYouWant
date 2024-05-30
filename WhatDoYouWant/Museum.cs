using StardewValley;
using StardewValley.Locations;
using StardewValley.Extensions;

namespace WhatDoYouWant
{
    internal class Museum
    {
        public static void ShowMuseumList(ModEntry modInstance)
        {
            var linesToDisplay = new List<string>();

            var itemType_Mineral = Game1.content.LoadString("Strings\\UI:Collections_Minerals");
            var itemType_Artifact = Game1.content.LoadString("Strings\\UI:Collections_Artifacts");

            // adapted from base game logic to award A Complete Collection achievement
            //   TODO sort options: alpha, type (mineral / artifact); mod items first, last
            foreach (var parsedItemData in ItemRegistry.GetObjectTypeDefinition().GetAllData())
            {
                // Does it need to be donated?
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

                // Add it to the list
                var itemType = baseContextTags.Contains("item_type_minerals") ? itemType_Mineral : itemType_Artifact;
                var dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(qualifiedItemId);
                var itemDescription = modInstance.Helper.Translation.Get("Museum_Item", new
                {
                    type = itemType,
                    item = dataOrErrorItem.DisplayName // TODO distinguish e.g. Ancient Doll (Y) / Ancient Doll (G)
                });
                linesToDisplay.Add($"* {itemDescription}{ModEntry.LineBreak}");
            }

            if (linesToDisplay.Count == 0)
            {
                var completeDescription = modInstance.Helper.Translation.Get("Museum_Complete", new { title = ModEntry.Title_Museum });
                Game1.drawDialogueNoTyping(completeDescription);
                return;
            }

            modInstance.ShowLines(linesToDisplay, title: ModEntry.Title_Museum);
        }

    }
}
