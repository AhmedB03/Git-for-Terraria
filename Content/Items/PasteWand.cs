using Git.Common.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Git.Content.Items
{
    public class PasteWand : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(silver: 50);
        }

        public override bool? UseItem(Player player)
        {
            if (SchematicManager.Selected == null)
            {
                Main.NewText("[Git] No schematic selected. Open the Git UI to select one.", Color.Orange);
                return true;
            }

            Point anchor = Main.MouseWorld.ToTileCoordinates();
            int idx = SchematicManager.SelectedSchematicIndex;
            int commitIdx = SchematicManager.PasteCommitIndex;

            if (commitIdx < 0)
                SchematicManager.PasteLatest(idx, anchor);
            else
                SchematicManager.PasteCommitAt(idx, commitIdx, anchor);

            string name = SchematicManager.Selected.Name;
            Main.NewText($"[Git] Pasted \"{name}\" at ({anchor.X}, {anchor.Y}).", Color.LightGreen);
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 10)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
