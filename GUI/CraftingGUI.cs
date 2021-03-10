//using MagicStorage.Components;
//using MagicStorage.Items;
//using MagicStorage.Sorting;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Input;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using Terraria;
//using Terraria.DataStructures;
//using Terraria.GameContent.UI.Elements;
//using Terraria.ID;
//using Terraria.Localization;
//using Terraria.Map;
//using Terraria.ModLoader;
//using Terraria.UI;
//using CraftingAccess = MagicStorage.Components.CraftingAccess;
//using CraftingStorageAccess = MagicStorage.Components.CraftingStorageAccess;

//namespace MagicStorage.GUI {
//	public static class CraftingGUI {

//		public const int AVAILABLE_RECIPES_NUM_COLS = 10;
//		public const int AVAILABLE_INGREDIENT_NUM_COLS = 7;
//		public const float RECIPES_SCALE = 0.85f;
//		public const float INGREDIENTS_SCALE = 0.7f;
//		private const float TOP_BAR_HEIGHT = 32f;


//		private const int START_MAX_CRAFT_TIMER = 20;
//		private const int SMART_MAX_CLICK_TIMER = 20;

//		// Padding between Item Slots

//		// Element Spacing
//		private const int SPACING = UICommon.PADDING * 3;

//		// The left offset from the screen
//		private const float PANEL_LEFT = 20;

//		public static MouseState curMouse;
//		public static MouseState oldMouse;

//		public static bool MouseClicked => curMouse.LeftButton == ButtonState.Pressed && oldMouse.LeftButton == ButtonState.Released;

//		private static UIPanel recipesPanel;
//		private static float panelTop;
//		private static float panelWidth;
//		private static float panelHeight;

//		private static UIElement recipesTopBar;
//		internal static UISearchBar itemNameSearch;
//		private static UIButtonChoice sortButtons;
//		private static UIButtonChoice recipeButtons;
//		private static UIElement ingredientsTopBar;
//		private static UIButtonChoice filterButtons;
//		internal static UISearchBar modNameSearch;

//		private static UIText stationText;
//		private static UISlotZone stationZone = new UISlotZone(HoverStation, GetStation, RECIPES_SCALE);
//		private static UIText recipeText;
//		private static UISlotZone recipeZone = new UISlotZone(HoverRecipe, GetRecipe, RECIPES_SCALE);

//		internal static UIScrollbar scrollBar = new UIScrollbar();
//		private static int scrollBarFocus = 0;
//		private static int scrollBarFocusMouseStart;
//		private static float scrollBarFocusPositionStart;
//		private static float scrollBarViewSize = 1f;
//		private static float scrollBarMaxViewSize = 2f;

//		private static List<Item> items = new List<Item>();
//		private static Dictionary<int, int> itemCounts = new Dictionary<int, int>();
//		private static bool[] adjTiles = new bool[TileLoader.TileCount];
//		private static bool adjWater = false;
//		private static bool adjLava = false;
//		private static bool adjHoney = false;
//		private static bool zoneSnow = false;
//		private static bool alchemyTable = false;
//		private static List<Recipe> recipes = new List<Recipe>();
//		private static List<bool> recipeAvailable = new List<bool>();
//		private static Recipe selectedRecipe = null;
//		private static int numRows;
//		private static int displayRows;
//		private static bool slotFocus = false;

//		private static UIElement bottomBar = new UIElement();
//		private static UIText capacityText;

//		private static UIPanel ingredientsPanel;
//		private static float ingredientTop;
//		private static float ingredientLeft;
//		private static float ingredientWidth;
//		private static float ingredientHeight;

//		private static UIText recipePanelHeader;
//		private static UIText ingredientText;
//		private static UISlotZone ingredientZone = new UISlotZone(HoverItem, GetIngredient, INGREDIENTS_SCALE);
//		private static UIText requiresObjHeader;
//		private static UIText requiresObjText;
//		private static UIText storedItemsText;

//		private static UISlotZone storageZone = new UISlotZone(HoverStorage, GetStorage, INGREDIENTS_SCALE);
//		private static int numRows2;
//		private static int displayRows2;
//		private static List<Item> storageItems = new List<Item>();
//		private static List<ItemData> blockStorageItems = new List<ItemData>();

//		internal static UIScrollbar scrollBar2 = new UIScrollbar();
//		private static float scrollBar2ViewSize = 1f;
//		private static float scrollBar2MaxViewSize = 2f;

//		internal static UITextPanel<LocalizedText> craftButton;
//		private static Item result = null;
//		private static UISlotZone resultZone = new UISlotZone(HoverResult, GetResult, RECIPES_SCALE);

//		private static int craftTimer = 0;
//		private static int maxCraftTimer = START_MAX_CRAFT_TIMER;
//		private static int rightClickTimer = 0;
//		private static int maxRightClickTimer = SMART_MAX_CLICK_TIMER;

//		private static readonly object threadLOCK = new object();
//		private static readonly object recipeLOCK = new object();
//		private static readonly object itemsLOCK = new object();

//		private static bool threadRunning = false;
//		internal static bool threadNeedsRestart = false;
//		private static SortMode threadSortMode;
//		private static FilterMode threadFilterMode;
//		private static List<Recipe> threadRecipes = new List<Recipe>();
//		private static List<bool> threadRecipeAvailable = new List<bool>();
//		private static List<Recipe> nextRecipes = new List<Recipe>();
//		private static List<bool> nextRecipeAvailable = new List<bool>();

//		private static ManualResetEventSlim evnt = new ManualResetEventSlim();

//		public static void Initialize() {
//			lock (recipeLOCK) {
//				recipes = nextRecipes;
//				recipeAvailable = nextRecipeAvailable;
//			}

//			InitLangStuff();

//			float itemSlotWidth = Main.inventoryBackTexture.Width * RECIPES_SCALE;
//			float itemSlotHeight = Main.inventoryBackTexture.Height * RECIPES_SCALE;
//			float smallSlotWidth = Main.inventoryBackTexture.Width * INGREDIENTS_SCALE;
//			float smallSlotHeight = Main.inventoryBackTexture.Height * INGREDIENTS_SCALE;

//			panelTop = Main.instance.invBottom + 60;
//			panelHeight = Main.screenHeight - panelTop - 60;

//			recipesPanel = new UIPanel();

//			float innerPanelWidth = AVAILABLE_RECIPES_NUM_COLS * (itemSlotWidth + UICommon.PADDING) + 20f + UICommon.PADDING;
//			panelWidth = recipesPanel.PaddingLeft + innerPanelWidth + recipesPanel.PaddingRight;
//			recipesPanel.Left.Set(PANEL_LEFT, 0f);
//			recipesPanel.Top.Set(panelTop, 0f);
//			recipesPanel.Width.Set(panelWidth, 0f);
//			recipesPanel.Height.Set(panelHeight, 0f);
//			recipesPanel.Recalculate();


//			ingredientsPanel = new UIPanel();

//			ingredientTop = panelTop;
//			ingredientLeft = PANEL_LEFT + panelWidth;
//			ingredientWidth = AVAILABLE_INGREDIENT_NUM_COLS * (smallSlotWidth + UICommon.PADDING) + 20f + UICommon.PADDING;
//			ingredientWidth += ingredientsPanel.PaddingLeft + ingredientsPanel.PaddingRight;
//			ingredientHeight = panelHeight;

//			ingredientsPanel.Left.Set(ingredientLeft, 0f);
//			ingredientsPanel.Top.Set(ingredientTop, 0f);
//			ingredientsPanel.Width.Set(ingredientWidth, 0f);
//			ingredientsPanel.Height.Set(ingredientHeight, 0f);
//			ingredientsPanel.Recalculate();

//			recipesTopBar = new UIElement();
//			recipesTopBar.Width.Set(0f, 1f);
//			recipesTopBar.Height.Set(TOP_BAR_HEIGHT, 0f);
//			recipesPanel.Append(recipesTopBar);

//			InitSortButtons();
//			recipesTopBar.Append(sortButtons);
//			float sortButtonsRight = sortButtons.GetDimensions().Width + UICommon.PADDING;
//			InitRecipeButtons();
//			float recipeButtonsLeft = sortButtonsRight + UIButtonChoice.BUTTON_SIZE + 3 * UICommon.PADDING;
//			recipeButtons.Left.Set(recipeButtonsLeft, 0f);
//			recipesTopBar.Append(recipeButtons);
//			float recipeButtonsRight = recipeButtonsLeft + recipeButtons.GetDimensions().Width + UICommon.PADDING;

//			itemNameSearch.Left.Set(recipeButtonsRight + UICommon.PADDING, 0f);
//			itemNameSearch.Width.Set(-recipeButtonsRight - 2 * UICommon.PADDING, 1f);
//			itemNameSearch.Height.Set(0f, 1f);
//			recipesTopBar.Append(itemNameSearch);

//			ingredientsTopBar = new UIElement();
//			ingredientsTopBar.Width.Set(0f, 1f);
//			ingredientsTopBar.Height.Set(TOP_BAR_HEIGHT, 0f);
//			ingredientsTopBar.Top.Set(TOP_BAR_HEIGHT + UICommon.PADDING, 0f);
//			recipesPanel.Append(ingredientsTopBar);

//			InitFilterButtons();
//			float filterButtonsRight = filterButtons.GetDimensions().Width + UICommon.PADDING;
//			ingredientsTopBar.Append(filterButtons);
//			modNameSearch.Left.Set(filterButtonsRight + UICommon.PADDING, 0f);
//			modNameSearch.Width.Set(-filterButtonsRight - 2 * UICommon.PADDING, 1f);
//			modNameSearch.Height.Set(0f, 1f);
//			ingredientsTopBar.Append(modNameSearch);
//			stationText.Top.Set(sortButtons.Height.Pixels + filterButtons.Height.Pixels + UICommon.PADDING * 4, 0f);
//			recipesPanel.Append(stationText);

//			stationZone.Width.Set(0f, 1f);
//			stationZone.Top.Set(stationText.Top.Pixels + stationText.GetDimensions().Height + UICommon.PADDING * 3, 0f);
//			stationZone.SetDimensions(AVAILABLE_RECIPES_NUM_COLS, 1);
//			recipesPanel.Append(stationZone);

//			recipeText.Top.Set(stationZone.Top.Pixels + stationZone.ActualHeight + UICommon.PADDING * 2, 0f);
//			recipesPanel.Append(recipeText);

//			recipeZone.Width.Set(0f, 1f);
//			recipeZone.Top.Set(recipeText.Top.Pixels + recipeText.GetDimensions().Height + UICommon.PADDING * 3, 0f);
//			numRows = (recipes.Count + AVAILABLE_RECIPES_NUM_COLS - 1) / AVAILABLE_RECIPES_NUM_COLS;
//			recipeZone.Height.Set(panelHeight - recipeText.Top.Pixels - recipeText.GetDimensions().Height - UICommon.PADDING * 2, 0f);

//			recipesPanel.Append(recipeZone);

//			numRows = (recipes.Count + AVAILABLE_RECIPES_NUM_COLS - 1) / AVAILABLE_RECIPES_NUM_COLS;
//			displayRows = (int)recipeZone.GetDimensions().Height / ((int)itemSlotHeight + UICommon.PADDING * 2);
//			recipeZone.SetDimensions(AVAILABLE_RECIPES_NUM_COLS, displayRows);

//			int noDisplayRows = numRows - displayRows;
//			if (noDisplayRows < 0) {
//				noDisplayRows = 0;
//			}
//			scrollBarMaxViewSize = 1 + noDisplayRows;
//			scrollBar.Height.Set(displayRows * (itemSlotHeight + UICommon.PADDING), 0f);
//			scrollBar.Left.Set(-20f, 1f);
//			scrollBar.SetView(scrollBarViewSize, scrollBarMaxViewSize);
//			recipeZone.Append(scrollBar);

//			bottomBar.Width.Set(0f, 1f);
//			bottomBar.Height.Set(TOP_BAR_HEIGHT, 0f);
//			bottomBar.Top.Set(-TOP_BAR_HEIGHT, 1f);
//			recipesPanel.Append(bottomBar);

//			capacityText.Left.Set(6f, 0f);
//			capacityText.Top.Set(6f, 0f);

//			TEStorageHeart heart = GetHeart();
//			int numItems = 0;
//			int capacity = 0;
//			if (heart != null) {
//				foreach (TEAbstractStorageUnit abstractStorageUnit in heart.GetStorageUnits()) {
//					if (abstractStorageUnit is TEStorageUnit storageUnit) {
//						numItems += storageUnit.NumItems;
//						capacity += storageUnit.Capacity;
//					}
//				}
//			}
//			capacityText.SetText($"{numItems} / {capacity} {Locale.GetStr(Locale.C.ITEMS)}");
//			bottomBar.Append(capacityText);

//			ingredientsPanel.Append(recipePanelHeader);
//			ingredientText.Top.Set(30f, 0f);
//			ingredientsPanel.Append(ingredientText);

//			ingredientZone.SetDimensions(AVAILABLE_INGREDIENT_NUM_COLS, 2);
//			ingredientZone.Top.Set(54f, 0f);
//			ingredientZone.Width.Set(0f, 1f);
//			ingredientZone.Height.Set(60f, 0f);
//			ingredientsPanel.Append(ingredientZone);

//			requiresObjHeader.Top.Set(136f, 0f);
//			ingredientsPanel.Append(requiresObjHeader);
//			requiresObjText.Top.Set(160f, 0f);
//			ingredientsPanel.Append(requiresObjText);
//			storedItemsText.Top.Set(190f, 0f);
//			ingredientsPanel.Append(storedItemsText);

//			storageZone.Top.Set(214f, 0f);
//			storageZone.Width.Set(0f, 1f);
//			storageZone.Height.Set(-214f - 36, 1f);
//			ingredientsPanel.Append(storageZone);
//			numRows2 = (storageItems.Count + AVAILABLE_INGREDIENT_NUM_COLS - 1) / AVAILABLE_INGREDIENT_NUM_COLS;
//			displayRows2 = (int)storageZone.GetDimensions().Height / ((int)smallSlotHeight + UICommon.PADDING);
//			storageZone.SetDimensions(AVAILABLE_INGREDIENT_NUM_COLS, displayRows2);
//			int noDisplayRows2 = numRows2 - displayRows2;
//			if (noDisplayRows2 < 0) {
//				noDisplayRows2 = 0;
//			}
//			scrollBar2MaxViewSize = 1 + noDisplayRows2;
//			scrollBar2.Height.Set(displayRows2 * (smallSlotHeight + UICommon.PADDING), 0f);
//			scrollBar2.Left.Set(-20f, 1f);
//			scrollBar2.SetView(scrollBar2ViewSize, scrollBar2MaxViewSize);
//			storageZone.Append(scrollBar2);

//			craftButton.Top.Set(-32f, 1f);
//			craftButton.Width.Set(100f, 0f);
//			craftButton.Height.Set(24f, 0f);
//			craftButton.PaddingTop = 8f;
//			craftButton.PaddingBottom = 8f;
//			ingredientsPanel.Append(craftButton);

//			resultZone.SetDimensions(1, 1);
//			resultZone.Left.Set(-itemSlotWidth, 1f);
//			resultZone.Top.Set(-itemSlotHeight, 1f);
//			resultZone.Width.Set(itemSlotWidth, 0f);
//			resultZone.Height.Set(itemSlotHeight, 0f);
//			ingredientsPanel.Append(resultZone);

//			evnt.Set();
//		}

//		private static void InitLangStuff() {
//			if (itemNameSearch == null) {
//				itemNameSearch = new UISearchBar(null,Locale.Get(Locale.C.SAERCH_NAME));
//			}
//			if (modNameSearch == null) {
//				modNameSearch = new UISearchBar(null, Locale.Get(Locale.C.SAERCH_MOD));
//			}
//			if (stationText == null) {
//				stationText = new UIText(Locale.Get(Locale.C.CRAFT_STATIONS));
//			}
//			if (recipeText == null) {
//				recipeText = new UIText(Locale.Get(Locale.C.RECIPES));
//			}
//			if (capacityText == null) {
//				capacityText = new UIText(Locale.Get(Locale.C.ITEMS));
//			}
//			if (recipePanelHeader == null) {
//				recipePanelHeader = new UIText(Locale.Get(Locale.C.SEL_RECIPE));
//			}
//			if (ingredientText == null) {
//				ingredientText = new UIText(Locale.Get(Locale.C.INGREDS));
//			}
//			if (requiresObjHeader == null) {
//				requiresObjHeader = new UIText(Locale.Get(Locale.C.L.REQUIRED_OBJS));
//			}
//			if (requiresObjText == null) {
//				requiresObjText = new UIText("");
//			}
//			if (storedItemsText == null) {
//				storedItemsText = new UIText(Locale.Get(Locale.C.STORED_ITEMS));
//			}
//			if (craftButton == null) {
//				craftButton = new UITextPanel<LocalizedText>(Locale.Get(Locale.C.L.CRAFT));
//			}
//		}

//		private static void InitSortButtons() {
//			if (sortButtons == null) {
//				sortButtons = new UIButtonChoice(null, new Texture2D[]
//				{
//					Main.inventorySortTexture[0],
//					MagicStorage.Instance.GetTexture("Textures/Sorting/SortID"),
//					MagicStorage.Instance.GetTexture("Textures/Sorting/SortName")
//				},
//				new LocalizedText[]
//				{
//					Locale.Get(Locale.C.SORT_DEF),
//					Locale.Get(Locale.C.SORT_ID),
//					Locale.Get(Locale.C.SORT_NAME)
//				});
//			}
//		}

//		private static void InitRecipeButtons() {
//			if (recipeButtons == null) {
//				recipeButtons = new UIButtonChoice(null, new Texture2D[]
//				{
//					MagicStorage.Instance.GetTexture("Textures/Filtering/RecipeAvailable"),
//					MagicStorage.Instance.GetTexture("Textures/Filtering/RecipeAll")
//				},
//				new LocalizedText[]
//				{
//					Locale.Get(Locale.C.RECIPE_AVAIL),
//					Locale.Get(Locale.C.RECIPE_ALL),
//				});
//			}
//		}

//		private static void InitFilterButtons() {
//			if (filterButtons == null) {
//				filterButtons = new UIButtonChoice(null, new Texture2D[]
//				{
//					MagicStorage.Instance.GetTexture("Textures/Filtering/FilterAll"),
//					MagicStorage.Instance.GetTexture("Textures/Filtering/FilterMelee"),
//					MagicStorage.Instance.GetTexture("Textures/Filtering/FilterPickaxe"),
//					MagicStorage.Instance.GetTexture("Textures/Filtering/FilterArmor"),
//					MagicStorage.Instance.GetTexture("Textures/Filtering/FilterPotion"),
//					MagicStorage.Instance.GetTexture("Textures/Filtering/FilterTile"),
//					MagicStorage.Instance.GetTexture("Textures/Filtering/FilterMisc"),
//				},
//				new LocalizedText[]
//				{
//					Locale.Get(Locale.C.FILTER_ALL),
//					Locale.Get(Locale.C.FILTER_WEAP),
//					Locale.Get(Locale.C.FILTER_TOOL),
//					Locale.Get(Locale.C.FILTER_EQUIP),
//					Locale.Get(Locale.C.FILTER_POT),
//					Locale.Get(Locale.C.FILTER_TILE),
//					Locale.Get(Locale.C.FILTER_MISC),
//				});
//			}
//		}

//		internal static void Unload() {
//			sortButtons = null;
//			filterButtons = null;
//			recipeButtons = null;
//			evnt.Reset();
//		}

//		public static void Update(GameTime gameTime) {
//			try {
//				oldMouse = StorageGUI.oldMouse;
//				curMouse = StorageGUI.curMouse;
//				if (Main.playerInventory) {
//					(Point16 Pos, Type Tile) = Main.player[Main.myPlayer].GetModPlayer<StoragePlayer>().ViewingStorage();
//					if (Pos.X >= 0 && Tile == typeof(CraftingAccess) || Tile == typeof(CraftingStorageAccess)) {
//						if (curMouse.RightButton == ButtonState.Released) {
//							ResetSlotFocus();
//						}
//						if (recipesPanel != null) {
//							recipesPanel.Update(gameTime);
//						}
//						if (ingredientsPanel != null) {
//							ingredientsPanel.Update(gameTime);
//						}
//						UpdateRecipeText();
//						UpdateScrollBar();
//						UpdateCraftButton();
//					}
//				}
//				else {
//					scrollBarFocus = 0;
//					selectedRecipe = null;
//					craftTimer = 0;
//					maxCraftTimer = START_MAX_CRAFT_TIMER;
//					ResetSlotFocus();
//				}
//			}
//			catch (Exception e) { Main.NewTextMultiline(e.ToString()); }
//		}

//		public static void Draw(TEStorageHeart heart) {
//			try {
//				Player player = Main.player[Main.myPlayer];

//				StoragePlayer modPlayer = player.GetModPlayer<StoragePlayer>();
//				Initialize();
//				if (Main.mouseX > PANEL_LEFT && Main.mouseX < ingredientLeft + ingredientWidth && Main.mouseY > panelTop && Main.mouseY < panelTop + panelHeight) {
//					player.mouseInterface = true;
//					player.showItemIcon = false;
//					InterfaceHelper.HideItemIconCache();
//				}
//				recipesPanel.Draw(Main.spriteBatch);
//				ingredientsPanel.Draw(Main.spriteBatch);
//				Vector2 pos = recipeZone.GetDimensions().Position();
//				if (threadRunning) {
//					Utils.DrawBorderString(Main.spriteBatch, Locale.GetStr(Locale.C.LOADING), pos + new Vector2(8f, 8f), Color.White);
//				}
//				stationZone.DrawText();
//				recipeZone.DrawText();
//				ingredientZone.DrawText();
//				storageZone.DrawText();
//				resultZone.DrawText();
//				sortButtons.DrawText();
//				recipeButtons.DrawText();
//				filterButtons.DrawText();
//			}
//			catch (Exception e) { Main.NewTextMultiline(e.ToString()); }
//		}

//		private static Item GetStation(int slot, ref int context) {
//			Item[] stations = GetCraftingStations();
//			if (stations == null || slot >= stations.Length) {
//				return new Item();
//			}
//			return stations[slot];
//		}

//		private static Item GetRecipe(int slot, ref int context) {
//			if (threadRunning) {
//				return new Item();
//			}
//			int index = slot + AVAILABLE_RECIPES_NUM_COLS * (int)Math.Round(scrollBar.ViewPosition);
//			Item item = index < recipes.Count ? recipes[index].createItem : new Item();
//			if (!item.IsAir && recipes[index] == selectedRecipe) {
//				context = 6;
//			}
//			if (!item.IsAir && !recipeAvailable[index]) {
//				context = recipes[index] == selectedRecipe ? 4 : 3;
//			}
//			return item;
//		}

//		private static Item GetIngredient(int slot, ref int context) {
//			if (selectedRecipe == null || slot >= selectedRecipe.requiredItem.Length) {
//				return new Item();
//			}
//			Item item = selectedRecipe.requiredItem[slot].Clone();
//			if (selectedRecipe.anyWood && item.type == ItemID.Wood) {
//				item.SetNameOverride($"{Locale.GetStr(Locale.C.L.ANY)} {Lang.GetItemNameValue(ItemID.Wood)}");
//			}
//			if (selectedRecipe.anySand && item.type == ItemID.SandBlock) {
//				item.SetNameOverride($"{Locale.GetStr(Locale.C.L.ANY)} {Lang.GetItemNameValue(ItemID.SandBlock)}");
//			}
//			if (selectedRecipe.anyIronBar && item.type == ItemID.IronBar) {
//				item.SetNameOverride($"{Locale.GetStr(Locale.C.L.ANY)} {Lang.GetItemNameValue(ItemID.IronBar)}");
//			}
//			if (selectedRecipe.anyFragment && item.type == ItemID.FragmentSolar) {
//				item.SetNameOverride($"{Locale.GetStr(Locale.C.L.ANY)} {Locale.GetStr(Locale.C.L.FRAGMENT)}");
//			}
//			if (selectedRecipe.anyPressurePlate && item.type == ItemID.GrayPressurePlate) {
//				item.SetNameOverride($"{Locale.GetStr(Locale.C.L.ANY)} {Locale.GetStr(Locale.C.L.PPLATE)}");
//			}
//			if (selectedRecipe.ProcessGroupsForText(item.type, out string nameOverride)) {
//				item.SetNameOverride(nameOverride);
//			}
//			return item;
//		}

//		private static Item GetStorage(int slot, ref int context) {
//			int index = slot + AVAILABLE_INGREDIENT_NUM_COLS * (int)Math.Round(scrollBar2.ViewPosition);
//			Item item = index < storageItems.Count ? storageItems[index] : new Item();
//			if (blockStorageItems.Contains(new ItemData(item))) {
//				context = 3;
//			}
//			return item;
//		}

//		private static Item GetResult(int slot, ref int context) {
//			return slot == 0 && result != null ? result : new Item();
//		}

//		private static void UpdateRecipeText() {
//			if (selectedRecipe == null) {
//				requiresObjText.SetText("");
//			}
//			else {
//				bool isEmpty = true;
//				string text = "";
//				for (int k = 0; k < selectedRecipe.requiredTile.Length; k++) {
//					if (selectedRecipe.requiredTile[k] == -1) {
//						break;
//					}
//					if (!isEmpty) {
//						text += ", ";
//					}
//					text += Lang.GetMapObjectName(MapHelper.TileToLookup(selectedRecipe.requiredTile[k], 0));
//					isEmpty = false;
//				}
//				if (selectedRecipe.needWater) {
//					if (!isEmpty) {
//						text += ", ";
//					}
//					text += Locale.GetStr(Locale.C.L.WATER);
//					isEmpty = false;
//				}
//				if (selectedRecipe.needHoney) {
//					if (!isEmpty) {
//						text += ", ";
//					}
//					text += Locale.GetStr(Locale.C.L.HONEY);
//					isEmpty = false;
//				}
//				if (selectedRecipe.needLava) {
//					if (!isEmpty) {
//						text += ", ";
//					}
//					text += Locale.GetStr(Locale.C.L.LAVA);
//					isEmpty = false;
//				}
//				if (selectedRecipe.needSnowBiome) {
//					if (!isEmpty) {
//						text += ", ";
//					}
//					text += Locale.GetStr(Locale.C.L.COLD_WEATHER);
//					isEmpty = false;
//				}
//				if (isEmpty) {
//					text = Locale.GetStr(Locale.C.L.NONE);
//				}
//				requiresObjText.SetText(text);
//			}
//		}

//		private static void UpdateScrollBar() {
//			if (slotFocus) {
//				scrollBarFocus = 0;
//				return;
//			}
//			Rectangle dim = scrollBar.GetClippingRectangle(Main.spriteBatch);
//			Vector2 boxPos = new Vector2(dim.X, dim.Y + dim.Height * (scrollBar.ViewPosition / scrollBarMaxViewSize));
//			float boxWidth = 20f * Main.UIScale;
//			float boxHeight = dim.Height * (scrollBarViewSize / scrollBarMaxViewSize);
//			Rectangle dim2 = scrollBar2.GetClippingRectangle(Main.spriteBatch);
//			Vector2 box2Pos = new Vector2(dim2.X, dim2.Y + dim2.Height * (scrollBar2.ViewPosition / scrollBar2MaxViewSize));
//			float box2Height = dim2.Height * (scrollBar2ViewSize / scrollBar2MaxViewSize);
//			if (scrollBarFocus > 0) {
//				if (curMouse.LeftButton == ButtonState.Released) {
//					scrollBarFocus = 0;
//				}
//				else {
//					int difference = curMouse.Y - scrollBarFocusMouseStart;
//					if (scrollBarFocus == 1) {
//						scrollBar.ViewPosition = scrollBarFocusPositionStart + (float)difference / boxHeight;
//					}
//					else if (scrollBarFocus == 2) {
//						scrollBar2.ViewPosition = scrollBarFocusPositionStart + (float)difference / box2Height;
//					}
//				}
//			}
//			else if (MouseClicked) {
//				if (curMouse.X > boxPos.X && curMouse.X < boxPos.X + boxWidth && curMouse.Y > boxPos.Y - 3f && curMouse.Y < boxPos.Y + boxHeight + 4f) {
//					scrollBarFocus = 1;
//					scrollBarFocusMouseStart = curMouse.Y;
//					scrollBarFocusPositionStart = scrollBar.ViewPosition;
//				}
//				else if (curMouse.X > box2Pos.X && curMouse.X < box2Pos.X + boxWidth && curMouse.Y > box2Pos.Y - 3f && curMouse.Y < box2Pos.Y + box2Height + 4f) {
//					scrollBarFocus = 2;
//					scrollBarFocusMouseStart = curMouse.Y;
//					scrollBarFocusPositionStart = scrollBar2.ViewPosition;
//				}
//			}
//			if (scrollBarFocus == 0) {
//				int difference = oldMouse.ScrollWheelValue / 250 - curMouse.ScrollWheelValue / 250;
//				scrollBar.ViewPosition += difference;
//			}
//		}

//		private static void UpdateCraftButton() {
//			Rectangle dim = InterfaceHelper.GetFullRectangle(craftButton);
//			bool flag = false;
//			if (curMouse.X > dim.X && curMouse.X < dim.X + dim.Width && curMouse.Y > dim.Y && curMouse.Y < dim.Y + dim.Height) {
//				craftButton.BackgroundColor = new Color(73, 94, 171);
//				if (curMouse.LeftButton == ButtonState.Pressed) {
//					if (selectedRecipe != null && IsAvailable(selectedRecipe) && PassesBlock(selectedRecipe)) {
//						if (craftTimer <= 0) {
//							craftTimer = maxCraftTimer;
//							maxCraftTimer = maxCraftTimer * 3 / 4;
//							if (maxCraftTimer <= 0) {
//								maxCraftTimer = 1;
//							}
//							TryCraft();
//							RefreshItems();
//							StorageGUI.RefreshItems();
//							Main.PlaySound(SoundID.Grab, -1, -1, 1);
//						}
//						craftTimer--;
//						flag = true;
//					}
//				}
//			}
//			else {
//				craftButton.BackgroundColor = new Color(63, 82, 151) * 0.7f;
//			}
//			if (selectedRecipe == null || !IsAvailable(selectedRecipe) || !PassesBlock(selectedRecipe)) {
//				craftButton.BackgroundColor = new Color(30, 40, 100) * 0.7f;
//			}
//			if (!flag) {
//				craftTimer = 0;
//				maxCraftTimer = START_MAX_CRAFT_TIMER;
//			}
//		}

//		private static TEStorageHeart GetHeart() {
//			Player player = Main.player[Main.myPlayer];
//			StoragePlayer modPlayer = player.GetModPlayer<StoragePlayer>();
//			return modPlayer.GetStorageHeart();
//		}

//		private static TECraftingAccess GetCraftingEntity() {
//			Player player = Main.player[Main.myPlayer];
//			StoragePlayer modPlayer = player.GetModPlayer<StoragePlayer>();
//			return modPlayer.GetCraftingAccess();
//		}

//		private static Item[] GetCraftingStations() {
//			TECraftingAccess ent = GetCraftingEntity();
//			return ent?.stations;
//		}

//		public static void RefreshItems() {
//			items.Clear();
//			TEStorageHeart heart = GetHeart();
//			if (heart == null) {
//				return;
//			}
//			items.AddRange(ItemSorter.SortAndFilter(heart.GetStoredItems(), SortMode.Id, FilterMode.All, "", ""));
//			AnalyzeIngredients();
//			InitLangStuff();
//			InitSortButtons();
//			InitRecipeButtons();
//			InitFilterButtons();
//			SortMode sortMode;
//			switch (sortButtons.Choice) {
//				case 0:
//					sortMode = SortMode.Default;
//					break;
//				case 1:
//					sortMode = SortMode.Id;
//					break;
//				case 2:
//					sortMode = SortMode.Name;
//					break;
//				default:
//					sortMode = SortMode.Default;
//					break;
//			}
//			FilterMode filterMode;
//			switch (filterButtons.Choice) {
//				case 0:
//					filterMode = FilterMode.All;
//					break;
//				case 1:
//					filterMode = FilterMode.Weapons;
//					break;
//				case 2:
//					filterMode = FilterMode.Tools;
//					break;
//				case 3:
//					filterMode = FilterMode.Equipment;
//					break;
//				case 4:
//					filterMode = FilterMode.Potions;
//					break;
//				case 5:
//					filterMode = FilterMode.Placeables;
//					break;
//				case 6:
//					filterMode = FilterMode.Misc;
//					break;
//				default:
//					filterMode = FilterMode.All;
//					break;
//			}
//			RefreshStorageItems();
//			lock (threadLOCK) {
//				threadNeedsRestart = true;
//				threadSortMode = sortMode;
//				threadFilterMode = filterMode;
//				if (!threadRunning) {
//					threadRunning = true;
//					Thread thread = new Thread(RefreshRecipes);
//					thread.Start();
//				}
//			}
//		}

//		private static void RefreshRecipes() {
//			evnt.Wait();
//			while (true) {
//				try {
//					SortMode sortMode;
//					FilterMode filterMode;
//					lock (threadLOCK) {
//						threadNeedsRestart = false;
//						sortMode = threadSortMode;
//						filterMode = threadFilterMode;
//					}
//					var temp = ItemSorter.GetRecipes(sortMode, filterMode, modNameSearch.Text, itemNameSearch.Text);
//					threadRecipes.Clear();
//					threadRecipeAvailable.Clear();
//					try {
//						if (recipeButtons.Choice == 0) {
//							threadRecipes.AddRange(temp.Where(recipe => IsAvailable(recipe)));
//							threadRecipeAvailable.AddRange(threadRecipes.Select(recipe => true));
//						}
//						else {
//							threadRecipes.AddRange(temp);
//							threadRecipeAvailable.AddRange(threadRecipes.Select(recipe => IsAvailable(recipe)));
//						}
//					}
//					catch (InvalidOperationException) {
//					}
//					catch (KeyNotFoundException) {
//					}
//					catch (NullReferenceException) {
//						//UI Closed
//					}
//					lock (recipeLOCK) {
//						nextRecipes = new List<Recipe>();
//						nextRecipeAvailable = new List<bool>();
//						nextRecipes.AddRange(threadRecipes);
//						nextRecipeAvailable.AddRange(threadRecipeAvailable);

//					}
//					lock (threadLOCK) {
//						if (!threadNeedsRestart) {
//							threadRunning = false;
//							return;
//						}
//					}
//				}
//				catch (Exception e) { Main.NewTextMultiline(e.ToString()); }
//			}
//		}

//		private static void AnalyzeIngredients() {
//			Player player = Main.player[Main.myPlayer];
//			itemCounts.Clear();
//			if (adjTiles.Length != player.adjTile.Length) {
//				Array.Resize(ref adjTiles, player.adjTile.Length);
//			}
//			for (int k = 0; k < adjTiles.Length; k++) {
//				adjTiles[k] = false;
//			}
//			adjWater = false;
//			adjLava = false;
//			adjHoney = false;
//			zoneSnow = false;
//			alchemyTable = false;

//			foreach (Item item in items) {
//				if (itemCounts.ContainsKey(item.netID)) {
//					itemCounts[item.netID] += item.stack;
//				}
//				else {
//					itemCounts[item.netID] = item.stack;
//				}
//			}
//			foreach (Item item in GetCraftingStations()) {
//				if (item.createTile >= TileID.Dirt) {
//					adjTiles[item.createTile] = true;
//					if (item.createTile == TileID.GlassKiln || item.createTile == TileID.Hellforge || item.createTile == TileID.AdamantiteForge) {
//						adjTiles[TileID.Furnaces] = true;
//					}
//					if (item.createTile == TileID.AdamantiteForge) {
//						adjTiles[TileID.Hellforge] = true;
//					}
//					if (item.createTile == TileID.MythrilAnvil) {
//						adjTiles[TileID.Anvils] = true;
//					}
//					if (item.createTile == TileID.BewitchingTable || item.createTile == TileID.Tables2) {
//						adjTiles[TileID.Tables] = true;
//					}
//					if (item.createTile == TileID.AlchemyTable) {
//						adjTiles[TileID.Bottles] = true;
//						adjTiles[TileID.Tables] = true;
//						alchemyTable = true;
//					}
//					bool[] oldAdjTile = player.adjTile;
//					bool oldAdjWater = adjWater;
//					bool oldAdjLava = adjLava;
//					bool oldAdjHoney = adjHoney;
//					bool oldAlchemyTable = alchemyTable;
//					player.adjTile = adjTiles;
//					player.adjWater = false;
//					player.adjLava = false;
//					player.adjHoney = false;
//					player.alchemyTable = false;
//					TileLoader.AdjTiles(player, item.createTile);
//					if (player.adjWater) {
//						adjWater = true;
//					}
//					if (player.adjLava) {
//						adjLava = true;
//					}
//					if (player.adjHoney) {
//						adjHoney = true;
//					}
//					if (player.alchemyTable) {
//						alchemyTable = true;
//					}
//					player.adjTile = oldAdjTile;
//					player.adjWater = oldAdjWater;
//					player.adjLava = oldAdjLava;
//					player.adjHoney = oldAdjHoney;
//					player.alchemyTable = oldAlchemyTable;
//				}
//				if (item.type == ItemID.WaterBucket || item.type == ItemID.BottomlessBucket) {
//					adjWater = true;
//				}
//				if (item.type == ItemID.LavaBucket) {
//					adjLava = true;
//				}
//				if (item.type == ItemID.HoneyBucket) {
//					adjHoney = true;
//				}
//				if (item.type == MagicStorage.Instance.ItemType(nameof(SnowBiomeEmulator))) {
//					zoneSnow = true;
//				}
//			}
//			adjTiles[MagicStorage.Instance.TileType(nameof(Components.CraftingAccess))] = true;
//		}

//		private static bool IsAvailable(Recipe recipe) {
//			foreach (int tile in recipe.requiredTile) {
//				if (tile == -1) {
//					break;
//				}
//				if (!adjTiles[tile]) {
//					return false;
//				}
//			}
//			foreach (Item ingredient in recipe.requiredItem) {
//				if (ingredient.type == ItemID.None) {
//					break;
//				}
//				int stack = ingredient.stack;
//				bool useRecipeGroup = false;
//				foreach (int type in itemCounts.Keys) {
//					if (RecipeGroupMatch(recipe, type, ingredient.type)) {
//						stack -= itemCounts[type];
//						useRecipeGroup = true;
//					}
//				}
//				if (!useRecipeGroup && itemCounts.ContainsKey(ingredient.netID)) {
//					stack -= itemCounts[ingredient.netID];
//				}
//				if (stack > 0) {
//					return false;
//				}
//			}
//			if (recipe.needWater && !adjWater && !adjTiles[TileID.Sinks]) {
//				return false;
//			}
//			if (recipe.needLava && !adjLava) {
//				return false;
//			}
//			if (recipe.needHoney && !adjHoney) {
//				return false;
//			}
//			if (recipe.needSnowBiome && !zoneSnow) {
//				return false;
//			}
//			try {
//				BlockRecipes.active = false;
//				if (!RecipeHooks.RecipeAvailable(recipe)) {
//					return false;
//				}
//			}
//			finally {
//				BlockRecipes.active = true;
//			}
//			return true;
//		}

//		private static bool PassesBlock(Recipe recipe) {
//			foreach (Item ingredient in recipe.requiredItem) {
//				if (ingredient.type == ItemID.None) {
//					break;
//				}
//				int stack = ingredient.stack;
//				bool useRecipeGroup = false;
//				foreach (Item item in storageItems) {
//					ItemData data = new ItemData(item);
//					if (!blockStorageItems.Contains(data) && RecipeGroupMatch(recipe, item.netID, ingredient.type)) {
//						stack -= item.stack;
//						useRecipeGroup = true;
//					}
//				}
//				if (!useRecipeGroup) {
//					foreach (Item item in storageItems) {
//						ItemData data = new ItemData(item);
//						if (!blockStorageItems.Contains(data) && item.netID == ingredient.netID) {
//							stack -= item.stack;
//						}
//					}
//				}
//				if (stack > 0) {
//					return false;
//				}
//			}
//			return true;
//		}

//		private static void RefreshStorageItems() {
//			storageItems.Clear();
//			result = null;
//			if (selectedRecipe != null) {
//				foreach (Item item in items) {
//					for (int k = 0; k < selectedRecipe.requiredItem.Length; k++) {
//						if (selectedRecipe.requiredItem[k].type == ItemID.None) {
//							break;
//						}
//						if (item.type == selectedRecipe.requiredItem[k].type || RecipeGroupMatch(selectedRecipe, selectedRecipe.requiredItem[k].type, item.type)) {
//							storageItems.Add(item);
//						}
//					}
//					if (item.type == selectedRecipe.createItem.type) {
//						result = item;
//					}
//				}
//				if (result == null) {
//					result = new Item();
//					result.SetDefaults(selectedRecipe.createItem.type);
//					result.stack = 0;
//				}
//			}
//		}

//		private static bool RecipeGroupMatch(Recipe recipe, int type1, int type2) {
//			return recipe.useWood(type1, type2) || recipe.useSand(type1, type2) || recipe.useIronBar(type1, type2) || recipe.useFragment(type1, type2) || recipe.AcceptedByItemGroups(type1, type2) || recipe.usePressurePlate(type1, type2);
//		}

//		private static void HoverStation(int slot, ref int hoverSlot) {
//			TECraftingAccess ent = GetCraftingEntity();
//			if (ent == null || slot >= ent.stations.Length) {
//				return;
//			}

//			Player player = Main.player[Main.myPlayer];
//			if (MouseClicked) {
//				bool changed = false;
//				if (!ent.stations[slot].IsAir && ItemSlot.ShiftInUse) {
//					Item result = player.GetItem(Main.myPlayer, DoWithdraw(slot), false, true);
//					if (!result.IsAir && Main.mouseItem.IsAir) {
//						Main.mouseItem = result;
//						result = new Item();
//					}
//					if (!result.IsAir && Main.mouseItem.type == result.type && Main.mouseItem.stack < Main.mouseItem.maxStack) {
//						Main.mouseItem.stack += result.stack;
//						result = new Item();
//					}
//					if (!result.IsAir) {
//						player.QuickSpawnClonedItem(result);
//					}
//					changed = true;
//				}
//				else if (player.itemAnimation == 0 && player.itemTime == 0) {
//					int oldType = Main.mouseItem.type;
//					int oldStack = Main.mouseItem.stack;
//					Main.mouseItem = DoStationSwap(Main.mouseItem, slot);
//					if (oldType != Main.mouseItem.type || oldStack != Main.mouseItem.stack) {
//						changed = true;
//					}
//				}
//				if (changed) {
//					RefreshItems();
//					StorageGUI.RefreshItems();
//					Main.PlaySound(SoundID.Grab, -1, -1, 1);
//				}
//			}

//			hoverSlot = slot;
//		}

//		private static void HoverRecipe(int slot, ref int hoverSlot) {
//			int visualSlot = slot;
//			slot += AVAILABLE_RECIPES_NUM_COLS * (int)Math.Round(scrollBar.ViewPosition);
//			if (slot < recipes.Count) {
//				if (MouseClicked) {
//					selectedRecipe = recipes[slot];
//					RefreshStorageItems();
//					blockStorageItems.Clear();
//				}
//				hoverSlot = visualSlot;
//			}
//		}

//		private static void HoverItem(int slot, ref int hoverSlot) {
//			hoverSlot = slot;
//		}

//		private static void HoverStorage(int slot, ref int hoverSlot) {
//			int visualSlot = slot;
//			slot += AVAILABLE_INGREDIENT_NUM_COLS * (int)Math.Round(scrollBar2.ViewPosition);
//			if (slot < storageItems.Count) {
//				if (MouseClicked) {
//					ItemData data = new ItemData(storageItems[slot]);
//					if (blockStorageItems.Contains(data)) {
//						blockStorageItems.Remove(data);
//					}
//					else {
//						blockStorageItems.Add(data);
//					}
//				}
//				hoverSlot = visualSlot;
//			}
//		}

//		private static void HoverResult(int slot, ref int hoverSlot) {
//			if (slot != 0) {
//				return;
//			}

//			Player player = Main.player[Main.myPlayer];
//			if (MouseClicked) {
//				bool changed = false;
//				if (!Main.mouseItem.IsAir && player.itemAnimation == 0 && player.itemTime == 0 && result != null && Main.mouseItem.type == result.type) {
//					if (TryDepositResult(Main.mouseItem)) {
//						changed = true;
//					}
//				}
//				else if (Main.mouseItem.IsAir && result != null && !result.IsAir) {
//					Item toWithdraw = result.Clone();
//					if (toWithdraw.stack > toWithdraw.maxStack) {
//						toWithdraw.stack = toWithdraw.maxStack;
//					}
//					Main.mouseItem = DoWithdrawResult(toWithdraw, ItemSlot.ShiftInUse);
//					if (ItemSlot.ShiftInUse) {
//						Main.mouseItem = player.GetItem(Main.myPlayer, Main.mouseItem, false, true);
//					}
//					changed = true;
//				}
//				if (changed) {
//					RefreshItems();
//					StorageGUI.RefreshItems();
//					Main.PlaySound(SoundID.Grab, -1, -1, 1);
//				}
//			}

//			if (curMouse.RightButton == ButtonState.Pressed && oldMouse.RightButton == ButtonState.Released && result != null && !result.IsAir && (Main.mouseItem.IsAir || ItemData.Matches(Main.mouseItem, items[slot]) && Main.mouseItem.stack < Main.mouseItem.maxStack)) {
//				slotFocus = true;
//			}

//			hoverSlot = slot;

//			if (slotFocus) {
//				SlotFocusLogic();
//			}
//		}

//		private static void SlotFocusLogic() {
//			if (result == null || result.IsAir || (!Main.mouseItem.IsAir && (!ItemData.Matches(Main.mouseItem, result) || Main.mouseItem.stack >= Main.mouseItem.maxStack))) {
//				ResetSlotFocus();
//			}
//			else {
//				if (rightClickTimer <= 0) {
//					rightClickTimer = maxRightClickTimer;
//					maxRightClickTimer = maxRightClickTimer * 3 / 4;
//					if (maxRightClickTimer <= 0) {
//						maxRightClickTimer = 1;
//					}
//					Item toWithdraw = result.Clone();
//					toWithdraw.stack = 1;
//					Item withdrawn = DoWithdrawResult(toWithdraw);
//					if (Main.mouseItem.IsAir) {
//						Main.mouseItem = withdrawn;
//					}
//					else {
//						Main.mouseItem.stack += withdrawn.stack;
//					}
//					Main.soundInstanceMenuTick.Stop();
//					Main.soundInstanceMenuTick = Main.soundMenuTick.CreateInstance();
//					Main.PlaySound(SoundID.MenuTick, -1, -1, 1);
//					RefreshItems();
//					StorageGUI.RefreshItems();
//				}
//				rightClickTimer--;
//			}
//		}

//		private static void ResetSlotFocus() {
//			slotFocus = false;
//			rightClickTimer = 0;
//			maxRightClickTimer = SMART_MAX_CLICK_TIMER;
//		}

//		private static Item DoWithdraw(int slot) {
//			TECraftingAccess access = GetCraftingEntity();
//			if (Main.netMode == NetmodeID.SinglePlayer) {
//				Item result = access.TryWithdrawStation(slot);
//				RefreshItems();
//				StorageGUI.RefreshItems();
//				return result;
//			}
//			else {
//				NetHelper.SendWithdrawStation(access.ID, slot);
//				return new Item();
//			}
//		}

//		private static Item DoStationSwap(Item item, int slot) {
//			TECraftingAccess access = GetCraftingEntity();
//			if (Main.netMode == NetmodeID.SinglePlayer) {
//				Item result = access.DoStationSwap(item, slot);
//				RefreshItems();
//				StorageGUI.RefreshItems();
//				return result;
//			}
//			else {
//				NetHelper.SendStationSlotClick(access.ID, item, slot);
//				return new Item();
//			}
//		}

//		private static void TryCraft() {
//			List<Item> availableItems = new List<Item>(storageItems.Where(item => !blockStorageItems.Contains(new ItemData(item))).Select(item => item.Clone()));
//			List<Item> toWithdraw = new List<Item>();
//			for (int k = 0; k < selectedRecipe.requiredItem.Length; k++) {
//				Item item = selectedRecipe.requiredItem[k];
//				if (item.type == ItemID.None) {
//					break;
//				}
//				int stack = item.stack;
//				if (selectedRecipe is ModRecipe modRecipe) {
//					stack = modRecipe.ConsumeItem(item.type, item.stack);
//				}
//				if (selectedRecipe.alchemy && alchemyTable) {
//					int save = 0;
//					for (int j = 0; j < stack; j++) {
//						if (Main.rand.Next(3) == 0) {
//							save++;
//						}
//					}
//					stack -= save;
//				}
//				if (stack > 0) {
//					foreach (Item tryItem in availableItems) {
//						if (item.type == tryItem.type || RecipeGroupMatch(selectedRecipe, item.type, tryItem.type)) {
//							if (tryItem.stack > stack) {
//								Item temp = tryItem.Clone();
//								temp.stack = stack;
//								toWithdraw.Add(temp);
//								tryItem.stack -= stack;
//								stack = 0;
//							}
//							else {
//								toWithdraw.Add(tryItem.Clone());
//								stack -= tryItem.stack;
//								tryItem.stack = 0;
//								tryItem.type = ItemID.None;
//							}
//						}
//					}
//				}
//			}
//			Item resultItem = selectedRecipe.createItem.Clone();
//			resultItem.Prefix(-1);

//			RecipeHooks.OnCraft(resultItem, selectedRecipe);
//			ItemLoader.OnCraft(resultItem, selectedRecipe);

//			if (Main.netMode == NetmodeID.SinglePlayer) {
//				foreach (Item item in DoCraft(GetHeart(), toWithdraw, resultItem)) {
//					Main.player[Main.myPlayer].QuickSpawnClonedItem(item, item.stack);
//				}
//			}
//			else if (Main.netMode == NetmodeID.MultiplayerClient) {
//				NetHelper.SendCraftRequest(GetHeart().ID, toWithdraw, resultItem);
//			}
//		}

//		internal static List<Item> DoCraft(TEStorageHeart heart, List<Item> toWithdraw, Item result) {
//			List<Item> items = new List<Item>();
//			foreach (Item tryWithdraw in toWithdraw) {
//				Item withdrawn = heart.TryWithdraw(tryWithdraw);
//				if (!withdrawn.IsAir) {
//					items.Add(withdrawn);
//				}
//				if (withdrawn.stack < tryWithdraw.stack) {
//					for (int k = 0; k < items.Count; k++) {
//						heart.DepositItem(items[k]);
//						if (items[k].IsAir) {
//							items.RemoveAt(k);
//							k--;
//						}
//					}
//					return items;
//				}
//			}
//			items.Clear();
//			heart.DepositItem(result);
//			if (!result.IsAir) {
//				items.Add(result);
//			}
//			return items;
//		}

//		private static bool TryDepositResult(Item item) {
//			int oldStack = item.stack;
//			DoDepositResult(item);
//			return oldStack != item.stack;
//		}

//		private static void DoDepositResult(Item item) {
//			TEStorageHeart heart = GetHeart();
//			if (Main.netMode == NetmodeID.SinglePlayer) {
//				heart.DepositItem(item);
//			}
//			else {
//				NetHelper.SendDeposit(heart.ID, item);
//				item.SetDefaults(0, true);
//			}
//		}

//		private static Item DoWithdrawResult(Item item, bool toInventory = false) {
//			TEStorageHeart heart = GetHeart();
//			if (Main.netMode == NetmodeID.SinglePlayer) {
//				return heart.TryWithdraw(item);
//			}
//			else {
//				NetHelper.SendWithdraw(heart.ID, item, toInventory);
//				return new Item();
//			}
//		}
//	}
//}