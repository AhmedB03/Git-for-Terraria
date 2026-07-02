using System.Collections.Generic;
using Git.Common.Data;
using Terraria.ModLoader;

namespace Git.Common.Systems
{
    // Holds the in-memory list of schematics and coordinates commit/paste actions.
    public class SchematicManager : ModSystem
    {
        public static List<Schematic> Schematics { get; private set; } = new();

        // Currently selected schematic — one of ours, or a built-in template.
        public static Schematic Selected { get; set; }

        // Which commit index to paste, -1 = latest
        public static int PasteCommitIndex { get; set; } = -1;

        public override void OnWorldLoad()
        {
            Schematics = Schematic.LoadAll();
            Selected = null;
            PasteCommitIndex = -1;
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

        // The commit that would be pasted right now: the one picked in the UI,
        // or the latest if none is picked. Shared by the wand and the preview.
        public static SchematicCommit GetActiveCommit()
        {
            var s = Selected;
            if (s == null || s.Commits.Count == 0) return null;
            if (PasteCommitIndex >= 0 && PasteCommitIndex < s.Commits.Count)
                return s.Commits[PasteCommitIndex];
            return s.Commits[^1];
        }

        // Adds a schematic downloaded from the sharing site, renaming on collision.
        public static void AddDownloaded(Schematic schematic)
        {
            schematic.PruneInvalidCommits();
            if (schematic.Commits.Count == 0) return;

            string baseName = schematic.Name;
            int suffix = 2;
            while (Schematics.Exists(s => s.Name == schematic.Name))
                schematic.Name = $"{baseName}-{suffix++}";

            Schematics.Add(schematic);
            schematic.Save();
        }

        // Returns false if nothing is selected or a template is selected.
        public static bool DeleteSelected()
        {
            var s = Selected;
            if (s == null || !Schematics.Contains(s))
                return false;

            s.DeleteFile();
            Schematics.Remove(s);
            Selected = null;
            PasteCommitIndex = -1;
            return true;
        }
    }
}
