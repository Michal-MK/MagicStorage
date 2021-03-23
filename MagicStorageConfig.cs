using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace MagicStorageTwo {
	public class MagicStorageConfig : ModConfig {
		public override ConfigScope Mode => ConfigScope.ClientSide;


		[DefaultValue(false)]
		[Header("Show debug particles for BFS algorithm")]
		public bool DebugDustParticles;
	}
}
