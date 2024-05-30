using StardewValley;
using StardewValley.Extensions;

namespace WhatDoYouWant
{
    internal class Shipping
    {
        public static void ShowShippingList(ModEntry modInstance, Farmer who)
        {
            var linesToDisplay = new List<string>();

            // adapted from base game logic to calculate full shipment %
            //   TODO sort options: alpha, collection tab order, category / type; mod items first, last
            foreach (var parsedItemData in ItemRegistry.GetObjectTypeDefinition().GetAllData())
            {
                switch (parsedItemData.Category)
                {
                    // Is it part of Full Shipment?
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

                        // Has it already been shipped?
                        if (who.basicShipped.ContainsKey(parsedItemData.ItemId))
                        {
                            continue;
                        }

                        // Add it to the list
                        linesToDisplay.Add($"* {parsedItemData.DisplayName}{ModEntry.LineBreak}");
                        break;
                }
            }

            if (linesToDisplay.Count == 0)
            {
                var completeDescription = modInstance.Helper.Translation.Get("Shipping_Complete", new { title = ModEntry.Title_Shipping });
                Game1.drawDialogueNoTyping(completeDescription);
                return;
            }

            modInstance.ShowLines(linesToDisplay, title: ModEntry.Title_Shipping);
        }

    }
}
