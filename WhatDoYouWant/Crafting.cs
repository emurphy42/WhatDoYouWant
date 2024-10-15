using StardewValley;

namespace WhatDoYouWant
{
    internal class Crafting
    {
        public const string SortOrder_KnownRecipesFirst = "KnownRecipesFirst";
        public const string SortOrder_RecipeName = "RecipeName";
        public const string SortOrder_CraftingMenu = "CraftingMenu";
        public const string SortOrder_Materials = "Materials";

        public const string CraftingIngredient_AnyWildSeeds = "-777"; // hardcoded instead of CraftingRecipe.wild_seed_special_category.ToString() so other code can switch() on it

        public class RecipeData
        {
            public string? RecipeKey { get; set; }
            public string? RecipeName { get; set; }
            public string? RecipeIngredients { get; set; }
            public bool RecipeLearned { get; set; }
            public string? RecipeCondition { get; set; }
            public int OriginalSortOrder { get; set; }
        }

        public static void ShowCraftingList(ModEntry modInstance, Farmer who)
        {
            // adapted from base game logic to calculate crafting %
            var recipeList = new List<RecipeData>();
            var originalSortOrder = 0;
            var materialListAllRecipes = new Dictionary<string, int>();
            var sortByMaterials = (modInstance.Config.CraftingSortOrder == SortOrder_Materials);
            foreach (var keyValuePair in DataLoader.CraftingRecipes(Game1.content))
            {
                // keyValuePair = e.g. <"Wood Fence", "388 2/Field/322/false/l 0">
                // value = list of ingredient IDs and quantities / unused / item ID of crafted item / big craftable? / unlock conditions
                var key1 = keyValuePair.Key;
                var key2 = ArgUtility.SplitBySpaceAndGet(ArgUtility.Get(keyValuePair.Value.Split('/'), CraftingRecipe.index_output), 0);
                // Only needed in multiplayer (TODO detect, or at least include it and mention this condition)
                if (key1 == "Wedding Ring")
                {
                    continue;
                }
                // Already crafted?
                var recipeLearned = who.craftingRecipes.TryGetValue(key1, out int numberCrafted);
                if (numberCrafted > 0)
                {
                    continue;
                }

                // Add it to the list

                var ingredients = ArgUtility.Get(keyValuePair.Value.Split('/'), CraftingRecipe.index_ingredients);
                if (sortByMaterials)
                {
                    var ingredientList = ingredients.Trim().Split(' ');
                    var ingredientTextList = new List<string>();
                    for (var index = 0; index < ingredientList.Length; index += 2)
                    {
                        var ingredientId = ingredientList[index];
                        var ingredientName = ModEntry.GetIngredientName(ingredientId);

                        var ingredientQuantity = ArgUtility.GetInt(ingredientList, index + 1, 1);

                        if (!materialListAllRecipes.ContainsKey(ingredientName))
                        {
                            materialListAllRecipes.Add(ingredientName, 0);
                        }
                        materialListAllRecipes[ingredientName] += ingredientQuantity;
                    }
                    continue;
                }

                var isBigCraftable = ArgUtility.SplitBySpaceAndGet(ArgUtility.Get(keyValuePair.Value.Split('/'), CraftingRecipe.index_craftingBigCraftable), 0);
                var itemPrefix = (isBigCraftable == "true") ? "(BC)" : "(O)";
                var dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(itemPrefix + key2);
                
                var condition = ArgUtility.Get(keyValuePair.Value.Split('/'), CraftingRecipe.index_craftingUnlockConditions);

                ++originalSortOrder;

                recipeList.Add(new RecipeData()
                {
                    RecipeKey = key1,
                    RecipeName = dataOrErrorItem.DisplayName,
                    RecipeIngredients = ModEntry.GetIngredientText(ingredients),
                    RecipeLearned = recipeLearned,
                    RecipeCondition = condition,
                    OriginalSortOrder = originalSortOrder
                });
            }

            if ((sortByMaterials ? materialListAllRecipes.Count : recipeList.Count) == 0)
            {
                var completeDescription = modInstance.Helper.Translation.Get("Crafting_Complete", new { title = ModEntry.GetTitle_Crafting() });
                Game1.drawDialogueNoTyping(completeDescription);
                return;
            }

            var linesToDisplay = new List<string>();
            if (sortByMaterials)
            {
                foreach (var ingredientNameQuantity in materialListAllRecipes.OrderBy(entry => entry.Key))
                {
                    linesToDisplay.Add($"* {ingredientNameQuantity.Key} x{ingredientNameQuantity.Value}{ModEntry.LineBreak}");
                }
            }
            else
            {
                var sortByKnownRecipesFirst = (modInstance.Config.CraftingSortOrder == SortOrder_KnownRecipesFirst);
                var sortByRecipeName = (modInstance.Config.CraftingSortOrder == SortOrder_RecipeName);
                var sortByCraftingMenu = (modInstance.Config.CraftingSortOrder == SortOrder_CraftingMenu);

                var notYetLearnedPrefix = modInstance.Helper.Translation.Get("Crafting_NotYetLearned");

                foreach (var recipe in recipeList
                    .OrderBy(entry => sortByCraftingMenu ? entry.OriginalSortOrder : 0)
                    .ThenByDescending(entry => sortByKnownRecipesFirst ? entry.RecipeLearned : false)
                    .ThenBy(entry => entry.RecipeName)
                )
                {
                    var learnedPrefix = "";
                    if (!recipe.RecipeLearned)
                    {
                        var conditionDescription = modInstance.GetConditionDescription(recipe: recipe.RecipeKey, condition: recipe.RecipeCondition, isCooking: false);
                        learnedPrefix = $"{notYetLearnedPrefix} ({conditionDescription}) - ";
                    }
                    linesToDisplay.Add($"* {recipe.RecipeName} - {learnedPrefix}{recipe.RecipeIngredients}{ModEntry.LineBreak}");
                }
            }

            modInstance.ShowLines(linesToDisplay, title: ModEntry.GetTitle_Crafting(), longerLinesExpected: !sortByMaterials);
        }

    }
}
