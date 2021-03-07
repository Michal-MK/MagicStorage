using MagicStorage.Items;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace MagicStorage.Components {
	public class StorageHeart : StorageAccess {
		public override ModTileEntity GetTileEntity() {
			return mod.GetTileEntity(nameof(TEStorageHeart));
		}

		public override int ItemType(int frameX, int frameY) {
			return mod.ItemType(nameof(StorageHeart));
		}

		public override bool HasSmartInteract() {
			return true;
		}

		public override TEStorageHeart GetHeart(int i, int j) {
			return (TEStorageHeart)TileEntity.ByPosition[new Point16(i, j)];
		}

		public override bool NewRightClick(int i, int j) {
			Player player = Main.player[Main.myPlayer];
			Item item = player.inventory[player.selectedItem];
			if (item.type == mod.ItemType(nameof(Locator)) || item.type == mod.ItemType(nameof(LocatorDisk)) || item.type == mod.ItemType(nameof(PortableAccess))) {
				if (Main.tile[i, j].frameX % 36 == 18) {
					i--;
				}
				if (Main.tile[i, j].frameY % 36 == 18) {
					j--;
				}
				Locator locator = (Locator)item.modItem;
				locator.location = new Point16(i, j);
				if (player.selectedItem == 58) {
					Main.mouseItem = item.Clone();
				}
				Main.NewText(Locale.Get(Locale.C.LOCATOR_SUCCESS));
				return true;
			}
			else {
				return base.NewRightClick(i, j);
			}
		}
	}
}