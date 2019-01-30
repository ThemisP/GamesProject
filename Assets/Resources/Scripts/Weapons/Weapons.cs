using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Resources.Scripts.Weapons {   

    class Weapons {
        public static Weapons instance = new Weapons();

        //The pattern for each of this is 
        // Weapon(Damage, Lifetime, Firerate, Spread, Speed, NumberOfBullets)
        public Weapon GetPistol() {
            return new Weapon(10f, 0.7f, 1f, 2f, 2f, 1);
        }
        public Weapon GetAssaultRifle() {
            return new Weapon(20f, 3f, 0.6f, 1f, 2.5f, 1);
        }
        public Weapon GetShotgun() {
            return new Weapon(10f, 0.7f, 1.2f, 2f, 3f, 3);
        }
        public Weapon GetSniper() {
            return new Weapon(50f, 4f, 2f, 0.5f, 20f, 1);
        }
    }

    public class Weapon {
        float Damage;
        float Lifetime; //range
        float Firerate; //time between bullet fire
        float Spread;
        float Speed;
        int NumberOfBullets;

        public Weapon(float dmg, float lifetime, float firerate, float spread, float speed, int numberOfBullets) {
            this.Damage = dmg;
            this.Lifetime = lifetime;
            this.Firerate = firerate;
            this.Spread = spread;
            this.Speed = speed;
            this.NumberOfBullets = numberOfBullets;
        }
        #region "Getters"
        public float GetDamage() {
            return this.Damage;
        }
        public float GetLifetime() {
            return this.Lifetime;
        }
        public float GetFirerate() {
            return this.Firerate;
        }
        public float GetSpread() {
            return this.Spread;
        }
        public float GetSpeed() {
            return this.Speed;
        }
        public int GetNumberOfBullets() {
            return this.NumberOfBullets;
        }
        #endregion
    }
}
