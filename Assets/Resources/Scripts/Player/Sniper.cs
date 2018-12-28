using System;
namespace AssemblyCSharp.Assets.Resources.Scripts.Player
{
    public class Sniper : Weapon
    {
        public Sniper()
        {
            damage = 50;
            lifeTime = 5f;
            velocityFactor = 25f;
            fireRate = 15f;
        }
    }
}
