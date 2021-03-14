using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace MagicStorage.Components {
	public abstract class TEStorageCenter : TEStorageComponent {
		public List<Point16> storageUnits = new List<Point16>();
		public List<TCraftingTileSocket> sockets = new List<TCraftingTileSocket>();

		public void ResetAndSearch() {
			List<Point16> oldStorageUnits = new List<Point16>(storageUnits);
			storageUnits.Clear();
			HashSet<Point16> hashStorageUnits = new HashSet<Point16>();
			HashSet<Point16> explored = new HashSet<Point16>();
			explored.Add(Position);
			Queue<Point16> toExplore = new Queue<Point16>();
			foreach (Point16 point in AdjacentComponents()) {
				toExplore.Enqueue(point);
			}
			bool changed = false;

			while (toExplore.Count > 0) {
				Point16 explore = toExplore.Dequeue();
				if (!explored.Contains(explore) && explore != TStorageComponent.killTile) {
					explored.Add(explore);
					if (TileEntity.ByPosition.ContainsKey(explore) && TileEntity.ByPosition[explore] is TEAbstractStorageUnit) {
						TEAbstractStorageUnit storageUnit = (TEAbstractStorageUnit)TileEntity.ByPosition[explore];
						if (storageUnit.Link(Position)) {
							NetHelper.SendTEUpdate(storageUnit.ID, storageUnit.Position);
							changed = true;
						}
						storageUnits.Add(explore);
						hashStorageUnits.Add(explore);
					}
					foreach (Point16 point in AdjacentComponents(explore)) {
						toExplore.Enqueue(point);
					}
				}
			}

			sockets = GetCraftingSockets();

			foreach (Point16 oldStorageUnit in oldStorageUnits) {
				if (!hashStorageUnits.Contains(oldStorageUnit)) {
					if (TileEntity.ByPosition.ContainsKey(oldStorageUnit) && TileEntity.ByPosition[oldStorageUnit] is TEAbstractStorageUnit) {
						TileEntity storageUnit = TileEntity.ByPosition[oldStorageUnit];
						((TEAbstractStorageUnit)storageUnit).Unlink();
						NetHelper.SendTEUpdate(storageUnit.ID, storageUnit.Position);
					}
					changed = true;
				}
			}

			if (changed) {
				TEStorageHeart heart = GetHeart();
				if (heart != null) {
					heart.ResetCompactStage();
				}
				NetHelper.SendTEUpdate(ID, Position);
				if (Main.netMode != NetmodeID.Server) {
					MagicStorage.Instance.guiM.Refresh();
				}
			}
		}

		protected List<TCraftingTileSocket> GetCraftingSockets() {
			List<TCraftingTileSocket> sockets = new List<TCraftingTileSocket>();
			HashSet<Point16> explored = new HashSet<Point16>();
			explored.Add(Position);
			Queue<Point16> toExplore = new Queue<Point16>();
			foreach (Point16 point in AdjacentComponents(Position)) {
				if (!explored.Contains(point)) {
					toExplore.Enqueue(point);
				}
			}
			while (toExplore.Count != 0) {
				Point16 explore = toExplore.Dequeue();
				if (!explored.Contains(explore) && explore != TStorageComponent.killTile) {
					explored.Add(explore);
					if (Main.tile[explore.X, explore.Y].type == ModContent.GetInstance<TCraftingTileSocket>().Type ||
						Main.tile[explore.X, explore.Y].type == ModContent.GetInstance<TCraftingTileSocketLarge>().Type) {
						TCraftingTileSocket cts = new TCraftingTileSocket(explore);
						if (cts.GetItemTypeFromTileAbove() != -1) {
							sockets.Add(cts);
						}
					}
					foreach (Point16 point in AdjacentComponents(explore)) {
						if (!explored.Contains(point)) {
							toExplore.Enqueue(point);
						}
					}
				}

			}
			return sockets;
		}

		public override void OnPlace() {
			ResetAndSearch();
		}

		public override void OnKill() {
			foreach (Point16 storageUnit in storageUnits) {
				TEAbstractStorageUnit unit = (TEAbstractStorageUnit)TileEntity.ByPosition[storageUnit];
				unit.Unlink();
				NetHelper.SendTEUpdate(unit.ID, unit.Position);
			}
		}

		public abstract TEStorageHeart GetHeart();

		public static bool IsStorageCenter(Point16 point) {
			return TileEntity.ByPosition.ContainsKey(point) && TileEntity.ByPosition[point] is TEStorageCenter;
		}

		public override TagCompound Save() {
			TagCompound tag = new TagCompound();
			List<TagCompound> tagUnits = new List<TagCompound>();
			foreach (Point16 storageUnit in storageUnits) {
				TagCompound tagUnit = new TagCompound();
				tagUnit.Set("X", storageUnit.X);
				tagUnit.Set("Y", storageUnit.Y);
				tagUnits.Add(tagUnit);
			}
			tag.Set("StorageUnits", tagUnits);

			List<TagCompound> tagSockets = new List<TagCompound>();
			foreach (TCraftingTileSocket socket in sockets) {
				TagCompound tagSocket = new TagCompound();
				tagSocket.Set("X", socket.Position.X);
				tagSocket.Set("Y", socket.Position.Y);
				tagSockets.Add(tagSocket);
			}
			tag.Set("CraftingSockets", tagSockets);
			return tag;
		}

		public override void Load(TagCompound tag) {
			foreach (TagCompound tagUnit in tag.GetList<TagCompound>("StorageUnits")) {
				storageUnits.Add(new Point16(tagUnit.GetShort("X"), tagUnit.GetShort("Y")));
			}
			foreach (TagCompound tagUnit in tag.GetList<TagCompound>("CraftingSockets")) {
				sockets.Add(new TCraftingTileSocket(new Point16(tagUnit.GetShort("X"), tagUnit.GetShort("Y"))));
			}
		}

		public override void NetSend(BinaryWriter writer, bool lightSend) {
			writer.Write((short)storageUnits.Count);
			foreach (Point16 storageUnit in storageUnits) {
				writer.Write(storageUnit.X);
				writer.Write(storageUnit.Y);
			}
			writer.Write((short)sockets.Count);
			foreach (TCraftingTileSocket socket in sockets) {
				writer.Write(socket.Position.X);
				writer.Write(socket.Position.Y);
			}
		}

		public override void NetReceive(BinaryReader reader, bool lightReceive) {
			int count = reader.ReadInt16();
			for (int k = 0; k < count; k++) {
				storageUnits.Add(new Point16(reader.ReadInt16(), reader.ReadInt16()));
			}
			count = reader.ReadInt16();
			for (int k = 0; k < count; k++) {
				sockets.Add(new TCraftingTileSocket(new Point16(reader.ReadInt16(), reader.ReadInt16())));
			}

			MagicStorage.Instance.guiM.Refresh(this);
		}
	}
}