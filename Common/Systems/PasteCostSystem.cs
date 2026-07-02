using System.Collections.Generic;
using Git.Common.Data;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Git.Common.Systems
{
    // Computes the material cost of pasting a schematic and consumes items
    // from the player's inventory. Tiles with no known placing item (grass,
    // trees, etc.) are free.
    public class PasteCostSystem : ModSystem
    {
        private static Dictionary<int, int> _tileToItem;
        private static Dictionary<int, int> _wallToItem;

        // Tiles with no placing item that should still be pastable for a fair
        // price (grass variants charge their underlying soil block).
        private static readonly Dictionary<int, int> SubstituteItems = new()
        {
            { TileID.Grass, ItemID.DirtBlock },
            { TileID.CorruptGrass, ItemID.DirtBlock },
            { TileID.CrimsonGrass, ItemID.DirtBlock },
            { TileID.HallowedGrass, ItemID.DirtBlock },
            { TileID.GolfGrass, ItemID.DirtBlock },
            { TileID.GolfGrassHallowed, ItemID.DirtBlock },
            { TileID.JungleGrass, ItemID.MudBlock },
            { TileID.MushroomGrass, ItemID.MudBlock },
            { TileID.AshGrass, ItemID.AshBlock },
        };

        // Item that pays for this tile type, or 0 if there is none — meaning
        // the tile can't legitimately be placed (trees, pots, heart crystals)
        // and gets skipped entirely when materials are required.
        public static int GetItemForTile(int tileType)
        {
            if (SubstituteItems.TryGetValue(tileType, out int sub))
                return sub;
            if (_tileToItem != null && _tileToItem.TryGetValue(tileType, out int item))
                return item;
            return 0;
        }

        public static int GetItemForWall(int wallType)
        {
            if (_wallToItem != null && _wallToItem.TryGetValue(wallType, out int item))
                return item;
            return 0;
        }

        public override void PostSetupContent()
        {
            _tileToItem = new Dictionary<int, int>();
            _wallToItem = new Dictionary<int, int>();

            for (int i = 1; i < ItemLoader.ItemCount; i++)
            {
                if (!ContentSamples.ItemsByType.TryGetValue(i, out Item item) || item == null)
                    continue;
                if (item.createTile >= 0 && !_tileToItem.ContainsKey(item.createTile))
                    _tileToItem[item.createTile] = i;
                if (item.createWall > 0 && !_wallToItem.ContainsKey(item.createWall))
                    _wallToItem[item.createWall] = i;
            }
        }

        public override void Unload()
        {
            _tileToItem = null;
            _wallToItem = null;
        }

        // Returns itemType -> count required to paste this commit.
        public static Dictionary<int, int> ComputeCost(SchematicCommit commit)
        {
            var cost = new Dictionary<int, int>();

            for (int y = 0; y < commit.Height; y++)
            {
                for (int x = 0; x < commit.Width; x++)
                {
                    TileData t = commit.TilesFlat[y * commit.Width + x];
                    if (t == null)
                        continue;

                    if (t.HasTile)
                    {
                        int tileItem = GetItemForTile(t.TileType);
                        if (tileItem > 0 && IsChargeOrigin(t))
                            AddCost(cost, tileItem, 1);
                    }

                    if (t.WallType > 0)
                    {
                        int wallItem = GetItemForWall(t.WallType);
                        if (wallItem > 0)
                            AddCost(cost, wallItem, 1);
                    }

                    int wires = (t.RedWire ? 1 : 0) + (t.BlueWire ? 1 : 0) + (t.GreenWire ? 1 : 0) + (t.YellowWire ? 1 : 0);
                    if (wires > 0)
                        AddCost(cost, ItemID.Wire, wires);

                    if (t.Actuator)
                        AddCost(cost, ItemID.Actuator, 1);
                }
            }

            return cost;
        }

        // Multi-tile furniture (3x2 table etc.) should cost one item, not one
        // per cell — only charge the cell at the frame origin.
        private static bool IsChargeOrigin(TileData t)
        {
            TileObjectData data = TileObjectData.GetTileData(t.TileType, 0);
            if (data == null)
                return true; // plain block: every cell is one item

            return t.TileFrameX % data.CoordinateFullWidth == 0
                && t.TileFrameY % data.CoordinateFullHeight == 0;
        }

        private static void AddCost(Dictionary<int, int> cost, int itemType, int amount)
        {
            cost.TryGetValue(itemType, out int current);
            cost[itemType] = current + amount;
        }

        // Verifies the player has everything, then consumes it. If anything is
        // missing, consumes nothing and reports what's short.
        public static bool TryConsume(Player player, Dictionary<int, int> cost, out List<(int type, int shortBy)> missing)
        {
            missing = new List<(int, int)>();

            foreach (var kv in cost)
            {
                int have = player.CountItem(kv.Key, kv.Value);
                if (have < kv.Value)
                    missing.Add((kv.Key, kv.Value - have));
            }

            if (missing.Count > 0)
                return false;

            foreach (var kv in cost)
                for (int i = 0; i < kv.Value; i++)
                    player.ConsumeItem(kv.Key);

            return true;
        }
    }
}
