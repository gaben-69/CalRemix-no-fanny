﻿using CalamityMod.Dusts;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using CalamityMod;
using CalamityMod.BiomeManagers;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using System;
using System.Collections.Generic;
using CalamityMod.Events;
using CalamityMod.UI;
using System.Linq;
using Terraria.ModLoader.Core;
using CalRemix.NPCs.TownNPCs;
using Terraria.ModLoader.IO;
using System.IO;
using CalRemix.Projectiles.Hostile;
using Microsoft.Build.Tasks.Deployment.ManifestUtilities;
using Terraria.WorldBuilding;
using CalRemix.NPCs.Bosses.Carcinogen;
using Terraria.DataStructures;

namespace CalRemix.NPCs.PandemicPanic
{
    public class PandemicPanic : ModSystem
    {
        /// <summary>
        /// Whether or not the event is active
        /// </summary>
        public static bool IsActive;

        /// <summary>
        /// Enemies considered defenders
        /// </summary>
        public static List<int> DefenderNPCs = new List<int>() { ModContent.NPCType<Eosinine>(), ModContent.NPCType<Platelet>(), ModContent.NPCType<WhiteBloodCell>(), ModContent.NPCType<RedBloodCell>(), ModContent.NPCType<Dendritiator>(), ModContent.NPCType<DendtritiatorArm>() };

        /// <summary>
        /// Enemies considered invaders
        /// </summary>
        public static List<int> InvaderNPCs = new List<int>() { ModContent.NPCType<Malignant>(), ModContent.NPCType<Ecolium>(), ModContent.NPCType<Basilius>(), ModContent.NPCType<BasiliusBody>(), ModContent.NPCType<Tobasaia>(), ModContent.NPCType<MaserPhage>() };

        /// <summary>
        /// Projectiles considered defenders
        /// </summary>
        public static List<int> DefenderProjectiles = new List<int>() { ModContent.ProjectileType<EosinineProj>(), ProjectileID.BloodNautilusShot };

        /// <summary>
        /// Projectiles considered invaders
        /// </summary>
        public static List<int> InvaderProjectiles = new List<int>() { ProjectileID.BloodShot, ModContent.ProjectileType<TobaccoSeed>(), ProjectileID.DeathLaser, ModContent.ProjectileType<MaserDeathray>() };

        /// <summary>
        /// Defender NPC kill count
        /// </summary>
        public static float DefendersKilled = 0;

        /// <summary>
        /// Invader NPC kill count
        /// </summary>
        public static float InvadersKilled = 0;

        /// <summary>
        /// Total kills, duh
        /// </summary>
        public static float TotalKills => DefendersKilled + InvadersKilled;

        /// <summary>
        /// Defender kills required in order to end the event as an invader
        /// </summary>
        public static float MaxRequired => MinToSummonPathogen + 200;

        /// <summary>
        /// How much higher a faction's kill count has to be to side with them
        /// </summary>
        public const float KillBuffer = 30;

        /// <summary>
        /// The amount of kills required to summon Pathogen
        /// </summary>
        public const float MinToSummonPathogen = 300;

        /// <summary>
        /// If the player is on the defenders' side
        /// </summary>
        public static bool DefendersWinning => InvadersKilled > DefendersKilled + KillBuffer;

        /// <summary>
        /// If the player is on the invaders' side
        /// </summary>
        public static bool InvadersWinning => DefendersKilled > InvadersKilled + KillBuffer;

        /// <summary>
        /// If Pathogen has been summoned
        /// </summary>
        public static bool SummonedPathogen = false;

        public static float coughTimer = 0;

        public override void PreUpdateWorld()
        {
            IsActive = true;
            if (IsActive)
            {
                if (TotalKills >= 300 && !SummonedPathogen)
                {
                    NPC.SpawnOnPlayer(Main.LocalPlayer.whoAmI, NPCID.Frankenstein);
                    SummonedPathogen = true;
                }
                if (Main.rand.NextBool((int)MathHelper.Lerp(1200, 300, DefendersKilled / MaxRequired)) && coughTimer <= 0)
                {
                    coughTimer = Main.rand.Next(60, 120);
                }
                if (coughTimer > 0)
                {
                    coughTimer--;
                    if (coughTimer % MathHelper.Lerp(40, 20, DefendersKilled / MaxRequired) == 0)
                    {
                        SoundEngine.PlaySound(Carcinogen.DeathSound);
                        Main.LocalPlayer.Calamity().GeneralScreenShakePower = 100;
                    }
                }
            }
            if (DefendersKilled >= MaxRequired)
            {
                EndEvent();
            }
        }

        public static void EndEvent()
        {
            DefendersKilled = 0;
            InvadersKilled = 0;
            SummonedPathogen = false;
            IsActive = false;
            CalRemixWorld.UpdateWorldBool();
        }

        public override void OnWorldLoad()
        {
            DefendersKilled = 0;
            InvadersKilled = 0;
            SummonedPathogen = false;
            IsActive = false;
        }

        public override void OnWorldUnload()
        {
            DefendersKilled = 0;
            InvadersKilled = 0;
            SummonedPathogen = false;
            IsActive = false;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            tag["BioDefenders"] = DefendersKilled;
            tag["BioInvaders"] = InvadersKilled;
            tag["PathoSummon"] = SummonedPathogen;
            tag["BioActive"] = IsActive;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            IsActive = tag.Get<bool>("BioActive");
            SummonedPathogen = tag.Get<bool>("PathoSummon");
            DefendersKilled = tag.Get<float>("BioDefenders");
            InvadersKilled = tag.Get<float>("BioInvaders");
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(DefendersKilled);
            writer.Write(InvadersKilled);
            writer.Write(SummonedPathogen);
            writer.Write(IsActive);
        }

        public override void NetReceive(BinaryReader reader)
        {
            DefendersKilled = reader.ReadSingle();
            InvadersKilled = reader.ReadSingle();
            SummonedPathogen = reader.ReadBoolean();
            IsActive = reader.ReadBoolean();
        }

        public static Entity BioGetTarget(bool defender, NPC npc)
        {
            float currentDist = 0;
            Entity targ = null;
            List<int> enemies = defender ? InvaderNPCs : DefenderNPCs;
            if (defender ? DefendersWinning : InvadersWinning)
            {
                foreach (NPC n in Main.npc)
                {
                    if (n == null)
                        continue;
                    if (!n.active)
                        continue;
                    if (n.life <= 0)
                        continue;
                    if (!enemies.Contains(n.type))
                        continue;
                    if (n.Distance(npc.Center) < currentDist)
                        continue;
                    currentDist = n.Distance(npc.Center);
                    targ = n;
                }
            }
            else
            {
                foreach (NPC n in Main.npc)
                {
                    if (n == null)
                        continue;
                    if (!n.active)
                        continue;
                    if (n.life <= 0)
                        continue;
                    if (!enemies.Contains(n.type))
                        continue;
                    if (n.Distance(npc.Center) < currentDist)
                        continue;
                    currentDist = n.Distance(npc.Center);
                    targ = n;
                }
                foreach (Player n in Main.player)
                {
                    if (n == null)
                        continue;
                    if (!n.active)
                        continue;
                    if (n.dead)
                        continue;
                    if (n.Distance(npc.Center) < currentDist)
                        continue;
                    currentDist = n.Distance(npc.Center);
                    targ = n;
                }
            }
            return targ;
        }
    }

    public class PandemicPanicNPC : GlobalNPC
    {
        public float hitCooldown = 0;
        public override bool InstancePerEntity => true;

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            hitCooldown = binaryReader.ReadSingle();
        }

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            binaryWriter.Write(hitCooldown);
        }

        public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        {
            return PandemicPanic.DefenderNPCs.Contains(entity.type) || PandemicPanic.InvaderNPCs.Contains(entity.type);
        }

        public override bool PreAI(NPC npc)
        {
            if (npc.life <= 0)
                return true;
            if (hitCooldown > 0)
            {
                hitCooldown--;
                return true;
            }
            if (npc.type != ModContent.NPCType<DendtritiatorArm>() && PandemicPanic.DefenderNPCs.Contains(npc.type))
            {
                npc.chaseable = !PandemicPanic.DefendersWinning;
                foreach (NPC n in Main.npc)
                {
                    if (n == null)
                        continue;
                    if (!n.active)
                        continue;
                    if (n.life <= 0)
                        continue;
                    if (!PandemicPanic.InvaderNPCs.Contains(n.type))
                        continue;
                    if (!n.getRect().Intersects(npc.getRect()))
                        continue;
                    int dam = npc.type == ModContent.NPCType<Platelet>() ? (int)(n.damage * 0.33f) : n.damage;
                    npc.SimpleStrikeNPC(dam, n.direction, false);
                    hitCooldown = 20;
                    if (npc.life <= 0 && n.type == ModContent.NPCType<Malignant>()/* && NPC.CountNPCS(ModContent.NPCType<Malignant>()) < 22*/)
                    {
                        int np = NPC.NewNPC(npc.GetSource_Death(), (int)npc.Center.X, (int)npc.Center.Y, ModContent.NPCType<Malignant>());
                        Main.npc[np].npcSlots = 0;
                        Main.npc[np].lifeMax = Main.npc[np].life = (int)MathHelper.Max(1, n.lifeMax / 2);
                        Main.npc[np].damage = (int)MathHelper.Max(1, n.damage * 0.75f);
                        Main.npc[np].scale = MathHelper.Max(0.2f, n.scale * 0.8f);
                    }
                    if (npc.life <= 0)
                    {
                        PandemicPanic.DefendersKilled++;
                        if (npc.type == ModContent.NPCType<Dendritiator>())
                        {
                            PandemicPanic.DefendersKilled += 4;
                        }
                    }
                    break;
                }
                if (hitCooldown > 0)
                {
                    return true;
                }
                foreach (Projectile n in Main.projectile)
                {
                    if (n == null)
                        continue;
                    if (!n.active)
                        continue;
                    if (!PandemicPanic.InvaderProjectiles.Contains(n.type))
                        continue;
                    if (!n.getRect().Intersects(npc.getRect()))
                        continue;
                    int dam = npc.type == ModContent.NPCType<Platelet>() ? (int)(n.damage * 0.1f) : n.damage;
                    npc.SimpleStrikeNPC(dam * (Main.expertMode ? 2 : 4), n.direction, false);
                    n.penetrate--;
                    hitCooldown = 20;
                    if (npc.life <= 0)
                    {
                        PandemicPanic.DefendersKilled++;
                        if (npc.type == ModContent.NPCType<Dendritiator>())
                        {
                            PandemicPanic.DefendersKilled += 4;
                        }
                    }
                    break;
                }
            }
            if (PandemicPanic.InvaderNPCs.Contains(npc.type))
            {
                npc.chaseable = !PandemicPanic.InvadersWinning;
                foreach (NPC n in Main.npc)
                {
                    if (n == null)
                        continue;
                    if (!n.active)
                        continue;
                    if (n.life <= 0)
                        continue;
                    if (n.type == ModContent.NPCType<Dendritiator>())
                        continue;
                    if (!PandemicPanic.DefenderNPCs.Contains(n.type))
                        continue;
                    bool armhit = false;
                    DendtritiatorArm arm = n.ModNPC<DendtritiatorArm>();
                    if (arm != null)
                    {
                        for (int i = 30 - 1; i > 5; i--)
                        {
                            if (npc.getRect().Intersects(new Rectangle((int)arm.Segments[i].position.X, (int)arm.Segments[i].position.Y, 10, 10)))
                            {
                                armhit = true;
                                break;
                            }
                        }
                    }
                    if (!n.getRect().Intersects(npc.getRect()) && !armhit)
                        continue;
                    if (n.damage <= 0)
                        continue;
                    npc.SimpleStrikeNPC(n.damage, n.direction, false);
                    hitCooldown = armhit ? 1 : 20;
                    if (npc.life <= 0)
                    {
                        PandemicPanic.InvadersKilled++;
                        if (npc.type == ModContent.NPCType<MaserPhage>())
                        {
                            PandemicPanic.InvadersKilled += 4;
                        }
                    }
                    break;
                }
                if (hitCooldown > 0)
                {
                    return true;
                }
                foreach (Projectile n in Main.projectile)
                {
                    if (n == null)
                        continue;
                    if (!n.active)
                        continue;
                    if (!PandemicPanic.DefenderProjectiles.Contains(n.type))
                        continue;
                    if (!n.getRect().Intersects(npc.getRect()))
                        continue;
                    npc.SimpleStrikeNPC(n.damage * (Main.expertMode ? 2 : 4), n.direction, false);
                    n.penetrate--;
                    hitCooldown = 20;
                    if (npc.life <= 0)
                    {
                        PandemicPanic.InvadersKilled++;
                        if (npc.type == ModContent.NPCType<MaserPhage>())
                        {
                            PandemicPanic.InvadersKilled += 4;
                        }
                    }
                    break;
                }
            }
            return true;
        }

        public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            if (npc.life <= 0)
            {
                if (PandemicPanic.InvaderNPCs.Contains(npc.type))
                    PandemicPanic.InvadersKilled++;
                if (PandemicPanic.DefenderNPCs.Contains(npc.type))
                    PandemicPanic.DefendersKilled++;
                if (npc.type == ModContent.NPCType<Dendritiator>())
                {
                    PandemicPanic.DefendersKilled += 4;
                }
                if (npc.type == ModContent.NPCType<MaserPhage>())
                {
                    PandemicPanic.InvadersKilled += 4;
                }
            }
        }

        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (projectile.friendly)
            {
                if (npc.life <= 0)
                {
                    if (PandemicPanic.InvaderNPCs.Contains(npc.type))
                        PandemicPanic.InvadersKilled++;
                    if (PandemicPanic.DefenderNPCs.Contains(npc.type))
                        PandemicPanic.DefendersKilled++;
                    if (npc.type == ModContent.NPCType<Dendritiator>())
                    {
                        PandemicPanic.DefendersKilled += 4;
                    }
                    if (npc.type == ModContent.NPCType<MaserPhage>())
                    {
                        PandemicPanic.InvadersKilled += 4;
                    }
                }
            }
        }

        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
        {
            if (PandemicPanic.InvaderNPCs.Contains(npc.type) && PandemicPanic.InvadersWinning)
                return false;
            if (PandemicPanic.DefenderNPCs.Contains(npc.type) && PandemicPanic.DefendersWinning)
                return false;
            return true;
        }

        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            if (PandemicPanic.IsActive)
            {
                if (PandemicPanic.SummonedPathogen && PandemicPanic.InvadersWinning)
                {
                    maxSpawns += 16;
                    spawnRate = 8;
                }
                else
                {
                    maxSpawns += 8;
                    spawnRate = 16;
                }
            }
        }

        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            if (PandemicPanic.IsActive)
            {
                pool.Clear();
                float defMult = PandemicPanic.SummonedPathogen && PandemicPanic.InvadersWinning ? 3f : PandemicPanic.DefendersWinning ? 0.8f : 1f;
                float invMult = PandemicPanic.InvadersWinning ? 0.8f : 1f;
                pool.Add(ModContent.NPCType<WhiteBloodCell>(), 0.6f * defMult);
                pool.Add(ModContent.NPCType<RedBloodCell>(), 0.4f * defMult);
                pool.Add(ModContent.NPCType<Platelet>(), 1f * defMult);
                pool.Add(ModContent.NPCType<Eosinine>(), 0.33f * defMult);
                if (!NPC.AnyNPCs(ModContent.NPCType<Dendritiator>()))
                    pool.Add(ModContent.NPCType<Dendritiator>(), 0.025f * defMult);

                pool.Add(ModContent.NPCType<Malignant>(), 0.7f * invMult);
                pool.Add(ModContent.NPCType<Ecolium>(), 0.5f * invMult);
                if (!NPC.AnyNPCs(ModContent.NPCType<Basilius>()))
                    pool.Add(ModContent.NPCType<Basilius>(), 0.1f * invMult);
                pool.Add(ModContent.NPCType<Tobasaia>(), 0.1f * invMult);
                if (!NPC.AnyNPCs(ModContent.NPCType<MaserPhage>()))
                    pool.Add(ModContent.NPCType<MaserPhage>(), 0.025f * invMult);
            }
        }
    }

    public class PandemicPanicProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return PandemicPanic.DefenderProjectiles.Contains(entity.type) || PandemicPanic.InvaderProjectiles.Contains(entity.type);
        }

        public override bool CanHitPlayer(Projectile projectile, Player target)
        {
            if (!PandemicPanic.IsActive)
                return true;
            if (PandemicPanic.InvaderProjectiles.Contains(projectile.type) && PandemicPanic.InvadersWinning)
                return false;
            if (PandemicPanic.DefenderProjectiles.Contains(projectile.type) && PandemicPanic.DefendersWinning)
                return false;
            return true;
        }
    }
}
