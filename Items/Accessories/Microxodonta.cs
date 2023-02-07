﻿using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using CalamityMod;
using CalamityMod.Items;
using Terraria.ModLoader;
using CalamityMod.Rarities;
using CalRemix.Projectiles.Accessories;

namespace CalRemix.Items.Accessories
{
    public class Microxodonta : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 1;
            DisplayName.SetDefault("Microxodonta");
            Tooltip.SetDefault("92% increased movement speed\n"+
            "Flying leaves behind batches of damaging microbial clusters");
        }

        public override void SetDefaults()
        {
            Item.defense = 20;
            Item.width = 20;
            Item.height = 22;
            Item.value = CalamityGlobalItem.Rarity13BuyPrice;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<PureGreen>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var source = player.GetSource_Accessory(Item);
            player.moveSpeed += 0.92f;
            if (player.wingTime > 0 && player.controlJump && Main.rand.NextBool(2))
            {
                if (player.whoAmI == Main.myPlayer)
                {
                    int microbeDamage = (int)player.GetBestClassDamage().ApplyTo(300);
                    int p = Projectile.NewProjectile(source, player.Center.X, player.Center.Y, 0f, 0f, ModContent.ProjectileType<MicrobeParticle>(), microbeDamage, 0f, player.whoAmI, 0f, 0f);
                    if (p.WithinBounds(Main.maxProjectiles))
                    {
                        Main.projectile[p].originalDamage = microbeDamage;
                    }
                }
            }
        }
    }
}
