using System.Collections.Generic;
using Git.Common.Data;
using Git.Common.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Git.Common.UI
{
    public class SchematicUIState : UIState
    {
        private UIPanel _panel;
        private UIList _schematicList;
        private UIList _commitList;
        private UITextPanel<string> _commitBtn;
        private UITextPanel<string> _pasteBtn;
        private UITextField _nameField;
        private UITextField _messageField;
        private UIText _statusText;

        public const float PanelWidth = 340f;
        public const float PanelHeight = 480f;

        public override void OnInitialize()
        {
            _panel = new UIPanel();
            _panel.Width.Set(PanelWidth, 0f);
            _panel.Height.Set(PanelHeight, 0f);
            _panel.HAlign = 0.85f;
            _panel.VAlign = 0.5f;
            _panel.BackgroundColor = new Color(40, 40, 80, 220);
            Append(_panel);

            // Title
            var title = new UIText("Git - Schematics", 0.85f, true);
            title.Top.Set(8f, 0f);
            title.HAlign = 0.5f;
            _panel.Append(title);

            // Schematic name input
            var nameLabel = new UIText("Name:", 0.75f);
            nameLabel.Top.Set(40f, 0f);
            nameLabel.Left.Set(8f, 0f);
            _panel.Append(nameLabel);

            _nameField = new UITextField("schematic name");
            _nameField.Top.Set(38f, 0f);
            _nameField.Left.Set(60f, 0f);
            _nameField.Width.Set(160f, 0f);
            _nameField.Height.Set(20f, 0f);
            _panel.Append(_nameField);

            // Commit message input
            var msgLabel = new UIText("Msg:", 0.75f);
            msgLabel.Top.Set(66f, 0f);
            msgLabel.Left.Set(8f, 0f);
            _panel.Append(msgLabel);

            _messageField = new UITextField("commit message");
            _messageField.Top.Set(64f, 0f);
            _messageField.Left.Set(60f, 0f);
            _messageField.Width.Set(160f, 0f);
            _messageField.Height.Set(20f, 0f);
            _panel.Append(_messageField);

            // Commit button
            _commitBtn = new UITextPanel<string>("Commit Selection", 0.75f);
            _commitBtn.Top.Set(38f, 0f);
            _commitBtn.Left.Set(232f, 0f);
            _commitBtn.Width.Set(96f, 0f);
            _commitBtn.Height.Set(46f, 0f);
            _commitBtn.OnLeftClick += OnCommitClicked;
            _panel.Append(_commitBtn);

            // Schematic list header
            var schHeader = new UIText("Schematics", 0.75f);
            schHeader.Top.Set(92f, 0f);
            schHeader.Left.Set(8f, 0f);
            _panel.Append(schHeader);

            // Schematic list
            _schematicList = new UIList();
            _schematicList.Top.Set(110f, 0f);
            _schematicList.Left.Set(4f, 0f);
            _schematicList.Width.Set(156f, 0f);
            _schematicList.Height.Set(280f, 0f);
            _schematicList.ListPadding = 4f;
            _panel.Append(_schematicList);
            AddScrollbar(_schematicList, 160f, 110f, 156f, 280f);

            // Commit list header
            var cmtHeader = new UIText("Commits", 0.75f);
            cmtHeader.Top.Set(92f, 0f);
            cmtHeader.Left.Set(172f, 0f);
            _panel.Append(cmtHeader);

            // Commit list
            _commitList = new UIList();
            _commitList.Top.Set(110f, 0f);
            _commitList.Left.Set(168f, 0f);
            _commitList.Width.Set(156f, 0f);
            _commitList.Height.Set(280f, 0f);
            _commitList.ListPadding = 4f;
            _panel.Append(_commitList);
            AddScrollbar(_commitList, 324f, 110f, 156f, 280f);

            // Paste button
            _pasteBtn = new UITextPanel<string>("Paste (use wand)", 0.75f);
            _pasteBtn.Top.Set(398f, 0f);
            _pasteBtn.HAlign = 0.5f;
            _pasteBtn.Width.Set(180f, 0f);
            _pasteBtn.Height.Set(28f, 0f);
            _pasteBtn.OnLeftClick += OnPasteClicked;
            _panel.Append(_pasteBtn);

            // Status text
            _statusText = new UIText("", 0.7f);
            _statusText.Top.Set(432f, 0f);
            _statusText.HAlign = 0.5f;
            _panel.Append(_statusText);

            RefreshSchematicList();
        }

        private void AddScrollbar(UIList list, float left, float top, float width, float height)
        {
            var scrollbar = new UIScrollbar();
            scrollbar.Left.Set(left, 0f);
            scrollbar.Top.Set(top, 0f);
            scrollbar.Height.Set(height, 0f);
            scrollbar.Width.Set(12f, 0f);
            list.SetScrollbar(scrollbar);
            _panel.Append(scrollbar);
        }

        private void RefreshSchematicList()
        {
            _schematicList.Clear();
            for (int i = 0; i < SchematicManager.Schematics.Count; i++)
            {
                int captured = i;
                var s = SchematicManager.Schematics[i];
                bool selected = SchematicManager.SelectedSchematicIndex == i;
                var btn = new UITextPanel<string>(s.Name, 0.72f)
                {
                    BackgroundColor = selected ? new Color(60, 120, 60) : new Color(50, 50, 90)
                };
                btn.Width.Set(148f, 0f);
                btn.Height.Set(24f, 0f);
                btn.OnLeftClick += (_, _) =>
                {
                    SchematicManager.SelectedSchematicIndex = captured;
                    SchematicManager.PasteCommitIndex = -1;
                    RefreshSchematicList();
                    RefreshCommitList();
                    _nameField.SetText(SchematicManager.Schematics[captured].Name);
                };
                _schematicList.Add(btn);
            }
        }

        private void RefreshCommitList()
        {
            _commitList.Clear();
            if (SchematicManager.Selected == null) return;

            var commits = SchematicManager.Selected.Commits;
            for (int i = commits.Count - 1; i >= 0; i--)
            {
                int captured = i;
                var c = commits[i];
                bool selected = SchematicManager.PasteCommitIndex == i;
                string label = $"#{c.CommitId} {c.Message}";
                if (label.Length > 18) label = label[..18] + "…";
                var btn = new UITextPanel<string>(label, 0.68f)
                {
                    BackgroundColor = selected ? new Color(60, 120, 60) : new Color(50, 50, 90)
                };
                btn.Width.Set(148f, 0f);
                btn.Height.Set(24f, 0f);
                btn.OnLeftClick += (_, _) =>
                {
                    SchematicManager.PasteCommitIndex = captured;
                    RefreshCommitList();
                    SetStatus($"Selected commit #{c.CommitId}");
                };
                _commitList.Add(btn);
            }
        }

        private void OnCommitClicked(UIMouseEvent evt, UIElement element)
        {
            if (!SelectionSystem.HasSelection)
            {
                SetStatus("No selection! Use the wand first.");
                return;
            }

            string name = _nameField.Text.Trim();
            string msg = _messageField.Text.Trim();
            if (string.IsNullOrEmpty(name)) name = "Unnamed";
            if (string.IsNullOrEmpty(msg)) msg = "no message";

            SchematicManager.CommitSelection(name, msg);

            // Select the newly committed schematic
            int idx = SchematicManager.Schematics.FindIndex(s => s.Name == name);
            if (idx >= 0) SchematicManager.SelectedSchematicIndex = idx;

            RefreshSchematicList();
            RefreshCommitList();
            SetStatus($"Committed \"{name}\"!");
        }

        private void OnPasteClicked(UIMouseEvent evt, UIElement element)
        {
            if (SchematicManager.Selected == null)
            {
                SetStatus("Select a schematic first.");
                return;
            }
            SetStatus("Click the paste wand in-world to place.");
        }

        private void SetStatus(string text) => _statusText.SetText(text);

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            // Prevent clicks from passing through the panel
            if (_panel.ContainsPoint(Main.MouseScreen))
                Main.LocalPlayer.mouseInterface = true;
        }
    }
}
