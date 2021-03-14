using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using MagicStorage.Items.Base;
using MagicStorage.Components;

namespace MagicStorage.Items {
	public class StorageHeart : BaseItem {
		public override void SetStaticDefaults() {
			DisplayName.AddTranslation(GameCulture.Russian, "Сердце Хранилища");
			DisplayName.AddTranslation(GameCulture.Polish, "Serce Jednostki Magazynującej");
			DisplayName.AddTranslation(GameCulture.French, "Cœur de Stockage");
			DisplayName.AddTranslation(GameCulture.Spanish, "Corazón de Almacenamiento");
			DisplayName.AddTranslation(GameCulture.Chinese, "存储核心");
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
			item.value = Item.sellPrice(0, 1, 35, 0);
			item.createTile = mod.TileType(nameof(TStorageHeart));
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, nameof(StorageComponent));
			recipe.AddRecipeGroup(Constants.ANY_DIA, 3);
			recipe.AddIngredient(ItemID.Emerald, 7);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
