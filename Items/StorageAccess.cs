using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using MagicStorageTwo.Items.Base;
using MagicStorageTwo.Components;

namespace MagicStorageTwo.Items {
	public class StorageAccess : BaseItem {
		public override void SetStaticDefaults() {
			DisplayName.AddTranslation(GameCulture.Russian, "Модуль Доступа к Хранилищу");
			DisplayName.AddTranslation(GameCulture.Polish, "Okno dostępu do magazynu");
			DisplayName.AddTranslation(GameCulture.French, "Access de Stockage");
			DisplayName.AddTranslation(GameCulture.Spanish, "Acceso de Almacenamiento");
			DisplayName.AddTranslation(GameCulture.Chinese, "存储装置");
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
			item.value = Item.sellPrice(0, 0, 67, 50);
			item.createTile = mod.TileType(nameof(TStorageAccess));
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, nameof(StorageComponent));
			recipe.AddRecipeGroup(Constants.ANY_DIA, 1);
			recipe.AddIngredient(ItemID.Topaz, 7);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
