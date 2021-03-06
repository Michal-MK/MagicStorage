using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.UI;

namespace MagicStorage {
	public class UIDebug : UIElement {

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			spriteBatch.Draw(MagicStorage.Instance.GetTexture("Textures/temp/tmp"),new Rectangle { X = (int)Left.Pixels, Y = (int)Top.Pixels, Width = (int)Width.Pixels, Height = (int)Height.Pixels }, Color.White);
			base.DrawSelf(spriteBatch);
		}
	}
}
