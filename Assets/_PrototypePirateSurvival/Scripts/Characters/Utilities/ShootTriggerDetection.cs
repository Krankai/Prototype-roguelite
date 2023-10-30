using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CustomEvents;

public class ShootTriggerDetection : MMTriggerAndCollision, MMEventListener<MMGameEvent>
{
    private Character Player => LevelManager.Instance.Players[0];

    public float DetectionAngleHalfArc => DetectionAngle / 2f;
    public float DetectionAngle = 30f;

    public CharacterHandleWeapon HandleWeaponAbility;
    public CharacterHandleSecondaryWeapon HandleSecondaryWeaponAbility;

    private bool _isLockingAim = false;
    private bool _isAimShooting = false;
    private int _lockedEnemyId;

    private Coroutine _coroutineTrigger;
    private WaitForSeconds _wfsTrigger;

    private float _delayLockingAim = 1f;


    private void Start()
    {
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
        if (HandleWeaponAbility == default)
        {
            HandleWeaponAbility = Player.FindAbility<CharacterHandleWeapon>();
        }

        if (HandleSecondaryWeaponAbility == default)
        {
            HandleSecondaryWeaponAbility = Player.FindAbility<CharacterHandleSecondaryWeapon>();
        }

        _wfsTrigger = new WaitForSeconds(_delayLockingAim);
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

        bool isFromSecondaryWeapon = false;
        bool isWeaponSightWithinRange = CheckWeaponSightWithinRange(enemyGameObject, HandleWeaponAbility);

        if (!isWeaponSightWithinRange)
        {
            isWeaponSightWithinRange = CheckWeaponSightWithinRange(enemyGameObject, HandleSecondaryWeaponAbility);
            isFromSecondaryWeapon = isWeaponSightWithinRange;
        }

        if (isWeaponSightWithinRange)
        {
#if UNITY_EDITOR
            _debugLineColor = Color.red;
#endif
            if (_isAimShooting)
            {
                EnemyDetectEvent.Trigger(enemyGameObject.transform.position, isFromSecondaryWeapon);
            }
            else if (!_isLockingAim)
            {
                _lockedEnemyId = enemyGameObject.GetInstanceID();
                _coroutineTrigger = StartCoroutine(CoroutineTriggerDetectEvent(enemyGameObject.transform.position, isFromSecondaryWeapon));
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
    }

    private bool CheckWeaponSightWithinRange(GameObject enemyGameObject, CharacterHandleWeapon handleWeaponAbility)
    {
        var weaponPoint = handleWeaponAbility.CurrentWeapon.transform.position;
        var weaponAimDirection = handleWeaponAbility.CurrentWeapon.transform.forward;

        var enemyContactSameHeight = enemyGameObject.transform.position;
        enemyContactSameHeight.y = weaponPoint.y;

        var distanceDirectionToEnemy = (enemyContactSameHeight - weaponPoint).normalized;
        var angle = Vector3.Angle(distanceDirectionToEnemy, weaponAimDirection);

        return (angle <= DetectionAngleHalfArc && angle >= 0);
    }

    private IEnumerator CoroutineTriggerDetectEvent(Vector3 enemyPosition, bool isFromSecondaryWeapon)
    {
        _isLockingAim = true;
        _isAimShooting = false;

        Debug.LogError("Start locking aim...");
        yield return _wfsTrigger;
        Debug.LogError("Finish locking aim");

        EnemyDetectEvent.Trigger(enemyPosition, isFromSecondaryWeapon);

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

            Debug.LogError("Cancel locking aim and/or shooting");
        }
    }

    public void OnMMEvent(MMGameEvent eventType)
    {
        if (eventType.EventName.Equals("EnemyDeath", System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log("Enemy die");

            CancelAimLocking();
        }
    }



#if UNITY_EDITOR
    private Color _debugLineColor = Color.yellow;

    private void OnDrawGizmos()
    {
        if (HandleWeaponAbility != default && HandleWeaponAbility.CurrentWeapon != default)
        {
            var weaponContact = HandleWeaponAbility.CurrentWeapon.transform.position;
            var weaponDirection = HandleWeaponAbility.CurrentWeapon.transform.forward;

            Gizmos.color = _debugLineColor;
            Gizmos.DrawLine(weaponContact, weaponContact + 10 * weaponDirection);

            var limitDirection = Quaternion.AngleAxis(DetectionAngleHalfArc, HandleWeaponAbility.CurrentWeapon.transform.up) * weaponDirection;
            Gizmos.DrawLine(weaponContact, weaponContact + 10 * limitDirection);

            limitDirection = Quaternion.AngleAxis(-DetectionAngleHalfArc, HandleWeaponAbility.CurrentWeapon.transform.up) * weaponDirection;
            Gizmos.DrawLine(weaponContact, weaponContact + 10 * limitDirection);


            var enemyGameObject = GameObject.Find("NPC");
            if (enemyGameObject != default)
            {
                var weaponPoint = HandleWeaponAbility.CurrentWeapon.transform.position;

                var enemyContactSameHeight = enemyGameObject.transform.position;
                enemyContactSameHeight.y = weaponPoint.y;

                var distanceDirectionToEnemy = (enemyContactSameHeight - weaponPoint).normalized;

                Gizmos.color = Color.blue;
                Gizmos.DrawLine(weaponPoint, weaponPoint + 10 * distanceDirectionToEnemy.normalized);
            }
        }

        if (HandleSecondaryWeaponAbility != default && HandleSecondaryWeaponAbility.CurrentWeapon != default)
        {
            var weaponContact = HandleSecondaryWeaponAbility.CurrentWeapon.transform.position;
            var weaponDirection = HandleSecondaryWeaponAbility.CurrentWeapon.transform.forward;

            Gizmos.color = _debugLineColor;
            Gizmos.DrawLine(weaponContact, weaponContact + 10 * weaponDirection);

            var limitDirection = Quaternion.AngleAxis(DetectionAngleHalfArc, HandleSecondaryWeaponAbility.CurrentWeapon.transform.up) * weaponDirection;
            Gizmos.DrawLine(weaponContact, weaponContact + 10 * limitDirection);

            limitDirection = Quaternion.AngleAxis(-DetectionAngleHalfArc, HandleSecondaryWeaponAbility.CurrentWeapon.transform.up) * weaponDirection;
            Gizmos.DrawLine(weaponContact, weaponContact + 10 * limitDirection);


            var enemyGameObject = GameObject.Find("NPC");
            if (enemyGameObject != default)
            {
                var weaponPoint = HandleSecondaryWeaponAbility.CurrentWeapon.transform.position;

                var enemyContactSameHeight = enemyGameObject.transform.position;
                enemyContactSameHeight.y = weaponPoint.y;

                var distanceDirectionToEnemy = (enemyContactSameHeight - weaponPoint).normalized;

                Gizmos.color = Color.blue;
                Gizmos.DrawLine(weaponPoint, weaponPoint + 10 * distanceDirectionToEnemy.normalized);
            }
        }
    }
#endif // UNITY_EDITOR
}
