using MagicStorageTwo.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagicStorageTwo.Components {
	public class TCreativeStorageUnit : TStorageComponent {
		public override ModTileEntity GetTileEntity() {
			return mod.GetTileEntity(nameof(TECreativeStorageUnit));
		}

		public override int ItemType(int frameX, int frameY) {
			return mod.ItemType(nameof(CreativeStorageUnit));
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			Tile tile = Main.tile[i, j];
			Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
			Vector2 drawPos = zero + 16f * new Vector2(i, j) - Main.screenPosition;
			Rectangle frame = new Rectangle(tile.frameX, tile.frameY, 16, 16);
			Color lightColor = Lighting.GetColor(i, j, Color.White);
			Color color = Color.Lerp(Color.White, lightColor, 0.5f);
			spriteBatch.Draw(mod.GetTexture("Textures/Tiles/" + ActualName + "_Glow"), drawPos, frame, color);
		}
	}
}