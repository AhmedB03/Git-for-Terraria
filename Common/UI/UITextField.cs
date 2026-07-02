using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.UI;

namespace Git.Common.UI
{
    // Simple single-line text input element. Click to focus and type directly;
    // Enter or Escape (or clicking elsewhere) unfocuses.
    public class UITextField : UIElement
    {
        public string Text { get; private set; } = "";
        private readonly string _hint;
        private bool _focused;

        public UITextField(string hint = "")
        {
            _hint = hint;
        }

        public void SetText(string text) => Text = text ?? "";

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            var dims = GetDimensions();
            var rect = dims.ToRectangle();

            spriteBatch.Draw(TextureAssets.MagicPixel.Value, rect, _focused ? new Color(60, 60, 100) : new Color(40, 40, 70));

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
            spriteBatch.DrawString(font, display, new Vector2(dims.X + 4, dims.Y + 3), textColor, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f);
        }

        public override void LeftClick(UIMouseEvent evt)
        {
            base.LeftClick(evt);
            _focused = true;
            Main.blockInput = true;
            Main.clrInput(); // discard buffered keystrokes from before focusing
        }

        private void Unfocus()
        {
            _focused = false;
            Main.blockInput = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (!_focused) return;

            // Click outside → unfocus
            if (Main.mouseLeft && !ContainsPoint(Main.MouseScreen))
            {
                Unfocus();
                return;
            }

            // Route keyboard input to us instead of the player. HandleIME only
            // starts a text-input session when something registers as the text
            // taker (chat, signs, etc.) — so register ourselves.
            PlayerInput.WritingText = true;
            Main.CurrentInputTextTakerOverride = this;
            Main.instance.HandleIME();
            Text = Main.GetInputText(Text);

            // Enter or Escape finishes editing
            if (Main.inputText.IsKeyDown(Keys.Enter) || Main.inputText.IsKeyDown(Keys.Escape))
                Unfocus();
        }
    }
}
