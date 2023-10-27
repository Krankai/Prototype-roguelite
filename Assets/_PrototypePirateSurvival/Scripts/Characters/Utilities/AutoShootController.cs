using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoShootController : MonoBehaviour, MMEventListener<MMGameEvent>
{
    [SerializeField, MMReadOnly]
    private bool _needRecheckEnemies = false;


    private void OnEnable()
    {
        this.MMEventStartListening<MMGameEvent>();
    }

    private void OnDisable()
    {
        this.MMEventStopListening<MMGameEvent>();
    }

    public void EnablePlayerAutoShoot()
    {
        return;

        var mainPlayer = LevelManager.Instance.Players[0];
        if (mainPlayer != default)
        {
            var handleWeapon = mainPlayer.FindAbility<CharacterHandleWeapon>();
            var handleSecondaryWeapon = mainPlayer.FindAbility<CharacterHandleSecondaryWeapon>();

            handleWeapon.ForceAlwaysShoot = true;
            handleSecondaryWeapon.ForceAlwaysShoot = true;
        }

        _needRecheckEnemies = false;
    }

    public void DisablePlayerAutoShoot()
    {
        return;

        var mainPlayer = LevelManager.Instance.Players[0];
        if (mainPlayer != default)
        {
            var handleWeapon = mainPlayer.FindAbility<CharacterHandleWeapon>();
            var handleSecondaryWeapon = mainPlayer.FindAbility<CharacterHandleSecondaryWeapon>();

            handleWeapon.ForceAlwaysShoot = false;
            handleSecondaryWeapon.ForceAlwaysShoot = false;
        }
    }

    public void MonitorPlayerAutooShoot()
    {
        return;

        if (_needRecheckEnemies)
        {
            EnablePlayerAutoShoot();
            _needRecheckEnemies = true;

            return;
        }
    }

    public void OnMMEvent(MMGameEvent eventType)
    {
        if (eventType.EventName.Equals("EnemyDeath", System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log("Enemy die");
            DisablePlayerAutoShoot();
            _needRecheckEnemies = true;
        }
    }
}
