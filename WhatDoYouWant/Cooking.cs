using StardewValley;

namespace WhatDoYouWant
{
    internal class Cooking
    {
        public static void ShowCookingList(ModEntry modInstance, Farmer who)
        {
            var linesToDisplay = new List<string>();

            var notYetLearnedPrefix = modInstance.Helper.Translation.Get("Cooking_NotYetLearned") + " - ";

            // adapted from base game logic to calculate cooking %
            //   TODO sort options: mod items first, last
            var cookingDictionary = DataLoader.CookingRecipes(Game1.content);
            foreach (var keyValuePair in cookingDictionary)
            {
                // keyValuePair = e.g. <"Fried Egg", "-5 1/10 10/194/default">
                // value = list of ingredient IDs and quantities / unused / item ID of cooked dish / unlock conditions
                var key1 = keyValuePair.Key;
                var key2 = ArgUtility.SplitBySpaceAndGet(ArgUtility.Get(keyValuePair.Value.Split('/'), 2), 0);
                var recipeLearned = who.cookingRecipes.ContainsKey(key1);
                var recipeCooked = who.recipesCooked.ContainsKey(key2);
                if (recipeLearned && recipeCooked)
                {
                    continue;
                }

                // TODO parse unlock conditions
                var learnedPrefix = recipeLearned ? "" : notYetLearnedPrefix;

                var ingredients = ArgUtility.Get(keyValuePair.Value.Split('/'), 0);
                var ingredientText = ModEntry.GetIngredientText(ingredients);

                var dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(key2);
                linesToDisplay.Add($"* {dataOrErrorItem.DisplayName} - {learnedPrefix}{ingredientText}{ModEntry.LineBreak}");
            }

            if (linesToDisplay.Count == 0)
            {
                var completeDescription = modInstance.Helper.Translation.Get("Cooking_Complete", new { title = ModEntry.Title_Cooking });
                Game1.drawDialogueNoTyping(completeDescription);
                return;
            }

            modInstance.ShowLines(linesToDisplay, title: ModEntry.Title_Cooking, longLinesExpected: true);
        }
    }
}
