using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;

namespace WhatDoYouWant
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            Helper.Events.Input.ButtonsChanged += (_sender, _e) => buttonsChanged(_sender, _e);
        }

        private void buttonsChanged(object? _sender, ButtonsChangedEventArgs _e)
        {
            var key = KeybindList.Parse("F2");
            if (!key.JustPressed())
            {
                return;
            }

            var responseList = new List<Response>();
            responseList.Add(new Response(responseKey: "FullShipment", responseText: "Full Shipment"));
            responseList.Add(new Response(responseKey: "Cancel", responseText: "(Cancel)"));
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
                case "FullShipment":
                    showFullShipmentList(who: who, answer: answer);
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
                        var includedInFullShipment = StardewValley.Object.isPotentialBasicShipped(
                            itemId: parsedItemData.ItemId,
                            category: parsedItemData.Category,
                            objectType: parsedItemData.ObjectType
                        );
                        var hasBeenShipped = who.basicShipped.ContainsKey(parsedItemData.ItemId);
                        if (includedInFullShipment && !hasBeenShipped)
                        {
                            linesToDisplay.Add($"{parsedItemData.DisplayName}^"); // "^" -> line break
                        }
                        break;
                }
            }

            if (linesToDisplay.Count == 0)
            {
                linesToDisplay.Add("Full Shipment is complete!");
            }

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
