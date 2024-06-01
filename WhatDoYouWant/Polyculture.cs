using StardewValley;
using StardewValley.GameData.Crops;

namespace WhatDoYouWant
{
    internal class PolycultureCropData
    {
        public string? Seasons { get; set; }
        public int SeasonsSortOrder { get; set; }
        public string? CropName { get; set; }
        public int NumberNeeded { get; set; }
    }

    internal class Polyculture
    {
        private const int NumberShippedForPolyculture = 15;

        public const string SortOrder_SeasonsSpringFirst = "SeasonsSpringFirst";
        public const string SortOrder_SeasonsCurrentFirst = "SeasonsCurrentFirst";
        public const string SortOrder_CropName = "CropName";
        public const string SortOrder_NumberNeeded = "NumberNeeded";

        private static Season GetNextSeason(Season season)
        {
            switch (season)
            {
                case Season.Spring:
                    return Season.Summer;
                case Season.Summer:
                    return Season.Fall;
                case Season.Fall:
                    return Season.Winter;
                case Season.Winter:
                    return Season.Spring;
                default: // should never happen
                    return Season.Spring;
            }
        }

        public static void ShowPolycultureList(ModEntry modInstance)
        {
            var sortBySeasonsSpringFirst = (modInstance.Config.PolycultureSortOrder == SortOrder_SeasonsSpringFirst);
            var sortBySeasonsCurrentFirst = (modInstance.Config.PolycultureSortOrder == SortOrder_SeasonsCurrentFirst);
            var sortByCropName = (modInstance.Config.PolycultureSortOrder == SortOrder_CropName);
            var sortByNumberNeeded = (modInstance.Config.PolycultureSortOrder == SortOrder_NumberNeeded);

            var seasonsCurrentFirst = new List<Season>();
            if (sortBySeasonsCurrentFirst)
            {
                var season = Game1.season;
                seasonsCurrentFirst.Add(season);
                for (int i = 1; i <= 3; ++i)
                {
                    season = GetNextSeason(season);
                    seasonsCurrentFirst.Add(season);
                }
            }

            // adapted from base game logic to award Polyculure achievement
            var PolycultureCropList = new List<PolycultureCropData>();
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

                // Sort by first season in which it grows, break ties in favor of single-season crops
                //   e.g. spring first: Parsnip (Spring -> 10) is ahead of Coffee (Spring, Summer -> 11), which is ahead of Blueberry (Summer -> 12)
                var seasonsSortOrder = 0;
                var seasonsSortOrderList = sortBySeasonsCurrentFirst ? seasonsCurrentFirst : ModEntry.seasons;
                for (var seasonIndex = 0; seasonIndex < 4; ++seasonIndex)
                {
                    if (cropData.Seasons.Contains(seasonsSortOrderList[seasonIndex]))
                    {
                        seasonsSortOrder += 2 * seasonIndex + 8;
                        break;
                    }
                }
                if (cropData.Seasons.Count > 1)
                {
                    ++seasonsSortOrder;
                }

                var dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(cropData.HarvestItemId);

                PolycultureCropList.Add(new PolycultureCropData()
                {
                    Seasons = String.Join(", ", seasonsList),
                    SeasonsSortOrder = seasonsSortOrder,
                    CropName = dataOrErrorItem.DisplayName,
                    NumberNeeded = NumberShippedForPolyculture - numberShipped
                });
            }

            if (PolycultureCropList.Count == 0)
            {
                var completeDescription = modInstance.Helper.Translation.Get("Polyculture_Complete", new { title = ModEntry.GetTitle_Polyculture() });
                Game1.drawDialogueNoTyping(completeDescription);
                return;
            }

            var linesToDisplay = new List<string>();
            foreach (var polycultureCrop in PolycultureCropList
                .OrderBy(entry => sortBySeasonsSpringFirst || sortBySeasonsCurrentFirst ? entry.SeasonsSortOrder : 0)
                .ThenBy(entry => sortByNumberNeeded ? entry.NumberNeeded : 0)
                .ThenBy(entry => entry.CropName)
            )
            {
                var cropDescription = (sortBySeasonsSpringFirst || sortBySeasonsCurrentFirst)
                    ? $"{polycultureCrop.Seasons} - {polycultureCrop.CropName}"
                    : $"{polycultureCrop.CropName} - {polycultureCrop.Seasons}";
                var numberNeeded = modInstance.Helper.Translation.Get("Polyculture_NumberNeeded", new { number = polycultureCrop.NumberNeeded });
                linesToDisplay.Add($"* {cropDescription} - {numberNeeded}{ModEntry.LineBreak}");
            }

            modInstance.ShowLines(linesToDisplay, title: ModEntry.GetTitle_Polyculture());
        }
    }
}
