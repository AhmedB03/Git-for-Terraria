using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Git.Common.Systems
{
    public class SelectionSystem : ModSystem
    {
        public static bool IsSelecting { get; private set; }
        public static Point FirstCorner { get; private set; }
        public static Point SecondCorner { get; private set; }
        public static bool HasSelection { get; private set; }

        // Normalized so TopLeft.X <= BottomRight.X etc.
        public static Point TopLeft => new(
            System.Math.Min(FirstCorner.X, SecondCorner.X),
            System.Math.Min(FirstCorner.Y, SecondCorner.Y));

        public static Point BottomRight => new(
            System.Math.Max(FirstCorner.X, SecondCorner.X),
            System.Math.Max(FirstCorner.Y, SecondCorner.Y));

        public static int Width => BottomRight.X - TopLeft.X + 1;
        public static int Height => BottomRight.Y - TopLeft.Y + 1;

        public static void SetFirstCorner(Point tile)
        {
            FirstCorner = tile;
            IsSelecting = true;
            HasSelection = false;
        }

        public static void SetSecondCorner(Point tile)
        {
            SecondCorner = tile;
            IsSelecting = false;
            HasSelection = true;
        }

        public static void ClearSelection()
        {
            HasSelection = false;
            IsSelecting = false;
        }
    }
}
