using Terraria.ModLoader;

namespace MagicStorage.Items.Base {
	public abstract class BaseItem : ModItem {
		public override string Texture => "MagicStorage/Textures/Items/" + GetType().Name;
	}
}
