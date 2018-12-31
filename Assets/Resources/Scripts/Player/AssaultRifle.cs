using System;
namespace AssemblyCSharp.Assets.Resources.Scripts.Player
{
    public class AssaultRifle : Weapon
    {
        public AssaultRifle()
        {
            damage = 10;
            lifeTime = 2f;
            velocityFactor = 10f;
            fireRate = 1f;
        }
    }
}
