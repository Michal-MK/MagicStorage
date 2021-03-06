using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagicStorageTwo.Items.Base;

namespace MagicStorageTwo.Items {
	public class RadiantJewelDrop : GlobalNPC {
		public override void NPCLoot(NPC npc) {
			if (npc.type == NPCID.MoonLordCore && !Main.expertMode && Main.rand.Next(20) == 0) {
				Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, mod.ItemType(nameof(RadiantJewel)));
			}
		}
	}
}
