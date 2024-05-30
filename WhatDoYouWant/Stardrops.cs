using StardewValley;

namespace WhatDoYouWant
{
    internal class Stardrops
    {
        private static readonly List<string> StardropList = new()
        {
            "CF_Fair",
            "CF_Mines",
            "CF_Spouse",
            "CF_Sewer",
            "CF_Statue",
            "CF_Fish",
            "museumComplete"
        };

        public static void ShowStardropList(ModEntry modInstance, Farmer who)
        {
            var linesToDisplay = new List<string>();

            // adapted from base game logic for counting stardrops found
            foreach (var stardrop in StardropList)
            {
                // Do they already have it?
                if (who.hasOrWillReceiveMail(stardrop))
                {
                    continue;
                }
                if (stardrop == "CF_Mines" && who.chestConsumedMineLevels.ContainsKey(100) && who.chestConsumedMineLevels[100])
                {
                    continue;
                }

                // Add it to the list
                var stardropDescription = modInstance.Helper.Translation.Get($"Stardrop_{stardrop}");
                linesToDisplay.Add($"* {stardropDescription}{ModEntry.LineBreak}");
            }

            if (linesToDisplay.Count == 0)
            {
                var completeDescription = modInstance.Helper.Translation.Get("Stardrop_Complete", new { title = ModEntry.Title_Stardrops });
                Game1.drawDialogueNoTyping(completeDescription);
                return;
            }

            modInstance.ShowLines(linesToDisplay, title: ModEntry.Title_Stardrops);
        }
    }
}
