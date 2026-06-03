using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Git.Common.Data
{
    public class SchematicCommit
    {
        public string CommitId { get; set; } = Guid.NewGuid().ToString("N")[..8];
        public string Message { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public int Width { get; set; }
        public int Height { get; set; }
        public TileData[,] Tiles { get; set; }

        // Serialization helpers — 2D arrays don't serialize natively
        public TileData[] TilesFlat
        {
            get
            {
                var arr = new TileData[Width * Height];
                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)
                        arr[y * Width + x] = Tiles[x, y];
                return arr;
            }
            set
            {
                Tiles = new TileData[Width, Height];
                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)
                        Tiles[x, y] = value[y * Width + x];
            }
        }
    }

    public class Schematic
    {
        public string Name { get; set; } = "Unnamed";
        public List<SchematicCommit> Commits { get; set; } = new();

        public SchematicCommit LatestCommit => Commits.Count > 0 ? Commits[^1] : null;

        public static string SaveDirectory
        {
            get
            {
                string dir = Path.Combine(Main.SavePath, "Schematics");
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        public static List<Schematic> LoadAll()
        {
            var list = new List<Schematic>();
            foreach (string file in Directory.GetFiles(SaveDirectory, "*.json"))
            {
                try
                {
                    string json = File.ReadAllText(file);
                    var s = JsonSerializer.Deserialize<Schematic>(json);
                    if (s != null)
                        list.Add(s);
                }
                catch { }
            }
            return list;
        }

        public void Save()
        {
            string path = Path.Combine(SaveDirectory, SanitizeFileName(Name) + ".json");
            File.WriteAllText(path, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        }

        public static SchematicCommit CaptureRegion(Point topLeft, int width, int height, string message)
        {
            var commit = new SchematicCommit
            {
                Message = message,
                Width = width,
                Height = height,
                Tiles = new TileData[width, height]
            };

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    commit.Tiles[x, y] = TileData.Capture(topLeft.X + x, topLeft.Y + y);

            return commit;
        }

        public static void PasteCommit(SchematicCommit commit, Point topLeft)
        {
            for (int x = 0; x < commit.Width; x++)
                for (int y = 0; y < commit.Height; y++)
                    commit.Tiles[x, y].Place(topLeft.X + x, topLeft.Y + y);

            // Refresh the pasted region so tiles render correctly
            for (int x = topLeft.X - 1; x <= topLeft.X + commit.Width; x++)
                for (int y = topLeft.Y - 1; y <= topLeft.Y + commit.Height; y++)
                    WorldGen.SquareTileFrame(x, y, false);

            NetMessage.SendTileSquare(-1, topLeft.X, topLeft.Y, commit.Width, commit.Height);
        }

        private static string SanitizeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }
    }
}
