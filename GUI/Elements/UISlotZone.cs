using MagicStorage.GUI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.GameInput;
using Terraria.UI;

namespace MagicStorage {
	public class UISlotZone : UIElement {
		public delegate void HoverItemSlot(int slot, ref int hoverSlot);
		public delegate Item GetItem(int slot, ref int context);

		private int hoverSlot = -1;
		private readonly HoverItemSlot onHoverAction;
		private readonly GetItem getItemFunc;

		private GUIBase parentGui;

		public UISlotZone(GUIBase parent, HoverItemSlot onHover, GetItem getItem, float scale) {
			parentGui = parent;
			onHoverAction = onHover;
			getItemFunc = getItem;
			OverallScale = scale;
		}

		public void SetDimensions(int columns, int rows) {
			Dimensions = new Vector2(columns, rows);
		}

		public Vector2 Origin => GetDimensions().Position();
		public Vector2 Spacing { get; set; } = new Vector2(4, 4);
		public Vector2 Dimensions { get; set; }
		public float OverallScale { get; set; } = 1;

		public float ActualHeight => Dimensions.Y * Spacing.Y + Dimensions.Y * Main.inventoryBackTexture.Height * OverallScale;
		public float ActualWidth => Dimensions.X * Spacing.X + Dimensions.X * Main.inventoryBackTexture.Width * OverallScale;

		public int MaxRows => (int)(Main.screenHeight - Origin.Y) / (int)(Main.inventoryBackTexture.Height * Main.UIScale * OverallScale);

		public override void Update(GameTime gameTime) {
			hoverSlot = -1;

			Vector2 mousePos = new Vector2(parentGui.curMouse.X, parentGui.curMouse.Y);

			float slotWidth = Main.inventoryBackTexture.Width * Main.UIScale * OverallScale;
			float slotHeight = Main.inventoryBackTexture.Height * Main.UIScale * OverallScale;

			float actualWidth = slotWidth + Spacing.X * Main.UIScale;
			float actualHeight = slotHeight + Spacing.Y * Main.UIScale;

			if (mousePos.X >= Origin.X * Main.UIScale && mousePos.X < Origin.X * Main.UIScale + (actualWidth * Dimensions.X) &&
				mousePos.Y >= Origin.Y * Main.UIScale && mousePos.Y < Origin.Y * Main.UIScale + (actualHeight * Dimensions.Y)) {
				float normalMouseX = mousePos.X - (Origin.X * Main.UIScale);
				float normalMouseY = mousePos.Y - (Origin.Y * Main.UIScale);

				float actualWidthPosX = normalMouseX % actualWidth;
				float actualHeightPosY = normalMouseY % actualHeight;

				int index = (int)(normalMouseX / actualWidth) + (int)Dimensions.X * (int)(normalMouseY / actualHeight);

				if (actualWidthPosX < slotWidth && actualHeightPosY < slotHeight) {
					onHoverAction(index, ref hoverSlot);
				}
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			int context = 1;
			float s = Main.inventoryScale;
			Main.inventoryScale = OverallScale;
			for (int i = 0; i < Dimensions.X; i++) {
				for (int j = 0; j < Dimensions.Y; j++) {
					context = 1;
					Item item = getItemFunc((int)Dimensions.X * j + i, ref context);
					float x = i * Spacing.X + Origin.X + i * Main.inventoryBackTexture.Width * OverallScale;
					float y = j * Spacing.Y + Origin.Y + j * Main.inventoryBackTexture.Height * OverallScale;
					ItemSlot.Draw(spriteBatch, ref item, context, new Vector2(x, y));
				}
			}
			Main.inventoryScale = s;
		}

		public void DrawText() {
			if (hoverSlot >= 0) {
				int context = 0;
				Item hoverItem = getItemFunc(hoverSlot, ref context);
				if (!hoverItem.IsAir) {
					Main.HoverItem = hoverItem.Clone();
					Main.instance.MouseText(string.Empty);
				}
			}
		}
	}
}