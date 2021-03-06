﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Resources.Scripts.Weapons {   

    class Weapons {
        public static Weapons instance = new Weapons();

        //The pattern for each of this is 
        // Weapon(Damage, Lifetime, Firerate, Spread, Speed, Magazine, NumberOfBullets, ReloadSpeed, name)
        public Weapon GetPistol() {
            return new Weapon(10f, 1.5f, 0.5f, 2f, 15f, 10, 1, 0.8f, "Pistol");
        }
        public Weapon GetAssaultRifle() {
            return new Weapon(20f, 3f, 0.4f, 1f, 20f, 20, 1, 1f, "Assault Rifle");
        }
        public Weapon GetShotgun() {
            return new Weapon(20f, 0.7f, 1f, 2f, 20f, 4, 3, 1.3f, "Shotgun");
        }
        public Weapon GetSniper() {
            return new Weapon(50f, 4f, 1.5f, 0.5f, 40f, 5, 1, 1.5f, "Sniper");
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
        float ReloadTime;
        string weaponName;

        public Weapon(float dmg, float lifetime, float firerate, float spread, float speed, int magazine, int numberOfBullets, float reloadTime, string weaponName) {
            this.Damage = dmg;
            this.Lifetime = lifetime;
            this.Firerate = firerate;
            this.Spread = spread;
            this.Speed = speed;
            this.Magazine = magazine;
            this.NumberOfBullets = numberOfBullets;
            this.ReloadTime = reloadTime;
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
        public float GetReloadTime() {
            return this.ReloadTime;
        }
        public string GetWeaponName() {
            return this.weaponName;
        }
        #endregion

        #region "Setters"
        public void SetRange(Weapon weapon) {
            float weaponRange = weapon.Lifetime;
            weapon.Lifetime = 0.2f + weaponRange;
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
