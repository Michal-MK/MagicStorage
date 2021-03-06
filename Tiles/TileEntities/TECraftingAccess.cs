using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader.IO;
using Terraria.ID;

namespace MagicStorageTwo.Components {
	public class TECraftingAccess : TEStorageComponent {
		public Item[] stations = new Item[10];

		public TECraftingAccess() {
			for (int k = 0; k < stations.Length; k++) {
				stations[k] = new Item();
			}
		}

		public override bool ValidTile(Tile tile) {
			return tile.type == mod.TileType(nameof(TCraftingAccess)) && tile.frameX == 0 && tile.frameY == 0;
		}

		public void TryDepositStation(Item item) {
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				return;
			}
			foreach (Item station in stations) {
				if (station.type == item.type) {
					return;
				}
			}
			for (int k = 0; k < stations.Length; k++) {
				if (stations[k].IsAir) {
					stations[k] = item.Clone();
					stations[k].stack = 1;
					item.stack--;
					if (item.stack <= 0) {
						item.SetDefaults(0);
					}
					NetHelper.SendTEUpdate(ID, Position);
					return;
				}
			}
		}

		public Item TryWithdrawStation(int slot) {
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				return new Item();
			}
			if (!stations[slot].IsAir) {
				Item item = stations[slot];
				stations[slot] = new Item();
				NetHelper.SendTEUpdate(ID, Position);
				return item;
			}
			return new Item();
		}

		public Item DoStationSwap(Item item, int slot) {
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				return new Item();
			}
			if (!item.IsAir) {
				for (int k = 0; k < stations.Length; k++) {
					if (k != slot && stations[k].type == item.type) {
						return item;
					}
				}
			}
			if ((item.IsAir || item.stack == 1) && !stations[slot].IsAir) {
				Item temp = item;
				item = stations[slot];
				stations[slot] = temp;
				NetHelper.SendTEUpdate(ID, Position);
				return item;
			}
			else if (!item.IsAir && stations[slot].IsAir) {
				stations[slot] = item.Clone();
				stations[slot].stack = 1;
				item.stack--;
				if (item.stack <= 0) {
					item.SetDefaults(0);
				}
				NetHelper.SendTEUpdate(ID, Position);
				return item;
			}
			return item;
		}

		public List<Item> DoCraft(TEStorageHeart heart, List<Item> toWithdraw, Item result) {
			List<Item> items = new List<Item>();
			foreach (Item tryWithdraw in toWithdraw) {
				Item withdrawn = heart.TryWithdraw(tryWithdraw);
				if (!withdrawn.IsAir) {
					items.Add(withdrawn);
				}
				if (withdrawn.stack < tryWithdraw.stack) {
					for (int k = 0; k < items.Count; k++) {
						heart.DepositItem(items[k]);
						if (items[k].IsAir) {
							items.RemoveAt(k);
							k--;
						}
					}
					return items;
				}
			}
			items.Clear();
			heart.DepositItem(result);
			if (!result.IsAir) {
				items.Add(result);
			}
			return items;
		}

		public override TagCompound Save() {
			TagCompound tag = new TagCompound();
			IList<TagCompound> listStations = new List<TagCompound>();
			foreach (Item item in stations) {
				listStations.Add(ItemIO.Save(item));
			}
			tag["Stations"] = listStations;
			return tag;
		}

		public override void Load(TagCompound tag) {
			IList<TagCompound> listStations = tag.GetList<TagCompound>("Stations");
			if (listStations != null && listStations.Count > 0) {
				for (int k = 0; k < stations.Length; k++) {
					stations[k] = ItemIO.Load(listStations[k]);
				}
			}
		}

		public override void NetSend(BinaryWriter writer, bool lightSend) {
			foreach (Item item in stations) {
				ItemIO.Send(item, writer, true, false);
			}
		}

		public override void NetReceive(BinaryReader reader, bool lightReceive) {
			for (int k = 0; k < stations.Length; k++) {
				stations[k] = ItemIO.Receive(reader, true, false);
			}
		}
	}
}