using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicStorage.GUI {
	public abstract class GUIBase {

		public MouseState curMouse;
		public MouseState oldMouse;

		public bool MouseClicked => curMouse.LeftButton == ButtonState.Pressed && oldMouse.LeftButton == ButtonState.Released;
	}
}
