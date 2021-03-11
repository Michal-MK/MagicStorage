using MagicStorage.Items;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace MagicStorage.Components {
	public class TStorageComponent : ModTile {

		public TStorageComponent() {
			tileTexture = "MagicStorage/Textures/Tiles/" + ActualName;
		}

		public string ActualName => GetType().Name.Remove(0, 1);

		public static Point16 killTile = new Point16(-1, -1);

		public override string HighlightTexture => "MagicStorage/Textures/Tiles/StorageComponent_Highlight";

		public string tileTexture;

		public override bool Autoload(ref string name, ref string texture) {
			texture = tileTexture;
			return base.Autoload(ref name, ref texture);
		}

		public override void SetDefaults() {
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			Main.tileSolidTop[Type] = true;
			Main.tileTable[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			AnchorData d = TileObjectData.newTile.AnchorBottom;
			d.type |= Terraria.Enums.AnchorType.EmptyTile | Terraria.Enums.AnchorType.AlternateTile;
			d.tileCount = 0;
			TileObjectData.newTile.AnchorBottom = d;
			TileObjectData.newTile.Origin = new Point16(0, 1);
			TileObjectData.newTile.HookCheck = new PlacementHook(CanPlace, -1, 0, true);
			ModifyObjectData();
			ModTileEntity tileEntity = GetTileEntity();
			if (tileEntity != null) {
				TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(tileEntity.Hook_AfterPlacement, -1, 0, false);
			}
			else {
				TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(TEStorageComponent.Hook_AfterPlacement_NoEntity, -1, 0, false);
			}
			TileObjectData.addTile(Type);
			ModTranslation text = CreateMapEntryName();
			text.SetDefault("Magic Storage");
			AddMapEntry(new Color(153, 107, 61), text);
			dustType = 7;
			disableSmartCursor = true;
			TileID.Sets.HasOutlines[Type] = HasSmartInteract();
		}

		public virtual void ModifyObjectData() {
		}

		public virtual ModTileEntity GetTileEntity() {
			return null;
		}

		public virtual int ItemType(int frameX, int frameY) {
			return mod.ItemType(nameof(StorageComponent));
		}

		public int CanPlace(int i, int j, int type, int style, int direction) {
			int count = 0;
			if (GetTileEntity() != null && GetTileEntity() is TEStorageCenter) {
				count++;
			}

			Point16 startSearch = new Point16(i, j - 1);
			HashSet<Point16> explored = new HashSet<Point16>();
			explored.Add(startSearch);
			Queue<Point16> toExplore = new Queue<Point16>();
			foreach (Point16 point in TEStorageComponent.AdjacentComponents(startSearch)) {
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

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			Item.NewItem(i * 16, j * 16, 32, 32, ItemType(frameX, frameY));
			killTile = new Point16(i, j);
			ModTileEntity tileEntity = GetTileEntity();
			if (tileEntity != null) {
				tileEntity.Kill(i, j);
			}
			else {
				if (Main.netMode == NetmodeID.MultiplayerClient) {
					NetHelper.SendSearchAndRefresh(killTile.X, killTile.Y);
				}
				else {
					TEStorageComponent.SearchAndRefreshNetwork(killTile);
				}
			}
			killTile = new Point16(-1, -1);
		}
	}
}