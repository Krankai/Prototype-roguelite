using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Means, depth
/// </summary>
public class WeaponAim3DLongitudinal : WeaponAim3D
{
    [Header("Longitudial Direction")]
    // whether the weapon locks its aim to the front
    [Tooltip("whether the weapon locks its aim to the front")]
    public bool IsFaceForward;

    // to modify main or secondary weapon
    [HideInInspector]
    public bool IsSecondaryWeapon;

    private CharacterHandleWeapon HandleWeaponAbility
    {
        get
        {
            if (_weapon.Owner == default)
            {
                return _handleWeaponAbility;
            }

            if (_handleWeaponAbility == default)
            {
                if (!IsSecondaryWeapon)
                {
                    _handleWeaponAbility = _weapon.Owner.FindAbility<CharacterHandleWeapon>();
                }
                else
                {
                    _handleWeaponAbility = _weapon.Owner.FindAbility<CharacterHandleSecondaryWeapon>();
                }
            }

            return _handleWeaponAbility;
        }
    }
    private CharacterHandleWeapon _handleWeaponAbility;

    private Transform WeaponAttachment
    {
        get
        {
            if (_weaponAttachment == default)
            {
                _weaponAttachment = _weapon.transform.parent;
            }
            return _weaponAttachment;
        }
    }
    [SerializeField, MMReadOnly]
    private Transform _weaponAttachment;

    public override void GetScriptAim()
    {
        if (WeaponAttachment == default)
        {
            base.GetScriptAim();
            return;
        }

        var directionFactor = IsFaceForward ? 1 : -1;
        _currentAim = directionFactor * WeaponAttachment.forward;

        base.GetScriptAim();
    }
}
