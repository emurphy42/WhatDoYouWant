using StardewValley;
using StardewValley.GameData.Crops;

namespace WhatDoYouWant
{
    internal class Polyculture
    {
        private const int NumberShippedForPolyculture = 15;

        public static void ShowPolycultureList(ModEntry modInstance)
        {
            var linesToDisplay = new List<string>();

            // adapted from base game logic to award Polyculure achievement
            //   TODO sort options: season(s) <spring first>, season(s) <current first>, alpha, number shipped; mod items first, last
            foreach (CropData cropData in (IEnumerable<CropData>)Game1.cropData.Values)
            {
                // Is it part of Polyculture?
                if (!cropData.CountForPolyculture)
                {
                    continue;
                }

                // Has enough of it already been shipped?
                Game1.player.basicShipped.TryGetValue(cropData.HarvestItemId, out int numberShipped);
                if (numberShipped >= NumberShippedForPolyculture)
                {
                    continue;
                }

                // Add it to the list
                var seasonsList = new List<string>();
                foreach (var season in ModEntry.seasons)
                {
                    if (cropData.Seasons.Contains(season))
                    {
                        seasonsList.Add(Utility.getSeasonNameFromNumber((int)season));
                    }
                }
                var numberNeeded = NumberShippedForPolyculture - numberShipped;
                var dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(cropData.HarvestItemId);
                var cropDescription = modInstance.Helper.Translation.Get("Polyculture_Crop", new {
                    season = String.Join(", ", seasonsList),
                    crop = dataOrErrorItem.DisplayName,
                    number = numberNeeded
                });
                linesToDisplay.Add($"* {cropDescription}{ModEntry.LineBreak}");
            }

            if (linesToDisplay.Count == 0)
            {
                var completeDescription = modInstance.Helper.Translation.Get("Polyculture_Complete", new { title = ModEntry.Title_Polyculture });
                Game1.drawDialogueNoTyping(completeDescription);
                return;
            }

            modInstance.ShowLines(linesToDisplay, title: ModEntry.Title_Polyculture);
        }
    }
}
