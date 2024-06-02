using StardewModdingAPI.Utilities;

namespace WhatDoYouWant
{
    public sealed class ModConfig
    {
        public KeybindList WhatDoYouWantKeypress = KeybindList.Parse("F2");

        // TODO Community Center

        // TODO Golden Walnuts

        // Full Shipment

        public string ShippingSortOrder = Shipping.SortOrder_Category;

        // Gourmet Chef

        public string CookingSortOrder = Cooking.SortOrder_KnownRecipesFirst;

        // Craft Master

        public string CraftingSortOrder = Crafting.SortOrder_KnownRecipesFirst;

        // Master Angler

        public string FishingSortOrder = Fishing.SortOrder_SeasonsSpringFirst;

        // A Complete Collection

        public string MuseumSortOrder = Museum.SortOrder_Type;

        // TODO Stardrops

        // Polyculture

        public string PolycultureSortOrder = Polyculture.SortOrder_SeasonsSpringFirst;
    }
}
