using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using MagicStorageTwo.Items.Base;
using Terraria.ID;
using MagicStorageTwo.Components;

namespace MagicStorageTwo.Items {
	public class StorageUnitCrimtane : BaseItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Crimtane Storage Unit");
			DisplayName.AddTranslation(GameCulture.Russian, "Кримтановая Ячейка Хранилища");
			DisplayName.AddTranslation(GameCulture.Polish, "Jednostka magazynująca (Karmazynium)");
			DisplayName.AddTranslation(GameCulture.French, "Unité de stockage (Carmitane)");
			DisplayName.AddTranslation(GameCulture.Spanish, "Unidad de Almacenamiento (Carmesí)");
			DisplayName.AddTranslation(GameCulture.Chinese, "存储单元(血腥)");
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
			item.value = Item.sellPrice(0, 0, 32, 0);
			item.createTile = mod.TileType(nameof(TStorageUnit));
			item.placeStyle = 2;
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(mod.ItemType(nameof(StorageUnit)));
			recipe.AddIngredient(mod.ItemType(nameof(UpgradeCrimtane)));
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
