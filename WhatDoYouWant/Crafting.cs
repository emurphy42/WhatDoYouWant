using StardewValley;

namespace WhatDoYouWant
{
    internal class Crafting
    {
        public static void ShowCraftingList(ModEntry modInstance, Farmer who)
        {
            var linesToDisplay = new List<string>();

            // adapted from base game logic to calculate crafting %
            //   TODO note crafting recipes not yet acquired
            //   TODO sort options: mod items first, last
            var craftingDictionary = DataLoader.CraftingRecipes(Game1.content);
            foreach (var keyValuePair in craftingDictionary)
            {
                // keyValuePair = e.g. <"Wood Fence", "388 2/Field/322/false/l 0">
                // value = list of ingredient IDs and quantities / unused / item ID of crafted item / big craftable? / unlock conditions
                var key1 = keyValuePair.Key;
                var key2 = ArgUtility.SplitBySpaceAndGet(ArgUtility.Get(keyValuePair.Value.Split('/'), 2), 0);
                // Only needed in multiplayer (TODO detect, or at least include it and mention this condition)
                if (key1 == "Wedding Ring")
                {
                    continue;
                }
                // Already crafted?
                who.craftingRecipes.TryGetValue(key1, out int numberCrafted);
                if (numberCrafted > 0)
                {
                    continue;
                }

                // Add it to the list
                // TODO parse unlock conditions
                var isBigCraftable = ArgUtility.SplitBySpaceAndGet(ArgUtility.Get(keyValuePair.Value.Split('/'), 3), 0);
                var itemPrefix = (isBigCraftable == "true") ? "(BC)" : "(O)";
                var dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(itemPrefix + key2);
                var ingredients = ArgUtility.Get(keyValuePair.Value.Split('/'), 0);
                var ingredientText = ModEntry.GetIngredientText(ingredients);
                linesToDisplay.Add($"* {dataOrErrorItem.DisplayName} - {ingredientText}{ModEntry.LineBreak}");
            }

            if (linesToDisplay.Count == 0)
            {
                var completeDescription = modInstance.Helper.Translation.Get("Crafting_Complete", new { title = ModEntry.Title_Crafting });
                Game1.drawDialogueNoTyping(completeDescription);
                return;
            }

            modInstance.ShowLines(linesToDisplay, title: ModEntry.Title_Crafting, longLinesExpected: true);
        }

    }
}
