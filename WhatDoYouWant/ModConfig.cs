using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace WhatDoYouWant
{
    public sealed class ModConfig
    {
        public KeybindList WhatDoYouWantKeypress = KeybindList.Parse("F2");

        // TODO CC options

        // TODO walnut options

        public string ShippingSortOrder = Shipping.SortOrder_Category;

        public string CookingSortOrder = Cooking.SortOrder_KnownRecipesFirst;

        public string CraftingSortOrder = Crafting.SortOrder_KnownRecipesFirst;

        // TODO fishing options

        // TODO museum options

        // TODO stardrop options

        // TDOO polyculture options
    }
}
