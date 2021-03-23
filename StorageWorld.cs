using MagicStorageTwo.Extensions;
using System.Linq;
using System.Reflection;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace MagicStorageTwo {
	public class StorageWorld : ModWorld {
		private const int saveVersion = 0;
		public static bool kingSlimeDiamond = false;
		public static bool boss1Diamond = false;
		public static bool boss2Diamond = false;
		public static bool boss3Diamond = false;
		public static bool queenBeeDiamond = false;
		public static bool hardmodeDiamond = false;
		public static bool mechBoss1Diamond = false;
		public static bool mechBoss2Diamond = false;
		public static bool mechBoss3Diamond = false;
		public static bool plantBossDiamond = false;
		public static bool golemBossDiamond = false;
		public static bool fishronDiamond = false;
		public static bool ancientCultistDiamond = false;
		public static bool moonlordDiamond = false;

		public override void Initialize() {
			GetType().GetFields(BindingFlags.Public | BindingFlags.Static)
				.Where(w => w.FieldType == typeof(bool) && w.Name.Contains("Diamond"))
				.ForEach(f => f.SetValue(null, false));
		}

		public override TagCompound Save() {
			TagCompound tag = new TagCompound();
			tag[nameof(saveVersion)] = saveVersion;

			PropertyInfo tagProp = tag.GetType().GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
			GetType().GetFields(BindingFlags.Public | BindingFlags.Static)
				.Where(w => w.FieldType == typeof(bool) && w.Name.Contains("Diamond"))
				.ForEach(f => tagProp.SetValue(tag, f.GetValue(null), new[] { f.Name }));
			return tag;
		}

		public override void Load(TagCompound tag) {
			GetType().GetFields(BindingFlags.Public | BindingFlags.Static)
				.Where(w => w.FieldType == typeof(bool) && w.Name.Contains("Diamond"))
				.ForEach(f => f.SetValue(null, tag.GetBool(f.Name)));
		}
	}
}
