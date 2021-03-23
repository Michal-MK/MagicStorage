using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace MagicStorageTwo.Components {
	public abstract class TEStorageComponent : ModTileEntity {

		public abstract bool ValidTile(Tile tile);

		public override bool ValidTile(int i, int j) {
			Tile tile = Main.tile[i, j];
			return tile.active() && ValidTile(tile);
		}

		public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction) {
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				NetHelper.SendComponentPlace(i, j - 1, Type);
				return -1;
			}
			int id = Place(i, j - 1);
			((TEStorageComponent)TileEntity.ByID[id]).OnPlace();
			return id;
		}

		public static int Hook_AfterPlacement_NoEntity(int i, int j, int type, int style, int direction) {
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				NetMessage.SendTileRange(Main.myPlayer, i, j - 1, 2, 2);
				NetHelper.SendSearchAndRefresh(i, j - 1);
				return 0;
			}
			SearchAndRefreshNetwork(new Point16(i, j - 1));
			return 0;
		}

		public virtual void OnPlace() {
			SearchAndRefreshNetwork(Position);
		}

		public override void OnKill() {
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				NetHelper.SendSearchAndRefresh(Position.X, Position.Y);
				MagicStorageTwo.Instance.guiM.Refresh();
			}
			else {
				SearchAndRefreshNetwork(Position);
			}
		}

		private static Point16[] checkNeighbors2x2 = new Point16[] {
			new Point16(-1, 0),
			new Point16(-1, 1),
			new Point16(0, -1),
			new Point16(1, -1),
			new Point16(2, 0),
			new Point16(2, 1),
			new Point16(1, 2),
			new Point16(0, 2)
		};

		private static Point16[] checkNeighbors1x1 = new Point16[] {
			new Point16(-1, 0),
			new Point16(0, -1),
			new Point16(1, 0),
			new Point16(0, 1)
		};

		private static Point16[] checkNeighbors2x1 = new Point16[] {
			new Point16(0, 1),
			new Point16(1, 1),
		};

		private static Point16[] checkNeighbors3x1 = new Point16[] {
			new Point16(0, 1),
			new Point16(1, 1),
			new Point16(2, 1),
		};

		public IEnumerable<Point16> AdjacentComponents() {
			return AdjacentComponents(Position);
		}

		public static IEnumerable<Point16> AdjacentComponents(Point16 point, bool isConnectorCanPlace = false) {
			List<Point16> points = new List<Point16>();
			bool isConnector = Main.tile[point.X, point.Y].type == MagicStorageTwo.Instance.TileType(nameof(TStorageConnector)) || isConnectorCanPlace;
			bool isLargeSocket = Main.tile[point.X, point.Y].type == MagicStorageTwo.Instance.TileType(nameof(TCraftingTileSocketLarge));
			bool isSocket = Main.tile[point.X, point.Y].type == MagicStorageTwo.Instance.TileType(nameof(TCraftingTileSocket));

			foreach (Point16 add in (isConnector ? checkNeighbors1x1 : isLargeSocket ? checkNeighbors3x1 : isSocket ? checkNeighbors2x1 : checkNeighbors2x2)) {
				int checkX = point.X + add.X;
				int checkY = point.Y + add.Y;
				Tile tile = Main.tile[checkX, checkY];
				if (ModContent.GetInstance<MagicStorageConfig>().DebugDustParticles) {
					Dust.NewDust(new Vector2(checkX * 16 + 4, checkY * 16 + 4), 1, 1, 
						isConnector ? 60 : isLargeSocket || isSocket ? 61 : 62, Alpha: 0, Scale: 0.3f);
				}
				if (!tile.active()) {
					continue;
				}
				if (TileLoader.GetTile(tile.type) is TStorageComponent) {
					if (tile.frameX == 36 && !(TileLoader.GetTile(tile.type) is TStorageUnit)) {
						checkX -= 2;
					}
					if (tile.frameX % 36 == 18) {
						checkX--;
					}
					if (tile.frameY % 36 == 18) {
						checkY--;
					}
					Point16 check = new Point16(checkX, checkY);
					if (!points.Contains(check)) {
						points.Add(check);
					}
				}
				else if (tile.type == MagicStorageTwo.Instance.TileType(nameof(TStorageConnector))) {
					Point16 check = new Point16(checkX, checkY);
					if (!points.Contains(check)) {
						points.Add(check);
					}
				}
			}
			return points;
		}

		public static Point16 FindStorageCenter(Point16 startSearch) {
			HashSet<Point16> explored = new HashSet<Point16>();
			explored.Add(startSearch);
			Queue<Point16> toExplore = new Queue<Point16>();
			foreach (Point16 point in AdjacentComponents(startSearch)) {
				toExplore.Enqueue(point);
			}

			while (toExplore.Count > 0) {
				Point16 explore = toExplore.Dequeue();
				if (!explored.Contains(explore) && explore != TStorageComponent.killTile) {
					explored.Add(explore);
					if (TEStorageCenter.IsStorageCenter(explore)) {
						return explore;
					}
					foreach (Point16 point in AdjacentComponents(explore)) {
						toExplore.Enqueue(point);
					}
				}
			}
			return new Point16(-1, -1);
		}

		public override void OnNetPlace() {
			OnPlace();
			NetHelper.SendTEUpdate(ID, Position);
		}

		public static void SearchAndRefreshNetwork(Point16 position) {
			Point16 center = FindStorageCenter(position);
			if (center.X >= 0 && center.Y >= 0) {
				TEStorageCenter centerEnt = (TEStorageCenter)TileEntity.ByPosition[center];
				centerEnt.ResetAndSearch();
			}
		}
	}
}