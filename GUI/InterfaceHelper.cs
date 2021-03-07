using MagicStorage.Components;
using MagicStorage.GUI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.UI;

namespace MagicStorage {
	public static class InterfaceHelper {
		private static FieldInfo _itemIconCacheTimeInfo;


		public static void Initialize() {
			_itemIconCacheTimeInfo = typeof(Main).GetField("_itemIconCacheTime", BindingFlags.NonPublic | BindingFlags.Static);
		}

		public static void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int mouseItemIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Inventory");
			layers.Insert(mouseItemIndex, new LegacyGameInterfaceLayer("MagicStorage: StorageAccess", DrawStorageGUI, InterfaceScaleType.UI));
		}

		public static bool DrawStorageGUI() {
			Main.inventoryScale = 0.85f;
			Player player = Main.player[Main.myPlayer];
			StoragePlayer modPlayer = player.GetModPlayer<StoragePlayer>();
			(Point16 storageAccess, Type _) = modPlayer.ViewingStorage();
			if (!Main.playerInventory || storageAccess.X < 0 || storageAccess.Y < 0) {
				StorageGUI.Unload();
				CraftingGUI.Unload();
				return true;
			}
			ModTile modTile = TileLoader.GetTile(Main.tile[storageAccess.X, storageAccess.Y].type);
			if (modTile == null || !(modTile is StorageAccess)) {
				return true;
			}
			TEStorageHeart heart = ((StorageAccess)modTile).GetHeart(storageAccess.X, storageAccess.Y);
			if (heart == null) {
				return true;
			}

			if (modTile is CraftingStorageAccess) {
				CraftingGUI.Draw(heart);
				StorageGUI.Draw(heart, true);
			}
			else if (modTile is CraftingAccess) {
				CraftingGUI.Draw(heart);
			}
			else {
				StorageGUI.Draw(heart);
			}
			Main.instance.MouseText($"[{Main.mouseX},{Main.mouseY}]");
			return true;
		}

		public static void HideItemIconCache() {
			_itemIconCacheTimeInfo.SetValue(null, 0);
		}

		public static Rectangle GetFullRectangle(UIElement element) {
			Vector2 vector = new Vector2(element.GetDimensions().X, element.GetDimensions().Y);
			Vector2 position = new Vector2(element.GetDimensions().Width, element.GetDimensions().Height) + vector;
			vector = Vector2.Transform(vector, Main.UIScaleMatrix);
			position = Vector2.Transform(position, Main.UIScaleMatrix);
			Rectangle result = new Rectangle((int)vector.X, (int)vector.Y, (int)(position.X - vector.X), (int)(position.Y - vector.Y));
			int width = Main.spriteBatch.GraphicsDevice.Viewport.Width;
			int height = Main.spriteBatch.GraphicsDevice.Viewport.Height;
			result.X = Utils.Clamp<int>(result.X, 0, width);
			result.Y = Utils.Clamp<int>(result.Y, 0, height);
			result.Width = Utils.Clamp<int>(result.Width, 0, width - result.X);
			result.Height = Utils.Clamp<int>(result.Height, 0, height - result.Y);
			return result;
		}
	}
}