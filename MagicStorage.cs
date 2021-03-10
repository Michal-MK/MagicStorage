using MagicStorage.GUI;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using System.Linq;
using MagicStorage.Components;

namespace MagicStorage {
	public class MagicStorage : Mod {
		public static MagicStorage Instance;
		public static Mod bluemagicMod;
		public static Mod legendMod;

		public static readonly Version requiredVersion = new Version(0, 11);


		public override void Load() {
			if (ModLoader.version < requiredVersion) {
				throw new Exception("Magic storage requires a tModLoader version of at least " + requiredVersion);
			}
			Instance = this;
			InterfaceHelper.Initialize();
			legendMod = ModLoader.GetMod("LegendOfTerraria3");
			bluemagicMod = ModLoader.GetMod("Bluemagic");
			AddTranslations();
			guiM = new GUIManager();
		}

		public static Dictionary<int, int> tilesToItems = new Dictionary<int, int>();

		public override void PostSetupContent() {
			for (int i = 0; i < ItemLoader.ItemCount; i++) {
				Item item = new Item();
				item.SetDefaults(i);
				if (!tilesToItems.ContainsKey(item.createTile)) {
					tilesToItems.Add(item.createTile, item.type);
				}
			}
			StorageConnector.SetupConnectors();
		}

		public override void Unload() {
			Instance = null;
			bluemagicMod = null;
			legendMod = null;
			tilesToItems.Clear();
			//StorageGUI.Unload();
			//CraftingGUI.Unload();
		}

		private void AddTranslations() {
			ModTranslation text = CreateTranslation("SetTo");
			text.SetDefault("Set to: X={0}, Y={1}");
			text.AddTranslation(GameCulture.Polish, "Ustawione na: X={0}, Y={1}");
			text.AddTranslation(GameCulture.French, "Mis à: X={0}, Y={1}");
			text.AddTranslation(GameCulture.Spanish, "Ajustado a: X={0}, Y={1}");
			text.AddTranslation(GameCulture.Chinese, "已设置为: X={0}, Y={1}");
			AddTranslation(text);

			text = CreateTranslation("SnowBiomeBlock");
			text.SetDefault("Snow Biome Block");
			text.AddTranslation(GameCulture.French, "Bloc de biome de neige");
			text.AddTranslation(GameCulture.Spanish, "Bloque de Biomas de la Nieve");
			text.AddTranslation(GameCulture.Chinese, "雪地环境方块");
			AddTranslation(text);

			text = CreateTranslation("DepositAll");
			text.SetDefault("Deposit All");
			text.AddTranslation(GameCulture.Russian, "Переместить всё");
			text.AddTranslation(GameCulture.French, "Déposer tout");
			text.AddTranslation(GameCulture.Spanish, "Depositar todo");
			text.AddTranslation(GameCulture.Chinese, "全部存入");
			AddTranslation(text);

			text = CreateTranslation("Search");
			text.SetDefault("Search");
			text.AddTranslation(GameCulture.Russian, "Поиск");
			text.AddTranslation(GameCulture.French, "Rechercher");
			text.AddTranslation(GameCulture.Spanish, "Buscar");
			text.AddTranslation(GameCulture.Chinese, "搜索");
			AddTranslation(text);

			text = CreateTranslation("SearchName");
			text.SetDefault("Search Name");
			text.AddTranslation(GameCulture.Russian, "Поиск по имени");
			text.AddTranslation(GameCulture.French, "Recherche par nom");
			text.AddTranslation(GameCulture.Spanish, "búsqueda por nombre");
			text.AddTranslation(GameCulture.Chinese, "搜索名称");
			AddTranslation(text);

			text = CreateTranslation("SearchMod");
			text.SetDefault("Search Mod");
			text.AddTranslation(GameCulture.Russian, "Поиск по моду");
			text.AddTranslation(GameCulture.French, "Recherche par mod");
			text.AddTranslation(GameCulture.Spanish, "búsqueda por mod");
			text.AddTranslation(GameCulture.Chinese, "搜索模组");
			AddTranslation(text);

			text = CreateTranslation("SortDefault");
			text.SetDefault("Default Sorting");
			text.AddTranslation(GameCulture.Russian, "Стандартная сортировка");
			text.AddTranslation(GameCulture.French, "Tri Standard");
			text.AddTranslation(GameCulture.Spanish, "Clasificación por defecto");
			text.AddTranslation(GameCulture.Chinese, "默认排序");
			AddTranslation(text);

			text = CreateTranslation("SortID");
			text.SetDefault("Sort by ID");
			text.AddTranslation(GameCulture.Russian, "Сортировка по ID");
			text.AddTranslation(GameCulture.French, "Trier par ID");
			text.AddTranslation(GameCulture.Spanish, "Ordenar por ID");
			text.AddTranslation(GameCulture.Chinese, "按ID排序");
			AddTranslation(text);

			text = CreateTranslation("SortName");
			text.SetDefault("Sort by Name");
			text.AddTranslation(GameCulture.Russian, "Сортировка по имени");
			text.AddTranslation(GameCulture.French, "Trier par nom");
			text.AddTranslation(GameCulture.Spanish, "Ordenar por nombre");
			text.AddTranslation(GameCulture.Chinese, "按名称排序");
			AddTranslation(text);

			text = CreateTranslation("SortStack");
			text.SetDefault("Sort by Stacks");
			text.AddTranslation(GameCulture.Russian, "Сортировка по стакам");
			text.AddTranslation(GameCulture.French, "Trier par piles");
			text.AddTranslation(GameCulture.Spanish, "Ordenar por pilas");
			text.AddTranslation(GameCulture.Chinese, "按堆栈排序");
			AddTranslation(text);

			text = CreateTranslation("FilterAll");
			text.SetDefault("Filter All");
			text.AddTranslation(GameCulture.Russian, "Фильтр (Всё)");
			text.AddTranslation(GameCulture.French, "Filtrer tout");
			text.AddTranslation(GameCulture.Spanish, "Filtrar todo");
			text.AddTranslation(GameCulture.Chinese, "筛选全部");
			AddTranslation(text);

			text = CreateTranslation("FilterWeapons");
			text.SetDefault("Filter Weapons");
			text.AddTranslation(GameCulture.Russian, "Фильтр (Оружия)");
			text.AddTranslation(GameCulture.French, "Filtrer par armes");
			text.AddTranslation(GameCulture.Spanish, "Filtrar por armas");
			text.AddTranslation(GameCulture.Chinese, "筛选武器");
			AddTranslation(text);

			text = CreateTranslation("FilterTools");
			text.SetDefault("Filter Tools");
			text.AddTranslation(GameCulture.Russian, "Фильтр (Инструменты)");
			text.AddTranslation(GameCulture.French, "Filtrer par outils");
			text.AddTranslation(GameCulture.Spanish, "Filtrar por herramientas");
			text.AddTranslation(GameCulture.Chinese, "筛选工具");
			AddTranslation(text);

			text = CreateTranslation("FilterEquips");
			text.SetDefault("Filter Equipment");
			text.AddTranslation(GameCulture.Russian, "Фильтр (Снаряжения)");
			text.AddTranslation(GameCulture.French, "Filtrer par Équipement");
			text.AddTranslation(GameCulture.Spanish, "Filtrar por equipamiento");
			text.AddTranslation(GameCulture.Chinese, "筛选装备");
			AddTranslation(text);

			text = CreateTranslation("FilterPotions");
			text.SetDefault("Filter Potions");
			text.AddTranslation(GameCulture.Russian, "Фильтр (Зелья)");
			text.AddTranslation(GameCulture.French, "Filtrer par potions");
			text.AddTranslation(GameCulture.Spanish, "Filtrar por poción");
			text.AddTranslation(GameCulture.Chinese, "筛选药水");
			AddTranslation(text);

			text = CreateTranslation("FilterTiles");
			text.SetDefault("Filter Placeables");
			text.AddTranslation(GameCulture.Russian, "Фильтр (Размещаемое)");
			text.AddTranslation(GameCulture.French, "Filtrer par placeable");
			text.AddTranslation(GameCulture.Spanish, "Filtrar por metido");
			text.AddTranslation(GameCulture.Chinese, "筛选放置物");
			AddTranslation(text);

			text = CreateTranslation("FilterMisc");
			text.SetDefault("Filter Misc");
			text.AddTranslation(GameCulture.Russian, "Фильтр (Разное)");
			text.AddTranslation(GameCulture.French, "Filtrer par miscellanées");
			text.AddTranslation(GameCulture.Spanish, "Filtrar por otros");
			text.AddTranslation(GameCulture.Chinese, "筛选杂项");
			AddTranslation(text);

			text = CreateTranslation("CraftingStations");
			text.SetDefault("Crafting Stations");
			text.AddTranslation(GameCulture.Russian, "Станции создания");
			text.AddTranslation(GameCulture.French, "Stations d'artisanat");
			text.AddTranslation(GameCulture.Spanish, "Estaciones de elaboración");
			text.AddTranslation(GameCulture.Chinese, "制作站");
			AddTranslation(text);

			text = CreateTranslation("Recipes");
			text.SetDefault("Recipes");
			text.AddTranslation(GameCulture.Russian, "Рецепты");
			text.AddTranslation(GameCulture.French, "Recettes");
			text.AddTranslation(GameCulture.Spanish, "Recetas");
			text.AddTranslation(GameCulture.Chinese, "合成配方");
			AddTranslation(text);

			text = CreateTranslation("SelectedRecipe");
			text.SetDefault("Selected Recipe");
			text.AddTranslation(GameCulture.French, "Recette sélectionnée");
			text.AddTranslation(GameCulture.Spanish, "Receta seleccionada");
			text.AddTranslation(GameCulture.Chinese, "选择配方");
			AddTranslation(text);

			text = CreateTranslation("Ingredients");
			text.SetDefault("Ingredients");
			text.AddTranslation(GameCulture.French, "Ingrédients");
			text.AddTranslation(GameCulture.Spanish, "Ingredientes");
			text.AddTranslation(GameCulture.Chinese, "材料");
			AddTranslation(text);

			text = CreateTranslation("StoredItems");
			text.SetDefault("Stored Ingredients");
			text.AddTranslation(GameCulture.French, "Ingrédients Stockés");
			text.AddTranslation(GameCulture.Spanish, "Ingredientes almacenados");
			text.AddTranslation(GameCulture.Chinese, "存储中的材料");
			AddTranslation(text);

			text = CreateTranslation("RecipeAvailable");
			text.SetDefault("Show available recipes");
			text.AddTranslation(GameCulture.French, "Afficher les recettes disponibles");
			text.AddTranslation(GameCulture.Spanish, "Mostrar recetas disponibles");
			text.AddTranslation(GameCulture.Chinese, "显示可合成配方");
			AddTranslation(text);

			text = CreateTranslation("RecipeAll");
			text.SetDefault("Show all recipes");
			text.AddTranslation(GameCulture.French, "Afficher toutes les recettes");
			text.AddTranslation(GameCulture.Spanish, "Mostrar todas las recetas");
			text.AddTranslation(GameCulture.Chinese, "显示全部配方");
			AddTranslation(text);

			text = CreateTranslation("Items");
			text.SetDefault("Items");
			text.AddTranslation(GameCulture.French, "Articles");
			text.AddTranslation(GameCulture.Spanish, "Artículos");
			text.AddTranslation(GameCulture.Chinese, "项目");
			AddTranslation(text);

			text = CreateTranslation("Loading");
			text.SetDefault("Loading");
			text.AddTranslation(GameCulture.French, "Chargement");
			text.AddTranslation(GameCulture.Spanish, "Cargando");
			text.AddTranslation(GameCulture.Chinese, "载入中");
			AddTranslation(text);

			text = CreateTranslation("NotConnectedToSH");
			text.SetDefault("This access point is not connected to a Storage Heart!");
			text.AddTranslation(GameCulture.French, "Ce point d'accès n'est pas connecté à un Storage Heart!");
			text.AddTranslation(GameCulture.Spanish, "¡Este punto de acceso no está conectado a un Storage Heart!");
			text.AddTranslation(GameCulture.Chinese, "该访问点未连接到Storage Heart！");
			AddTranslation(text);

			text = CreateTranslation("LocatorSavedSuccess");
			text.SetDefault("Locator configuration saved successfully!");
			text.AddTranslation(GameCulture.French, "La configuration du localisateur a été enregistrée avec succès!");
			text.AddTranslation(GameCulture.Spanish, "¡La configuración del localizador se guardó correctamente!");
			text.AddTranslation(GameCulture.Chinese, "定位器配置成功保存！");
			AddTranslation(text);

			text = CreateTranslation("Active");
			text.SetDefault("Active");
			text.AddTranslation(GameCulture.French, "Actif");
			text.AddTranslation(GameCulture.Spanish, "Activo");
			text.AddTranslation(GameCulture.Chinese, "积极的");
			AddTranslation(text);

			text = CreateTranslation("Inactive");
			text.SetDefault("Inactive");
			text.AddTranslation(GameCulture.French, "Inactif");
			text.AddTranslation(GameCulture.Spanish, "Inactivo");
			text.AddTranslation(GameCulture.Chinese, "不活跃");
			AddTranslation(text);
		}

		public override void AddRecipeGroups() {
			IEnumerable<int> chests = typeof(ItemID).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase)
				.Where(w => w.Name.Contains("Chest") && !w.Name.Contains("Fake_") && !w.Name.Contains("Statue"))
				.Select(s => (int)(short)s.GetValue(null));
			RecipeGroup anyChest = new RecipeGroup(() => Language.GetText(Constants.ANY).Value + " Chest", chests.ToArray());
			RecipeGroup.RegisterGroup(Constants.ANY_CHEST, anyChest);

			string anySnowText = $"{Language.GetText(Constants.ANY).Value} {Language.GetTextValue("Mods.MagicStorage.SnowBiomeBlock")}";
			RecipeGroup anySnowBiomeBlock = new RecipeGroup(() => anySnowText, ItemID.SnowBlock, ItemID.IceBlock, ItemID.PurpleIceBlock, ItemID.PinkIceBlock);
			if (bluemagicMod != null) {
				anySnowBiomeBlock.ValidItems.Add(bluemagicMod.ItemType("DarkBlueIce"));
			}
			RecipeGroup.RegisterGroup(Constants.ANY_SNOWBLOCK, anySnowBiomeBlock);

			RecipeGroup anyDiamod = new RecipeGroup(() => Any(ItemID.Diamond), ItemID.Diamond, ItemType("ShadowDiamond"));
			if (legendMod != null) {
				anyDiamod.ValidItems.Add(legendMod.ItemType("GemChrysoberyl"));
				anyDiamod.ValidItems.Add(legendMod.ItemType("GemAlexandrite"));
			}
			RecipeGroup.RegisterGroup(Constants.ANY_DIA, anyDiamod);

			if (legendMod != null) {
				RecipeGroup amethyst = new RecipeGroup(() => Any(ItemID.Amethyst), ItemID.Amethyst, legendMod.ItemType("GemOnyx"), legendMod.ItemType("GemSpinel"));
				RecipeGroup.RegisterGroup(Constants.ANY_AME, amethyst);

				RecipeGroup topaz = new RecipeGroup(() => Any(ItemID.Topaz), ItemID.Topaz, legendMod.ItemType("GemGarnet"));
				RecipeGroup.RegisterGroup(Constants.ANY_TOPA, topaz);
				
				RecipeGroup sapphire = new RecipeGroup(() => Any(ItemID.Sapphire), ItemID.Sapphire, legendMod.ItemType("GemCharoite"));
				RecipeGroup.RegisterGroup(Constants.ANY_SAPH, sapphire);
				
				RecipeGroup emerald = new RecipeGroup(() => Any(ItemID.Emerald), legendMod.ItemType("GemPeridot"));
				RecipeGroup.RegisterGroup(Constants.ANY_EMER, emerald);
				
				RecipeGroup ruby = new RecipeGroup(() => Any(ItemID.Ruby), ItemID.Ruby, legendMod.ItemType("GemOpal"));
				RecipeGroup.RegisterGroup(Constants.ANY_RUBY, ruby);
			}

			string Any(int itemID) => $"{Language.GetText(Constants.ANY).Value} {Lang.GetItemNameValue(itemID)}";
		}

		public GUIManager guiM;

		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			NetHelper.HandlePacket(reader, whoAmI);
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			//InterfaceHelper.ModifyInterfaceLayers(layers);
			guiM.UILayersHook(layers);
		}

		public override void PostUpdateInput() {
			if (!Main.instance.IsActive) {
				return;
			}

			guiM?.Update(null);
			//StorageGUI.Update(null);
			//CraftingGUI.Update(null);
		}
	}
}

