using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpiritBomb.Prototype.SuckAndShoot
{
    public class WeaponAim3DForward : WeaponAim3D
    {
        protected override void Initialization()
        {
            base.Initialization();
            _weapon = gameObject.MMGetComponentNoAlloc<Weapon>();
        }

        public override void GetScriptAim()
        {
            _currentAim = _weapon.transform.forward;
            base.GetScriptAim();
        }
    }
}
