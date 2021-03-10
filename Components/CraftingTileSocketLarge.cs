﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace MagicStorage.Components {
	public class CraftingTileSocketLarge : CraftingTileSocket {

		private readonly Point16 storedPosition;

		public CraftingTileSocketLarge() {
			tileTexture = "MagicStorage/Textures/Tiles/" + GetType().Name;
		}

		public CraftingTileSocketLarge(Point16 pos) : this() {
			storedPosition = pos;
		}

		public override void SetDefaults() {
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileSolidTop[Type] = true;
			Main.tileSolid[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileTable[Type] = true;
			TileObjectData.newTile.HookCheck = new PlacementHook(CanPlace, -1, 0, true);
			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			TileObjectData.newTile.Height = 1;
			TileObjectData.newTile.CoordinateHeights = new[] { 18 };
			TileObjectData.newTile.Origin = new Point16(1, 0);
			var bottomAnch = TileObjectData.newTile.AnchorBottom;
			bottomAnch.type |= Terraria.Enums.AnchorType.EmptyTile | Terraria.Enums.AnchorType.AlternateTile;
			bottomAnch.tileCount = 0;
			TileObjectData.newTile.AnchorBottom = bottomAnch;
			TileObjectData.addTile(Type);
			disableSmartCursor = true;
		}

		public override void PostSetDefaults() {
			Main.tileNoSunLight[Type] = false;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			Item.NewItem(i * 16, j * 16, 48, 16, ModContent.ItemType<Items.CraftingTileSocketLarge>());
		}

		public override ushort GetTileAbove(int i, int j) {
			if (j - 1 < 0) {
				return 0;
			}
			if (!Main.tile[i, j - 1].active()) {
				return ushort.MaxValue;
			}
			return Main.tile[i, j - 1].type;
		}

		public override int GetItemTypeFromTileAbove(int i, int j) {
			ushort val = GetTileAbove(i, j);
			if (val == ushort.MaxValue) {
				return -1;
			}
			return MagicStorage.tilesToItems[val];
		}

		public override int GetItemTypeFromTileAbove() {
			return GetItemTypeFromTileAbove(storedPosition.X, storedPosition.Y);
		}

		public override bool NewRightClick(int i, int j) {
			int itemType = GetItemTypeFromTileAbove(i, j);
			if (itemType != -1) {
				Item item = new Item();
				item.SetDefaults(itemType);
				Main.NewText($"There is: {item.Name} ({item.type}, {item.createTile}) above!");
			}
			else {
				Main.NewText($"There is nothing above!");
			}
			return true;
		}
	}
}
