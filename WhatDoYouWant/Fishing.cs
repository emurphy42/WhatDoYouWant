using StardewValley;
using StardewValley.GameData.Objects;
using StardewValley.Extensions;
using StardewModdingAPI;

namespace WhatDoYouWant
{
    internal class FishData
    {
        public string? Seasons { get; set; }
        public int SeasonsSortOrder { get; set; }
        public string? FishName { get; set; }
        public string? TextureName { get; set; } // Collections Tab sort option
        public int SpriteIndex { get; set; } // Collections Tab sort option
    }

    internal class Fishing
    {
        public const string SortOrder_SeasonsSpringFirst = "SeasonsSpringFirst";
        public const string SortOrder_SeasonsCurrentFirst = "SeasonsCurrentFirst";
        public const string SortOrder_FishName = "FishName";
        public const string SortOrder_CollectionsTab = "CollectionsTab";

        // Fish that can be caught in any season
        private static readonly List<string> AllSeasonFish = new()
        {
            "(O)132", // Bream
            "(O)136", // Largemouth Bass
            "(O)152", // Seaweed
            "(O)153", // Green Algae
            "(O)156", // Ghostfish
            "(O)157", // White Algae
            "(O)158", // Stonefish
            "(O)161", // Ice Pip
            "(O)162", // Lava Eel
            "(O)164", // Sandfish
            "(O)165", // Scorpion Carp
            "(O)372", // Clam
            "(O)682", // Mutant Carp
            "(O)700", // Bullhead
            "(O)702", // Chub
            "(O)715", // Lobster
            "(O)716", // Crayfish
            "(O)717", // Crab
            "(O)718", // Cockle
            "(O)719", // Mussel
            "(O)720", // Shrimp
            "(O)721", // Snail
            "(O)722", // Periwinkle
            "(O)723", // Oyster
            "(O)734", // Woodskip
            "(O)795", // Void Salmon
            "(O)796", // Slimejack
            "(O)836", // Stingray
            "(O)837", // Lionfish
            "(O)838", // Blue Discus
            "(O)SeaJelly",
            "(O)CaveJelly",
            "(O)RiverJelly",
            "(O)Goby"
        };

        // Fish that can be caught in limited seasons, and not recognized by GetSeason()
        private static readonly Dictionary<string, List<Season>> FishSeasons = new()
        {
            { "(O)129", new List<Season> { Season.Spring, Season.Fall } }, // Anchovy
            { "(O)130", new List<Season> { Season.Summer, Season.Winter } }, // Tuna
            { "(O)131", new List<Season> { Season.Spring, Season.Fall, Season.Winter } }, // Sardine
            { "(O)137", new List<Season> { Season.Spring, Season.Fall } }, // Smallmouth Bass
            { "(O)140", new List<Season> { Season.Fall } }, // Walleye
            { "(O)142", new List<Season> { Season.Spring, Season.Summer, Season.Fall } }, // Carp
            { "(O)143", new List<Season> { Season.Spring, Season.Fall } }, // Catfish
            { "(O)144", new List<Season> { Season.Summer, Season.Winter } }, // Pike
            { "(O)145", new List<Season> { Season.Spring, Season.Summer } }, // Sunfish
            { "(O)146", new List<Season> { Season.Summer, Season.Winter } }, // Red Mullet
            { "(O)147", new List<Season> { Season.Spring, Season.Winter } }, // Herring
            { "(O)148", new List<Season> { Season.Spring, Season.Fall } }, // Eel
            { "(O)150", new List<Season> { Season.Summer, Season.Fall } }, // Red Snapper
            { "(O)154", new List<Season> { Season.Fall, Season.Winter } }, // Sea Cucumber
            { "(O)155", new List<Season> { Season.Summer, Season.Fall } }, // Super Cucumber
            { "(O)698", new List<Season> { Season.Summer, Season.Winter } }, // Sturgeon
            { "(O)699", new List<Season> { Season.Fall, Season.Winter } }, // Tiger Trout
            { "(O)701", new List<Season> { Season.Summer, Season.Fall } }, // Tilapia
            { "(O)705", new List<Season> { Season.Fall, Season.Winter } }, // Albacore
            { "(O)706", new List<Season> { Season.Spring, Season.Summer, Season.Fall } }, // Shad
            { "(O)708", new List<Season> { Season.Spring, Season.Summer, Season.Winter } }, // Halibut
            { "(O)798", new List<Season> { Season.Winter } }, // Midnight Squid
            { "(O)799", new List<Season> { Season.Winter } }, // Spook Fish
            { "(O)800", new List<Season> { Season.Winter } }, // Blobfish
            { "(O)267", new List<Season> { Season.Spring, Season.Summer } }, // Flounder
            { "(O)269", new List<Season> { Season.Fall, Season.Winter } } // Midnight Carp
        };

        // Check if fish can be caught anywhere in a single season
        private static Season? GetSeason(string fishId)
        {
            foreach (var location in Game1.locations)
            {
                var locationData = location.GetData();
                var locationFishList = locationData.Fish;
                foreach (var locationFish in locationFishList)
                {
                    if (locationFish.ItemId == fishId)
                    {
                        return locationFish.Season;
                    }
                }
            }
            return null;
        }

        // Check which season(s) fish can be caught
        //   * May vary by location and several other conditions
        //   * For simplicity, this covers all base game fish and some modded fish (if GetSeason() can figure them out)
        //   * TODO figure out how to parse locationData.Fish conditions more complex than "single season"
        private static List<Season> GetSeasons(string fishId)
        {
            if (AllSeasonFish.Contains(fishId))
            {
                return ModEntry.seasons;
            }

            var season = GetSeason(fishId);
            if (season != null)
            {
                return new List<Season>() { (Season)season };
            }

            if (FishSeasons.ContainsKey(fishId))
            {
                return FishSeasons[fishId];
            };

            return new List<Season>();
        }
        
        public static void ShowFishingList(ModEntry modInstance, Farmer who)
        {
            var sortBySeasonsSpringFirst = (modInstance.Config.FishingSortOrder == SortOrder_SeasonsSpringFirst);
            var sortBySeasonsCurrentFirst = (modInstance.Config.FishingSortOrder == SortOrder_SeasonsCurrentFirst);
            var sortByFishName = (modInstance.Config.FishingSortOrder == SortOrder_FishName);
            var sortByCollectionsTab = (modInstance.Config.FishingSortOrder == SortOrder_CollectionsTab);

            var seasonsSortOrderList = ModEntry.GetSeasons(currentFirst: sortBySeasonsCurrentFirst);

            // adapted from base game logic to calculate fishing %
            var fishList = new List<FishData>();
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

                var seasons = GetSeasons(parsedItemData.QualifiedItemId);

                fishList.Add(new FishData()
                {
                    Seasons = modInstance.GetSeasonsDescription(seasons, seasonsSortOrderList: seasonsSortOrderList),
                    SeasonsSortOrder = ModEntry.GetSeasonsSortOrder(seasons, seasonsSortOrderList: seasonsSortOrderList),
                    FishName = modInstance.Helper.Translation.Get("Fishing_Fish", new { fish = dataOrErrorItem.DisplayName }),
                    TextureName = dataOrErrorItem.TextureName,
                    SpriteIndex = dataOrErrorItem.SpriteIndex
                });
            }

            if (fishList.Count == 0)
            {
                var completeDescription = modInstance.Helper.Translation.Get("Fishing_Complete", new { title = ModEntry.GetTitle_Fishing() });
                Game1.drawDialogueNoTyping(completeDescription);
                return;
            }

            var linesToDisplay = new List<string>();

            foreach (var fish in fishList
                .OrderBy(entry => sortBySeasonsSpringFirst || sortBySeasonsCurrentFirst ? entry.SeasonsSortOrder : 0)
                .ThenBy(entry => sortByCollectionsTab ? entry.TextureName : "")
                .ThenBy(entry => sortByCollectionsTab ? entry.SpriteIndex : 0)
                .ThenBy(entry => entry.FishName)
            )
            {
                var seasonName = string.IsNullOrWhiteSpace(fish.Seasons) ? "???" : fish.Seasons;
                var fishName = (sortBySeasonsSpringFirst || sortBySeasonsCurrentFirst)
                    ? $"{seasonName} - {fish.FishName}"
                    : $"{fish.FishName} - {seasonName}";
                linesToDisplay.Add($"* {fishName}{ModEntry.LineBreak}");
            }

            modInstance.ShowLines(linesToDisplay, title: ModEntry.GetTitle_Fishing());
        }
    }
}
