using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CustomEvents;

public class ShootTriggerDetection : MMTriggerAndCollision, MMEventListener<MMGameEvent>
{
    private Character Player => (LevelManager.Instance != default && LevelManager.Instance.Players.Count > 0)
        ? LevelManager.Instance.Players[0]
        : default;

    public float DetectionAngleHalfArc => DetectionAngle / 2f;
    public float DetectionAngle = 30f;

    public List<CharacterHandleWeapon> ListHandleWeaponAbilities;

    private bool _isLockingAim = false;
    private bool _isAimShooting = false;
    private int _lockedEnemyId;

    private Coroutine _coroutineTrigger;
    private WaitForSeconds _wfsTrigger;

    private readonly float _delayLockingAim = .2f;


    private void Start()
    {
        _wfsTrigger = new WaitForSeconds(_delayLockingAim);

        Initialization();
    }

    private void OnEnable()
    {
        this.MMEventStartListening();
    }

    private void OnDisable()
    {
        this.MMEventStopListening();
    }

    private void Initialization()
    {
        var multiWeaponHandle = Player.AdditionalAbilityNodes[0].GetComponentInChildren<MultiWeaponHandle>();
        if (multiWeaponHandle == default)
        {
            if (ListHandleWeaponAbilities == default || ListHandleWeaponAbilities.Count <= 0)
            {
                ListHandleWeaponAbilities = new();
                ListHandleWeaponAbilities = Player.FindAbilities<CharacterHandleWeapon>();
            }
        }
        else
        {
            ListHandleWeaponAbilities = multiWeaponHandle.ListActivatedHandleWeaponAbilities;
        }
    }

    protected override void OnTriggerStay(Collider collider)
    {
        base.OnTriggerStay(collider);

        if (!TriggerLayerMask.MMContains(collider.gameObject))
        {
            return;
        }

        var enemyGameObject = collider.gameObject;
        if (enemyGameObject == default)
        {
            return;
        }

        bool isEnemyInSight = IsEnemyInSight(enemyGameObject, out var detectedHandle);
        if (isEnemyInSight)
        {
#if UNITY_EDITOR
            _debugLineColor = Color.red;
#endif
            if (_isAimShooting)
            {
                EnemyDetectEvent.Trigger(enemyGameObject.transform.position, detectedHandle);
            }
            else if (!_isLockingAim)
            {
                _lockedEnemyId = enemyGameObject.GetInstanceID();
                _coroutineTrigger = StartCoroutine(CoroutineTriggerDetectEvent(enemyGameObject.transform.position, false, detectedHandle));
            }
        }
        else
        {
#if UNITY_EDITOR
            _debugLineColor = Color.yellow;
#endif
            if ((_isAimShooting || _isLockingAim) && _lockedEnemyId == enemyGameObject.GetInstanceID())
            {
                CancelAimLocking();
            }
        }
    }

    protected override void OnTriggerExit(Collider collider)
    {
        base.OnTriggerExit(collider);

        if (!TriggerLayerMask.MMContains(collider.gameObject))
        {
            return;
        }

        if (_lockedEnemyId == collider.gameObject.GetInstanceID())
        {
            CancelAimLocking();
        }

#if UNITY_EDITOR
        _debugLockedEnemy = default;
        _debugDetectedHandle = default;
#endif
    }

    private bool IsEnemyInSight(GameObject enemyGameObject, out CharacterHandleWeapon detectedHandle)
    {
        for (int i = 0, count = ListHandleWeaponAbilities.Count; i < count; ++i)
        {
            var handleWeaponAbility = ListHandleWeaponAbilities[i];
            if (handleWeaponAbility == default)
            {
                continue;
            }

            bool isEnemyWithinRange = CheckEnemyInWeaponRange(enemyGameObject, handleWeaponAbility);
            if (isEnemyWithinRange)
            {
#if UNITY_EDITOR
                _debugLockedEnemy = enemyGameObject;
                _debugDetectedHandle = handleWeaponAbility;
#endif

                detectedHandle = handleWeaponAbility;
                return true;
            }
        }

#if UNITY_EDITOR
        _debugDetectedHandle = default;
#endif

        detectedHandle = default;
        return false;
    }

    private bool CheckEnemyInWeaponRange(GameObject enemyGameObject, CharacterHandleWeapon handleWeaponAbility)
    {
        var weaponPoint = handleWeaponAbility.CurrentWeapon.transform.position;
        var weaponAimDirection = handleWeaponAbility.CurrentWeapon.transform.forward;

        var enemyContactSameHeight = enemyGameObject.transform.position;
        enemyContactSameHeight.y = weaponPoint.y;

        var distanceDirectionToEnemy = (enemyContactSameHeight - weaponPoint).normalized;
        var angle = Vector3.Angle(distanceDirectionToEnemy, weaponAimDirection);

        return (angle <= DetectionAngleHalfArc && angle >= 0);
    }

    private IEnumerator CoroutineTriggerDetectEvent(Vector3 enemyPosition, bool isFromSecondaryWeapon, CharacterHandleWeapon detectedHandle)
    {
        _isLockingAim = true;
        _isAimShooting = false;

        //Debug.LogError("Start locking aim...");
        yield return _wfsTrigger;
        //Debug.LogError("Finish locking aim");

        EnemyDetectEvent.Trigger(enemyPosition, detectedHandle);

        _isAimShooting = true;
        _isLockingAim = false;
    }

    private void CancelAimLocking()
    {
        if (_isLockingAim || _isAimShooting)
        {
            if (_coroutineTrigger != default)
            {
                StopCoroutine(_coroutineTrigger);
            }

            _isLockingAim = false;
            _isAimShooting = false;
            _lockedEnemyId = 0;

            //Debug.LogError("Cancel locking aim and/or shooting");
        }
    }

    public void OnMMEvent(MMGameEvent eventType)
    {
        if (eventType.EventName.Equals("EnemyDeath", System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log("Enemy die");
            CancelAimLocking();
        }
        //else if (eventType.EventName.Equals("UpdateActivatedWeapons", System.StringComparison.OrdinalIgnoreCase))
        //{
        //    Initialization();
        //}
    }



#if UNITY_EDITOR
    private Color _debugLineColor = Color.yellow;
    private GameObject _debugLockedEnemy;
    private CharacterHandleWeapon _debugDetectedHandle;

    private void OnDrawGizmos()
    {
        if (Player == default)
        {
            return;
        }

        var handleShootingRange = Player.FindAbility<CharacterHandleShootingRange>();
        if (handleShootingRange == default || handleShootingRange.ShootingStyle != ShootingStyle.LockedAimInRange)
        {
            return;
        }

        for (int i = 0, count = ListHandleWeaponAbilities.Count; i < count; ++i)
        {
            var handleWeaponAbility = ListHandleWeaponAbilities[i];
            if (handleWeaponAbility == default || handleWeaponAbility.CurrentWeapon == default)
            {
                continue;
            }

            var weapon = handleWeaponAbility.CurrentWeapon;
            var weaponContact = weapon.transform.position;
            var weaponDirection = weapon.transform.forward;

            Gizmos.color = _debugLineColor;
            Gizmos.DrawLine(weaponContact, weaponContact + 10 * weaponDirection);

            var limitDirection = Quaternion.AngleAxis(DetectionAngleHalfArc, weapon.transform.up) * weaponDirection;
            Gizmos.DrawLine(weaponContact, weaponContact + 10 * limitDirection);

            limitDirection = Quaternion.AngleAxis(-DetectionAngleHalfArc, weapon.transform.up) * weaponDirection;
            Gizmos.DrawLine(weaponContact, weaponContact + 10 * limitDirection);

            if (_debugLockedEnemy != default && _debugDetectedHandle == handleWeaponAbility)
            {
                var weaponPoint = weapon.transform.position;

                var enemyContactSameHeight = _debugLockedEnemy.transform.position;
                enemyContactSameHeight.y = weaponPoint.y;

                Gizmos.color = Color.blue;
                Gizmos.DrawLine(weaponPoint, enemyContactSameHeight);
            }
        }
    }
#endif // UNITY_EDITOR
}
