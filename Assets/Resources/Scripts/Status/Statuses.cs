using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Resources.Scripts.Statuses
{

    class Statuses
    {
        public static Statuses instance = new Statuses();

        //The pattern for each of this is 
        // Weapon(Damage, Lifetime, Firerate, Spread, Speed, NumberOfBullets)
        public Status GetHealthy()
        {
            return new Status(0f, 0f, 6f);
        }
        public Status GetBurnt()
        {
            return new Status(0.15f, 10f, 7f);
        }
        public Status GetPoisoned()
        {
            return new Status(0.075f, 20f, 6f);
        }
        public Status GetInvincible()
        {
            return new Status(0f, 10f, 6f);
        }
        public Status GetParalyzed()
        {
            return new Status(0f, 15f, 3f);
        }
    }

    public class Status
    {
        float Damage;
        float Duration;
        float Speed;

        public Status(float damage, float duration, float speed)
        {
            this.Damage = damage;
            this.Duration = duration;
            this.Speed = speed;
        }
        #region "Getters"
        public float GetDamage()
        {
            return this.Damage;
        }
        public float GetDuration()
        {
            return this.Duration;
        }
        public float GetSpeed()
        {
            return this.Speed;
        }
        #endregion

    }
}
