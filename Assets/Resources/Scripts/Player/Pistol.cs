using System;
namespace AssemblyCSharp.Assets.Resources.Scripts.Player
{
    public class Pistol : Weapon
    {
        public Pistol()
        {
            damage = 10;
            lifeTime = 2f;
            velocityFactor = 10f;
            fireRate = 2f;
        }
    }
}
