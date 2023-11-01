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

        var directionFactor = IsFaceRight ? 1 : -1;
        _currentAim = directionFactor * WeaponAttachment.right;

        base.GetScriptAim();
    }
}
