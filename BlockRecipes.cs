using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace MagicStorageTwo {
	public class BlockRecipes : GlobalRecipe {
		public static bool active = true;

		public override bool RecipeAvailable(Recipe recipe) {
			if (!active) {
				return true;
			}
			try {
				Player player = Main.player[Main.myPlayer];
				StoragePlayer modPlayer = player.GetModPlayer<StoragePlayer>();
				(Point16 storageAccess, Type t) = modPlayer.ViewingStorage();
				return storageAccess.X < 0 || storageAccess.Y < 0;
			}
			catch {
				return true;
			}
		}
	}
}