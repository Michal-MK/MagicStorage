using Terraria.DataStructures;

namespace MagicStorageTwo.Extensions {
	public static class Point16Ext {
		/// <summary>
		/// Is the <see cref="Point16"/> made of non-negative elements
		/// </summary>
		public static bool N0(this Point16 point) => point.X >= 0 && point.Y >= 0;
	}
}
