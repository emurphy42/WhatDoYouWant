using GenericModConfigMenu;
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

        public ModConfig Config = new();

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            Helper.Events.GameLoop.GameLaunched += (_sender, _e) => OnGameLaunched(_sender, _e);
            Helper.Events.Input.ButtonsChanged += (_sender, _e) => ButtonsChanged(_sender, _e);
        }

        private void OnGameLaunched(object? _sender, GameLaunchedEventArgs _e)
        {
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
            {
                return;
            }

            configMenu.Register(
                mod: this.ModManifest,
                reset: () => this.Config = new ModConfig(),
                save: () => this.Helper.WriteConfig(this.Config)
            );

            configMenu.AddKeybindList(
                mod: this.ModManifest,
                getValue: () => this.Config.WhatDoYouWantKeypress,
                setValue: value => this.Config.WhatDoYouWantKeypress = value,
                name: () => Helper.Translation.Get("Options_OpenMenuKey")
            );

            // TODO Community Center

            // TODO Golden Walnuts

            // Full Shipment

            configMenu.AddTextOption(
                mod: this.ModManifest,
                getValue: () => this.Config.ShippingSortOrder,
                setValue: value => this.Config.ShippingSortOrder = value,
                name: () => Helper.Translation.Get("Options_ShippingSortOrder"),
                allowedValues: new string[] {
                    Shipping.SortOrder_Category,
                    Shipping.SortOrder_ItemName,
                    Shipping.SortOrder_CollectionsTab
                },
                formatAllowedValue: value => Helper.Translation.Get($"Options_ShippingSortOrder_{value}")
            );

            // Gourmet Chef

            configMenu.AddTextOption(
                mod: this.ModManifest,
                getValue: () => this.Config.CookingSortOrder,
                setValue: value => this.Config.CookingSortOrder = value,
                name: () => Helper.Translation.Get("Options_CookingSortOrder"),
                allowedValues: new string[] {
                    Cooking.SortOrder_KnownRecipesFirst,
                    Cooking.SortOrder_RecipeName,
                    Cooking.SortOrder_CollectionsTab
                },
                formatAllowedValue: value => Helper.Translation.Get($"Options_CookingSortOrder_{value}")
            );

            // Craft Master

            configMenu.AddTextOption(
                mod: this.ModManifest,
                getValue: () => this.Config.CraftingSortOrder,
                setValue: value => this.Config.CraftingSortOrder = value,
                name: () => Helper.Translation.Get("Options_CraftingSortOrder"),
                allowedValues: new string[] {
                    Crafting.SortOrder_KnownRecipesFirst,
                    Crafting.SortOrder_RecipeName,
                    Crafting.SortOrder_CraftingMenu
                },
                formatAllowedValue: value => Helper.Translation.Get($"Options_CraftingSortOrder_{value}")
            );

            // TODO Master Angler

            // TODO A Complete Collection

            configMenu.AddTextOption(
                mod: this.ModManifest,
                getValue: () => this.Config.MuseumSortOrder,
                setValue: value => this.Config.MuseumSortOrder = value,
                name: () => Helper.Translation.Get("Options_MuseumSortOrder"),
                allowedValues: new string[] {
                    Museum.SortOrder_Type,
                    Museum.SortOrder_ItemName,
                    Museum.SortOrder_CollectionsTabs
                },
                formatAllowedValue: value => Helper.Translation.Get($"Options_MuseumSortOrder_{value}")
            );

            // TODO Stardrops

            // TDOO Polyculture
        }

        private void ButtonsChanged(object? _sender, ButtonsChangedEventArgs _e)
        {
            if (!Config.WhatDoYouWantKeypress.JustPressed())
            {
                return;
            }

            if (!Game1.hasStartedDay)
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
            responseList.Add(new Response(responseKey: ResponseToken_Cancel, responseText: "(" + Helper.Translation.Get("Menu_Cancel") + ")"));
            Game1.currentLocation.createQuestionDialogue(
              question: Helper.Translation.Get("Menu_Question"),
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
                default: // should never happen
                    Game1.drawDialogueNoTyping(Helper.Translation.Get("Response_NotYetImplemented"));
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
                        ingredientName = "Milk (any)"; // TODO i18n
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
                    var sectionDescription = Helper.Translation.Get("Response_Section", new { section = index + 1, numberSections = sectionsOfHeight.Count });
                    sectionsOfHeight[index] += $"({sectionDescription})\n";
                }
            }

            Game1.drawDialogueNoTyping(sectionsOfHeight);
        }

    }
}
