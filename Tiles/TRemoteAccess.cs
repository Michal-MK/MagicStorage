using MagicStorage.Items;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace MagicStorage.Components {
	public class TRemoteAccess : TStorageAccess {
		public override ModTileEntity GetTileEntity() {
			return mod.GetTileEntity(nameof(TERemoteAccess));
		}

		public override int ItemType(int frameX, int frameY) {
			return mod.ItemType(nameof(RemoteAccess));
		}

		public override bool HasSmartInteract() {
			return true;
		}

		public override TEStorageHeart GetHeart(int i, int j) {
			TileEntity ent = TileEntity.ByPosition[new Point16(i, j)];
			return ((TERemoteAccess)ent).GetHeart();
		}

		public override bool NewRightClick(int i, int j) {
			Player player = Main.player[Main.myPlayer];
			Item item = player.inventory[player.selectedItem];
			if (item.type == mod.ItemType(nameof(Locator)) || item.type == mod.ItemType(nameof(LocatorDisk))) {
				if (Main.tile[i, j].frameX % 36 == 18) {
					i--;
				}
				if (Main.tile[i, j].frameY % 36 == 18) {
					j--;
				}
				TERemoteAccess ent = (TERemoteAccess)TileEntity.ByPosition[new Point16(i, j)];
				Locator locator = (Locator)item.modItem;
				string message;
				if (ent.TryLocate(locator.location, out message)) {
					if (item.type == mod.ItemType(nameof(LocatorDisk))) {
						locator.location = new Point16(-1, -1);
					}
					else {
						item.SetDefaults(0);
					}
				}
				if (player.selectedItem == 58) {
					Main.mouseItem = item.Clone();
				}
				Main.NewText(message);
				return true;
			}
			else {
				return base.NewRightClick(i, j);
			}
		}
	}
}