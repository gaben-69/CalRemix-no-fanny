﻿using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.Rarities;
using CalRemix.Projectiles.Weapons;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalRemix.Items.Weapons;

public class AergianTechnistaff : ModItem
{

    public override void SetStaticDefaults()
    {
        DisplayName.SetDefault("Aergian Technistaff");
        Tooltip.SetDefault("Summons an orbital Aergia Neuron\nHolding right click causes the orbit to accelerate\nOnce fully charged, the ring can be thrown to chase enemies");
    }

    public override void SetDefaults()
    {
        Item.damage = 127;
        Item.DamageType = DamageClass.Summon;
        Item.width = 10;
        Item.height = 10;
        Item.useTime = 6;
        Item.useAnimation = 6;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.noMelee = true;
        Item.knockBack = 0;
        Item.rare = ModContent.RarityType<Violet>();
        Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
        Item.UseSound = BetterSoundID.ItemDeadlySphereVroom;
        Item.autoReuse = true;
        Item.shoot = ModContent.ProjectileType<NobodyKnows>();
        Item.shootSpeed = 0f;
        Item.mana = 28;
    }

    public override bool AltFunctionUse(Player player)
    {
        return true;
    }

    public override bool? UseItem(Player player)
    {
        Item.noMelee = player.altFunctionUse == 2;
        return null;
    }

    public override void HoldItem(Player player)
    {
        if (Main.myPlayer == player.whoAmI)
            player.Calamity().rightClickListener = true;

        if (player.Calamity().mouseRight && CanUseItem(player) && player.whoAmI == Main.myPlayer && !Main.mapFullscreen && !Main.blockMouse)
        {
            Item.autoReuse = true;
        }
        else
        {
            Item.autoReuse = true;
        }
    }

    public override void UseAnimation(Player player)
    {
        if (player.altFunctionUse == 2f)
        {
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTurn = false;
            Item.shoot = ProjectileID.None;
        }
        else
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.shoot = ModContent.ProjectileType<AergiaNeuronSummon>();
        }
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.ownedProjectileCounts[ModContent.ProjectileType<AergiaNeuronCore>()] <= 0)
        {
            Projectile.NewProjectile(source, player.Center, Vector2.Zero, ModContent.ProjectileType<AergiaNeuronCore>(), damage, knockback, player.whoAmI);
        }
        int coreIndex = -1;
        int totalNeurons = player.ownedProjectileCounts[type];
        float currentRot = 0;
        foreach (Projectile proj in Main.ActiveProjectiles)
        {
            if (proj.type == ModContent.ProjectileType<AergiaNeuronCore>() && proj.owner == player.whoAmI)
            {
                coreIndex = proj.whoAmI;
            }
            if (proj.type == ModContent.ProjectileType<AergiaNeuronSummon>() && proj.owner == player.whoAmI)
            {
                proj.ai[2] = totalNeurons + 1;
                currentRot = proj.localAI[0];
            }
        }
        int neuron = Projectile.NewProjectile(source, player.Center, Vector2.Zero, type, damage, knockback, player.whoAmI, coreIndex, totalNeurons, totalNeurons + 1);
        Main.projectile[neuron].localAI[0] = currentRot;
        Main.projectile[neuron].localAI[1] = 1;
        return false;
    }
}
