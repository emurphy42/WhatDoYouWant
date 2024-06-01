using StardewModdingAPI;
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

        // TODO Master Angler

        // TODO A Complete Collection

        public string MuseumSortOrder = Museum.SortOrder_Type;

        // TODO Stardrops

        // TDOO Polyculture
    }
}
