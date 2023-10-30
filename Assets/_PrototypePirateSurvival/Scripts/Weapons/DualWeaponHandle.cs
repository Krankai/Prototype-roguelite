using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;

public class DualWeaponHandle : MonoBehaviour, MMEventListener<MMGameEvent>
{
    [Header("Character")]
    [SerializeField]
    private Character _character;

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


    protected virtual void Awake()
    {
        Initialization();
    }

    protected void OnEnable()
    {
        if (_character != default && _character.CharacterType == Character.CharacterTypes.Player)
        {
            this.MMEventStartListening();
        }
    }

    protected void OnDisable()
    {
        if (_character != default && _character.CharacterType == Character.CharacterTypes.Player)
        {
            this.MMEventStopListening();
        }
    }

    protected virtual void Initialization()
    {
        if (_character == default)
        {
            _character = transform.parent.GetComponentInParent<Character>();
        }

        if (_handleWeaponAbility != default)
        {
            _handleWeaponAbility.OnWeaponChange += OnWeaponChange;
        }

        if (_handleSecondaryWeaponAbility != default)
        {
            _handleSecondaryWeaponAbility.OnWeaponChange += OnSecondaryWeaponChange;
        }
    }

    protected virtual void ShootDualWeapon()
    {
        _handleWeaponAbility.ShootStart();
        _handleSecondaryWeaponAbility.ShootStart();
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
    }

    public void OnMMEvent(MMGameEvent eventType)
    {
        if (eventType.EventName.Equals("DualShoot", System.StringComparison.OrdinalIgnoreCase))
        {
            ShootDualWeapon();
        }
    }
}
