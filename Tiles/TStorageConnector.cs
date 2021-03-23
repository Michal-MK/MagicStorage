using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.ID;
using System.Reflection;
using System.Linq;
using System;
using MagicStorageTwo.Items;

namespace MagicStorageTwo.Components {
	public class TStorageConnector : ModTile {

		private static HashSet<ushort> tilesToConnect = new HashSet<ushort>();

		public TStorageConnector() {
			tileTexture = "MagicStorageTwo/Textures/Tiles/StorageConnector";
		}

		public static void SetupConnectors() {
			for (ushort i = 0; i < TileLoader.TileCount; i++) {
				ModTile mt = TileLoader.GetTile(i);
				if (mt is TStorageComponent || mt is TStorageConnector) {
					tilesToConnect.Add(i);
				}
			}
		}


		public override void SetDefaults() {
			Main.tileSolid[Type] = false;
			TileObjectData.newTile.Width = 1;
			TileObjectData.newTile.Height = 1;
			TileObjectData.newTile.Origin = new Point16(0, 0);
			TileObjectData.newTile.CoordinateHeights = new int[] { 16 };
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.HookCheck = new PlacementHook(CanPlace, -1, 0, true);
			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(Hook_AfterPlacement, -1, 0, false);
			TileObjectData.addTile(Type);
			ModTranslation text = CreateMapEntryName();
			text.SetDefault("Magic Storage");
			AddMapEntry(new Color(153, 107, 61), text);
			dustType = 7;
			drop = mod.ItemType(nameof(StorageConnector));
		}


		public string tileTexture;

		public override bool Autoload(ref string name, ref string texture) {
			texture = tileTexture;
			return base.Autoload(ref name, ref texture);
		}


		public int CanPlace(int i, int j, int type, int style, int direction) {
			int count = 0;

			Point16 startSearch = new Point16(i, j);
			HashSet<Point16> explored = new HashSet<Point16> { startSearch };
			Queue<Point16> toExplore = new Queue<Point16>();
			foreach (Point16 point in TEStorageComponent.AdjacentComponents(startSearch, true)) {
				toExplore.Enqueue(point);
			}

			while (toExplore.Count > 0) {
				Point16 explore = toExplore.Dequeue();
				if (!explored.Contains(explore) && explore != TStorageComponent.killTile) {
					explored.Add(explore);
					if (TEStorageCenter.IsStorageCenter(explore)) {
						count++;
						if (count >= 2) {
							return -1;
						}
					}
					foreach (Point16 point in TEStorageComponent.AdjacentComponents(explore)) {
						toExplore.Enqueue(point);
					}
				}
			}
			return count;
		}

		public static int Hook_AfterPlacement(int i, int j, int type, int style, int direction) {
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				NetMessage.SendTileRange(Main.myPlayer, i, j, 1, 1);
				NetHelper.SendSearchAndRefresh(i, j);
				return 0;
			}
			TEStorageComponent.SearchAndRefreshNetwork(new Point16(i, j));
			return 0;
		}

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			int frameX = 0;
			int frameY = 0;
			if (WorldGen.InWorld(i - 1, j) && Main.tile[i - 1, j].active() && tilesToConnect.Contains(Main.tile[i - 1, j].type)) {
				frameX += 18;
			}
			if (WorldGen.InWorld(i + 1, j) && Main.tile[i + 1, j].active() && tilesToConnect.Contains(Main.tile[i + 1, j].type)) {
				frameX += 36;
			}
			if (WorldGen.InWorld(i, j - 1) && Main.tile[i, j - 1].active() && tilesToConnect.Contains(Main.tile[i, j - 1].type)) {
				frameY += 18;
			}
			if (WorldGen.InWorld(i, j + 1) && Main.tile[i, j + 1].active() && tilesToConnect.Contains(Main.tile[i, j + 1].type)) {
				frameY += 36;
			}
			Main.tile[i, j].frameX = (short)frameX;
			Main.tile[i, j].frameY = (short)frameY;
			return false;
		}

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
			if (fail || effectOnly) {
				return;
			}
			TStorageComponent.killTile = new Point16(i, j);
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				NetHelper.SendSearchAndRefresh(TStorageComponent.killTile.X, TStorageComponent.killTile.Y);
				MagicStorageTwo.Instance.guiM.Refresh();
			}
			else {
				TEStorageComponent.SearchAndRefreshNetwork(TStorageComponent.killTile);
			}
			TStorageComponent.killTile = new Point16(-1, -1);
		}
	}
}