using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpiritBomb.Prototype.SuckAndShoot
{
    public class ProjectileMotionControl : Projectile
    {
        public virtual void PauseControl()
        {
            _shouldMove = false;
        }

        public virtual void ResumeControl()
        {
            _shouldMove = true;
        }
    }
}
