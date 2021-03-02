﻿using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using MagicStorage.Items.Base;
using Terraria.ID;

namespace MagicStorage.Items {
	public class StorageUnitLuminite : BaseItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Luminite Storage Unit");
			DisplayName.AddTranslation(GameCulture.Russian, "Люминитовая Ячейка Хранилища");
			DisplayName.AddTranslation(GameCulture.Polish, "Jednostka magazynująca (Luminowana)");
			DisplayName.AddTranslation(GameCulture.French, "Unité de stockage (Luminite)");
			DisplayName.AddTranslation(GameCulture.Spanish, "Unidad de Almacenamiento (Luminita)");
			DisplayName.AddTranslation(GameCulture.Chinese, "存储单元(夜明)");
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
			item.rare = ItemRarityID.Red;
			item.value = Item.sellPrice(0, 2, 50, 0);
			item.createTile = mod.TileType("StorageUnit");
			item.placeStyle = 6;
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(mod.ItemType("StorageUnitBlueChlorophyte"));
			recipe.AddIngredient(mod.ItemType("UpgradeLuminite"));
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
