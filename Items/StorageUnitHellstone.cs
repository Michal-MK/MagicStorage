using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using MagicStorageTwo.Items.Base;
using Terraria.ID;
using MagicStorageTwo.Components;

namespace MagicStorageTwo.Items {
	public class StorageUnitHellstone : BaseItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Hellstone Storage Unit");
			DisplayName.AddTranslation(GameCulture.Russian, "Адская Ячейка Хранилища");
			DisplayName.AddTranslation(GameCulture.Polish, "Jednostka magazynująca (Piekielny kamień)");
			DisplayName.AddTranslation(GameCulture.French, "Unité de stockage (Infernale)");
			DisplayName.AddTranslation(GameCulture.Spanish, "Unidad de Almacenamiento (Piedra Infernal)");
			DisplayName.AddTranslation(GameCulture.Chinese, "存储单元(狱岩)");
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
			item.rare = ItemRarityID.Green;
			item.value = Item.sellPrice(0, 0, 50, 0);
			item.createTile = mod.TileType(nameof(TStorageUnit));
			item.placeStyle = 3;
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(mod.ItemType(nameof(StorageUnitDemonite)));
			recipe.AddIngredient(mod.ItemType(nameof(UpgradeHellstone)));
			recipe.SetResult(this);
			recipe.AddRecipe();

			recipe = new ModRecipe(mod);
			recipe.AddIngredient(mod.ItemType(nameof(StorageUnitCrimtane)));
			recipe.AddIngredient(mod.ItemType(nameof(UpgradeHellstone)));
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
