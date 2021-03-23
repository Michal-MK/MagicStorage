using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace MagicStorageTwo.GUI.Elements {
	public class UIPanelWithChildren : UIPanel {
		public List<UIElement> Children => Elements;
	}
}
