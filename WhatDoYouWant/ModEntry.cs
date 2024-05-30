using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace WhatDoYouWant
{
    public class ModEntry : Mod
    {
        public const string LineBreak = "^";

        private const string ResponseToken_CommunityCenter = "CommunityCenter";
        private const string ResponseToken_Walnuts = "Walnuts";
        private const string ResponseToken_Shipping = "Shipping";
        private const string ResponseToken_Cooking = "Cooking";
        private const string ResponseToken_Crafting = "Crafting";
        private const string ResponseToken_Fishing = "Fishing";
        private const string ResponseToken_Museum = "Museum";
        private const string ResponseToken_Stardrops = "Stardrops";
        private const string ResponseToken_Polyculture = "Polyculture";
        private const string ResponseToken_Cancel = "Cancel";

        public const string Title_CommunityCenter = "Community Center"; // TODO replace these with functions pulling them from base game content
        public const string Title_Walnuts = "Golden Walnuts";
        public const string Title_Shipping = "Full Shipment";
        public const string Title_Cooking = "Gourmet Chef";
        public const string Title_Crafting = "Craft Master";
        public const string Title_Fishing = "Master Angler";
        public const string Title_Museum = "A Complete Collection";
        public const string Title_Stardrops = "Stardrops";
        public const string Title_Polyculture = "Polyculture";

        // Used by Polyculture, will also be used by Master Angler if/when season logic is added
        public static readonly List<Season> seasons = new()
        {
            Season.Spring,
            Season.Summer,
            Season.Fall,
            Season.Winter
        };

        // TODO https://stardewcommunitywiki.com/Modding:Modder_Guide/APIs/Translation

        public override void Entry(IModHelper helper)
        {
            Helper.Events.Input.ButtonsChanged += (_sender, _e) => ButtonsChanged(_sender, _e);
        }

        private void ButtonsChanged(object? _sender, ButtonsChangedEventArgs _e)
        {
            if (!Game1.hasStartedDay)
            {
                return;
            }

            var key = KeybindList.Parse("F2"); // TODO make this a mod option, changeable via GMCM
            if (!key.JustPressed())
            {
                return;
            }

            // TODO default to omitting things already completed or unavailable (check mail / achievement data when possible), option to include them anyway
            //   * CC if already completed or bought Joja membership
            //   * walnuts if island not yet unlocked
            List<Response> responseList = new();
            responseList.Add(new Response(responseKey: ResponseToken_CommunityCenter, responseText: Title_CommunityCenter));
            responseList.Add(new Response(responseKey: ResponseToken_Walnuts, responseText: Title_Walnuts));
            responseList.Add(new Response(responseKey: ResponseToken_Shipping, responseText: Title_Shipping));
            responseList.Add(new Response(responseKey: ResponseToken_Cooking, responseText: Title_Cooking));
            responseList.Add(new Response(responseKey: ResponseToken_Crafting, responseText: Title_Crafting));
            responseList.Add(new Response(responseKey: ResponseToken_Fishing, responseText: Title_Fishing));
            responseList.Add(new Response(responseKey: ResponseToken_Museum, responseText: Title_Museum));
            responseList.Add(new Response(responseKey: ResponseToken_Stardrops, responseText: Title_Stardrops));
            responseList.Add(new Response(responseKey: ResponseToken_Polyculture, responseText: Title_Polyculture));
            responseList.Add(new Response(responseKey: ResponseToken_Cancel, responseText: "(Cancel)"));
            Game1.currentLocation.createQuestionDialogue(
              question: "Show items still needed for...",
              answerChoices: responseList.ToArray(),
              afterDialogueBehavior: new GameLocation.afterQuestionBehavior(this.GotResponse)
            );
        }

        public virtual void GotResponse(Farmer who, string answer)
        {
            switch (answer)
            {
                case ResponseToken_CommunityCenter:
                    CommunityCenter.ShowCommunityCenterList(modInstance: this);
                    break;
                case ResponseToken_Walnuts:
                    Walnuts.ShowWalnutsList(modInstance: this);
                    break;
                case ResponseToken_Shipping:
                    Shipping.ShowShippingList(modInstance: this, who: who);
                    break;
                case ResponseToken_Cooking:
                    Cooking.ShowCookingList(modInstance: this, who: who);
                    break;
                case ResponseToken_Crafting:
                    Crafting.ShowCraftingList(modInstance: this, who: who);
                    break;
                case ResponseToken_Fishing:
                    Fishing.ShowFishingList(modInstance: this, who: who);
                    break;
                case ResponseToken_Museum:
                    Museum.ShowMuseumList(modInstance: this);
                    break;
                case ResponseToken_Stardrops:
                    Stardrops.ShowStardropList(modInstance: this, who: who);
                    break;
                case ResponseToken_Polyculture:
                    Polyculture.ShowPolycultureList(modInstance: this);
                    break;
                case ResponseToken_Cancel:
                    break;
                default:
                    Game1.drawDialogueNoTyping("Not yet implemented");
                    break;
            }
        }

        // used by cooking and crafting
        public static string GetIngredientText(string ingredients)
        {
            var ingredientList = ingredients.Trim().Split(' ');
            var ingredientTextList = new List<string>();
            for (var index = 0; index < ingredientList.Length; index += 2)
            {
                var ingredientId = ingredientList[index];
                string ingredientName;
                switch (ingredientId)
                {
                    case Cooking.CookingIngredient_AnyMilk:
                        ingredientName = "Milk (any)";
                        break;
                    case Cooking.CookingIngredient_AnyEgg:
                        ingredientName = "Egg (any)";
                        break;
                    case Cooking.CookingIngredient_AnyFish:
                        ingredientName = "Fish (any)";
                        break;
                    default:
                        var ingredientDataOrErrorItem = ItemRegistry.GetDataOrErrorItem(ingredientId);
                        ingredientName = ingredientDataOrErrorItem.DisplayName;
                        break;
                }

                var ingredientQuantity = ingredientList[index + 1];
                if (ingredientQuantity != "1")
                {
                    ingredientName += $" x{ingredientQuantity}";
                }

                ingredientTextList.Add(ingredientName);
            }

            return String.Join(", ", ingredientTextList);
        }

        // non-static because Monitor is tied to the instance
        public void ShowLines(List<string> linesToDisplay, string? title = null, bool longLinesExpected = false, bool longerLinesExpected = false)
        {
            // Log output - can be copy/pasted from the SMAPI window while game is running, or from the log after it's closed
            if (title != null)
            {
                Monitor.Log(title, LogLevel.Info);
                foreach (var line in linesToDisplay)
                {
                    Monitor.Log(line.Replace(LineBreak, ""), LogLevel.Info);
                }
            }

            // Display output in-game
            // adapted from base game logic to display Perfection Tracker output
            var sectionsOfHeight = SpriteText.getStringBrokenIntoSectionsOfHeight(
                s: string.Concat(linesToDisplay),
                width: 9999,
                height: longerLinesExpected
                    ? Game1.uiViewport.Height / 3
                    : (longLinesExpected
                        ? Game1.uiViewport.Height / 2
                        : Game1.uiViewport.Height - 100
                    )
            );
            if (sectionsOfHeight.Count > 1)
            {
                for (var index = 0; index < sectionsOfHeight.Count; ++index)
                {
                    sectionsOfHeight[index] += $"(part {index + 1} of {sectionsOfHeight.Count})\n";
                }
            }

            Game1.drawDialogueNoTyping(sectionsOfHeight);
        }

    }
}
