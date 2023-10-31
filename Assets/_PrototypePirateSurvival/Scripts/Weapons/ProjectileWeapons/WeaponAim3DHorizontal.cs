using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;

public class WeaponAim3DHorizontal : WeaponAim3D
{
    [Header("Horizontal Direction")]
    // whether the weapon locks its aim to the right
    [Tooltip("whether the weapon locks its aim to the right")]
    public bool IsFaceRight;

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
        //if (HandleWeaponAbility == default || HandleWeaponAbility.WeaponAttachment == default)
        //{
        //    base.GetScriptAim();
        //    return;
        //}

        //var directionFactor = IsFaceRight ? 1 : -1;
        //_currentAim = directionFactor * HandleWeaponAbility.WeaponAttachment.right;

        if (WeaponAttachment == default)
        {
            base.GetScriptAim();
            return;
        }

        var directionFactor = IsFaceRight ? 1 : -1;
        _currentAim = directionFactor * WeaponAttachment.right;

        base.GetScriptAim();
    }
}
