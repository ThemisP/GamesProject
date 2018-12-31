using System;
namespace AssemblyCSharp.Assets.Resources.Scripts.Player
{
    public class Shotgun : Weapon
    {
        public Shotgun()
        {
            damage = 25;
            lifeTime = 1f;
            velocityFactor = 10f;
            fireRate = 2f;
        }
    }
}
