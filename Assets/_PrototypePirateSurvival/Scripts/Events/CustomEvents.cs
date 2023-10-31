using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomEvents : MonoBehaviour
{
    // TODO: check more before use
    public struct EnemyDeathEvent
    {
        public string EventName;

        public EnemyDeathEvent(string newName)
        {
            EventName = newName;
        }

        static EnemyDeathEvent e;

        public static void Trigger(string newName)
        {
            e.EventName = newName;
            MMEventManager.TriggerEvent(e);
        }
    }

    public struct EnemyDetectEvent
    {
        public Vector3 EnemyPosition;
        public CharacterHandleWeapon HandleWeaponAbility;

        public EnemyDetectEvent(Vector3 position, CharacterHandleWeapon ability)
        {
            EnemyPosition = position;
            HandleWeaponAbility = ability;
        }

        static EnemyDetectEvent e;

        public static void Trigger(Vector3 position, CharacterHandleWeapon ability)
        {
            e.EnemyPosition = position;
            e.HandleWeaponAbility = ability;

            MMEventManager.TriggerEvent(e);
        }
    }
}
