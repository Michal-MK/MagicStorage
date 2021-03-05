﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace MagicStorage.Components {
	public class CraftingStorageAccess : CraftingAccess {
		public override ModTileEntity GetTileEntity() {
			return mod.GetTileEntity(nameof(TECraftingStorageAccess));
		}

		public override int ItemType(int frameX, int frameY) {
			return mod.ItemType(nameof(CraftingStorageAccess));
		}
	}
}
