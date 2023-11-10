using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpiritBomb.Prototype.SuckAndShoot
{
    public class AIActionMeleeAttack : AIAction
    {
        [Header("Binding")]
        /// the CharacterHandleWeapon ability this AI action should pilot. If left blank, the system will grab the first one it finds.
        [Tooltip("the CharacterHandleWeapon ability this AI action should pilot. If left blank, the system will grab the first one it finds.")]
        public CharacterHandleWeapon TargetHandleWeaponAbility;

        protected Character _character;
        protected MeleeWeapon _meleeWeapon;


        public override void Initialization()
        {
            if (!ShouldInitialize) return;
            base.Initialization();

            _character = GetComponentInParent<Character>();
            if (TargetHandleWeaponAbility == null && _character != default)
            {
                TargetHandleWeaponAbility = _character.FindAbility<CharacterHandleWeapon>();
            }
        }

        public override void OnEnterState()
        {
            base.OnEnterState();
            _meleeWeapon = TargetHandleWeaponAbility.CurrentWeapon.gameObject.MMGetComponentNoAlloc<MeleeWeapon>();
        }

        public override void OnExitState()
        {
            base.OnExitState();
            TargetHandleWeaponAbility.ForceStop();
        }

        public override void PerformAction()
        {
            if (_meleeWeapon == default)
            {
                return;
            }

            _meleeWeapon.WeaponUse();
        }
    }
}
