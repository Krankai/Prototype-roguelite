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
    [Tooltip("to modify main or secondary weapon")]
    [HideInInspector]
    public bool IsSecondaryWeapon;

    private CharacterHandleWeapon CharacterHandleWeaponScript
    {
        get
        {
            if (_weapon.Owner == default)
            {
                return _characterHandleWeaponScript;
            }

            if (_characterHandleWeaponScript == default)
            {
                if (!IsSecondaryWeapon)
                {
                    //_characterHandleWeaponScript = _weapon.Owner.gameObject.GetComponent<CharacterHandleWeapon>();
                    _characterHandleWeaponScript = _weapon.Owner.FindAbility<CharacterHandleWeapon>();
                }
                else
                {
                    //_characterHandleWeaponScript = _weapon.Owner.gameObject.GetComponent<CharacterHandleSecondaryWeapon>();
                    _characterHandleWeaponScript = _weapon.Owner.FindAbility<CharacterHandleSecondaryWeapon>();
                }
            }

            return _characterHandleWeaponScript;
        }
    }
    private CharacterHandleWeapon _characterHandleWeaponScript;

    public override void GetScriptAim()
    {
        if (CharacterHandleWeaponScript == default || CharacterHandleWeaponScript.WeaponAttachment == default)
        {
            base.GetScriptAim();
            return;
        }

        var directionFactor = IsFaceRight ? 1 : -1;
        _currentAim = directionFactor * CharacterHandleWeaponScript.WeaponAttachment.right;

        base.GetScriptAim();
    }
}
