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
            return new Weapon(10f, 0.7f, 1f, 2f, 8f, 10, 1, "Pistol");
        }
        public Weapon GetAssaultRifle() {
            return new Weapon(20f, 3f, 0.4f, 1f, 10f, 20, 1, "Assault Rifle");
        }
        public Weapon GetShotgun() {
            return new Weapon(10f, 0.7f, 1f, 2f, 12f, 9, 3, "Shotgun");
        }
        public Weapon GetSniper() {
            return new Weapon(50f, 4f, 1.5f, 0.5f, 20f, 5, 1, "Sniper");
        }
    }

    public class Weapon {
        float Damage;
        float Lifetime; //range
        float Firerate; //time between bullet fire
        float Spread;
        float Speed;
        int Magazine;
        int NumberOfBullets;
        string weaponName;

        public Weapon(float dmg, float lifetime, float firerate, float spread, float speed, int magazine, int numberOfBullets, string weaponName) {
            this.Damage = dmg;
            this.Lifetime = lifetime;
            this.Firerate = firerate;
            this.Spread = spread;
            this.Speed = speed;
            this.Magazine = magazine;
            this.NumberOfBullets = numberOfBullets;
            this.weaponName = weaponName;
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
        public int GetMagazine() {
            return this.Magazine;
        }
        public int GetNumberOfBullets() {
            return this.NumberOfBullets;
        }
        public string GetWeaponName() {
            return this.weaponName;
        }
        #endregion

        #region "Setters"
        public void SetRange(Weapon weapon) {
            float weaponRange = weapon.Lifetime;
            weapon.Lifetime = 1.1f + weaponRange;

        }

        public void SetCapacity(Weapon weapon) {

            int weaponMagazine = weapon.Magazine;
            weapon.Magazine =  1 + weaponMagazine;

        }

        public void SetDamage(Weapon weapon) {
            float weaponDamage = weapon.Damage;
            weapon.Lifetime = 1.25f + weaponDamage;

        }
        #endregion
    }
}
