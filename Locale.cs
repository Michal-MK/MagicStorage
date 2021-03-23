using Terraria.Localization;

namespace MagicStorageTwo {
	public class Locale {

		public static string GetStr(string code) {
			return Get(code).Value;
		}

		public static LocalizedText Get(string code) {
			return Language.GetText(code);
		}

		public static class C {
			private const string BASE = "Mods.MagicStorageTwo.";

			public const string SAERCH_NAME = BASE + "SearchName";
			public const string SAERCH_MOD = BASE + "SearchMod";
			public const string CRAFT_STATIONS = BASE + "CraftingStations";
			public const string RECIPES = BASE + "Recipes";
			public const string ITEMS = BASE + "Items";
			public const string SEL_RECIPE = BASE + "SelectedRecipe";
			public const string INGREDS = BASE + "Ingredients";
			public const string STORED_ITEMS = BASE + "StoredItems";
			public const string SEARCH = BASE + "Search";


			public const string SORT_DEF = BASE + "SortDefault";
			public const string SORT_ID = BASE + "SortID";
			public const string SORT_NAME = BASE + "SortName";
			public const string SORT_STACK = BASE + "SortStack";

			public const string RECIPE_AVAIL = BASE + "RecipeAvailable";
			public const string RECIPE_ALL = BASE + "RecipeAll";


			public const string FILTER_ALL = BASE + "FilterAll";
			public const string FILTER_WEAP = BASE + "FilterWeapons";
			public const string FILTER_TOOL = BASE + "FilterTools";
			public const string FILTER_EQUIP = BASE + "FilterEquips";
			public const string FILTER_POT = BASE + "FilterPotions";
			public const string FILTER_TILE = BASE + "FilterTiles";
			public const string FILTER_MISC = BASE + "FilterMisc";

			public const string LOADING = BASE + "Loading";
			public const string DEPOSIT_ALL = BASE + "DepositAll";

			public const string NOT_CONNECTED = BASE + "NotConnectedToSH";
			public const string LOCATOR_SUCCESS = BASE + "LocatorSavedSuccess";

			public const string ACTIVE = BASE + "Active";
			public const string INACTIVE = BASE + "Inactive";

			public static class L {
				public const string REQUIRED_OBJS = "LegacyInterface.22";
				public const string CRAFT = "LegacyMisc.72";
				public const string ANY = "LegacyMisc.37";
				public const string FRAGMENT = "LegacyMisc.51";
				public const string PPLATE = "LegacyMisc.38";

				public const string WATER = "LegacyInterface.53";
				public const string HONEY = "LegacyInterface.58";
				public const string LAVA = "LegacyInterface.56";
				public const string NONE = "LegacyInterface.23";
				public const string COLD_WEATHER = "LegacyInterface.123";
			}
		}
	}
}
