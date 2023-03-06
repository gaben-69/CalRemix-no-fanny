using CalamityMod;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace CalRemix.Projectiles
{
	public class HornetShot : ModProjectile
	{
        List<int> hit = new List<int>{ -1 };
        public override string Texture => "Terraria/Images/Projectile_242";
        public override void SetStaticDefaults() 
        {
			DisplayName.SetDefault("22 Hornet Round");
		}
		public override void SetDefaults() 
        {
            Projectile.CloneDefaults(ProjectileID.BulletHighVelocity);
            AIType = ProjectileID.BulletHighVelocity;
            Projectile.tileCollide = false;
            Projectile.penetrate = 22;
            Projectile.timeLeft = 2222;
        }
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
            hit.Add(target.whoAmI);
            Projectile.Center = target.Center;
            NPC npc = target;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC newTarget = Main.npc[i];
                if (newTarget != npc && newTarget != null && !hit.Contains(newTarget.whoAmI) && newTarget.chaseable && !newTarget.friendly)
                {
                    npc = newTarget;
                    break;
                }
            }
            if (npc != null && npc != target)
            {
                Projectile.velocity = npc.velocity + Projectile.DirectionTo(npc.Center) * 22f;
            }
        }
    }
}