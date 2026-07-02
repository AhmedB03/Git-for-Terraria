using Terraria;
using Terraria.ID;

namespace Git.Common.Data
{
    public class TileData
    {
        public ushort TileType;
        public ushort WallType;
        public bool HasTile;
        public byte TileColor;
        public byte WallColor;
        public byte Slope;
        public bool IsHalfBlock;
        public bool RedWire;
        public bool BlueWire;
        public bool GreenWire;
        public bool YellowWire;
        public bool Actuator;
        public bool ActuationIsActive;
        public byte LiquidAmount;
        public byte LiquidType;
        public short TileFrameX;
        public short TileFrameY;

        public static TileData Capture(int worldX, int worldY)
        {
            Tile t = Main.tile[worldX, worldY];
            return new TileData
            {
                HasTile = t.HasTile,
                TileType = t.TileType,
                WallType = t.WallType,
                TileColor = t.TileColor,
                WallColor = t.WallColor,
                Slope = (byte)t.Slope,
                IsHalfBlock = t.IsHalfBlock,
                RedWire = t.RedWire,
                BlueWire = t.BlueWire,
                GreenWire = t.GreenWire,
                YellowWire = t.YellowWire,
                Actuator = t.HasActuator,
                ActuationIsActive = t.IsActuated,
                LiquidAmount = t.LiquidAmount,
                LiquidType = (byte)t.LiquidType,
                TileFrameX = t.TileFrameX,
                TileFrameY = t.TileFrameY,
            };
        }

        public void Place(int worldX, int worldY, bool placeTile = true, bool placeWall = true)
        {
            if (worldX < 0 || worldY < 0 || worldX >= Main.maxTilesX || worldY >= Main.maxTilesY)
                return;

            bool hasTile = HasTile && placeTile;

            Tile t = Main.tile[worldX, worldY];
            t.ClearTile();
            t.WallType = WallID.None;

            t.HasTile = hasTile;
            if (hasTile)
            {
                t.TileType = TileType;
                t.TileColor = TileColor;
                t.Slope = (SlopeType)Slope;
                t.IsHalfBlock = IsHalfBlock;
                t.TileFrameX = TileFrameX;
                t.TileFrameY = TileFrameY;
            }

            if (placeWall)
            {
                t.WallType = WallType;
                t.WallColor = WallColor;
            }
            t.RedWire = RedWire;
            t.BlueWire = BlueWire;
            t.GreenWire = GreenWire;
            t.YellowWire = YellowWire;
            t.HasActuator = Actuator;
            t.IsActuated = ActuationIsActive;
            t.LiquidAmount = LiquidAmount;
            t.LiquidType = LiquidType;
        }
    }
}
