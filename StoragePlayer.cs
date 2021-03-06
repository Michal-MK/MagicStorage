using MagicStorage.Components;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.ID;
using MagicStorage.GUI;
using System;

namespace MagicStorage {
	public class StoragePlayer : ModPlayer {
		public int timeSinceOpen = 1;
		public Point16 storageAccess = new Point16(-1, -1);
		public Type tileType;
		public bool remoteAccess = false;

		public override void UpdateDead() {
			if (player.whoAmI == Main.myPlayer) {
				CloseStorage();
			}
		}

		public override void ResetEffects() {
			if (player.whoAmI != Main.myPlayer) {
				return;
			}
			if (timeSinceOpen < 1) {
				player.talkNPC = -1;
				Main.playerInventory = true;
				timeSinceOpen++;
			}
			if (storageAccess.X >= 0 && storageAccess.Y >= 0 && (player.chest != -1 || !Main.playerInventory || player.sign > -1 || player.talkNPC > -1)) {
				CloseStorage();
				Recipe.FindRecipes();
			}
			else if (storageAccess.X >= 0 && storageAccess.Y >= 0) {
				int playerX = (int)(player.Center.X / 16f);
				int playerY = (int)(player.Center.Y / 16f);
				if (!remoteAccess && (playerX < storageAccess.X - Player.tileRangeX || playerX > storageAccess.X + Player.tileRangeX + 1 || playerY < storageAccess.Y - Player.tileRangeY || playerY > storageAccess.Y + Player.tileRangeY + 1)) {
					Main.PlaySound(SoundID.MenuClose, -1, -1, 1);
					CloseStorage();
					Recipe.FindRecipes();
				}
				else if (!(TileLoader.GetTile(Main.tile[storageAccess.X, storageAccess.Y].type) is StorageAccess)) {
					Main.PlaySound(SoundID.MenuClose, -1, -1, 1);
					CloseStorage();
					Recipe.FindRecipes();
				}
			}
		}

		public void OpenStorage(Point16 point, Type tile, bool remote = false) {
			storageAccess = point;
			remoteAccess = remote;
			tileType = tile;
			if (tileType == typeof(StorageAccess) || tileType == typeof(StorageHeart)) {
				StorageGUI.RefreshItems();
			}
			if (tileType == typeof(CraftingAccess) || tileType == typeof(CraftingStorageAccess)) {
				CraftingGUI.RefreshItems();
			}
		}

		public void CloseStorage() {
			storageAccess = new Point16(-1, -1);
			Main.blockInput = false;
			if (StorageGUI.searchBar != null) {
				StorageGUI.searchBar.Reset();
			}
			if (StorageGUI.searchBar2 != null) {
				StorageGUI.searchBar2.Reset();
			}
			if (CraftingGUI.itemNameSearch != null) {
				CraftingGUI.itemNameSearch.Reset();
			}
			if (CraftingGUI.modNameSearch != null) {
				CraftingGUI.modNameSearch.Reset();
			}
		}

		public (Point16 Pos, Type Tile) ViewingStorage() {
			return (storageAccess, tileType);
		}

		public static void GetItem(Item item, bool toMouse) {
			Player player = Main.player[Main.myPlayer];
			if (toMouse && Main.playerInventory && Main.mouseItem.IsAir) {
				Main.mouseItem = item;
				item = new Item();
			}
			else if (toMouse && Main.playerInventory && Main.mouseItem.type == item.type) {
				int total = Main.mouseItem.stack + item.stack;
				if (total > Main.mouseItem.maxStack) {
					total = Main.mouseItem.maxStack;
				}
				int difference = total - Main.mouseItem.stack;
				Main.mouseItem.stack = total;
				item.stack -= difference;
			}
			if (!item.IsAir) {
				item = player.GetItem(Main.myPlayer, item, false, true);
				if (!item.IsAir) {
					player.QuickSpawnClonedItem(item, item.stack);
				}
			}
		}

		public override bool ShiftClickSlot(Item[] inventory, int context, int slot) {
			if (context != ItemSlot.Context.InventoryItem && context != ItemSlot.Context.InventoryCoin && context != ItemSlot.Context.InventoryAmmo) {
				return false;
			}
			if (storageAccess.X < 0 || storageAccess.Y < 0) {
				return false;
			}
			Item item = inventory[slot];
			if (item.favorited || item.IsAir) {
				return false;
			}
			int oldType = item.type;
			int oldStack = item.stack;
			if (Crafting()) {
				if (Main.netMode == NetmodeID.SinglePlayer) {
					GetCraftingAccess().TryDepositStation(item);
				}
				else {
					NetHelper.SendDepositStation(GetCraftingAccess().ID, item);
					item.SetDefaults(0, true);
				}
			}
			else {
				if (Main.netMode == NetmodeID.SinglePlayer) {
					GetStorageHeart().DepositItem(item);
				}
				else {
					NetHelper.SendDeposit(GetStorageHeart().ID, item);
					item.SetDefaults(0, true);
				}
			}
			if (item.type != oldType || item.stack != oldStack) {
				Main.PlaySound(SoundID.Grab, -1, -1, 1, 1f, 0f);
				StorageGUI.RefreshItems();
			}
			return true;
		}

		public TEStorageHeart GetStorageHeart() {
			if (storageAccess.X < 0 || storageAccess.Y < 0) {
				return null;
			}
			Tile tile = Main.tile[storageAccess.X, storageAccess.Y];
			if (tile == null) {
				return null;
			}
			int tileType = tile.type;
			ModTile modTile = TileLoader.GetTile(tileType);
			if (modTile == null || !(modTile is StorageAccess)) {
				return null;
			}
			return ((StorageAccess)modTile).GetHeart(storageAccess.X, storageAccess.Y);
		}

		public TECraftingAccess GetCraftingAccess() {
			if (storageAccess.X < 0 || storageAccess.Y < 0 || !TileEntity.ByPosition.ContainsKey(storageAccess)) {
				return null;
			}
			return TileEntity.ByPosition[storageAccess] as TECraftingAccess;
		}

		public bool StorageCrafting() {
			if (storageAccess.X < 0 || storageAccess.Y < 0) {
				return false;
			}
			Tile tile = Main.tile[storageAccess.X, storageAccess.Y];
			return tile != null && (tile.type == mod.TileType(nameof(CraftingAccess)) || tile.type == mod.TileType(nameof(CraftingStorageAccess)));
		}
		
		public bool Crafting() {
			if (storageAccess.X < 0 || storageAccess.Y < 0) {
				return false;
			}
			Tile tile = Main.tile[storageAccess.X, storageAccess.Y];
			return tile != null && (tile.type == mod.TileType(nameof(CraftingAccess)));
		}

		public static bool IsOnlyStorageCrafting() {
			return Main.player[Main.myPlayer].GetModPlayer<StoragePlayer>().Crafting();
		}
	}
}