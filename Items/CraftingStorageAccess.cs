using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using MagicStorage.Items.Base;

namespace MagicStorage.Items {
	public class CraftingStorageAccess : BaseItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Storage Crafting And Storage Interface");
			DisplayName.AddTranslation(GameCulture.Russian, "Модуль Создания Предметов");
			DisplayName.AddTranslation(GameCulture.Polish, "Interfejs Rzemieślniczy Magazynu");
			DisplayName.AddTranslation(GameCulture.French, "Interface de Stockage Artisanat");
			DisplayName.AddTranslation(GameCulture.Spanish, "Interfaz de Elaboración de almacenamiento");
			DisplayName.AddTranslation(GameCulture.Chinese, "制作存储单元");
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
			item.rare = ItemRarityID.LightRed;
			item.value = Item.sellPrice(0, 2, 16, 25);
			item.createTile = mod.TileType(nameof(CraftingStorageAccess));
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, nameof(CraftingAccess), 1);
			recipe.AddIngredient(null, nameof(StorageAccess), 1);
			recipe.AddIngredient(ItemID.Wire, 4);
			recipe.AddIngredient(ItemID.HallowedBar, 6);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
