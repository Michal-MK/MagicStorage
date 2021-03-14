using MagicStorage.Components;
using MagicStorage.GUI.Elements;
using MagicStorage.Items;
using MagicStorage.Sorting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI;
using TCraftingAccess = MagicStorage.Components.TCraftingAccess;
using TCraftingStorageAccess = MagicStorage.Components.TCraftingStorageAccess;
using TCraftingTileSocket = MagicStorage.Components.TCraftingTileSocket;

namespace MagicStorage.GUI {
	public class CraftingGUINew : GUIBase {

		public const int AVAILABLE_RECIPES_NUM_COLS = 10;
		public const int AVAILABLE_INGREDIENT_NUM_COLS = 7;
		public const float RECIPES_SCALE = 1f;
		public const float INGREDIENTS_SCALE = 0.85f;
		public const float TOP_BAR_HEIGHT = 32f;

		public const int START_MAX_CRAFT_TIMER = 20;
		public const int SMART_MAX_CLICK_TIMER = 20;

		// The left offset from the screen
		public const float PANEL_LEFT = 20;

		private UIPanelWithChildren recipesPanel;
		private float panelTop;
		private float panelWidth;
		private float panelHeight;

		private UIElement recipesTopBar;
		private UIButtonChoice sortButtons;
		private UIButtonChoice recipeButtons;
		public UISearchBar nameSearchBar;

		private UIElement ingredientsTopBar;
		private UIButtonChoice filterButtons;
		public UISearchBar modSearchBar;

		private UIText stationText;
		private UISlotZone stationZone;

		private UIText recipeText;
		private UISlotZone recipeZone;

		private UIScrollbar recipeZoneScrollBar = new UIScrollbar();
		private int recipeScrollBarFocus = 0;
		private int recipeScrollBarFocusMouseStart;
		private float recipeScrollBarFocusPositionStart;
		private const float RECIPE_SCROLLBAR_VIEW_SIZE = 1f;
		private float recipeScrollBarMaxViewSize = 2f;

		private readonly List<Item> items = new List<Item>();
		private readonly Dictionary<int, int> itemCounts = new Dictionary<int, int>();
		private bool[] adjTiles = new bool[TileLoader.TileCount];
		private bool adjWater = false;
		private bool adjLava = false;
		private bool adjHoney = false;
		private bool zoneSnow = false;
		private bool alchemyTable = false;

		private List<Recipe> recipes = new List<Recipe>();
		private List<bool> recipeAvailable = new List<bool>();
		private Recipe selectedRecipe = null;
		private int numRows;
		private int displayRows;
		private bool slotFocus = false;

		private UIElement bottomBar;
		private UIText capacityText;

		private UIPanelWithChildren ingredientsPanel;
		private float ingredientTop;
		private float ingredientLeft;
		private float ingredientWidth;
		private float ingredientHeight;

		private UIText recipePanelHeader;
		private UIText ingredientText;

		private UISlotZone ingredientZone;
		private UIText requiresObjHeader;
		private UIText requiresObjText;

		private UIText storedItemsText;

		private UISlotZone availableItemsZone;
		private int availableItemsRows;
		private int availableItemsDisplayRows;

		private readonly List<Item> storageItems = new List<Item>();
		private readonly List<ItemData> blockStorageItems = new List<ItemData>();

		private UIScrollbar availableItemsScrollBar = new UIScrollbar();
		private const float AVAILABLE_ITEMS_SCROLLBAR_VIEW_SIZE = 1f;
		private float availableItemsScrollBarMaxViewSize = 2f;

		private UITextPanel<LocalizedText> craftButton;
		private Item result = null;
		private readonly UISlotZone resultZone;

		private int craftTimer = 0;
		private int maxCraftTimer = START_MAX_CRAFT_TIMER;
		private int rightClickTimer = 0;
		private int maxRightClickTimer = SMART_MAX_CLICK_TIMER;

		private readonly object threadLOCK = new object();
		private readonly object recipeLOCK = new object();
		private readonly object itemsLOCK = new object();

		private bool threadRunning = false;
		internal bool threadNeedsRestart = false;
		private SortMode threadSortMode;
		private FilterMode threadFilterMode;
		private List<Recipe> threadRecipes = new List<Recipe>();
		private List<bool> threadRecipeAvailable = new List<bool>();
		private List<Recipe> nextRecipes = new List<Recipe>();
		private List<bool> nextRecipeAvailable = new List<bool>();

		IEnumerable<TCraftingTileSocket> craftingTileSocketItems;

		public float ActualWidth => panelWidth;
		public float ActualHeight => panelHeight;

		public bool HasRecipe => Active && selectedRecipe != null;

		private readonly ManualResetEventSlim evnt = new ManualResetEventSlim();

		public CraftingGUINew() {
			stationZone = new UISlotZone(this, HoverStation, GetStation, RECIPES_SCALE);
			recipeZone = new UISlotZone(this, HoverRecipe, GetRecipe, RECIPES_SCALE);
			availableItemsZone = new UISlotZone(this, HoverStorage, GetStorage, INGREDIENTS_SCALE);
			ingredientZone = new UISlotZone(this, HoverItem, GetIngredient, INGREDIENTS_SCALE);
			resultZone = new UISlotZone(this, HoverResult, GetResult, RECIPES_SCALE);
		}

		public void Initialize() {
			lock (recipeLOCK) {
				recipes = nextRecipes;
				recipeAvailable = nextRecipeAvailable;
			}

			float itemSlotWidth = Main.inventoryBackTexture.Width * RECIPES_SCALE;
			float itemSlotHeight = Main.inventoryBackTexture.Height * RECIPES_SCALE;

			panelTop = Main.instance.invBottom + 60;
			panelHeight = Main.screenHeight - panelTop - 60;

			recipesPanel = new UIPanelWithChildren();
			panelWidth = recipeZone.ActualWidth + recipesPanel.PaddingRight + recipesPanel.PaddingLeft + 20 + UICommon.PADDING;
			recipesPanel.Left.Set(PANEL_LEFT, 0f);
			recipesPanel.Top.Set(panelTop, 0f);
			recipesPanel.Width.Set(panelWidth, 0f);
			recipesPanel.Height.Set(panelHeight, 0f);
			recipesPanel.Recalculate();

			ingredientsPanel = new UIPanelWithChildren();
			ingredientTop = panelTop;
			ingredientLeft = PANEL_LEFT + panelWidth;
			ingredientWidth = ingredientZone.ActualWidth + ingredientsPanel.PaddingRight + ingredientsPanel.PaddingLeft + 20 + UICommon.PADDING;

			ingredientsPanel.Left.Set(ingredientLeft, 0f);
			ingredientsPanel.Top.Set(ingredientTop, 0f);
			ingredientsPanel.Width.Set(ingredientWidth, 0f);
			ingredientsPanel.Recalculate();

			recipesTopBar = new UIElement();
			recipesTopBar.Width.Set(0f, 1f);
			recipesTopBar.Height.Set(TOP_BAR_HEIGHT, 0f);
			recipesPanel.Append(recipesTopBar);

			InitSortButtons();

			recipesTopBar.Append(sortButtons);
			float sortButtonsRight = sortButtons.GetDimensions().Width + UICommon.PADDING;
			InitRecipeButtons();
			float recipeButtonsLeft = sortButtonsRight + UIButtonChoice.BUTTON_SIZE * 2 + 5 * UICommon.PADDING;
			recipeButtons.Left.Set(recipeButtonsLeft, 0f);
			recipesTopBar.Append(recipeButtons);
			float recipeButtonsRight = recipeButtonsLeft + recipeButtons.GetDimensions().Width + UICommon.PADDING;

			nameSearchBar.Left.Set(recipeButtonsRight + UICommon.PADDING, 0f);
			nameSearchBar.Width.Set(-recipeButtonsRight - 1 * UICommon.PADDING, 1f);
			nameSearchBar.Height.Set(0f, 1f);
			recipesTopBar.Append(nameSearchBar);

			ingredientsTopBar = new UIElement();
			ingredientsTopBar.Width.Set(0f, 1f);
			ingredientsTopBar.Height.Set(TOP_BAR_HEIGHT, 0f);
			ingredientsTopBar.Top.Set(TOP_BAR_HEIGHT + UICommon.PADDING, 0f);
			recipesPanel.Append(ingredientsTopBar);

			InitFilterButtons();
			float filterButtonsRight = filterButtons.Width.Pixels + UICommon.PADDING;
			ingredientsTopBar.Append(filterButtons);
			modSearchBar.Left.Set(filterButtonsRight + UICommon.PADDING, 0f);
			modSearchBar.Width.Set(-filterButtonsRight - 1 * UICommon.PADDING, 1f);
			modSearchBar.Height.Set(0f, 1f);
			ingredientsTopBar.Append(modSearchBar);
			stationText.Top.Set(sortButtons.Height.Pixels + filterButtons.Height.Pixels + UICommon.PADDING * 4, 0f);
			if (!MagicStorage.Instance.guiM.StorageGUI.Active) {
				recipesPanel.Append(stationText);
			}

			stationZone.Width.Set(0f, 1f);
			stationZone.Height.Set(stationZone.ActualHeight, 0);
			stationZone.Top.Set(stationText.Top.Pixels + stationText.GetDimensions().Height + UICommon.PADDING * 3, 0f);
			stationZone.SetDimensions(10, 1);
			if (!MagicStorage.Instance.guiM.StorageGUI.Active) {
				recipesPanel.Append(stationZone);
			}
			recipeText.Top.Set(stationZone.Top.Pixels + stationZone.ActualHeight + UICommon.PADDING * 2, 0f);
			if (!MagicStorage.Instance.guiM.StorageGUI.Active) {
				recipesPanel.Append(recipeText);
			}
			recipeZone.Width.Set(0f, 1f);
			if (MagicStorage.Instance.guiM.StorageGUI.Active) {
				recipeZone.Top.Set(recipesTopBar.Top.Pixels + recipesTopBar.GetDimensions().Height * 2 + UICommon.PADDING * 3, 0f);
			}
			else {
				recipeZone.Top.Set(recipeText.Top.Pixels + recipeText.GetDimensions().Height * 2 + UICommon.PADDING * 3, 0f);
			}
			numRows = (recipes.Count + AVAILABLE_RECIPES_NUM_COLS - 1) / AVAILABLE_RECIPES_NUM_COLS;
			recipeZone.Height.Set(recipeZone.ActualHeight, 0f);

			recipesPanel.Append(recipeZone);
			recipeZone.RemoveAllChildren();

			numRows = (recipes.Count + AVAILABLE_RECIPES_NUM_COLS - 1) / AVAILABLE_RECIPES_NUM_COLS;
			displayRows = Math.Min(recipeZone.MaxRows, 8);
			recipeZone.SetDimensions(AVAILABLE_RECIPES_NUM_COLS, displayRows);

			int noDisplayRows = numRows - displayRows;
			if (noDisplayRows < 0) {
				noDisplayRows = 0;
			}
			recipeScrollBarMaxViewSize = 1 + noDisplayRows;
			recipeZoneScrollBar.Top.Set(4, 0);

			recipeZoneScrollBar.Height.Set(recipeZone.ActualHeight - 8 - 4, 0f);
			recipeZoneScrollBar.Left.Set(-20f, 1f);
			recipeZoneScrollBar.SetView(RECIPE_SCROLLBAR_VIEW_SIZE, recipeScrollBarMaxViewSize);
			recipeZone.Append(recipeZoneScrollBar);

			bottomBar = new UIElement();
			bottomBar.Width.Set(0f, 1f);
			bottomBar.Height.Set(TOP_BAR_HEIGHT, 0f);
			bottomBar.Top.Set(recipeZone.Top.Pixels + recipeZone.ActualHeight, 0f);
			recipesPanel.Append(bottomBar);

			capacityText.Left.Set(6f, 0f);
			capacityText.Top.Set(6f, 0f);

			TEStorageHeart heart = GetHeart();
			int numItems = 0;
			int capacity = 0;
			if (heart != null) {
				foreach (TEAbstractStorageUnit abstractStorageUnit in heart.GetStorageUnits()) {
					if (abstractStorageUnit is TEStorageUnit storageUnit) {
						numItems += storageUnit.NumItems;
						capacity += storageUnit.Capacity;
					}
				}
			}
			capacityText.SetText($"{numItems} / {capacity} {Locale.GetStr(Locale.C.ITEMS)}");
			bottomBar.Append(capacityText);

			ingredientsPanel.Append(ingredientText);

			ingredientZone.SetDimensions(AVAILABLE_INGREDIENT_NUM_COLS, 2);
			ingredientZone.Top.Set(ingredientText.Top.Pixels + ingredientText.GetDimensions().Height + UICommon.PADDING * 3, 0f);
			ingredientZone.Width.Set(0f, 1f);
			ingredientsPanel.Append(ingredientZone);

			requiresObjHeader.Top.Set(ingredientZone.Top.Pixels + ingredientZone.ActualHeight + UICommon.PADDING * 3, 0f);
			ingredientsPanel.Append(requiresObjHeader);
			requiresObjText.Top.Set(requiresObjHeader.Top.Pixels + requiresObjHeader.GetDimensions().Height + UICommon.PADDING * 3, 0f);
			ingredientsPanel.Append(requiresObjText);

			storedItemsText.Top.Set(requiresObjText.Top.Pixels + requiresObjText.GetDimensions().Height + UICommon.PADDING * 5, 0f);
			ingredientsPanel.Append(storedItemsText);

			availableItemsZone.Top.Set(storedItemsText.Top.Pixels + storedItemsText.GetDimensions().Height + UICommon.PADDING * 3, 0f);
			availableItemsZone.Width.Set(0f, 1f);
			availableItemsZone.Height.Set(availableItemsZone.ActualHeight, 0);
			ingredientsPanel.Append(availableItemsZone);
			availableItemsRows = (storageItems.Count + AVAILABLE_INGREDIENT_NUM_COLS - 1) / AVAILABLE_INGREDIENT_NUM_COLS;
			availableItemsDisplayRows = Math.Min(availableItemsZone.MaxRows, 3);
			availableItemsZone.SetDimensions(7, availableItemsDisplayRows);
			int noDisplayRows2 = availableItemsRows - availableItemsDisplayRows;
			if (noDisplayRows2 < 0) {
				noDisplayRows2 = 0;
			}

			availableItemsScrollBarMaxViewSize = 1 + noDisplayRows2;
			availableItemsScrollBar.Height.Set(availableItemsZone.ActualHeight - 4, 0f);
			availableItemsScrollBar.Left.Set(-20f, 1f);
			availableItemsScrollBar.SetView(AVAILABLE_ITEMS_SCROLLBAR_VIEW_SIZE, availableItemsScrollBarMaxViewSize);
			availableItemsZone.Append(availableItemsScrollBar);
			availableItemsZone.Recalculate();
			ingredientsPanel.Recalculate();

			craftButton.Top.Set(availableItemsZone.Top.Pixels + availableItemsZone.ActualHeight + 20 + 6, 0f);
			craftButton.Width.Set(-resultZone.ActualWidth, 1f);
			craftButton.Height.Set(24f, 0f);
			ingredientsPanel.Append(craftButton);

			resultZone.SetDimensions(1, 1);
			resultZone.Left.Set(-itemSlotWidth, 1f);
			resultZone.Top.Set(availableItemsZone.Top.Pixels + availableItemsZone.ActualHeight + 20, 0f);
			resultZone.Width.Set(itemSlotWidth, 0f);
			resultZone.Height.Set(itemSlotHeight, 0f);
			ingredientsPanel.Append(resultZone);

			ingredientHeight = resultZone.Top.Pixels + resultZone.ActualHeight + 4 * 2 + 12;
			ingredientsPanel.Height.Set(ingredientHeight, 0f);
			ingredientsPanel.Recalculate();

			panelHeight = bottomBar.Top.Pixels + bottomBar.Height.Pixels + 12 * 2 - 4;
			recipesPanel.Height.Set(panelHeight, 0);
			recipesPanel.Recalculate();
			evnt.Set();
		}

		#region UI Initialization code

		private void InitLangStuff() {
			if (nameSearchBar != null) return;
			nameSearchBar = new UISearchBar(this, Locale.Get(Locale.C.SAERCH_NAME));
			modSearchBar = new UISearchBar(this, Locale.Get(Locale.C.SAERCH_MOD));
			stationText = new UIText(Locale.Get(Locale.C.CRAFT_STATIONS));
			recipeText = new UIText(Locale.Get(Locale.C.RECIPES));
			capacityText = new UIText(Locale.Get(Locale.C.ITEMS));
			recipePanelHeader = new UIText(Locale.Get(Locale.C.SEL_RECIPE));
			ingredientText = new UIText(Locale.Get(Locale.C.INGREDS));
			requiresObjHeader = new UIText(Locale.Get(Locale.C.L.REQUIRED_OBJS));
			requiresObjText = new UIText("");
			storedItemsText = new UIText(Locale.Get(Locale.C.STORED_ITEMS));
			craftButton = new UITextPanel<LocalizedText>(Locale.Get(Locale.C.L.CRAFT));
		}

		private void InitSortButtons() {
			if (sortButtons != null) return;
			sortButtons = new UIButtonChoice(this, new Texture2D[] {
				Main.inventorySortTexture[0],
				MagicStorage.Instance.GetTexture("Textures/Sorting/SortID"),
				MagicStorage.Instance.GetTexture("Textures/Sorting/SortName")
			}, new LocalizedText[] {
				Locale.Get(Locale.C.SORT_DEF),
				Locale.Get(Locale.C.SORT_ID),
				Locale.Get(Locale.C.SORT_NAME)
			});
		}

		private void InitRecipeButtons() {
			if (recipeButtons != null) return;
			recipeButtons = new UIButtonChoice(this, new Texture2D[] {
				MagicStorage.Instance.GetTexture("Textures/Filtering/RecipeAvailable"),
				MagicStorage.Instance.GetTexture("Textures/Filtering/RecipeAll")
			}, new LocalizedText[] {
				Locale.Get(Locale.C.RECIPE_AVAIL),
				Locale.Get(Locale.C.RECIPE_ALL),
			});
		}

		private void InitFilterButtons() {
			if (filterButtons != null) return;
			filterButtons = new UIButtonChoice(this, new Texture2D[] {
				MagicStorage.Instance.GetTexture("Textures/Filtering/FilterAll"),
				MagicStorage.Instance.GetTexture("Textures/Filtering/FilterMelee"),
				MagicStorage.Instance.GetTexture("Textures/Filtering/FilterPickaxe"),
				MagicStorage.Instance.GetTexture("Textures/Filtering/FilterArmor"),
				MagicStorage.Instance.GetTexture("Textures/Filtering/FilterPotion"),
				MagicStorage.Instance.GetTexture("Textures/Filtering/FilterTile"),
				MagicStorage.Instance.GetTexture("Textures/Filtering/FilterMisc"),
			}, new LocalizedText[] {
				Locale.Get(Locale.C.FILTER_ALL),
				Locale.Get(Locale.C.FILTER_WEAP),
				Locale.Get(Locale.C.FILTER_TOOL),
				Locale.Get(Locale.C.FILTER_EQUIP),
				Locale.Get(Locale.C.FILTER_POT),
				Locale.Get(Locale.C.FILTER_TILE),
				Locale.Get(Locale.C.FILTER_MISC),
			});
		}

		#endregion

		public void Update(GameTime gameTime) {
			try {
				oldMouse = curMouse;
				curMouse = PlayerInput.MouseInfo;
				if (Main.playerInventory) {
					(Point16 Pos, Type Tile) = Main.player[Main.myPlayer].GetModPlayer<StoragePlayer>().ViewingStorage();
					if (Pos.X >= 0 && (Tile == typeof(TCraftingAccess) || Tile == typeof(TCraftingStorageAccess))) {
						//Main.NewText("Found: " + craftingTileSocketItems.Count() + " (non-empty) sockets: " + string.Join(", ", craftingTileSocketItems
						//	.Select(s => { Item i = new Item(); i.SetDefaults(s.GetItemTypeFromTileAbove()); return i.Name; })));
						if (curMouse.RightButton == ButtonState.Released) {
							ResetSlotFocus();
							MagicStorage.Instance.guiM.WaitForUnpress = false;
						}
						if (recipesPanel != null) {
							recipesPanel.Update(gameTime);
						}
						if (ingredientsPanel != null) {
							ingredientsPanel.Update(gameTime);
						}
						UpdateRecipeText();
						UpdateScrollBar();
						UpdateCraftButton(TileEntity.ByPosition[Pos] as TECraftingAccess);
					}
				}
				else {
					recipeScrollBarFocus = 0;
					selectedRecipe = null;
					craftTimer = 0;
					maxCraftTimer = START_MAX_CRAFT_TIMER;
					ResetSlotFocus();
				}
			}
			catch (Exception e) { Main.NewTextMultiline(e.ToString()); }
		}

		public void Draw(TEStorageHeart heart) {
			try {
				Player player = Main.player[Main.myPlayer];

				StoragePlayer modPlayer = player.GetModPlayer<StoragePlayer>();
				Initialize();
				if (Main.mouseX > PANEL_LEFT && Main.mouseX < ingredientLeft + (selectedRecipe != null ? ingredientWidth : 0) && Main.mouseY > panelTop && Main.mouseY < panelTop + panelHeight) {
					player.mouseInterface = true;
					player.showItemIcon = false;
					GUIHelper.HideItemIconCache();
				}
				recipesPanel.Draw(Main.spriteBatch);
				if (selectedRecipe != null) {
					ingredientsPanel.Draw(Main.spriteBatch);
				}
				Vector2 pos = recipeZone.GetDimensions().Position();
				if (threadRunning) {
					Utils.DrawBorderString(Main.spriteBatch, Locale.GetStr(Locale.C.LOADING), pos + new Vector2(8f, 8f), Color.White);
				}

				if (!MagicStorage.Instance.guiM.StorageGUI.Active) {
					stationZone.DrawText();
				}
				recipeZone.DrawText();
				ingredientZone.DrawText();
				availableItemsZone.DrawText();
				resultZone.DrawText();
				sortButtons.DrawText();
				recipeButtons.DrawText();
				filterButtons.DrawText();
			}
			catch (Exception e) { Main.NewTextMultiline(e.ToString()); }
		}

		private Item GetStation(int slot, ref int context) {
			Item[] stations = GetCraftingStations();
			if (stations == null || slot >= stations.Length) {
				return new Item();
			}
			return stations[slot];
		}

		private Item GetRecipe(int slot, ref int context) {
			if (threadRunning) {
				return new Item();
			}
			int index = slot + AVAILABLE_RECIPES_NUM_COLS * (int)Math.Round(recipeZoneScrollBar.ViewPosition);
			Item item = index < recipes.Count ? recipes[index].createItem : new Item();
			if (!item.IsAir && recipes[index] == selectedRecipe) {
				context = 6;
			}
			if (!item.IsAir && !recipeAvailable[index]) {
				context = recipes[index] == selectedRecipe ? 4 : 3;
			}
			return item;
		}

		private Item GetIngredient(int slot, ref int context) {
			if (selectedRecipe == null || slot >= selectedRecipe.requiredItem.Length) {
				return new Item();
			}
			Item item = selectedRecipe.requiredItem[slot].Clone();
			if (selectedRecipe.anyWood && item.type == ItemID.Wood) {
				item.SetNameOverride($"{Locale.GetStr(Locale.C.L.ANY)} {Lang.GetItemNameValue(ItemID.Wood)}");
			}
			if (selectedRecipe.anySand && item.type == ItemID.SandBlock) {
				item.SetNameOverride($"{Locale.GetStr(Locale.C.L.ANY)} {Lang.GetItemNameValue(ItemID.SandBlock)}");
			}
			if (selectedRecipe.anyIronBar && item.type == ItemID.IronBar) {
				item.SetNameOverride($"{Locale.GetStr(Locale.C.L.ANY)} {Lang.GetItemNameValue(ItemID.IronBar)}");
			}
			if (selectedRecipe.anyFragment && item.type == ItemID.FragmentSolar) {
				item.SetNameOverride($"{Locale.GetStr(Locale.C.L.ANY)} {Locale.GetStr(Locale.C.L.FRAGMENT)}");
			}
			if (selectedRecipe.anyPressurePlate && item.type == ItemID.GrayPressurePlate) {
				item.SetNameOverride($"{Locale.GetStr(Locale.C.L.ANY)} {Locale.GetStr(Locale.C.L.PPLATE)}");
			}
			if (selectedRecipe.ProcessGroupsForText(item.type, out string nameOverride)) {
				item.SetNameOverride(nameOverride);
			}
			return item;
		}

		private Item GetStorage(int slot, ref int context) {
			int index = slot + AVAILABLE_INGREDIENT_NUM_COLS * (int)Math.Round(availableItemsScrollBar.ViewPosition);
			Item item = index < storageItems.Count ? storageItems[index] : new Item();
			if (blockStorageItems.Contains(new ItemData(item))) {
				context = 3;
			}
			return item;
		}

		private Item GetResult(int slot, ref int context) {
			return slot == 0 && result != null ? result : new Item();
		}

		private void UpdateRecipeText() {
			if (selectedRecipe == null) {
				requiresObjText.SetText("");
			}
			else {
				bool isEmpty = true;
				string text = "";
				for (int k = 0; k < selectedRecipe.requiredTile.Length; k++) {
					if (selectedRecipe.requiredTile[k] == -1) {
						break;
					}
					if (!isEmpty) {
						text += ", ";
					}
					text += Lang.GetMapObjectName(MapHelper.TileToLookup(selectedRecipe.requiredTile[k], 0));
					isEmpty = false;
				}
				if (selectedRecipe.needWater) {
					if (!isEmpty) {
						text += ", ";
					}
					text += Locale.GetStr(Locale.C.L.WATER);
					isEmpty = false;
				}
				if (selectedRecipe.needHoney) {
					if (!isEmpty) {
						text += ", ";
					}
					text += Locale.GetStr(Locale.C.L.HONEY);
					isEmpty = false;
				}
				if (selectedRecipe.needLava) {
					if (!isEmpty) {
						text += ", ";
					}
					text += Locale.GetStr(Locale.C.L.LAVA);
					isEmpty = false;
				}
				if (selectedRecipe.needSnowBiome) {
					if (!isEmpty) {
						text += ", ";
					}
					text += Locale.GetStr(Locale.C.L.COLD_WEATHER);
					isEmpty = false;
				}
				if (isEmpty) {
					text = Locale.GetStr(Locale.C.L.NONE);
				}
				requiresObjText.SetText(text);
			}
		}

		private void UpdateScrollBar() {
			if (slotFocus) {
				recipeScrollBarFocus = 0;
				return;
			}
			Rectangle dim = recipeZoneScrollBar.GetClippingRectangle(Main.spriteBatch);
			Vector2 boxPos = new Vector2(dim.X, dim.Y + dim.Height * (recipeZoneScrollBar.ViewPosition / recipeScrollBarMaxViewSize));
			float boxWidth = 20f * Main.UIScale;
			float boxHeight = dim.Height * (RECIPE_SCROLLBAR_VIEW_SIZE / recipeScrollBarMaxViewSize);
			Rectangle dim2 = availableItemsScrollBar.GetClippingRectangle(Main.spriteBatch);
			Vector2 box2Pos = new Vector2(dim2.X, dim2.Y + dim2.Height * (availableItemsScrollBar.ViewPosition / availableItemsScrollBarMaxViewSize));
			float box2Height = dim2.Height * (AVAILABLE_ITEMS_SCROLLBAR_VIEW_SIZE / availableItemsScrollBarMaxViewSize);
			if (recipeScrollBarFocus > 0) {
				if (curMouse.LeftButton == ButtonState.Released) {
					recipeScrollBarFocus = 0;
				}
				else {
					int difference = curMouse.Y - recipeScrollBarFocusMouseStart;
					if (recipeScrollBarFocus == 1) {
						recipeZoneScrollBar.ViewPosition = recipeScrollBarFocusPositionStart + (float)difference / boxHeight;
					}
					else if (recipeScrollBarFocus == 2) {
						availableItemsScrollBar.ViewPosition = recipeScrollBarFocusPositionStart + (float)difference / box2Height;
					}
				}
			}
			else if (MouseClicked) {
				if (curMouse.X > boxPos.X && curMouse.X < boxPos.X + boxWidth && curMouse.Y > boxPos.Y - 3f && curMouse.Y < boxPos.Y + boxHeight + 4f) {
					recipeScrollBarFocus = 1;
					recipeScrollBarFocusMouseStart = curMouse.Y;
					recipeScrollBarFocusPositionStart = recipeZoneScrollBar.ViewPosition;
				}
				else if (curMouse.X > box2Pos.X && curMouse.X < box2Pos.X + boxWidth && curMouse.Y > box2Pos.Y - 3f && curMouse.Y < box2Pos.Y + box2Height + 4f) {
					recipeScrollBarFocus = 2;
					recipeScrollBarFocusMouseStart = curMouse.Y;
					recipeScrollBarFocusPositionStart = availableItemsScrollBar.ViewPosition;
				}
			}
			if (recipeScrollBarFocus == 0) {
				int difference = oldMouse.ScrollWheelValue / 250 - curMouse.ScrollWheelValue / 250;
				recipeZoneScrollBar.ViewPosition += difference;
			}
		}

		private void UpdateCraftButton(TECraftingAccess access) {
			Rectangle dim = GUIHelper.GetFullRectangle(craftButton);
			bool flag = false;
			if (curMouse.X > dim.X && curMouse.X < dim.X + dim.Width && curMouse.Y > dim.Y && curMouse.Y < dim.Y + dim.Height) {
				craftButton.BackgroundColor = new Color(73, 94, 171);
				if (curMouse.LeftButton == ButtonState.Pressed) {
					if (selectedRecipe != null && IsAvailable(selectedRecipe) && PassesBlock(selectedRecipe)) {
						if (craftTimer <= 0) {
							craftTimer = maxCraftTimer;
							maxCraftTimer = maxCraftTimer * 3 / 4;
							if (maxCraftTimer <= 0) {
								maxCraftTimer = 1;
							}
							TryCraft(access);
							RefreshItems();
							MagicStorage.Instance.guiM?.StorageGUI.RefreshItems();
							Main.PlaySound(SoundID.Grab, -1, -1, 1);
						}
						craftTimer--;
						flag = true;
					}
				}
			}
			else {
				craftButton.BackgroundColor = new Color(63, 82, 151) * 0.7f;
			}
			if (selectedRecipe == null || !IsAvailable(selectedRecipe) || !PassesBlock(selectedRecipe)) {
				craftButton.BackgroundColor = new Color(30, 40, 100) * 0.7f;
			}
			if (!flag) {
				craftTimer = 0;
				maxCraftTimer = START_MAX_CRAFT_TIMER;
			}
		}

		private TEStorageHeart GetHeart() {
			Player player = Main.player[Main.myPlayer];
			StoragePlayer modPlayer = player.GetModPlayer<StoragePlayer>();
			return modPlayer.GetStorageHeart();
		}

		private TECraftingAccess GetCraftingEntity() {
			Player player = Main.player[Main.myPlayer];
			StoragePlayer modPlayer = player.GetModPlayer<StoragePlayer>();
			return modPlayer.GetCraftingAccess();
		}

		private Item[] GetCraftingStations() {
			TECraftingAccess ent = GetCraftingEntity();
			if (craftingTileSocketItems == null || (craftingTileSocketItems != null && !craftingTileSocketItems.SequenceEqual(GetHeart().sockets))) {
				craftingTileSocketItems = GetHeart().sockets;
			}
			if (ent is TECraftingStorageAccess a) {
				return craftingTileSocketItems.Where(w => w.GetItemTypeFromTileAbove() != -1)
				.Select(s => { Item i = new Item(); i.SetDefaults(s.GetItemTypeFromTileAbove()); return i; })
				.ToArray();
			}
			return ent?.stations;
		}

		public override void RefreshItems(TEStorageCenter center = null) {
			if (!Active) {
				return;
			}
			items.Clear();
			TEStorageHeart heart = center as TEStorageHeart ?? GetHeart();
			if (heart == null) {
				return;
			}
			items.AddRange(ItemSorter.SortAndFilter(heart.GetStoredItems(), SortMode.Id, FilterMode.All, "", ""));
			AnalyzeIngredients();
			InitLangStuff();
			InitSortButtons();
			InitRecipeButtons();
			InitFilterButtons();
			SortMode sortMode;
			switch (sortButtons.Choice) {
				case 0:
					sortMode = SortMode.Default;
					break;
				case 1:
					sortMode = SortMode.Id;
					break;
				case 2:
					sortMode = SortMode.Name;
					break;
				default:
					sortMode = SortMode.Default;
					break;
			}
			FilterMode filterMode;
			switch (filterButtons.Choice) {
				case 0:
					filterMode = FilterMode.All;
					break;
				case 1:
					filterMode = FilterMode.Weapons;
					break;
				case 2:
					filterMode = FilterMode.Tools;
					break;
				case 3:
					filterMode = FilterMode.Equipment;
					break;
				case 4:
					filterMode = FilterMode.Potions;
					break;
				case 5:
					filterMode = FilterMode.Placeables;
					break;
				case 6:
					filterMode = FilterMode.Misc;
					break;
				default:
					filterMode = FilterMode.All;
					break;
			}
			RefreshStorageItems();
			lock (threadLOCK) {
				threadNeedsRestart = true;
				threadSortMode = sortMode;
				threadFilterMode = filterMode;
				if (!threadRunning) {
					threadRunning = true;
					Thread thread = new Thread(RefreshRecipes);
					thread.Start();
				}
			}
		}

		private void RefreshRecipes() {
			evnt.Wait();
			while (true) {
				try {
					SortMode sortMode;
					FilterMode filterMode;
					lock (threadLOCK) {
						threadNeedsRestart = false;
						sortMode = threadSortMode;
						filterMode = threadFilterMode;
					}
					var temp = ItemSorter.GetRecipes(sortMode, filterMode, modSearchBar.Text, nameSearchBar.Text);
					threadRecipes.Clear();
					threadRecipeAvailable.Clear();
					try {
						if (recipeButtons.Choice == 0) {
							threadRecipes.AddRange(temp.Where(recipe => IsAvailable(recipe)));
							threadRecipeAvailable.AddRange(threadRecipes.Select(recipe => true));
						}
						else {
							threadRecipes.AddRange(temp);
							threadRecipeAvailable.AddRange(threadRecipes.Select(recipe => IsAvailable(recipe)));
						}
					}
					catch (InvalidOperationException) {
					}
					catch (KeyNotFoundException) {
					}
					catch (NullReferenceException) {
						//UI Closed
					}
					lock (recipeLOCK) {
						nextRecipes = new List<Recipe>();
						nextRecipeAvailable = new List<bool>();
						nextRecipes.AddRange(threadRecipes);
						nextRecipeAvailable.AddRange(threadRecipeAvailable);

					}
					lock (threadLOCK) {
						if (!threadNeedsRestart) {
							threadRunning = false;
							return;
						}
					}
				}
				catch (Exception e) { Main.NewTextMultiline(e.ToString()); }
			}
		}

		private void AnalyzeIngredients() {
			Player player = Main.player[Main.myPlayer];
			itemCounts.Clear();
			if (adjTiles.Length != player.adjTile.Length) {
				Array.Resize(ref adjTiles, player.adjTile.Length);
			}
			for (int k = 0; k < adjTiles.Length; k++) {
				adjTiles[k] = false;
			}
			adjWater = false;
			adjLava = false;
			adjHoney = false;
			zoneSnow = false;
			alchemyTable = false;

			foreach (Item item in items) {
				if (itemCounts.ContainsKey(item.netID)) {
					itemCounts[item.netID] += item.stack;
				}
				else {
					itemCounts[item.netID] = item.stack;
				}
			}
			foreach (Item item in GetCraftingStations()) {
				if (item.createTile >= TileID.Dirt) {
					adjTiles[item.createTile] = true;
					if (item.createTile == TileID.GlassKiln || item.createTile == TileID.Hellforge || item.createTile == TileID.AdamantiteForge) {
						adjTiles[TileID.Furnaces] = true;
					}
					if (item.createTile == TileID.AdamantiteForge) {
						adjTiles[TileID.Hellforge] = true;
					}
					if (item.createTile == TileID.MythrilAnvil) {
						adjTiles[TileID.Anvils] = true;
					}
					if (item.createTile == TileID.BewitchingTable || item.createTile == TileID.Tables2) {
						adjTiles[TileID.Tables] = true;
					}
					if (item.createTile == TileID.AlchemyTable) {
						adjTiles[TileID.Bottles] = true;
						adjTiles[TileID.Tables] = true;
						alchemyTable = true;
					}
					bool[] oldAdjTile = player.adjTile;
					bool oldAdjWater = adjWater;
					bool oldAdjLava = adjLava;
					bool oldAdjHoney = adjHoney;
					bool oldAlchemyTable = alchemyTable;
					player.adjTile = adjTiles;
					player.adjWater = false;
					player.adjLava = false;
					player.adjHoney = false;
					player.alchemyTable = false;
					TileLoader.AdjTiles(player, item.createTile);
					if (player.adjWater) {
						adjWater = true;
					}
					if (player.adjLava) {
						adjLava = true;
					}
					if (player.adjHoney) {
						adjHoney = true;
					}
					if (player.alchemyTable) {
						alchemyTable = true;
					}
					player.adjTile = oldAdjTile;
					player.adjWater = oldAdjWater;
					player.adjLava = oldAdjLava;
					player.adjHoney = oldAdjHoney;
					player.alchemyTable = oldAlchemyTable;
				}
				if (item.type == ItemID.WaterBucket || item.type == ItemID.BottomlessBucket) {
					adjWater = true;
				}
				if (item.type == ItemID.LavaBucket) {
					adjLava = true;
				}
				if (item.type == ItemID.HoneyBucket) {
					adjHoney = true;
				}
				if (item.type == MagicStorage.Instance.ItemType(nameof(SnowBiomeEmulator))) {
					zoneSnow = true;
				}
			}
			adjTiles[MagicStorage.Instance.TileType(nameof(Components.TCraftingAccess))] = true;
		}

		private bool IsAvailable(Recipe recipe) {
			foreach (int tile in recipe.requiredTile) {
				if (tile == -1) {
					break;
				}
				if (!adjTiles[tile]) {
					return false;
				}
			}
			foreach (Item ingredient in recipe.requiredItem) {
				if (ingredient.type == ItemID.None) {
					break;
				}
				int stack = ingredient.stack;
				bool useRecipeGroup = false;
				foreach (int type in itemCounts.Keys) {
					if (RecipeGroupMatch(recipe, type, ingredient.type)) {
						stack -= itemCounts[type];
						useRecipeGroup = true;
					}
				}
				if (!useRecipeGroup && itemCounts.ContainsKey(ingredient.netID)) {
					stack -= itemCounts[ingredient.netID];
				}
				if (stack > 0) {
					return false;
				}
			}
			if (recipe.needWater && !adjWater && !adjTiles[TileID.Sinks]) {
				return false;
			}
			if (recipe.needLava && !adjLava) {
				return false;
			}
			if (recipe.needHoney && !adjHoney) {
				return false;
			}
			if (recipe.needSnowBiome && !zoneSnow) {
				return false;
			}
			try {
				BlockRecipes.active = false;
				if (!RecipeHooks.RecipeAvailable(recipe)) {
					return false;
				}
			}
			finally {
				BlockRecipes.active = true;
			}
			return true;
		}

		private bool PassesBlock(Recipe recipe) {
			foreach (Item ingredient in recipe.requiredItem) {
				if (ingredient.type == ItemID.None) {
					break;
				}
				int stack = ingredient.stack;
				bool useRecipeGroup = false;
				foreach (Item item in storageItems) {
					ItemData data = new ItemData(item);
					if (!blockStorageItems.Contains(data) && RecipeGroupMatch(recipe, item.netID, ingredient.type)) {
						stack -= item.stack;
						useRecipeGroup = true;
					}
				}
				if (!useRecipeGroup) {
					foreach (Item item in storageItems) {
						ItemData data = new ItemData(item);
						if (!blockStorageItems.Contains(data) && item.netID == ingredient.netID) {
							stack -= item.stack;
						}
					}
				}
				if (stack > 0) {
					return false;
				}
			}
			return true;
		}

		private void RefreshStorageItems() {
			storageItems.Clear();
			result = null;
			if (selectedRecipe != null) {
				foreach (Item item in items) {
					for (int k = 0; k < selectedRecipe.requiredItem.Length; k++) {
						if (selectedRecipe.requiredItem[k].type == ItemID.None) {
							break;
						}
						if (item.type == selectedRecipe.requiredItem[k].type || RecipeGroupMatch(selectedRecipe, selectedRecipe.requiredItem[k].type, item.type)) {
							storageItems.Add(item);
						}
					}
					if (item.type == selectedRecipe.createItem.type) {
						result = item;
					}
				}
				if (result == null) {
					result = new Item();
					result.SetDefaults(selectedRecipe.createItem.type);
					result.stack = 0;
				}
			}
		}

		private bool RecipeGroupMatch(Recipe recipe, int type1, int type2) {
			return recipe.useWood(type1, type2) || recipe.useSand(type1, type2) || recipe.useIronBar(type1, type2) || recipe.useFragment(type1, type2) || recipe.AcceptedByItemGroups(type1, type2) || recipe.usePressurePlate(type1, type2);
		}

		private void HoverStation(int slot, ref int hoverSlot) {
			TECraftingAccess ent = GetCraftingEntity();
			if (ent == null || slot >= ent.stations.Length) {
				return;
			}

			Player player = Main.player[Main.myPlayer];
			if (MouseClicked) {
				bool changed = false;
				if (!ent.stations[slot].IsAir && ItemSlot.ShiftInUse) {
					Item result = player.GetItem(Main.myPlayer, DoWithdraw(slot), false, true);
					if (!result.IsAir && Main.mouseItem.IsAir) {
						Main.mouseItem = result;
						result = new Item();
					}
					if (!result.IsAir && Main.mouseItem.type == result.type && Main.mouseItem.stack < Main.mouseItem.maxStack) {
						Main.mouseItem.stack += result.stack;
						result = new Item();
					}
					if (!result.IsAir) {
						player.QuickSpawnClonedItem(result);
					}
					changed = true;
				}
				else if (player.itemAnimation == 0 && player.itemTime == 0) {
					int oldType = Main.mouseItem.type;
					int oldStack = Main.mouseItem.stack;
					Main.mouseItem = DoStationSwap(Main.mouseItem, slot);
					if (oldType != Main.mouseItem.type || oldStack != Main.mouseItem.stack) {
						changed = true;
					}
				}
				if (changed) {
					RefreshItems();
					MagicStorage.Instance.guiM?.StorageGUI.RefreshItems();
					Main.PlaySound(SoundID.Grab, -1, -1, 1);
				}
			}

			hoverSlot = slot;
		}

		private void HoverRecipe(int slot, ref int hoverSlot) {
			int visualSlot = slot;
			slot += AVAILABLE_RECIPES_NUM_COLS * (int)Math.Round(recipeZoneScrollBar.ViewPosition);
			if (slot < recipes.Count) {
				if (MouseClicked) {
					if (selectedRecipe != recipes[slot]) {
						selectedRecipe = recipes[slot];
						RefreshStorageItems();
						blockStorageItems.Clear();
					}
					else {
						selectedRecipe = null;
						RefreshStorageItems();
						blockStorageItems.Clear();
					}
				}
				hoverSlot = visualSlot;
			}
		}

		private void HoverItem(int slot, ref int hoverSlot) {
			hoverSlot = slot;
		}

		private void HoverStorage(int slot, ref int hoverSlot) {
			int visualSlot = slot;
			slot += AVAILABLE_INGREDIENT_NUM_COLS * (int)Math.Round(availableItemsScrollBar.ViewPosition);
			if (slot < storageItems.Count) {
				if (MouseClicked) {
					ItemData data = new ItemData(storageItems[slot]);
					if (blockStorageItems.Contains(data)) {
						blockStorageItems.Remove(data);
					}
					else {
						blockStorageItems.Add(data);
					}
				}
				hoverSlot = visualSlot;
			}
		}

		private void HoverResult(int slot, ref int hoverSlot) {
			if (slot != 0) {
				return;
			}

			Player player = Main.player[Main.myPlayer];
			if (MouseClicked) {
				bool changed = false;
				if (!Main.mouseItem.IsAir && player.itemAnimation == 0 && player.itemTime == 0 && result != null && Main.mouseItem.type == result.type) {
					if (TryDepositResult(Main.mouseItem)) {
						changed = true;
					}
				}
				else if (Main.mouseItem.IsAir && result != null && !result.IsAir) {
					Item toWithdraw = result.Clone();
					if (toWithdraw.stack > toWithdraw.maxStack) {
						toWithdraw.stack = toWithdraw.maxStack;
					}
					Main.mouseItem = DoWithdrawResult(toWithdraw, ItemSlot.ShiftInUse);
					if (ItemSlot.ShiftInUse) {
						Main.mouseItem = player.GetItem(Main.myPlayer, Main.mouseItem, false, true);
					}
					changed = true;
				}
				if (changed) {
					RefreshItems();
					MagicStorage.Instance.guiM?.StorageGUI.RefreshItems();
					Main.PlaySound(SoundID.Grab, -1, -1, 1);
				}
			}

			if (curMouse.RightButton == ButtonState.Pressed && oldMouse.RightButton == ButtonState.Released && result != null && !result.IsAir && (Main.mouseItem.IsAir || ItemData.Matches(Main.mouseItem, items[slot]) && Main.mouseItem.stack < Main.mouseItem.maxStack)) {
				slotFocus = true;
			}

			hoverSlot = slot;

			if (slotFocus) {
				SlotFocusLogic();
			}
		}

		private void SlotFocusLogic() {
			if (result == null || result.IsAir || (!Main.mouseItem.IsAir && (!ItemData.Matches(Main.mouseItem, result) || Main.mouseItem.stack >= Main.mouseItem.maxStack))) {
				ResetSlotFocus();
			}
			else {
				if (rightClickTimer <= 0) {
					rightClickTimer = maxRightClickTimer;
					maxRightClickTimer = maxRightClickTimer * 3 / 4;
					if (maxRightClickTimer <= 0) {
						maxRightClickTimer = 1;
					}
					Item toWithdraw = result.Clone();
					toWithdraw.stack = 1;
					Item withdrawn = DoWithdrawResult(toWithdraw);
					if (Main.mouseItem.IsAir) {
						Main.mouseItem = withdrawn;
					}
					else {
						Main.mouseItem.stack += withdrawn.stack;
					}
					Main.soundInstanceMenuTick.Stop();
					Main.soundInstanceMenuTick = Main.soundMenuTick.CreateInstance();
					Main.PlaySound(SoundID.MenuTick, -1, -1, 1);
					RefreshItems();
					MagicStorage.Instance.guiM?.StorageGUI.RefreshItems();
				}
				rightClickTimer--;
			}
		}

		private void ResetSlotFocus() {
			slotFocus = false;
			rightClickTimer = 0;
			maxRightClickTimer = SMART_MAX_CLICK_TIMER;
		}

		private Item DoWithdraw(int slot) {
			TECraftingAccess access = GetCraftingEntity();
			if (Main.netMode == NetmodeID.SinglePlayer) {
				Item result = access.TryWithdrawStation(slot);
				RefreshItems();
				MagicStorage.Instance.guiM?.StorageGUI.RefreshItems();
				return result;
			}
			else {
				NetHelper.SendWithdrawStation(access.ID, slot);
				return new Item();
			}
		}

		private Item DoStationSwap(Item item, int slot) {
			TECraftingAccess access = GetCraftingEntity();
			if (Main.netMode == NetmodeID.SinglePlayer) {
				Item result = access.DoStationSwap(item, slot);
				RefreshItems();
				MagicStorage.Instance.guiM?.StorageGUI.RefreshItems();
				return result;
			}
			else {
				NetHelper.SendStationSlotClick(access.ID, item, slot);
				return new Item();
			}
		}

		private void TryCraft(TECraftingAccess access) {
			List<Item> availableItems = new List<Item>(storageItems.Where(item => !blockStorageItems.Contains(new ItemData(item))).Select(item => item.Clone()));
			List<Item> toWithdraw = new List<Item>();
			for (int k = 0; k < selectedRecipe.requiredItem.Length; k++) {
				Item item = selectedRecipe.requiredItem[k];
				if (item.type == ItemID.None) {
					break;
				}
				int stack = item.stack;
				if (selectedRecipe is ModRecipe modRecipe) {
					stack = modRecipe.ConsumeItem(item.type, item.stack);
				}
				if (selectedRecipe.alchemy && alchemyTable) {
					int save = 0;
					for (int j = 0; j < stack; j++) {
						if (Main.rand.Next(3) == 0) {
							save++;
						}
					}
					stack -= save;
				}
				if (stack > 0) {
					foreach (Item tryItem in availableItems) {
						if (item.type == tryItem.type || RecipeGroupMatch(selectedRecipe, item.type, tryItem.type)) {
							if (tryItem.stack > stack) {
								Item temp = tryItem.Clone();
								temp.stack = stack;
								toWithdraw.Add(temp);
								tryItem.stack -= stack;
								stack = 0;
							}
							else {
								toWithdraw.Add(tryItem.Clone());
								stack -= tryItem.stack;
								tryItem.stack = 0;
								tryItem.type = ItemID.None;
							}
						}
					}
				}
			}
			Item resultItem = selectedRecipe.createItem.Clone();
			resultItem.Prefix(-1);

			RecipeHooks.OnCraft(resultItem, selectedRecipe);
			ItemLoader.OnCraft(resultItem, selectedRecipe);

			if (Main.netMode == NetmodeID.SinglePlayer) {
				foreach (Item item in access.DoCraft(GetHeart(), toWithdraw, resultItem)) {
					Main.player[Main.myPlayer].QuickSpawnClonedItem(item, item.stack);
				}
			}
			else if (Main.netMode == NetmodeID.MultiplayerClient) {
				NetHelper.SendCraftRequest(GetHeart().ID, access.ID, toWithdraw, resultItem);
			}
		}

		private bool TryDepositResult(Item item) {
			int oldStack = item.stack;
			DoDepositResult(item);
			return oldStack != item.stack;
		}

		private void DoDepositResult(Item item) {
			TEStorageHeart heart = GetHeart();
			if (Main.netMode == NetmodeID.SinglePlayer) {
				heart.DepositItem(item);
			}
			else {
				NetHelper.SendDeposit(heart.ID, item);
				item.SetDefaults(0, true);
			}
		}

		private Item DoWithdrawResult(Item item, bool toInventory = false) {
			TEStorageHeart heart = GetHeart();
			if (Main.netMode == NetmodeID.SinglePlayer) {
				return heart.TryWithdraw(item);
			}
			else {
				NetHelper.SendWithdraw(heart.ID, item, toInventory);
				return new Item();
			}
		}
	}
}