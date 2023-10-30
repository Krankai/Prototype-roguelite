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


    private void Start()
    {
        if (HandleWeaponAbility == default)
        {
            HandleWeaponAbility = Player.FindAbility<CharacterHandleWeapon>();
        }

        if (HandleSecondaryWeaponAbility == default)
        {
            HandleSecondaryWeaponAbility = Player.FindAbility<CharacterHandleSecondaryWeapon>();
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
            EnemyDetectEvent.Trigger(enemyGameObject.transform.position, isFromSecondaryWeapon);
        }
        else
        {
#if UNITY_EDITOR
            _debugLineColor = Color.yellow;
#endif
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

    private void EnableAutoShoot()
    {
        HandleWeaponAbility.ShootStart();
        HandleSecondaryWeaponAbility.ShootStart();
    }

    public void OnMMEvent(MMGameEvent eventType)
    {
        if (eventType.EventName.Equals("EnemyDeath", System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log("Enemy die");
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
