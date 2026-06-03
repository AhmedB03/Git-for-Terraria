using System.Collections.Generic;
using Git.Common.Data;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace Git.Common.Systems
{
    // Holds the in-memory list of schematics and coordinates commit/paste actions.
    public class SchematicManager : ModSystem
    {
        public static List<Schematic> Schematics { get; private set; } = new();

        // Index of schematic selected in the UI, -1 = none
        public static int SelectedSchematicIndex { get; set; } = -1;
        public static Schematic Selected => SelectedSchematicIndex >= 0 && SelectedSchematicIndex < Schematics.Count
            ? Schematics[SelectedSchematicIndex] : null;

        // Tile position where the next paste will be anchored (top-left)
        public static Point PasteAnchor { get; set; }

        // Which commit index to paste, -1 = latest
        public static int PasteCommitIndex { get; set; } = -1;

        public override void OnWorldLoad()
        {
            Schematics = Schematic.LoadAll();
        }

        public static Schematic GetOrCreate(string name)
        {
            var existing = Schematics.Find(s => s.Name == name);
            if (existing != null) return existing;
            var s = new Schematic { Name = name };
            Schematics.Add(s);
            return s;
        }

        public static void CommitSelection(string schematicName, string message)
        {
            if (!SelectionSystem.HasSelection) return;
            var schematic = GetOrCreate(schematicName);
            var commit = Schematic.CaptureRegion(SelectionSystem.TopLeft, SelectionSystem.Width, SelectionSystem.Height, message);
            schematic.Commits.Add(commit);
            schematic.Save();
        }

        public static void PasteLatest(int schematicIndex, Point anchor)
        {
            if (schematicIndex < 0 || schematicIndex >= Schematics.Count) return;
            var commit = Schematics[schematicIndex].LatestCommit;
            if (commit == null) return;
            Schematic.PasteCommit(commit, anchor);
        }

        public static void PasteCommitAt(int schematicIndex, int commitIndex, Point anchor)
        {
            if (schematicIndex < 0 || schematicIndex >= Schematics.Count) return;
            var commits = Schematics[schematicIndex].Commits;
            if (commitIndex < 0 || commitIndex >= commits.Count) return;
            Schematic.PasteCommit(commits[commitIndex], anchor);
        }
    }
}
