using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;

namespace Git.Common.UI
{
    // Simple single-line text input element.
    public class UITextField : UIElement
    {
        public string Text { get; private set; } = "";
        private string _hint;
        private bool _focused;

        public UITextField(string hint = "")
        {
            _hint = hint;
        }

        public void SetText(string text) => Text = text;

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            var dims = GetDimensions();
            var rect = dims.ToRectangle();

            // Background
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, rect, _focused ? new Color(60, 60, 100) : new Color(40, 40, 70));

            // Border (1px lines on each edge)
            Color border = _focused ? Color.CornflowerBlue : Color.Gray;
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(rect.X, rect.Y, rect.Width, 1), border);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(rect.X, rect.Bottom - 1, rect.Width, 1), border);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(rect.X, rect.Y, 1, rect.Height), border);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(rect.Right - 1, rect.Y, 1, rect.Height), border);

            string display = string.IsNullOrEmpty(Text) ? _hint : Text;
            Color textColor = string.IsNullOrEmpty(Text) ? Color.Gray : Color.White;

            if (_focused && Main.GameUpdateCount % 60 < 30)
                display += "|";

            var font = FontAssets.MouseText.Value;
            float scale = 0.75f;
            spriteBatch.DrawString(font, display, new Vector2(dims.X + 4, dims.Y + 3), textColor, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        public override void LeftClick(UIMouseEvent evt)
        {
            _focused = true;
            Main.blockInput = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (!_focused) return;

            // Unfocus on click outside
            if (Main.mouseLeft && !ContainsPoint(Main.MouseScreen))
            {
                _focused = false;
                Main.blockInput = false;
                return;
            }

            // Read keyboard input via PlayerInput
            var inputText = Main.GetInputText(Text);
            if (inputText != Text)
                Text = inputText;
        }
    }
}
