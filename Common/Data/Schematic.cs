using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
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

        // Stored as flat array for JSON compatibility; accessed via Tiles[x,y] in code
        public TileData[] TilesFlat { get; set; }

        [JsonIgnore]
        public TileData[,] Tiles
        {
            get
            {
                var arr = new TileData[Width, Height];
                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)
                        arr[x, y] = TilesFlat[y * Width + x];
                return arr;
            }
            set
            {
                TilesFlat = new TileData[Width * Height];
                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)
                        TilesFlat[y * Width + x] = value[x, y];
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
                    if (s == null)
                        continue;

                    s.PruneInvalidCommits();
                    if (s.Commits.Count > 0)
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

        public void DeleteFile()
        {
            string path = Path.Combine(SaveDirectory, SanitizeFileName(Name) + ".json");
            if (File.Exists(path))
                File.Delete(path);
        }

        // Drops commits written by older/buggy versions: null tile arrays,
        // size mismatches, or null cells all crash the preview and paste code.
        public void PruneInvalidCommits()
        {
            Commits ??= new List<SchematicCommit>();
            Commits.RemoveAll(c =>
                c == null ||
                c.TilesFlat == null ||
                c.TilesFlat.Length != c.Width * c.Height ||
                Array.Exists(c.TilesFlat, t => t == null));
        }

        public static SchematicCommit CaptureRegion(Point topLeft, int width, int height, string message)
        {
            var flat = new TileData[width * height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    flat[y * width + x] = TileData.Capture(topLeft.X + x, topLeft.Y + y);

            return new SchematicCommit
            {
                Message = message,
                Width = width,
                Height = height,
                TilesFlat = flat
            };
        }

        // skipUnobtainable: leave out tiles/walls that no item can place
        // (trees, pots, natural cave walls) so paste-with-materials can't
        // create things the player couldn't legitimately build.
        public static void PasteCommit(SchematicCommit commit, Point topLeft, bool skipUnobtainable = false)
        {
            for (int y = 0; y < commit.Height; y++)
            {
                for (int x = 0; x < commit.Width; x++)
                {
                    var t = commit.TilesFlat[y * commit.Width + x];
                    if (t == null) continue;

                    bool placeTile = !skipUnobtainable || !t.HasTile
                        || Systems.PasteCostSystem.GetItemForTile(t.TileType) > 0;
                    bool placeWall = !skipUnobtainable || t.WallType == 0
                        || Systems.PasteCostSystem.GetItemForWall(t.WallType) > 0;

                    t.Place(topLeft.X + x, topLeft.Y + y, placeTile, placeWall);
                }
            }

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
