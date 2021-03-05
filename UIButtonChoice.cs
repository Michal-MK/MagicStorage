using MagicStorage.GUI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Localization;
using Terraria.UI;

namespace MagicStorage {
	public class UIButtonChoice : UIElement {
		public const int BUTTON_SIZE = 32;
		public const int BUTTON_PADDING = 8;

		private readonly Texture2D[] buttons;
		private readonly LocalizedText[] mouseOverTexts;
		private int choice = 0;

		public int Choice {
			get {
				return choice;
			}
		}

		public UIButtonChoice(Texture2D[] _butons, LocalizedText[] mouseText) {
			if (_butons.Length != mouseText.Length || _butons.Length == 0) {
				throw new ArgumentException();
			}
			buttons = _butons;
			mouseOverTexts = mouseText;

			int width = BUTTON_SIZE * _butons.Length + BUTTON_PADDING * (_butons.Length - 1);
			Width.Set(width, 0f);
			MinWidth.Set(width, 0f);
			Height.Set(BUTTON_SIZE, 0f);
			MinHeight.Set(BUTTON_SIZE, 0f);
		}

		public override void Update(GameTime gameTime) {
			int oldChoice = choice;
			if (StorageGUI.MouseClicked && Parent != null) {
				for (int k = 0; k < buttons.Length; k++) {
					if (MouseOverButton(StorageGUI.curMouse.X, StorageGUI.curMouse.Y, k)) {
						choice = k;
						break;
					}
				}
			}
			if (oldChoice != choice) {
				StorageGUI.RefreshItems();
			}
		}

		private bool MouseOverButton(int mouseX, int mouseY, int button) {
			Rectangle dim = InterfaceHelper.GetFullRectangle(this);
			float left = dim.X + button * (BUTTON_SIZE + BUTTON_PADDING) * Main.UIScale;
			float right = left + BUTTON_SIZE * Main.UIScale;
			float top = dim.Y;
			float bottom = top + BUTTON_SIZE * Main.UIScale;
			return mouseX > left && mouseX < right && mouseY > top && mouseY < bottom;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			Texture2D backTexture = MagicStorage.Instance.GetTexture("Textures/Sorting/SortButtonBackground");
			Texture2D backTextureActive = MagicStorage.Instance.GetTexture("Textures/Sorting/SortButtonBackgroundActive");
			CalculatedStyle dim = GetDimensions();
			for (int k = 0; k < buttons.Length; k++) {
				Texture2D texture = k == choice ? backTextureActive : backTexture;
				Vector2 drawPos = new Vector2(dim.X + k * (BUTTON_SIZE + BUTTON_PADDING), dim.Y);
				Color color = MouseOverButton(StorageGUI.curMouse.X, StorageGUI.curMouse.Y, k) ? Color.Silver : Color.White;
				Main.spriteBatch.Draw(texture, drawPos, color);
				Main.spriteBatch.Draw(buttons[k], drawPos + new Vector2(1f), Color.White);
			}
		}

		public void DrawText() {
			for (int k = 0; k < buttons.Length; k++) {
				if (MouseOverButton(StorageGUI.curMouse.X, StorageGUI.curMouse.Y, k)) {
					Main.instance.MouseText(mouseOverTexts[k].Value);
				}
			}
		}
	}
}