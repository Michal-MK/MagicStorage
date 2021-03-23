using MagicStorageTwo.Components;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.ID;
using System;
using MagicStorageTwo.Extensions;
using Terraria.ObjectData;

namespace MagicStorageTwo {
	public class StoragePlayer : ModPlayer {

		private bool postOpenCleanup;
		public Point16 storageAccess = new Point16(-1, -1);
		public Type tileType;
		public bool remoteAccess = false;

		public Point16 PlayerPosTiles => new Point16((int)(player.Center.X / 16f), (int)(player.Center.Y / 16f));


		public override void UpdateDead() {
			if (player.whoAmI == Main.myPlayer) {
				CloseStorage();
			}
		}

		public override void ResetEffects() {
			if (player.whoAmI != Main.myPlayer) {
				return;
			}

			if (postOpenCleanup) {
				player.talkNPC = -1;
				Main.playerInventory = true;
				postOpenCleanup = false;
			}

			if (storageAccess.N0()) {
				if (player.chest != -1 || !Main.playerInventory || player.sign > -1 || player.talkNPC > -1) {
					CloseStorage();
					Recipe.FindRecipes();
				}
				else {
					if ((!remoteAccess && !InRange(PlayerPosTiles, storageAccess)) ||
						!(TileLoader.GetTile(Main.tile[storageAccess.X, storageAccess.Y].type) is TStorageAccess)) {
						Main.PlaySound(SoundID.MenuClose, -1, -1, 1);
						CloseStorage();
						Recipe.FindRecipes();
					}
				}
			}
		}

		public bool CanOpen(Point16 toOpen) {
			return InRange(PlayerPosTiles, toOpen) || remoteAccess;
		}

		private bool InRange(Point16 position, Point16 target) {
			TileObjectData tod = TileObjectData.GetTileData(Main.tile[target.X, target.Y]);
			if (tod == null) return false;
			return position.X >= target.X - Player.tileRangeX &&
				   position.X < target.X + Player.tileRangeX + tod.Width &&
				   position.Y >= target.Y - Player.tileRangeY &&
				   position.Y < target.Y + Player.tileRangeY + tod.Height;
		}

		public void OpenStorage(Point16 point, Type tile, bool remote = false) {
			storageAccess = point;
			remoteAccess = remote;
			tileType = tile;
			if (tileType == typeof(TStorageAccess) || tileType == typeof(TStorageHeart) ||
				tileType == typeof(TCraftingStorageAccess) || tileType == typeof(TRemoteAccess)) {
				MagicStorageTwo.Instance.guiM.StorageGUI.Active = true;
				MagicStorageTwo.Instance.guiM.StorageGUI.RefreshItems();
			}
			if (tileType == typeof(TCraftingAccess) || tileType == typeof(TCraftingStorageAccess)) {
				MagicStorageTwo.Instance.guiM.CraftingGUI.Active = true;
				MagicStorageTwo.Instance.guiM.CraftingGUI.RefreshItems();
			}
			postOpenCleanup = true;
		}

		public void CloseStorage() {
			storageAccess = new Point16(-1, -1);
			Main.blockInput = false;

			if (MagicStorageTwo.Instance.guiM.StorageGUI.Active) {
				MagicStorageTwo.Instance.guiM?.StorageGUI.nameSearchBar.Reset();
			}
			if (MagicStorageTwo.Instance.guiM.StorageGUI.Active) {
				MagicStorageTwo.Instance.guiM.StorageGUI.modSearchBar.Reset();
			}
			if (MagicStorageTwo.Instance.guiM.CraftingGUI.Active) {
				MagicStorageTwo.Instance.guiM.CraftingGUI.nameSearchBar.Reset();
			}
			if (MagicStorageTwo.Instance.guiM.CraftingGUI.Active) {
				MagicStorageTwo.Instance.guiM.CraftingGUI.modSearchBar.Reset();
			}
			MagicStorageTwo.Instance.guiM.StorageGUI.Active = false;
			MagicStorageTwo.Instance.guiM.CraftingGUI.Active = false;
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
				MagicStorageTwo.Instance.guiM?.Refresh();
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
			if (modTile == null || !(modTile is TStorageAccess)) {
				return null;
			}
			return ((TStorageAccess)modTile).GetHeart(storageAccess.X, storageAccess.Y);
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
			return tile != null && (tile.type == mod.TileType(nameof(TCraftingAccess)) || tile.type == mod.TileType(nameof(TCraftingStorageAccess)));
		}

		public bool Crafting() {
			if (storageAccess.X < 0 || storageAccess.Y < 0) {
				return false;
			}
			Tile tile = Main.tile[storageAccess.X, storageAccess.Y];
			return tile != null && (tile.type == mod.TileType(nameof(TCraftingAccess)));
		}

		public static bool IsOnlyStorageCrafting() {
			return Main.player[Main.myPlayer].GetModPlayer<StoragePlayer>().Crafting();
		}
	}
}