using MoreMountains.Tools;
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
        public bool IsFromSecondaryWeapon;

        public EnemyDetectEvent(Vector3 position, bool isFromSecondaryWeapon)
        {
            EnemyPosition = position;
            IsFromSecondaryWeapon = isFromSecondaryWeapon;
        }

        static EnemyDetectEvent e;

        public static void Trigger(Vector3 position, bool isFromSecondaryWeapon)
        {
            e.EnemyPosition = position;
            e.IsFromSecondaryWeapon = isFromSecondaryWeapon;

            MMEventManager.TriggerEvent(e);
        }
    }
}
