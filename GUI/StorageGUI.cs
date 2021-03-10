//using MagicStorage.Components;
//using MagicStorage.Sorting;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Input;
//using System;
//using System.Collections.Generic;
//using Terraria;
//using Terraria.GameContent.UI.Elements;
//using Terraria.Localization;
//using Terraria.UI;
//using Terraria.ID;
//using Terraria.DataStructures;

//namespace MagicStorage.GUI {
//	public static class StorageGUI {
//		private static int NUM_COLS = 10;
//		private static float INV_SCALE = 1f;

//		public static MouseState curMouse;
//		public static MouseState oldMouse;

//		public static bool MouseClicked => curMouse.LeftButton == ButtonState.Pressed && oldMouse.LeftButton == ButtonState.Released;
//		public static bool OffseLeft { get; set; } = false;


//		private static UIPanel storagePanel;
//		private static float panelTop;
//		private static float panelLeft;
//		private static float panelWidth;
//		private static float panelHeight;

//		private static UIElement upperTopBar;
//		internal static UISearchBar nameSearchBar;
//		private static UIButtonChoice sortButtons;
//		internal static UITextPanel<LocalizedText> depositButton;

//		private static UIElement lowerTopBar;
//		private static UIButtonChoice filterButtons;
//		internal static UISearchBar modSearchBar;

//		private static UISlotZone itemsZone = new UISlotZone(HoverItemSlot, GetItem, INV_SCALE);
//		private static int slotFocus = -1;
//		private static int rightClickTimer = 0;
//		private const int STRAT_MAX_RIGHTCLICK_TIMER_TICKS = 20;
//		private static int maxRightClickTimer = STRAT_MAX_RIGHTCLICK_TIMER_TICKS;

//		internal static UIScrollbar itemsScrollBar = new UIScrollbar();
//		private static bool scrollBarHasFocus = false;
//		private static int scrollBarFocusMouseStart;
//		private static float scrollBarFocusPositionStart;
//		private const float SCROLLBAR_SIZE = 1f;
//		private static float scrollBarMaxViewSize = 2f;

//		private static List<Item> items = new List<Item>();
//		private static int numRows;
//		private static int displayRows;

//		private static UIElement bottomBar = new UIElement();
//		private static UIText capacityText;

//		public static void Initialize() {
//			InitLangStuff();
//			float itemSlotWidth = Main.inventoryBackTexture.Width;
//			float itemSlotHeight = Main.inventoryBackTexture.Height;
//			float smallSlotWidth = Main.inventoryBackTexture.Width * CraftingGUI.INGREDIENTS_SCALE;


//			panelTop = Main.instance.invBottom + 60;

//			float innerCraftingPanelWidth = CraftingGUI.AVAILABLE_RECIPES_NUM_COLS * (itemSlotWidth + UICommon.PADDING) + 20f + UICommon.PADDING;
//			float craftingPanelWidth = 12 + innerCraftingPanelWidth + 12;
//			float ingredientWidth = CraftingGUI.AVAILABLE_INGREDIENT_NUM_COLS * (smallSlotWidth + UICommon.PADDING) + 20f + UICommon.PADDING;
//			ingredientWidth += 12 * 2;

//			panelLeft = 20f + (OffseLeft ? craftingPanelWidth + ingredientWidth : 0);
//			storagePanel = new UIPanel();
//			float innerPanelWidth = itemsZone.ActualWidth + storagePanel.PaddingRight + storagePanel.PaddingLeft + 20 + UICommon.PADDING;
//			storagePanel.Left.Set(panelLeft, 0f);
//			storagePanel.Top.Set(panelTop, 0f);
//			storagePanel.Width.Set(innerPanelWidth, 0f);
//			storagePanel.Recalculate();

//			upperTopBar = new UIElement();
//			upperTopBar.Width.Set(0f, 1f);
//			upperTopBar.Height.Set(32f, 0f);
//			storagePanel.Append(upperTopBar);

//			InitSortButtons();
//			upperTopBar.Append(sortButtons);

//			depositButton.Left.Set(sortButtons.GetDimensions().Width + 2 * UICommon.PADDING, 0f);
//			depositButton.Width.Set(128f, 0f);
//			depositButton.Height.Set(-2 * UICommon.PADDING, 1f);
//			depositButton.PaddingTop = 8f;
//			depositButton.PaddingBottom = 8f;
//			upperTopBar.Append(depositButton);

//			float depositButtonRight = sortButtons.GetDimensions().Width + 2 * UICommon.PADDING + depositButton.GetDimensions().Width;
//			nameSearchBar.Left.Set(depositButtonRight + UICommon.PADDING, 0f);
//			nameSearchBar.Width.Set(-depositButtonRight - 1 * UICommon.PADDING, 1f);
//			nameSearchBar.Height.Set(0f, 1f);
//			upperTopBar.Append(nameSearchBar);

//			lowerTopBar = new UIElement();
//			lowerTopBar.Width.Set(0f, 1f);
//			lowerTopBar.Height.Set(32f, 0f);
//			lowerTopBar.Top.Set(36f, 0f);
//			storagePanel.Append(lowerTopBar);

//			InitFilterButtons();
//			lowerTopBar.Append(filterButtons);
//			modSearchBar.Left.Set(depositButtonRight + UICommon.PADDING, 0f);
//			modSearchBar.Width.Set(-depositButtonRight - 1 * UICommon.PADDING, 1f);
//			modSearchBar.Height.Set(0f, 1f);
//			lowerTopBar.Append(modSearchBar);

//			itemsZone.Width.Set(0f, 1f);
//			itemsZone.Top.Set(76f, 0f);
//			storagePanel.Append(itemsZone);

//			numRows = (items.Count + NUM_COLS - 1) / NUM_COLS;
//			displayRows = Math.Min(itemsZone.MaxRows, 8);
//			itemsZone.SetDimensions(NUM_COLS, displayRows);
//			int noDisplayRows = numRows - displayRows;
//			if (noDisplayRows < 0) {
//				noDisplayRows = 0;
//			}
//			scrollBarMaxViewSize = 1 + noDisplayRows;
//			itemsScrollBar.Height.Set(itemsZone.ActualHeight, 0f);
//			itemsScrollBar.Left.Set(-20f, 1f);
//			itemsScrollBar.SetView(SCROLLBAR_SIZE, scrollBarMaxViewSize);
//			itemsZone.Append(itemsScrollBar);

//			bottomBar.Width.Set(0f, 1f);
//			bottomBar.Height.Set(32f, 0f);
//			bottomBar.Top.Set(itemsZone.Top.Pixels + itemsZone.ActualHeight, 0f);
//			storagePanel.Append(bottomBar);

//			capacityText.Left.Set(6f, 0f);
//			capacityText.Top.Set(6f, 0f);
//			TEStorageHeart heart = GetHeart();
//			int numItems = 0;
//			int capacity = 0;
//			if (heart != null) {
//				foreach (TEAbstractStorageUnit abstractStorageUnit in heart.GetStorageUnits()) {
//					if (abstractStorageUnit is TEStorageUnit) {
//						TEStorageUnit storageUnit = (TEStorageUnit)abstractStorageUnit;
//						numItems += storageUnit.NumItems;
//						capacity += storageUnit.Capacity;
//					}
//				}
//			}
//			capacityText.SetText($"{numItems}/{capacity} {Locale.GetStr(Locale.C.ITEMS)}");
//			bottomBar.Append(capacityText);
//			storagePanel.Height.Set(upperTopBar.Height.Pixels + lowerTopBar.Height.Pixels + itemsZone.ActualHeight + bottomBar.Height.Pixels  + 32, 0);
//			storagePanel.Recalculate();
//		}

//		private static void InitLangStuff() {
//			if (depositButton == null) {
//				depositButton = new UITextPanel<LocalizedText>(Locale.Get(Locale.C.DEPOSIT_ALL));
//			}
//			if (nameSearchBar == null) {
//				nameSearchBar = new UISearchBar(null, Locale.Get(Locale.C.SAERCH_NAME));
//			}
//			if (modSearchBar == null) {
//				modSearchBar = new UISearchBar(null, Locale.Get(Locale.C.SAERCH_MOD));
//			}
//			if (capacityText == null) {
//				capacityText = new UIText(Locale.Get(Locale.C.ITEMS));
//			}
//		}

//		internal static void Unload() {
//			sortButtons = null;
//			filterButtons = null;
//			storagePanel = null;
//		}

//		private static void InitSortButtons() {
//			if (sortButtons == null) {
//				sortButtons = new UIButtonChoice(null, new Texture2D[]
//				{
//					Main.inventorySortTexture[0],
//					MagicStorage.Instance.GetTexture("Textures/Sorting/SortID"),
//					MagicStorage.Instance.GetTexture("Textures/Sorting/SortName"),
//					MagicStorage.Instance.GetTexture("Textures/Sorting/SortNumber")
//				},
//				new LocalizedText[]
//				{
//					Locale.Get(Locale.C.SORT_DEF),
//					Locale.Get(Locale.C.SORT_ID),
//					Locale.Get(Locale.C.SORT_NAME),
//					Locale.Get(Locale.C.SORT_STACK),
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

//		public static void Update(GameTime gameTime) {
//			oldMouse = curMouse;
//			curMouse = Mouse.GetState();
//			if (Main.playerInventory) {
//				(Point16 Pos, Type Tile) = Main.player[Main.myPlayer].GetModPlayer<StoragePlayer>().ViewingStorage();
//				if ((Tile == typeof(StorageAccess) || Tile == typeof(StorageHeart)|| Tile == typeof(CraftingStorageAccess)) && Pos.X >= 0) {
//					if (curMouse.RightButton == ButtonState.Released) {
//						ResetSlotFocus();
//					}
//					if (storagePanel != null)
//						storagePanel.Update(gameTime);
//					UpdateScrollBar();
//					UpdateDepositButton();
//				}
//			}
//			else {
//				scrollBarHasFocus = false;
//				itemsScrollBar.ViewPosition = 0f;
//				ResetSlotFocus();
//			}
//		}

//		public static void Draw(TEStorageHeart heart, bool offset = false) {
//			OffseLeft = offset;
//			Player player = Main.player[Main.myPlayer];
//			StoragePlayer modPlayer = player.GetModPlayer<StoragePlayer>();
//			Initialize();
//			if (Main.mouseX > panelLeft && Main.mouseX < panelLeft + panelWidth && Main.mouseY > panelTop && Main.mouseY < panelTop + panelHeight) {
//				player.mouseInterface = true;
//				player.showItemIcon = false;
//				InterfaceHelper.HideItemIconCache();
//			}
//			storagePanel.Draw(Main.spriteBatch);
//			itemsZone.DrawText();
//			sortButtons.DrawText();
//			filterButtons.DrawText();
//		}

//		private static Item GetItem(int slot, ref int context) {
//			int index = slot + NUM_COLS * (int)Math.Round(itemsScrollBar.ViewPosition);
//			Item item = index < items.Count ? items[index] : new Item();
//			if (!item.IsAir) {
//				item.checkMat();
//			}
//			return item;
//		}

//		private static void UpdateScrollBar() {
//			if (slotFocus >= 0) {
//				scrollBarHasFocus = false;
//				return;
//			}
//			Rectangle dim = itemsScrollBar.GetClippingRectangle(Main.spriteBatch);
//			Vector2 boxPos = new Vector2(dim.X, dim.Y + dim.Height * (itemsScrollBar.ViewPosition / scrollBarMaxViewSize));
//			float boxWidth = 20f * Main.UIScale;
//			float boxHeight = dim.Height * (SCROLLBAR_SIZE / scrollBarMaxViewSize);
//			if (scrollBarHasFocus) {
//				if (curMouse.LeftButton == ButtonState.Released) {
//					scrollBarHasFocus = false;
//				}
//				else {
//					int difference = curMouse.Y - scrollBarFocusMouseStart;
//					itemsScrollBar.ViewPosition = scrollBarFocusPositionStart + (float)difference / boxHeight;
//				}
//			}
//			else if (MouseClicked) {
//				if (curMouse.X > boxPos.X && curMouse.X < boxPos.X + boxWidth && curMouse.Y > boxPos.Y - 3f && curMouse.Y < boxPos.Y + boxHeight + 4f) {
//					scrollBarHasFocus = true;
//					scrollBarFocusMouseStart = curMouse.Y;
//					scrollBarFocusPositionStart = itemsScrollBar.ViewPosition;
//				}
//			}
//			if (!scrollBarHasFocus) {
//				int difference = oldMouse.ScrollWheelValue / 250 - curMouse.ScrollWheelValue / 250;
//				itemsScrollBar.ViewPosition += difference;
//			}
//		}

//		private static TEStorageHeart GetHeart() {
//			Player player = Main.player[Main.myPlayer];
//			StoragePlayer modPlayer = player.GetModPlayer<StoragePlayer>();
//			return modPlayer.GetStorageHeart();
//		}

//		public static void RefreshItems() {
//			if (Main.player[Main.myPlayer].GetModPlayer<StoragePlayer>().tileType == typeof(CraftingAccess)) {
//				CraftingGUI.RefreshItems();
//			}
//			if (StoragePlayer.IsOnlyStorageCrafting()) {
//				return;
//			}
//			items.Clear();
//			TEStorageHeart heart = GetHeart();
//			if (heart == null) {
//				return;
//			}
//			InitLangStuff();
//			InitSortButtons();
//			InitFilterButtons();
//			SortMode sortMode = (SortMode)sortButtons.Choice;
//			FilterMode filterMode = (FilterMode)filterButtons.Choice;

//			items.AddRange(ItemSorter.SortAndFilter(heart.GetStoredItems(), sortMode, filterMode, modSearchBar.Text, nameSearchBar.Text));
//		}

//		private static void UpdateDepositButton() {
//			Rectangle dim = InterfaceHelper.GetFullRectangle(depositButton);
//			if (curMouse.X > dim.X && curMouse.X < dim.X + dim.Width && curMouse.Y > dim.Y && curMouse.Y < dim.Y + dim.Height) {
//				depositButton.BackgroundColor = new Color(73, 94, 171);
//				if (MouseClicked) {
//					if (TryDepositAll()) {
//						RefreshItems();
//						Main.PlaySound(SoundID.Grab);
//					}
//				}
//			}
//			else {
//				depositButton.BackgroundColor = new Color(63, 82, 151) * 0.7f;
//			}
//		}

//		private static void ResetSlotFocus() {
//			slotFocus = -1;
//			rightClickTimer = 0;
//			maxRightClickTimer = STRAT_MAX_RIGHTCLICK_TIMER_TICKS;
//		}

//		private static void HoverItemSlot(int slot, ref int hoverSlot) {
//			Player player = Main.player[Main.myPlayer];
//			int visualSlot = slot;
//			slot += NUM_COLS * (int)Math.Round(itemsScrollBar.ViewPosition);
//			if (MouseClicked) {
//				bool changed = false;
//				if (!Main.mouseItem.IsAir && (player.itemAnimation == 0 && player.itemTime == 0)) {
//					if (TryDeposit(Main.mouseItem)) {
//						changed = true;
//					}
//				}
//				else if (Main.mouseItem.IsAir && slot < items.Count && !items[slot].IsAir) {
//					Item toWithdraw = items[slot].Clone();
//					if (toWithdraw.stack > toWithdraw.maxStack) {
//						toWithdraw.stack = toWithdraw.maxStack;
//					}
//					Main.mouseItem = DoWithdraw(toWithdraw, ItemSlot.ShiftInUse);
//					if (ItemSlot.ShiftInUse) {
//						Main.mouseItem = player.GetItem(Main.myPlayer, Main.mouseItem, false, true);
//					}
//					changed = true;
//				}
//				if (changed) {
//					RefreshItems();
//					Main.PlaySound(SoundID.Grab, -1, -1, 1);
//				}
//			}

//			if (curMouse.RightButton == ButtonState.Pressed && oldMouse.RightButton == ButtonState.Released && slot < items.Count && (Main.mouseItem.IsAir || ItemData.Matches(Main.mouseItem, items[slot]) && Main.mouseItem.stack < Main.mouseItem.maxStack)) {
//				slotFocus = slot;
//			}

//			if (slot < items.Count && !items[slot].IsAir) {
//				hoverSlot = visualSlot;
//			}

//			if (slotFocus >= 0) {
//				SlotFocusLogic();
//			}
//		}

//		private static void SlotFocusLogic() {
//			if (slotFocus >= items.Count || (!Main.mouseItem.IsAir && (!ItemData.Matches(Main.mouseItem, items[slotFocus]) || Main.mouseItem.stack >= Main.mouseItem.maxStack))) {
//				ResetSlotFocus();
//			}
//			else {
//				if (rightClickTimer <= 0) {
//					rightClickTimer = maxRightClickTimer;
//					maxRightClickTimer = maxRightClickTimer * 3 / 4;
//					if (maxRightClickTimer <= 0) {
//						maxRightClickTimer = 1;
//					}
//					Item toWithdraw = items[slotFocus].Clone();
//					toWithdraw.stack = 1;
//					Item result = DoWithdraw(toWithdraw);
//					if (Main.mouseItem.IsAir) {
//						Main.mouseItem = result;
//					}
//					else {
//						Main.mouseItem.stack += result.stack;
//					}
//					Main.soundInstanceMenuTick.Stop();
//					Main.soundInstanceMenuTick = Main.soundMenuTick.CreateInstance();
//					Main.PlaySound(SoundID.MenuTick, -1, -1, 1);
//					RefreshItems();
//				}
//				rightClickTimer--;
//			}
//		}

//		private static bool TryDeposit(Item item) {
//			int oldStack = item.stack;
//			DoDeposit(item);
//			return oldStack != item.stack;
//		}

//		private static void DoDeposit(Item item) {
//			TEStorageHeart heart = GetHeart();
//			if (Main.netMode == NetmodeID.SinglePlayer) {
//				heart.DepositItem(item);
//			}
//			else {
//				NetHelper.SendDeposit(heart.ID, item);
//				item.SetDefaults(0, true);
//			}
//		}

//		private static bool TryDepositAll() {
//			Player player = Main.player[Main.myPlayer];
//			TEStorageHeart heart = GetHeart();
//			bool changed = false;
//			if (Main.netMode == NetmodeID.SinglePlayer) {
//				for (int k = 10; k < 50; k++) {
//					if (!player.inventory[k].IsAir && !player.inventory[k].favorited) {
//						int oldStack = player.inventory[k].stack;
//						heart.DepositItem(player.inventory[k]);
//						if (oldStack != player.inventory[k].stack) {
//							changed = true;
//						}
//					}
//				}
//			}
//			else {
//				List<Item> items = new List<Item>();
//				for (int k = 10; k < 50; k++) {
//					if (!player.inventory[k].IsAir && !player.inventory[k].favorited) {
//						items.Add(player.inventory[k]);
//					}
//				}
//				NetHelper.SendDepositAll(heart.ID, items);
//				foreach (Item item in items) {
//					item.SetDefaults(0, true);
//				}
//				changed = true;
//			}
//			return changed;
//		}

//		private static Item DoWithdraw(Item item, bool toInventory = false) {
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