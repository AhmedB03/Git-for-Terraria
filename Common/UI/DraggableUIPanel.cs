using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Git.Common.UI
{
    // Panel that can be dragged around by its title bar (top strip).
    public class DraggableUIPanel : UIPanel
    {
        private const float DragBarHeight = 32f;
        private Vector2 _offset;
        private bool _dragging;

        public override void LeftMouseDown(UIMouseEvent evt)
        {
            base.LeftMouseDown(evt);
            var dims = GetDimensions();
            if (evt.MousePosition.Y - dims.Y <= DragBarHeight)
            {
                _dragging = true;
                _offset = evt.MousePosition - new Vector2(dims.X, dims.Y);
                // Switch from alignment-based to pixel-based positioning
                HAlign = 0f;
                VAlign = 0f;
                Left.Set(dims.X, 0f);
                Top.Set(dims.Y, 0f);
            }
        }

        public override void LeftMouseUp(UIMouseEvent evt)
        {
            base.LeftMouseUp(evt);
            _dragging = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (_dragging)
            {
                Left.Set(Main.mouseX - _offset.X, 0f);
                Top.Set(Main.mouseY - _offset.Y, 0f);
                Recalculate();
            }
        }
    }
}
