using Terraria.ModLoader;

namespace MagicStorageTwo.Items.Base {
	public abstract class BaseItem : ModItem {
		public override string Texture => "MagicStorageTwo/Textures/Items/" + GetType().Name;
	}
}
