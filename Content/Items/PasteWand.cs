using System.Linq;
using Git.Common.Config;
using Git.Common.Data;
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
            SchematicCommit commit = SchematicManager.GetActiveCommit();
            if (commit == null)
            {
                Main.NewText("[Git] No schematic selected. Press G to open the UI.", Color.Orange);
                return true;
            }

            Point anchor = Main.MouseWorld.ToTileCoordinates();
            bool requireResources = ModContent.GetInstance<GitServerConfig>().RequirePasteResources;

            if (requireResources)
            {
                var cost = PasteCostSystem.ComputeCost(commit);
                if (!PasteCostSystem.TryConsume(player, cost, out var missing))
                {
                    var parts = missing.Take(4).Select(m => $"{m.shortBy} {Lang.GetItemNameValue(m.type)}");
                    string more = missing.Count > 4 ? $" (+{missing.Count - 4} more)" : "";
                    Main.NewText($"[Git] Missing materials: {string.Join(", ", parts)}{more}", Color.Orange);
                    return true;
                }
            }

            Schematic.PasteCommit(commit, anchor, skipUnobtainable: requireResources);
            Main.NewText($"[Git] Pasted \"{SchematicManager.Selected.Name}\" at ({anchor.X}, {anchor.Y}).", Color.LightGreen);
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
