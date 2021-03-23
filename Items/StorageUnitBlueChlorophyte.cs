using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using MagicStorageTwo.Items.Base;
using Terraria.ID;
using MagicStorageTwo.Components;

namespace MagicStorageTwo.Items {
	public class StorageUnitBlueChlorophyte : BaseItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Blue Chlorophyte Storage Unit");
			DisplayName.AddTranslation(GameCulture.Russian, "Синяя Хлорофитовая Ячейка Хранилища");
			DisplayName.AddTranslation(GameCulture.Polish, "Jednostka magazynująca (Niebieski Chlorofit)");
			DisplayName.AddTranslation(GameCulture.French, "Unité de stockage (Chlorophylle Bleu)");
			DisplayName.AddTranslation(GameCulture.Spanish, "Unidad de Almacenamiento (Clorofita Azul)");
			DisplayName.AddTranslation(GameCulture.Chinese, "存储单元(蓝色叶绿)");
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
			item.rare = ItemRarityID.Lime;
			item.value = Item.sellPrice(0, 1, 60, 0);
			item.createTile = mod.TileType(nameof(TStorageUnit));
			item.placeStyle = 5;
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(mod.ItemType(nameof(StorageUnitHallowed)));
			recipe.AddIngredient(mod.ItemType(nameof(UpgradeBlueChlorophyte)));
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
