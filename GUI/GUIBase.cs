using MagicStorage.Components;
using Microsoft.Xna.Framework.Input;

namespace MagicStorage.GUI {
	public abstract class GUIBase {
		public bool Active { get; set; }


		public MouseState curMouse;
		public MouseState oldMouse;

		public bool MouseClicked => curMouse.LeftButton == ButtonState.Pressed && oldMouse.LeftButton == ButtonState.Released;

		public abstract void RefreshItems(TEStorageCenter center = null);
	}
}
