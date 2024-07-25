﻿using CalRemix.Items.Lore;
using CalRemix.Items.Placeables.Trophies;
using Terraria;
using Terraria.ID;

namespace CalRemix.Items.Lore
{
    public class KnowledgeOrigen : RemixLoreItem
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            DisplayName.SetDefault("The Originator");
        }

        public override string LoreText => "Why wasn't this scoundrel dealt with sooner?\nI understand the concept of efficient time, yet this little deviant proved to be less than an issue.\nAgainst my wishes, I pity this construct deeply. You lived a flawed existence.\r\n";

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.rare = ItemRarityID.Blue;
            Item.consumable = false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<OrigenTrophy>().
                AddTile(TileID.Bookcases).
                Register();
        }
    }
}
