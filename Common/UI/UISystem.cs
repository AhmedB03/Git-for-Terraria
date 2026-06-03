using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace Git.Common.UI
{
    [Autoload(Side = ModSide.Client)]
    public class UISystem : ModSystem
    {
        private SchematicUIState _uiState;
        private UserInterface _ui;
        public static bool IsVisible { get; private set; }

        public override void Load()
        {
            _uiState = new SchematicUIState();
            _uiState.Activate();
            _ui = new UserInterface();
        }

        public static void Toggle()
        {
            IsVisible = !IsVisible;
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (IsVisible)
                _ui.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(System.Collections.Generic.List<GameInterfaceLayer> layers)
        {
            int idx = layers.FindIndex(l => l.Name == "Vanilla: Mouse Text");
            if (idx >= 0)
            {
                layers.Insert(idx, new LegacyGameInterfaceLayer(
                    "Git: Schematic UI",
                    () =>
                    {
                        if (IsVisible)
                        {
                            _ui.SetState(_uiState);
                            _ui.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI));
            }
        }
    }
}
