using MagicStorage.GUI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.UI;

namespace MagicStorage {
	public class UISlotZone : UIElement {
		public delegate void HoverItemSlot(int slot, ref int hoverSlot);
		public delegate Item GetItem(int slot, ref int context);

		private int numColumns = 10;
		private int numRows = 4;
		private int hoverSlot = -1;
		private HoverItemSlot onHover;
		private GetItem getItem;
		private float inventoryScale = 1;

		internal float ActualHeight => Main.inventoryBackTexture.Height * inventoryScale * numRows + numRows * UICommon.PADDING;

		public UISlotZone(HoverItemSlot onHover, GetItem getItem, float scale) {
			this.onHover = onHover;
			this.getItem = getItem;
			//this.inventoryScale = scale;
		}

		public void SetDimensions(int columns, int rows) {
			numColumns = columns;
			numRows = rows;
		}

		public override void Update(GameTime gameTime) {
			hoverSlot = -1;
			Vector2 origin = InterfaceHelper.GetFullRectangle(this).TopLeft();
			//origin = GetDimensions().Position();
			MouseState curMouse = StorageGUI.curMouse;
			if (curMouse.X <= origin.X || curMouse.Y <= origin.Y) {
				return;
			}
			float slotWidth = 52 + 5 * Main.UIScale;// Main.inventoryBackTexture.Width;
			float slotHeight = 52 + 5 * Main.UIScale;// Main.inventoryBackTexture.Height;
			int slotX = (int)((curMouse.X - origin.X) / (slotWidth));
			int slotY = (int)((curMouse.Y - origin.Y) / (slotHeight));
			if (slotX < 0 || slotX >= numColumns || slotY < 0 || slotY >= numRows) {
				return;
			}
			int k = slotY * numColumns + slotX;
			Vector2 drawPos = origin + new Vector2((slotWidth + UICommon.PADDING) * (k % numColumns), (slotHeight + UICommon.PADDING) * (k / numColumns));
			Vector2 slotPos = origin + new Vector2(slotX * (slotWidth + UICommon.PADDING), slotY * (slotHeight + UICommon.PADDING));
			if (curMouse.X > slotPos.X && curMouse.X < slotPos.X + slotWidth && curMouse.Y > slotPos.Y && curMouse.Y < slotPos.Y + slotHeight) {
				onHover(slotX + numColumns * slotY, ref hoverSlot);
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			//float slotWidth = 42 + 5;// Main.inventoryBackTexture.Width;
			//float slotHeight = 42 + 5;// Main.inventoryBackTexture.Height;
			float slotWidth = Main.inventoryBackTexture.Width+1 * 0.85f;
			float slotHeight = Main.inventoryBackTexture.Height+1 * 0.85f;
			Vector2 origin = GetDimensions().Position();
			Item[] temp = new Item[11];
			for (int k = 0; k < numColumns * numRows; k++) {
				int context = 0;
				Item item = getItem(k, ref context);
				Vector2 drawPos = origin + new Vector2((slotWidth) * (k % numColumns), (slotHeight) * (k / numColumns));
				temp[10] = item;
				ItemSlot.Draw(spriteBatch, temp, context, 10, drawPos);
			}
			Item i = new Item();
			ItemSlot.Draw(spriteBatch, ref i, 0, origin /*+ new Vector2(20+ 42 + 5, 20 + 42 + 5)*/);
		}

		public void DrawText() {
			if (hoverSlot >= 0) {
				int context = 0;
				Item hoverItem = getItem(hoverSlot, ref context);
				if (!hoverItem.IsAir) {
					Main.HoverItem = hoverItem.Clone();
					Main.instance.MouseText(string.Empty);
				}
			}
		}
	}
}