using System;
using System.Collections.Generic;
using Git.Common.Data;
using Terraria.ID;
using Terraria.ModLoader;

namespace Git.Common.Systems
{
    // Built-in read-only schematics shown under the "Templates" tab.
    // Generated in code so they ship with the mod — plain blocks reframe
    // automatically when pasted, so no stored frame data is needed.
    public class TemplateSystem : ModSystem
    {
        public static List<Schematic> Templates { get; private set; } = new();

        public override void PostSetupContent()
        {
            Templates = new List<Schematic>
            {
                StarterHouse(),
                ArenaPlatforms(),
                HellevatorSegment(),
            };
        }

        public override void Unload() => Templates = null;

        // ---------- template definitions ----------

        private static Schematic StarterHouse()
        {
            const int w = 14, h = 9;
            return Build("Starter House", w, h, (x, y) =>
            {
                bool border = x == 0 || x == w - 1 || y == 0 || y == h - 1;
                // 3-high door openings on both sides
                bool doorGap = (x == 0 || x == w - 1) && y >= h - 4 && y <= h - 2;

                if (border && !doorGap)
                    return Block(TileID.WoodBlock);
                return Wall(WallID.Wood);
            });
        }

        private static Schematic ArenaPlatforms()
        {
            const int w = 40, h = 15;
            return Build("Arena Platforms", w, h, (x, y) =>
            {
                // three platform rows with jumping room between them
                if (y == 4 || y == 9 || y == 14)
                    return Block(TileID.Platforms);
                return Air();
            });
        }

        private static Schematic HellevatorSegment()
        {
            const int w = 8, h = 20;
            return Build("Hellevator Segment", w, h, (x, y) =>
            {
                // brick side rails, empty shaft in the middle (pasting clears
                // the tiles that are in the way — that's the digging)
                if (x == 0 || x == w - 1)
                    return Block(TileID.GrayBrick);
                return Air();
            });
        }

        // ---------- builders ----------

        private static Schematic Build(string name, int width, int height, Func<int, int, TileData> cell)
        {
            var flat = new TileData[width * height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    flat[y * width + x] = cell(x, y) ?? Air();

            var schematic = new Schematic { Name = name };
            schematic.Commits.Add(new SchematicCommit
            {
                CommitId = "template",
                Message = "built-in template",
                Width = width,
                Height = height,
                TilesFlat = flat,
            });
            return schematic;
        }

        private static TileData Air() => new();

        private static TileData Block(ushort type) => new()
        {
            HasTile = true,
            TileType = type,
        };

        private static TileData Wall(ushort wallType) => new()
        {
            WallType = wallType,
        };
    }
}
