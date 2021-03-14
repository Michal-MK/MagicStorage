using MagicStorage.GUI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace MagicStorage {
	public class UISearchBar : UIElement {
		//TODO this hack
		private static List<UISearchBar> searchBars = new List<UISearchBar>();

		private readonly LocalizedText hintText = Locale.Get(Locale.C.SEARCH);

		private string text = "";
		public string Text => text;

		private int cursorPosition = 0;
		private bool hasFocus = false;
		private int cursorTimer = 0;

		private GUIBase parentGui;

		public UISearchBar(GUIBase parent, LocalizedText defaultText) {
			hintText = defaultText;
			parentGui = parent;
			SetPadding(UICommon.PADDING);
			searchBars.Add(this);
		}

		public void Reset() {
			text = string.Empty;
			cursorPosition = 0;
			hasFocus = false;
			CheckBlockInput();
		}

		public override void Update(GameTime gameTime) {
			cursorTimer++;
			cursorTimer %= 60;

			Rectangle dim = GUIHelper.GetFullRectangle(this);
			//MouseState mouse = StorageGUI.curMouse;
			MouseState mouse = PlayerInput.MouseInfo;
			bool mouseOver = mouse.X > dim.X && mouse.X < dim.X + dim.Width && mouse.Y > dim.Y && mouse.Y < dim.Y + dim.Height;
			
			bool changed = false;
			string prev = text;

			if ((parentGui?.MouseClicked).HasValue && (parentGui?.MouseClicked).Value/* || StorageGUI.MouseClicked*/) {
				if (!hasFocus && mouseOver) {
					hasFocus = true;
					CheckBlockInput();
				}
				else if (hasFocus && !mouseOver) {
					hasFocus = false;
					CheckBlockInput();
					cursorPosition = text.Length;
				}
			}
			//else if (StorageGUI.curMouse.RightButton == ButtonState.Pressed && StorageGUI.oldMouse.RightButton == ButtonState.Released && Parent != null && hasFocus && !mouseOver) {
			//	hasFocus = false;
			//	cursorPosition = text.Length;
			//	CheckBlockInput();
			//}
			//else if (StorageGUI.curMouse.RightButton == ButtonState.Pressed && StorageGUI.oldMouse.RightButton == ButtonState.Released && mouseOver) {
			//	if (text.Length > 0) {
			//		text = string.Empty;
			//		cursorPosition = 0;
			//	}
			//}	
			else if (parentGui.curMouse.RightButton == ButtonState.Pressed && parentGui.oldMouse.RightButton == ButtonState.Released && Parent != null && hasFocus && !mouseOver) {
				hasFocus = false;
				cursorPosition = text.Length;
				CheckBlockInput();
			}
			else if (parentGui.curMouse.RightButton == ButtonState.Pressed && parentGui.oldMouse.RightButton == ButtonState.Released && mouseOver) {
				if (text.Length > 0) {
					text = string.Empty;
					cursorPosition = 0;
					changed = true;
				}
			}

			if (hasFocus) {
				PlayerInput.WritingText = true;
				Main.instance.HandleIME();

				if (cursorPosition < text.Length && text.Length > 0)
					prev = prev.Remove(cursorPosition);

				string newString = Main.GetInputText(prev);

				if (!newString.Equals(prev)) {
					int newStringLength = newString.Length;
					if (prev != text) {
						newString += text.Substring(cursorPosition);
					}
					text = newString;
					cursorPosition = newStringLength;
					changed = true;
				}

				if (KeyTyped(Keys.Delete) && text.Length > 0 && cursorPosition <= text.Length - 1) {
					text = text.Remove(cursorPosition, 1);
					changed = true;
				}
				if (KeyTyped(Keys.Left) && cursorPosition > 0) {
					cursorPosition--;
				}
				if (KeyTyped(Keys.Right) && cursorPosition < text.Length) {
					cursorPosition++;
				}
				if (KeyTyped(Keys.Home)) {
					cursorPosition = 0;
				}
				if (KeyTyped(Keys.End)) {
					cursorPosition = text.Length;
				}
				if ((Main.keyState.IsKeyDown(Keys.LeftControl) || Main.keyState.IsKeyDown(Keys.RightControl)) && KeyTyped(Keys.Back)) {
					text = string.Empty;
					cursorPosition = 0;
					changed = true;
				}
				if ((Main.keyState.IsKeyDown(Keys.LeftControl) || Main.keyState.IsKeyDown(Keys.RightControl)) && KeyTyped(Keys.Delete)) {
					if (cursorPosition == text.Length) return;
					text = text.Remove(cursorPosition);
					changed = true;
				}

				if (changed) {
					parentGui.RefreshItems();
				}

				if (KeyTyped(Keys.Enter) || KeyTyped(Keys.Tab) || KeyTyped(Keys.Escape)) {
					hasFocus = false;
					CheckBlockInput();
				}
			}
			base.Update(gameTime);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			Texture2D texture = ModContent.GetTexture("MagicStorage/Textures/UI/SearchBar");
			CalculatedStyle dim = GetDimensions();
			const int P = UICommon.PADDING;
			int innerWidth = (int)dim.Width - 2 * P;
			int innerHeight = (int)dim.Height - 2 * P;
			spriteBatch.Draw(texture, dim.Position(), new Rectangle(0, 0, P, P), Color.White);
			spriteBatch.Draw(texture, new Rectangle((int)dim.X + P, (int)dim.Y, innerWidth, P), new Rectangle(P, 0, 1, P), Color.White);
			spriteBatch.Draw(texture, new Vector2(dim.X + P + innerWidth, dim.Y), new Rectangle(P + 1, 0, P, P), Color.White);
			spriteBatch.Draw(texture, new Rectangle((int)dim.X, (int)dim.Y + P, P, innerHeight), new Rectangle(0, P, P, 1), Color.White);
			spriteBatch.Draw(texture, new Rectangle((int)dim.X + P, (int)dim.Y + P, innerWidth, innerHeight), new Rectangle(P, P, 1, 1), Color.White);
			spriteBatch.Draw(texture, new Rectangle((int)dim.X + P + innerWidth, (int)dim.Y + P, P, innerHeight), new Rectangle(P + 1, P, P, 1), Color.White);
			spriteBatch.Draw(texture, new Vector2(dim.X, dim.Y + P + innerHeight), new Rectangle(0, P + 1, P, P), Color.White);
			spriteBatch.Draw(texture, new Rectangle((int)dim.X + P, (int)dim.Y + P + innerHeight, innerWidth, P), new Rectangle(P, P + 1, 1, P), Color.White);
			spriteBatch.Draw(texture, new Vector2(dim.X + P + innerWidth, dim.Y + P + innerHeight), new Rectangle(P + 1, P + 1, P, P), Color.White);

			bool isEmpty = text.Length == 0;

			string drawText = isEmpty ? hintText.Value : text;
			Vector2 size = Main.fontMouseText.MeasureString(drawText);
			float scale = innerHeight / size.Y;
			if (isEmpty && hasFocus) {
				drawText = string.Empty;
				isEmpty = false;
			}
			Color color = Color.Black;
			if (isEmpty) {
				color *= 0.75f;
			}
			spriteBatch.DrawString(Main.fontMouseText, drawText, new Vector2(dim.X + P, dim.Y + P + P), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
			if (!isEmpty && hasFocus && cursorTimer < 30) {
				float drawCursor = Main.fontMouseText.MeasureString(drawText.Substring(0, cursorPosition)).X * scale;
				spriteBatch.DrawString(Main.fontMouseText, "|", new Vector2(dim.X + P + drawCursor - 2, dim.Y + P + P), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
			}
		}

		public bool KeyTyped(Keys key) {
			return Main.keyState.IsKeyDown(key) && !Main.oldKeyState.IsKeyDown(key);
		}

		private static void CheckBlockInput() {
			Main.blockInput = false;
			foreach (UISearchBar searchBar in searchBars) {
				if (searchBar.hasFocus) {
					Main.blockInput = true;
					break;
				}
			}
		}
	}
}