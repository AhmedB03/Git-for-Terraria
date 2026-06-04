using Git.Common.Data;
using Git.Content.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Git.Common.Systems
{
    [Autoload(Side = ModSide.Client)]
    public class PreviewSystem : ModSystem
    {
        // Alpha for the ghost overlay (0-255)
        private const byte PreviewAlpha = 160;
        private static readonly Color TileColor  = new Color(255, 255, 255, PreviewAlpha);
        private static readonly Color WallColor  = new Color(180, 180, 180, PreviewAlpha / 2);
        private static readonly Color BorderColor = new Color(80, 220, 255, 220);

        public override void PostDrawTiles()
        {
            Player player = Main.LocalPlayer;
            if (player.HeldItem == null || !(player.HeldItem.ModItem is PasteWand))
                return;

            SchematicCommit commit = GetPreviewCommit();
            if (commit == null || commit.TilesFlat == null)
                return;

            Point anchor = Main.MouseWorld.ToTileCoordinates();
            float zoom   = Main.GameViewMatrix.Zoom.X;
            int tileSize = (int)(16f * zoom);

            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.None,
                RasterizerState.CullNone);

            for (int y = 0; y < commit.Height; y++)
            {
                for (int x = 0; x < commit.Width; x++)
                {
                    TileData tile = commit.TilesFlat[y * commit.Width + x];

                    // Screen position for this cell
                    int sx = (int)(((anchor.X + x) * 16f - Main.screenPosition.X) * zoom);
                    int sy = (int)(((anchor.Y + y) * 16f - Main.screenPosition.Y) * zoom);

                    // Cull off-screen
                    if (sx + tileSize < 0 || sx > Main.screenWidth)  continue;
                    if (sy + tileSize < 0 || sy > Main.screenHeight) continue;

                    var destRect = new Rectangle(sx, sy, tileSize, tileSize);

                    // --- Wall (drawn first, behind tiles) ---
                    if (tile.WallType != WallID.None)
                        DrawWall(tile, destRect);

                    // --- Tile ---
                    if (tile.HasTile)
                        DrawTile(tile, destRect);
                }
            }

            // Bounding box outline
            int bx = (int)((anchor.X * 16f - Main.screenPosition.X) * zoom);
            int by = (int)((anchor.Y * 16f - Main.screenPosition.Y) * zoom);
            int bw = (int)(commit.Width  * 16f * zoom);
            int bh = (int)(commit.Height * 16f * zoom);
            int t  = 2;
            var px = TextureAssets.MagicPixel.Value;
            Main.spriteBatch.Draw(px, new Rectangle(bx,          by,          bw, t),  BorderColor);
            Main.spriteBatch.Draw(px, new Rectangle(bx,          by + bh - t, bw, t),  BorderColor);
            Main.spriteBatch.Draw(px, new Rectangle(bx,          by,          t,  bh), BorderColor);
            Main.spriteBatch.Draw(px, new Rectangle(bx + bw - t, by,          t,  bh), BorderColor);

            Main.spriteBatch.End();
        }

        private static void DrawTile(TileData tile, Rectangle dest)
        {
            if (tile.TileType >= TextureAssets.Tile.Length) return;

            var asset = TextureAssets.Tile[tile.TileType];
            if (asset == null || asset.State != AssetState.Loaded) return;

            Texture2D tex = asset.Value;
            var src = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16);

            // Guard against out-of-bounds frames (some animated/special tiles)
            if (src.Right > tex.Width || src.Bottom > tex.Height) return;

            Main.spriteBatch.Draw(tex, dest, src, TileColor);
        }

        private static void DrawWall(TileData tile, Rectangle dest)
        {
            // Wall texture indices start at 1; index 0 = no wall
            if (tile.WallType == WallID.None || tile.WallType >= TextureAssets.Wall.Length) return;

            var asset = TextureAssets.Wall[tile.WallType];
            if (asset == null || asset.State != AssetState.Loaded) return;

            Texture2D tex = asset.Value;

            // Wall frames are 32x32 in the sheet but we only need the 16x16 tile-space portion.
            // Use stored frame coords; clamp to texture size.
            int fx = tile.TileFrameX;
            int fy = tile.TileFrameY;
            if (fx + 16 > tex.Width)  fx = 0;
            if (fy + 16 > tex.Height) fy = 0;

            var src = new Rectangle(fx, fy, 16, 16);
            Main.spriteBatch.Draw(tex, dest, src, WallColor);
        }

        private static SchematicCommit GetPreviewCommit()
        {
            if (SchematicManager.Selected == null) return null;

            var commits = SchematicManager.Selected.Commits;
            if (commits.Count == 0) return null;

            int idx = SchematicManager.PasteCommitIndex;
            if (idx >= 0 && idx < commits.Count)
                return commits[idx];

            return commits[commits.Count - 1];
        }
    }
}
