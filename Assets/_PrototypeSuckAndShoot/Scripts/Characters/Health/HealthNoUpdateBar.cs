using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpiritBomb.Prototype.SuckAndShoot
{
    public class HealthNoUpdateBar : Health
    {
        [MMInspectorGroup("Health Bar", true, 10)]
        // whether health bar should be updated
        [Tooltip("whether health bar should be updated")]
        public bool IsUpdateHealthBar = true;

        // overridden health bar
        [Tooltip("overridden health bar")]
        public MMHealthBar PrioritizedHealthBar;


        public override void Initialization()
        {
            base.Initialization();

            if (!IsUpdateHealthBar)
            {
                _healthBar = default;
            }
            else if (PrioritizedHealthBar != default)
            {
                _healthBar = PrioritizedHealthBar;
            }
        }
    }
}
