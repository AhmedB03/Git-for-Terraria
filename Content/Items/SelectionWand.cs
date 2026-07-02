using Git.Common.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Git.Content.Items
{
    public class SelectionWand : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(silver: 50);
        }

        // Right-click clears the selection
        public override bool AltFunctionUse(Player player) => true;

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                SelectionSystem.ClearSelection();
                Main.NewText("[Git] Selection cleared.", Color.Yellow);
                return true;
            }

            Point tile = Main.MouseWorld.ToTileCoordinates();

            if (!SelectionSystem.IsSelecting)
            {
                SelectionSystem.SetFirstCorner(tile);
                Main.NewText($"[Git] First corner set: ({tile.X}, {tile.Y})", Color.LightGreen);
            }
            else
            {
                SelectionSystem.SetSecondCorner(tile);
                Main.NewText(
                    $"[Git] Selection: ({SelectionSystem.TopLeft.X},{SelectionSystem.TopLeft.Y}) " +
                    $"to ({SelectionSystem.BottomRight.X},{SelectionSystem.BottomRight.Y}) " +
                    $"[{SelectionSystem.Width}x{SelectionSystem.Height}]",
                    Color.LightGreen);
            }

            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 2)
                .AddRecipeGroup(RecipeGroupID.IronBar, 1)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
