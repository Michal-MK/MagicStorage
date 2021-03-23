using MagicStorageTwo.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;

namespace MagicStorageTwo.GUI {
	public class GUIManager {

		public CraftingGUINew CraftingGUI { get; private set; }
		public StorageGUINew StorageGUI { get; private set; }

		private bool _waitForUnpnress;
		public bool WaitForUnpress { get => _waitForUnpnress && PlayerInput.MouseInfo.RightButton == ButtonState.Released; set => _waitForUnpnress = value; }

		public void Update(GameTime gt) {
			if (StorageGUI == null || CraftingGUI == null) return;

			if (StorageGUI.Active) {
				StorageGUI.Update(gt);
			}
			if (CraftingGUI.Active) {
				CraftingGUI.Update(gt);
			}
		}

		public void Draw(GameTime gt, TEStorageHeart sh) {
			if (StorageGUI == null || CraftingGUI == null) return;

			if (StorageGUI.Active) {
				StorageGUI.Draw(sh, CraftingGUI.Active);
			}
			if (CraftingGUI.Active) {
				CraftingGUI.Draw(sh);
			}
		}

		public void UILayersHook(List<GameInterfaceLayer> layers) {
			if (CraftingGUI == null) CraftingGUI = new CraftingGUINew();
			if (StorageGUI == null) StorageGUI = new StorageGUINew();
			int mouseItemIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Inventory");
			layers.Insert(mouseItemIndex, new LegacyGameInterfaceLayer("MagicStorage: StorageAccess", DrawStorageGUI, InterfaceScaleType.UI));
		}

		public void Refresh(TEStorageCenter center = null) {
			if (StorageGUI == null || CraftingGUI == null) return;

			if (StorageGUI.Active) {
				StorageGUI.RefreshItems(center);
			}
			if (CraftingGUI.Active) {
				CraftingGUI.RefreshItems(center);
			}
		}

		public bool DrawStorageGUI() {
			Player player = Main.player[Main.myPlayer];
			StoragePlayer modPlayer = player.GetModPlayer<StoragePlayer>();
			(Point16 storageAccess, Type _) = modPlayer.ViewingStorage();

			bool craftingGuiOldState = CraftingGUI.Active;
			bool storageGuiOldState = StorageGUI.Active;


			if (!Main.playerInventory || storageAccess.X < 0 || storageAccess.Y < 0) {
				return true;
			}

			ModTile modTile = TileLoader.GetTile(Main.tile[storageAccess.X, storageAccess.Y].type);
			if (modTile == null || !(modTile is TStorageAccess storageTile)) {
				return true;
			}

			TEStorageHeart heart = storageTile.GetHeart(storageAccess.X, storageAccess.Y);
			if (heart == null) {
				return true;
			}

			if (modTile is TCraftingStorageAccess) {
				if (!craftingGuiOldState) {
					CraftingGUI.Active = true;
				}
				if (!storageGuiOldState) {
					StorageGUI.Active = true;
				}
				WaitForUnpress = true;
			}
			else if (modTile is TCraftingAccess) {
				if (!craftingGuiOldState) {
					CraftingGUI.Active = true;
				}
				StorageGUI.Active = false;
				WaitForUnpress = true;
			}
			else if (modTile is TStorageHeart || (!(modTile is TCraftingAccess) && modTile is TStorageAccess)) {
				if (!storageGuiOldState) {
					StorageGUI.Active = true;
				}
				CraftingGUI.Active = false;
				WaitForUnpress = true;
			}

			Draw(new GameTime(), heart);
			//Main.instance.MouseText($"[{PlayerInput.MouseInfo.X},{PlayerInput.MouseInfo.Y}]");
			return true;
		}
	}
}
