using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponChangeNotifier : MonoBehaviour
{
    [Header("Main Weapon")]
    [SerializeField]
    private CharacterHandleWeapon _handleWeaponAbility;
    [SerializeField]
    private bool _isWeaponFaceRight;

    [Header("Secondary Weapon")]
    [SerializeField]
    private CharacterHandleSecondaryWeapon _handleSecondaryWeaponAbility;
    [SerializeField]
    private bool _isSecondaryWeaponFaceRight;

    //[SerializeField]
    //private Vector3 _fromSecondaryShootAngle = Vector3.zero;
    //[SerializeField]
    //private Vector3 _toSecondaryShootAngle = Vector3.zero;


    protected virtual void Awake()
    {
        Initialization();
    }

    protected virtual void Initialization()
    {
        if (_handleWeaponAbility != default)
        {
            _handleWeaponAbility.OnWeaponChange += OnWeaponChange;
        }

        if (_handleSecondaryWeaponAbility != default)
        {
            _handleSecondaryWeaponAbility.OnWeaponChange += OnSecondaryWeaponChange;
        }
    }

    private void OnWeaponChange()
    {
        var weaponHorizontalAim3D = _handleWeaponAbility.WeaponAimComponent as WeaponAim3DHorizontal;
        if (weaponHorizontalAim3D != default)
        {
            weaponHorizontalAim3D.IsFaceRight = _isWeaponFaceRight;
            weaponHorizontalAim3D.IsSecondaryWeapon = false;
        }
    }

    private void OnSecondaryWeaponChange()
    {
        var weaponHorizontalAim3D = _handleSecondaryWeaponAbility.WeaponAimComponent as WeaponAim3DHorizontal;
        if (weaponHorizontalAim3D != default)
        {
            weaponHorizontalAim3D.IsFaceRight = _isSecondaryWeaponFaceRight;
            weaponHorizontalAim3D.IsSecondaryWeapon = true;
        }

        //var weapon = _handleSecondaryWeaponAbility.CurrentWeapon as ProjectileWeaponAngle;
        //if (weapon != default)
        //{
        //    weapon.FromShootAngle = _fromSecondaryShootAngle;
        //    weapon.ToShootAngle = _toSecondaryShootAngle;
        //}
    }
}
