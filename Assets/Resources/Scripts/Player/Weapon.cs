using System;
using UnityEngine;
namespace AssemblyCSharp.Assets.Resources.Scripts.Player
{
    public abstract class Weapon : MonoBehaviour
    {
        public float damage;
        public float lifeTime;
        public float velocityFactor;
        public float fireRate;
    }
}
