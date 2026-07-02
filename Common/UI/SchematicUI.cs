using System.Collections.Generic;
using Git.Common.Config;
using Git.Common.Data;
using Git.Common.Net;
using Git.Common.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace Git.Common.UI
{
    public class SchematicUIState : UIState
    {
        private DraggableUIPanel _panel;
        private UIList _schematicList;
        private UIList _commitList;
        private UITextField _nameField;
        private UITextField _messageField;
        private UITextField _codeField;
        private UIText _statusText;
        private UITextPanel<string> _deleteBtn;
        private UITextPanel<string> _mineTab;
        private UITextPanel<string> _templatesTab;
        private bool _confirmDelete;
        private bool _showTemplates;

        private const float PanelWidth = 400f;
        private const float PanelHeight = 520f;
        private const float Inner = PanelWidth - 24f;

        private static readonly Color TabActive = new(60, 120, 60);
        private static readonly Color TabInactive = new(45, 48, 80);

        public override void OnInitialize()
        {
            _panel = new DraggableUIPanel();
            _panel.Width.Set(PanelWidth, 0f);
            _panel.Height.Set(PanelHeight, 0f);
            _panel.HAlign = 0.82f;
            _panel.VAlign = 0.5f;
            _panel.BackgroundColor = new Color(33, 36, 65, 235);
            Append(_panel);

            // --- Title bar ---
            var title = new UIText("Git — Schematics", 0.6f, true);
            title.Top.Set(2f, 0f);
            title.Left.Set(0f, 0f);
            _panel.Append(title);

            var closeBtn = new UITextPanel<string>("X", 0.8f)
            {
                BackgroundColor = new Color(120, 50, 50)
            };
            closeBtn.Width.Set(30f, 0f);
            closeBtn.Height.Set(26f, 0f);
            closeBtn.Left.Set(-30f, 1f);
            closeBtn.Top.Set(0f, 0f);
            closeBtn.OnLeftClick += (_, _) => UISystem.Toggle();
            _panel.Append(closeBtn);

            // --- Name / Note inputs with Commit button to the right ---
            AddLabel("Name", 0f, 42f);
            _nameField = AddField("build name", 56f, 40f, 200f);

            AddLabel("Note", 0f, 72f);
            _messageField = AddField("what changed?", 56f, 70f, 200f);

            var commitBtn = new UITextPanel<string>("Commit", 0.85f)
            {
                BackgroundColor = new Color(50, 110, 60)
            };
            commitBtn.Left.Set(Inner - 106f, 0f);
            commitBtn.Top.Set(40f, 0f);
            commitBtn.Width.Set(106f, 0f);
            commitBtn.Height.Set(52f, 0f);
            commitBtn.OnLeftClick += OnCommitClicked;
            _panel.Append(commitBtn);

            // --- Tabs over the left list ---
            _mineTab = MakeSmallButton("Mine", 0f, 102f, 80f, TabActive);
            _mineTab.OnLeftClick += (_, _) => SwitchTab(false);

            _templatesTab = MakeSmallButton("Templates", 84f, 102f, 92f, TabInactive);
            _templatesTab.OnLeftClick += (_, _) => SwitchTab(true);

            AddLabel("Commits", 196f, 110f);

            // --- Lists ---
            _schematicList = MakeList(0f, 136f, 168f, 228f);
            _commitList = MakeList(196f, 136f, 168f, 228f);

            // --- Share row ---
            AddLabel("Code", 0f, 380f);
            _codeField = AddField("share code", 56f, 378f, 120f);

            var uploadBtn = MakeSmallButton("Upload", 186f, 374f, 88f, new Color(60, 80, 130));
            uploadBtn.OnLeftClick += OnUploadClicked;

            var downloadBtn = MakeSmallButton("Download", 280f, 374f, 96f, new Color(60, 80, 130));
            downloadBtn.OnLeftClick += OnDownloadClicked;

            // --- Actions row ---
            _deleteBtn = MakeSmallButton("Delete", 0f, 414f, 88f, new Color(110, 55, 55));
            _deleteBtn.OnLeftClick += OnDeleteClicked;

            var hint = new UIText("Paste with the Paste Wand", 0.72f, false)
            {
                TextColor = Color.LightGray
            };
            hint.Left.Set(100f, 0f);
            hint.Top.Set(422f, 0f);
            _panel.Append(hint);

            // --- Status ---
            _statusText = new UIText("", 0.72f);
            _statusText.Top.Set(456f, 0f);
            _statusText.Left.Set(0f, 0f);
            _panel.Append(_statusText);

            RefreshAll();
        }

        // ---------- element factories ----------

        private void AddLabel(string text, float left, float top)
        {
            var label = new UIText(text, 0.72f) { TextColor = Color.LightGray };
            label.Left.Set(left, 0f);
            label.Top.Set(top, 0f);
            _panel.Append(label);
        }

        private UITextField AddField(string hint, float left, float top, float width)
        {
            var field = new UITextField(hint);
            field.Left.Set(left, 0f);
            field.Top.Set(top, 0f);
            field.Width.Set(width, 0f);
            field.Height.Set(22f, 0f);
            _panel.Append(field);
            return field;
        }

        private UIList MakeList(float left, float top, float width, float height)
        {
            var list = new UIList();
            list.Left.Set(left, 0f);
            list.Top.Set(top, 0f);
            list.Width.Set(width, 0f);
            list.Height.Set(height, 0f);
            list.ListPadding = 4f;
            _panel.Append(list);

            var scrollbar = new UIScrollbar();
            scrollbar.Left.Set(left + width + 4f, 0f);
            scrollbar.Top.Set(top, 0f);
            scrollbar.Width.Set(12f, 0f);
            scrollbar.Height.Set(height, 0f);
            list.SetScrollbar(scrollbar);
            _panel.Append(scrollbar);

            return list;
        }

        private UITextPanel<string> MakeSmallButton(string text, float left, float top, float width, Color color)
        {
            var btn = new UITextPanel<string>(text, 0.75f) { BackgroundColor = color };
            btn.Left.Set(left, 0f);
            btn.Top.Set(top, 0f);
            btn.Width.Set(width, 0f);
            btn.Height.Set(30f, 0f);
            _panel.Append(btn);
            return btn;
        }

        // ---------- tabs ----------

        private void SwitchTab(bool templates)
        {
            _showTemplates = templates;
            _mineTab.BackgroundColor = templates ? TabInactive : TabActive;
            _templatesTab.BackgroundColor = templates ? TabActive : TabInactive;
            _confirmDelete = false;
            RefreshAll();
        }

        private List<Schematic> ActiveList()
            => _showTemplates ? TemplateSystem.Templates : SchematicManager.Schematics;

        // ---------- list refresh ----------

        public void RefreshAll()
        {
            RefreshSchematicList();
            RefreshCommitList();
        }

        private void RefreshSchematicList()
        {
            _schematicList.Clear();
            foreach (var s in ActiveList())
            {
                var schematic = s;
                bool selected = ReferenceEquals(SchematicManager.Selected, schematic);

                var btn = new UITextPanel<string>(Truncate(schematic.Name, 16), 0.72f)
                {
                    BackgroundColor = selected ? new Color(60, 120, 60) : new Color(45, 48, 80)
                };
                btn.Width.Set(160f, 0f);
                btn.Height.Set(26f, 0f);
                btn.OnLeftClick += (_, _) =>
                {
                    SchematicManager.Selected = schematic;
                    SchematicManager.PasteCommitIndex = -1;
                    _confirmDelete = false;
                    _nameField.SetText(schematic.Name);
                    RefreshAll();
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
                bool selected = SchematicManager.PasteCommitIndex == i
                    || (SchematicManager.PasteCommitIndex == -1 && i == commits.Count - 1);

                var btn = new UITextPanel<string>(Truncate($"{c.CommitId} · {c.Message}", 17), 0.68f)
                {
                    BackgroundColor = selected ? new Color(60, 120, 60) : new Color(45, 48, 80)
                };
                btn.Width.Set(160f, 0f);
                btn.Height.Set(26f, 0f);
                btn.OnLeftClick += (_, _) =>
                {
                    SchematicManager.PasteCommitIndex = captured;
                    RefreshCommitList();
                    SetStatus($"Will paste commit {c.CommitId}");
                };
                _commitList.Add(btn);
            }
        }

        private static string Truncate(string text, int max)
            => text.Length > max ? text[..max] + "…" : text;

        // ---------- button handlers ----------

        private void OnCommitClicked(UIMouseEvent evt, UIElement element)
        {
            _confirmDelete = false;

            if (!SelectionSystem.HasSelection)
            {
                SetStatus("Select a region with the Selection Wand first.");
                return;
            }

            string name = _nameField.Text.Trim();
            string msg = _messageField.Text.Trim();
            if (string.IsNullOrEmpty(name)) name = "Unnamed";
            if (string.IsNullOrEmpty(msg)) msg = "no message";

            SchematicManager.CommitSelection(name, msg);
            SchematicManager.Selected = SchematicManager.Schematics.Find(s => s.Name == name);
            SchematicManager.PasteCommitIndex = -1;

            // Committing always lands in "Mine"
            if (_showTemplates)
                SwitchTab(false);
            else
                RefreshAll();

            SetStatus($"Committed \"{name}\".");
        }

        private void OnDeleteClicked(UIMouseEvent evt, UIElement element)
        {
            if (SchematicManager.Selected == null)
            {
                SetStatus("Select a schematic to delete.");
                return;
            }

            if (!SchematicManager.Schematics.Contains(SchematicManager.Selected))
            {
                SetStatus("Templates can't be deleted.");
                return;
            }

            if (!_confirmDelete)
            {
                _confirmDelete = true;
                _deleteBtn.SetText("Sure?");
                SetStatus($"Click again to delete \"{SchematicManager.Selected.Name}\".");
                return;
            }

            string name = SchematicManager.Selected.Name;
            SchematicManager.DeleteSelected();
            _confirmDelete = false;
            _deleteBtn.SetText("Delete");
            RefreshAll();
            SetStatus($"Deleted \"{name}\".");
        }

        private void OnUploadClicked(UIMouseEvent evt, UIElement element)
        {
            _confirmDelete = false;

            string url = ModContent.GetInstance<GitClientConfig>().SchematicServerUrl;
            if (string.IsNullOrWhiteSpace(url))
            {
                SetStatus("Set the server URL in Mod Config first.");
                return;
            }
            if (SchematicManager.Selected == null)
            {
                SetStatus("Select a schematic to upload.");
                return;
            }

            SetStatus("Uploading…");
            SchematicShareClient.Upload(SchematicManager.Selected, url, SetStatus);
        }

        private void OnDownloadClicked(UIMouseEvent evt, UIElement element)
        {
            _confirmDelete = false;

            string url = ModContent.GetInstance<GitClientConfig>().SchematicServerUrl;
            if (string.IsNullOrWhiteSpace(url))
            {
                SetStatus("Set the server URL in Mod Config first.");
                return;
            }

            string code = _codeField.Text.Trim();
            if (string.IsNullOrEmpty(code))
            {
                SetStatus("Enter a share code first.");
                return;
            }

            SetStatus("Downloading…");
            SchematicShareClient.Download(code, url, schematic =>
            {
                SchematicManager.AddDownloaded(schematic);
                if (_showTemplates)
                    SwitchTab(false);
                else
                    RefreshAll();
            }, SetStatus);
        }

        private void SetStatus(string text) => _statusText.SetText(text);

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (_panel.ContainsPoint(Main.MouseScreen))
                Main.LocalPlayer.mouseInterface = true;
        }
    }
}
