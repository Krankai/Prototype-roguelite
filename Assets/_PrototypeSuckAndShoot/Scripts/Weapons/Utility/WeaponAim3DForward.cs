using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpiritBomb.Prototype.SuckAndShoot
{
    public class WeaponAim3DForward : WeaponAim3D
    {
        public override void GetScriptAim()
        {
            _currentAim = _weapon.transform.forward;
            base.GetScriptAim();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + 3 * _currentAim);
        }
    }
}
