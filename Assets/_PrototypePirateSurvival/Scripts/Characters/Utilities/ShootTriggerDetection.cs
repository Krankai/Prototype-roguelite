using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootTriggerDetection : MMTriggerAndCollision
{
    private Character Player => LevelManager.Instance.Players[0];

    public float DetectionAngleHalfArc = 30f;

    public CharacterHandleWeapon HandleWeaponAbility;
    public CharacterHandleSecondaryWeapon HandleSecondaryWeaponAbility;


    private void Start()
    {
        if (HandleWeaponAbility == default)
        {
            HandleWeaponAbility = Player.FindAbility<CharacterHandleWeapon>();
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

        var direction = (enemyGameObject.transform.position - Player.transform.position).normalized;
        var weaponAimDirection = HandleWeaponAbility.CurrentWeapon.transform.forward;

        var angle = Vector3.Angle(direction, weaponAimDirection);
        if (angle <= DetectionAngleHalfArc && angle >= 0)
        {
            EnemyDetectEvent.Trigger(enemyGameObject.transform.position);
        }
    }
}

public struct EnemyDetectEvent
{
    public Vector3 EnemyPosition;

    public EnemyDetectEvent(Vector3 position)
    {
        EnemyPosition = position;
    }

    static EnemyDetectEvent e;

    public static void Trigger(Vector3 position)
    {
        e.EnemyPosition = position;
        MMEventManager.TriggerEvent(e);
    }
}
