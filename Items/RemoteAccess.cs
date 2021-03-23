using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using MagicStorageTwo.Items.Base;
using MagicStorageTwo.Components;

namespace MagicStorageTwo.Items {
	public class RemoteAccess : BaseItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Remote Storage Access");
			DisplayName.AddTranslation(GameCulture.Russian, "Модуль Удаленного Доступа к Хранилищу");
			DisplayName.AddTranslation(GameCulture.Polish, "Zdalna Jednostka Dostępu");
			DisplayName.AddTranslation(GameCulture.French, "Fenêtre d'accès éloigné");
			DisplayName.AddTranslation(GameCulture.Spanish, "Acceso a Almacenamiento Remoto");
			DisplayName.AddTranslation(GameCulture.Chinese, "远程存储装置");
		}

		public override void SetDefaults() {
			item.width = 26;
			item.height = 26;
			item.maxStack = 99;
			item.useTurn = true;
			item.autoReuse = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.consumable = true;
			item.rare = ItemRarityID.Blue;
			item.value = Item.sellPrice(0, 1, 72, 50);
			item.createTile = mod.TileType(nameof(TRemoteAccess));
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, nameof(StorageComponent));
			recipe.AddRecipeGroup(Constants.ANY_DIA, 3);
			recipe.AddIngredient(ItemID.Ruby, 7);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
