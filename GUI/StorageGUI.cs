using MagicStorage.Components;
using MagicStorage.Sorting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;
using Terraria.ID;

namespace MagicStorage.GUI {
	public static class StorageGUI {
		private const int numColumns = 10;
		public const float inventoryScale = 0.85f;
		public static bool doOffset = false;

		public static MouseState curMouse;
		public static MouseState oldMouse;
		public static bool MouseClicked {
			get {
				return curMouse.LeftButton == ButtonState.Pressed && oldMouse.LeftButton == ButtonState.Released;
			}
		}

		private static UIPanel basePanel;
		private static float panelTop;
		private static float panelLeft;
		private static float panelWidth;
		private static float panelHeight;

		private static UIElement topBar;
		internal static UISearchBar searchBar;
		private static UIButtonChoice sortButtons;
		internal static UITextPanel<LocalizedText> depositButton;
		private static UIElement topBar2;
		private static UIButtonChoice filterButtons;
		internal static UISearchBar searchBar2;

		private static UISlotZone slotZone = new UISlotZone(HoverItemSlot, GetItem, inventoryScale);
		private static int slotFocus = -1;
		private static int rightClickTimer = 0;
		private const int startMaxRightClickTimer = 20;
		private static int maxRightClickTimer = startMaxRightClickTimer;

		internal static UIScrollbar scrollBar = new UIScrollbar();
		private static bool scrollBarFocus = false;
		private static int scrollBarFocusMouseStart;
		private static float scrollBarFocusPositionStart;
		private static float scrollBarViewSize = 1f;
		private static float scrollBarMaxViewSize = 2f;

		private static List<Item> items = new List<Item>();
		private static List<bool> didMatCheck = new List<bool>();
		private static int numRows;
		private static int displayRows;

		private static UIElement bottomBar = new UIElement();
		private static UIText capacityText;

		public static void Initialize() {
			InitLangStuff();
			float itemSlotWidth = Main.inventoryBackTexture.Width * inventoryScale;
			float itemSlotHeight = Main.inventoryBackTexture.Height * inventoryScale;
			float smallSlotWidth = Main.inventoryBackTexture.Width * CraftingGUI.INGREDIENTS_SCALE;


			panelTop = Main.instance.invBottom + 60;

			float innerCraftingPanelWidth = CraftingGUI.AVAILABLE_RECIPES_NUM_COLS * (itemSlotWidth + UICommon.PADDING) + 20f + UICommon.PADDING;
			float craftingPanelWidth = 12 + innerCraftingPanelWidth + 12;
			float ingredientWidth = CraftingGUI.AVAILABLE_INGREDIENT_NUM_COLS * (smallSlotWidth + UICommon.PADDING) + 20f + UICommon.PADDING;
			ingredientWidth += 12 * 2;

			panelLeft = 20f + (doOffset ? craftingPanelWidth + ingredientWidth: 0);
			basePanel = new UIPanel();
			float innerPanelLeft = panelLeft + basePanel.PaddingLeft;
			float innerPanelWidth = numColumns * (itemSlotWidth + UICommon.PADDING) + 20f + UICommon.PADDING;
			panelWidth = basePanel.PaddingLeft + innerPanelWidth + basePanel.PaddingRight;
			panelHeight = Main.screenHeight - panelTop - 60f;
			basePanel.Left.Set(panelLeft, 0f);
			basePanel.Top.Set(panelTop, 0f);
			basePanel.Width.Set(panelWidth, 0f);
			basePanel.Height.Set(panelHeight, 0f);
			basePanel.Recalculate();

			topBar = new UIElement();
			topBar.Width.Set(0f, 1f);
			topBar.Height.Set(32f, 0f);
			basePanel.Append(topBar);

			InitSortButtons();
			topBar.Append(sortButtons);

			depositButton.Left.Set(sortButtons.GetDimensions().Width + 2 * UICommon.PADDING, 0f);
			depositButton.Width.Set(128f, 0f);
			depositButton.Height.Set(-2 * UICommon.PADDING, 1f);
			depositButton.PaddingTop = 8f;
			depositButton.PaddingBottom = 8f;
			topBar.Append(depositButton);

			float depositButtonRight = sortButtons.GetDimensions().Width + 2 * UICommon.PADDING + depositButton.GetDimensions().Width;
			searchBar.Left.Set(depositButtonRight + UICommon.PADDING, 0f);
			searchBar.Width.Set(-depositButtonRight - 2 * UICommon.PADDING, 1f);
			searchBar.Height.Set(0f, 1f);
			topBar.Append(searchBar);

			topBar2 = new UIElement();
			topBar2.Width.Set(0f, 1f);
			topBar2.Height.Set(32f, 0f);
			topBar2.Top.Set(36f, 0f);
			basePanel.Append(topBar2);

			InitFilterButtons();
			topBar2.Append(filterButtons);
			searchBar2.Left.Set(depositButtonRight + UICommon.PADDING, 0f);
			searchBar2.Width.Set(-depositButtonRight - 2 * UICommon.PADDING, 1f);
			searchBar2.Height.Set(0f, 1f);
			topBar2.Append(searchBar2);

			slotZone.Width.Set(0f, 1f);
			slotZone.Top.Set(76f, 0f);
			slotZone.Height.Set(-116f, 1f);
			basePanel.Append(slotZone);

			numRows = (items.Count + numColumns - 1) / numColumns;
			displayRows = (int)slotZone.GetDimensions().Height / ((int)itemSlotHeight + UICommon.PADDING);
			slotZone.SetDimensions(numColumns, displayRows);
			int noDisplayRows = numRows - displayRows;
			if (noDisplayRows < 0) {
				noDisplayRows = 0;
			}
			scrollBarMaxViewSize = 1 + noDisplayRows;
			scrollBar.Height.Set(displayRows * (itemSlotHeight + UICommon.PADDING), 0f);
			scrollBar.Left.Set(-20f, 1f);
			scrollBar.SetView(scrollBarViewSize, scrollBarMaxViewSize);
			slotZone.Append(scrollBar);

			bottomBar.Width.Set(0f, 1f);
			bottomBar.Height.Set(32f, 0f);
			bottomBar.Top.Set(-32f, 1f);
			basePanel.Append(bottomBar);

			capacityText.Left.Set(6f, 0f);
			capacityText.Top.Set(6f, 0f);
			TEStorageHeart heart = GetHeart();
			int numItems = 0;
			int capacity = 0;
			if (heart != null) {
				foreach (TEAbstractStorageUnit abstractStorageUnit in heart.GetStorageUnits()) {
					if (abstractStorageUnit is TEStorageUnit) {
						TEStorageUnit storageUnit = (TEStorageUnit)abstractStorageUnit;
						numItems += storageUnit.NumItems;
						capacity += storageUnit.Capacity;
					}
				}
			}
			capacityText.SetText(numItems + "/" + capacity + " Items");
			bottomBar.Append(capacityText);
		}

		private static void InitLangStuff() {
			if (depositButton == null) {
				depositButton = new UITextPanel<LocalizedText>(Language.GetText("Mods.MagicStorage.DepositAll"), 1f);
			}
			if (searchBar == null) {
				searchBar = new UISearchBar(Language.GetText("Mods.MagicStorage.SearchName"));
			}
			if (searchBar2 == null) {
				searchBar2 = new UISearchBar(Language.GetText("Mods.MagicStorage.SearchMod"));
			}
			if (capacityText == null) {
				capacityText = new UIText("Items");
			}
		}

		internal static void Unload() {
			sortButtons = null;
			filterButtons = null;
			basePanel = null;
		}

		private static void InitSortButtons() {
			if (sortButtons == null) {
				sortButtons = new UIButtonChoice(new Texture2D[]
				{
					Main.inventorySortTexture[0],
					MagicStorage.Instance.GetTexture("Textures/Sorting/SortID"),
					MagicStorage.Instance.GetTexture("Textures/Sorting/SortName"),
					MagicStorage.Instance.GetTexture("Textures/Sorting/SortNumber")
				},
				new LocalizedText[]
				{
					Language.GetText("Mods.MagicStorage.SortDefault"),
					Language.GetText("Mods.MagicStorage.SortID"),
					Language.GetText("Mods.MagicStorage.SortName"),
					Language.GetText("Mods.MagicStorage.SortStack")
				});
			}
		}

		private static void InitFilterButtons() {
			if (filterButtons == null) {
				filterButtons = new UIButtonChoice(new Texture2D[]
				{
					MagicStorage.Instance.GetTexture("Textures/Filtering/FilterAll"),
					MagicStorage.Instance.GetTexture("Textures/Filtering/FilterMelee"),
					MagicStorage.Instance.GetTexture("Textures/Filtering/FilterPickaxe"),
					MagicStorage.Instance.GetTexture("Textures/Filtering/FilterArmor"),
					MagicStorage.Instance.GetTexture("Textures/Filtering/FilterPotion"),
					MagicStorage.Instance.GetTexture("Textures/Filtering/FilterTile"),
					MagicStorage.Instance.GetTexture("Textures/Filtering/FilterMisc"),
				},
				new LocalizedText[]
				{
					Language.GetText("Mods.MagicStorage.FilterAll"),
					Language.GetText("Mods.MagicStorage.FilterWeapons"),
					Language.GetText("Mods.MagicStorage.FilterTools"),
					Language.GetText("Mods.MagicStorage.FilterEquips"),
					Language.GetText("Mods.MagicStorage.FilterPotions"),
					Language.GetText("Mods.MagicStorage.FilterTiles"),
					Language.GetText("Mods.MagicStorage.FilterMisc")
				});
			}
		}

		public static void Update(GameTime gameTime) {
			oldMouse = curMouse;
			curMouse = Mouse.GetState();
			if (Main.playerInventory && Main.player[Main.myPlayer].GetModPlayer<StoragePlayer>().ViewingStorage().X >= 0) {
				if (StorageGUI.curMouse.RightButton == ButtonState.Released) {
					ResetSlotFocus();
				}
				if (basePanel != null)
					basePanel.Update(gameTime);
				UpdateScrollBar();
				UpdateDepositButton();
			}
			else {
				scrollBarFocus = false;
				scrollBar.ViewPosition = 0f;
				ResetSlotFocus();
			}
		}

		public static void Draw(TEStorageHeart heart, bool offset = false) {
			doOffset = offset;
			Player player = Main.player[Main.myPlayer];
			StoragePlayer modPlayer = player.GetModPlayer<StoragePlayer>();
			Initialize();
			if (Main.mouseX > panelLeft && Main.mouseX < panelLeft + panelWidth && Main.mouseY > panelTop && Main.mouseY < panelTop + panelHeight) {
				player.mouseInterface = true;
				player.showItemIcon = false;
				InterfaceHelper.HideItemIconCache();
			}
			basePanel.Draw(Main.spriteBatch);
			slotZone.DrawText();
			sortButtons.DrawText();
			filterButtons.DrawText();
		}

		private static Item GetItem(int slot, ref int context) {
			int index = slot + numColumns * (int)Math.Round(scrollBar.ViewPosition);
			Item item = index < items.Count ? items[index] : new Item();
			if (!item.IsAir && !didMatCheck[index]) {
				item.checkMat();
				didMatCheck[index] = true;
			}
			return item;
		}

		private static void UpdateScrollBar() {
			if (slotFocus >= 0) {
				scrollBarFocus = false;
				return;
			}
			Rectangle dim = scrollBar.GetClippingRectangle(Main.spriteBatch);
			Vector2 boxPos = new Vector2(dim.X, dim.Y + dim.Height * (scrollBar.ViewPosition / scrollBarMaxViewSize));
			float boxWidth = 20f * Main.UIScale;
			float boxHeight = dim.Height * (scrollBarViewSize / scrollBarMaxViewSize);
			if (scrollBarFocus) {
				if (curMouse.LeftButton == ButtonState.Released) {
					scrollBarFocus = false;
				}
				else {
					int difference = curMouse.Y - scrollBarFocusMouseStart;
					scrollBar.ViewPosition = scrollBarFocusPositionStart + (float)difference / boxHeight;
				}
			}
			else if (MouseClicked) {
				if (curMouse.X > boxPos.X && curMouse.X < boxPos.X + boxWidth && curMouse.Y > boxPos.Y - 3f && curMouse.Y < boxPos.Y + boxHeight + 4f) {
					scrollBarFocus = true;
					scrollBarFocusMouseStart = curMouse.Y;
					scrollBarFocusPositionStart = scrollBar.ViewPosition;
				}
			}
			if (!scrollBarFocus) {
				int difference = oldMouse.ScrollWheelValue / 250 - curMouse.ScrollWheelValue / 250;
				scrollBar.ViewPosition += difference;
			}
		}

		private static TEStorageHeart GetHeart() {
			Player player = Main.player[Main.myPlayer];
			StoragePlayer modPlayer = player.GetModPlayer<StoragePlayer>();
			return modPlayer.GetStorageHeart();
		}

		public static void RefreshItems() {
			if (StoragePlayer.IsOnlyStorageCrafting()) {
				CraftingGUI.RefreshItems();
				return;
			}
			items.Clear();
			didMatCheck.Clear();
			TEStorageHeart heart = GetHeart();
			if (heart == null) {
				return;
			}
			InitLangStuff();
			InitSortButtons();
			InitFilterButtons();
			SortMode sortMode = (SortMode)sortButtons.Choice;
			FilterMode filterMode = (FilterMode)filterButtons.Choice;

			items.AddRange(ItemSorter.SortAndFilter(heart.GetStoredItems(), sortMode, filterMode, searchBar2.Text, searchBar.Text));
			for (int k = 0; k < items.Count; k++) {
				didMatCheck.Add(false);
			}
		}

		private static void UpdateDepositButton() {
			Rectangle dim = InterfaceHelper.GetFullRectangle(depositButton);
			if (curMouse.X > dim.X && curMouse.X < dim.X + dim.Width && curMouse.Y > dim.Y && curMouse.Y < dim.Y + dim.Height) {
				depositButton.BackgroundColor = new Color(73, 94, 171);
				if (MouseClicked) {
					if (TryDepositAll()) {
						RefreshItems();
						Main.PlaySound(SoundID.Grab);
					}
				}
			}
			else {
				depositButton.BackgroundColor = new Color(63, 82, 151) * 0.7f;
			}
		}

		private static void ResetSlotFocus() {
			slotFocus = -1;
			rightClickTimer = 0;
			maxRightClickTimer = startMaxRightClickTimer;
		}

		private static void HoverItemSlot(int slot, ref int hoverSlot) {
			Player player = Main.player[Main.myPlayer];
			int visualSlot = slot;
			slot += numColumns * (int)Math.Round(scrollBar.ViewPosition);
			if (MouseClicked) {
				bool changed = false;
				if (!Main.mouseItem.IsAir && (player.itemAnimation == 0 && player.itemTime == 0)) {
					if (TryDeposit(Main.mouseItem)) {
						changed = true;
					}
				}
				else if (Main.mouseItem.IsAir && slot < items.Count && !items[slot].IsAir) {
					Item toWithdraw = items[slot].Clone();
					if (toWithdraw.stack > toWithdraw.maxStack) {
						toWithdraw.stack = toWithdraw.maxStack;
					}
					Main.mouseItem = DoWithdraw(toWithdraw, ItemSlot.ShiftInUse);
					if (ItemSlot.ShiftInUse) {
						Main.mouseItem = player.GetItem(Main.myPlayer, Main.mouseItem, false, true);
					}
					changed = true;
				}
				if (changed) {
					RefreshItems();
					Main.PlaySound(SoundID.Grab, -1, -1, 1);
				}
			}

			if (curMouse.RightButton == ButtonState.Pressed && oldMouse.RightButton == ButtonState.Released && slot < items.Count && (Main.mouseItem.IsAir || ItemData.Matches(Main.mouseItem, items[slot]) && Main.mouseItem.stack < Main.mouseItem.maxStack)) {
				slotFocus = slot;
			}

			if (slot < items.Count && !items[slot].IsAir) {
				hoverSlot = visualSlot;
			}

			if (slotFocus >= 0) {
				SlotFocusLogic();
			}
		}

		private static void SlotFocusLogic() {
			if (slotFocus >= items.Count || (!Main.mouseItem.IsAir && (!ItemData.Matches(Main.mouseItem, items[slotFocus]) || Main.mouseItem.stack >= Main.mouseItem.maxStack))) {
				ResetSlotFocus();
			}
			else {
				if (rightClickTimer <= 0) {
					rightClickTimer = maxRightClickTimer;
					maxRightClickTimer = maxRightClickTimer * 3 / 4;
					if (maxRightClickTimer <= 0) {
						maxRightClickTimer = 1;
					}
					Item toWithdraw = items[slotFocus].Clone();
					toWithdraw.stack = 1;
					Item result = DoWithdraw(toWithdraw);
					if (Main.mouseItem.IsAir) {
						Main.mouseItem = result;
					}
					else {
						Main.mouseItem.stack += result.stack;
					}
					Main.soundInstanceMenuTick.Stop();
					Main.soundInstanceMenuTick = Main.soundMenuTick.CreateInstance();
					Main.PlaySound(SoundID.MenuTick, -1, -1, 1);
					RefreshItems();
				}
				rightClickTimer--;
			}
		}

		private static bool TryDeposit(Item item) {
			int oldStack = item.stack;
			DoDeposit(item);
			return oldStack != item.stack;
		}

		private static void DoDeposit(Item item) {
			TEStorageHeart heart = GetHeart();
			if (Main.netMode == NetmodeID.SinglePlayer) {
				heart.DepositItem(item);
			}
			else {
				NetHelper.SendDeposit(heart.ID, item);
				item.SetDefaults(0, true);
			}
		}

		private static bool TryDepositAll() {
			Player player = Main.player[Main.myPlayer];
			TEStorageHeart heart = GetHeart();
			bool changed = false;
			if (Main.netMode == NetmodeID.SinglePlayer) {
				for (int k = 10; k < 50; k++) {
					if (!player.inventory[k].IsAir && !player.inventory[k].favorited) {
						int oldStack = player.inventory[k].stack;
						heart.DepositItem(player.inventory[k]);
						if (oldStack != player.inventory[k].stack) {
							changed = true;
						}
					}
				}
			}
			else {
				List<Item> items = new List<Item>();
				for (int k = 10; k < 50; k++) {
					if (!player.inventory[k].IsAir && !player.inventory[k].favorited) {
						items.Add(player.inventory[k]);
					}
				}
				NetHelper.SendDepositAll(heart.ID, items);
				foreach (Item item in items) {
					item.SetDefaults(0, true);
				}
				changed = true;
			}
			return changed;
		}

		private static Item DoWithdraw(Item item, bool toInventory = false) {
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