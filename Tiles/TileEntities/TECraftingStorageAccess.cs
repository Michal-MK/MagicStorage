using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader.IO;
using Terraria.ID;

namespace MagicStorage.Components {
	public class TECraftingStorageAccess : TECraftingAccess {

		public override bool ValidTile(Tile tile) {
			return tile.type == mod.TileType(nameof(TCraftingStorageAccess)) && tile.frameX == 0 && tile.frameY == 0;
		}
	}
}