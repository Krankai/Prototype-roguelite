using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;
using UnityEngine.UI;

public class PrototypeGUIManager : GUIManager
{
    [Header("Prototype")]
    // the switch button to change player's shooting style
    [Tooltip("the switch button to change player's shooting style")]
    public Button ShootingSwitchButton;
    // the buttons to enable/disable corresponding weapons
    public Button RightWeaponButton;
    public Button LeftWeaponButton;
    public Button ForwardWeaponButton;
    public Button BackWeaponButton;

    private CharacterHandleShootingRange _handleShootingRangeAbility;
    private MultiWeaponHandle _multiWeaponHandle;


    protected override void Initialization()
    {
        if (_initialized)
        {
            base.Initialization();
            return;
        }

        Invoke(nameof(BindPlayerAbilities), 2f);

        base.Initialization();
    }

    private void BindPlayerAbilities()
    {
        var player = LevelManager.Instance.Players[0];
        if (player != default)
        {
            _handleShootingRangeAbility = player.FindAbility<CharacterHandleShootingRange>();
        }

        ShootingSwitchButton.gameObject.SetActive(true);


        if (_handleShootingRangeAbility != default)
        {
            _multiWeaponHandle = _handleShootingRangeAbility.gameObject.GetComponentInChildren<MultiWeaponHandle>();

            _isRightWeaponOn = _multiWeaponHandle.IsWeaponOn(Vector3.right);
            _isLeftWeaponOn = _multiWeaponHandle.IsWeaponOn(Vector3.left);
            _isForwardWeaponOn = _multiWeaponHandle.IsWeaponOn(Vector3.forward);
            _isBackWeaponOn = _multiWeaponHandle.IsWeaponOn(Vector3.back);
        }

        RightWeaponButton.gameObject.SetActive(true);
        LeftWeaponButton.gameObject.SetActive(true);
        ForwardWeaponButton.gameObject.SetActive(true);
        BackWeaponButton.gameObject.SetActive(true);
    }

    public void OnSwichShootingStyle()
    {
        if (_handleShootingRangeAbility == default)
        {
            MMDebug.LogDebugToConsole("Error: no ability of type CharacterHandleShootingRange found", "yello", 3, true);
            return;
        }

        _handleShootingRangeAbility.ToggleShootingStyle();
    }

    private bool _isRightWeaponOn = true;
    public void OnToggleRightWeapon()
    {
        _isRightWeaponOn = !_isRightWeaponOn;
        _multiWeaponHandle.ToggleStateWeapon(Vector3.right, _isRightWeaponOn);
    }

    private bool _isLeftWeaponOn = true;
    public void OnToggleLeftWeapon()
    {
        _isLeftWeaponOn = !_isLeftWeaponOn;
        _multiWeaponHandle.ToggleStateWeapon(Vector3.left, _isLeftWeaponOn);
    }

    private bool _isForwardWeaponOn = true;
    public void OnToggleForwardWeapon()
    {
        _isForwardWeaponOn = !_isForwardWeaponOn;
        _multiWeaponHandle.ToggleStateWeapon(Vector3.forward, _isForwardWeaponOn);
    }

    private bool _isBackWeaponOn = true;
    public void OnToggleBackWeapon()
    {
        _isBackWeaponOn = !_isBackWeaponOn;
        _multiWeaponHandle.ToggleStateWeapon(Vector3.back, _isBackWeaponOn);
    }
}
