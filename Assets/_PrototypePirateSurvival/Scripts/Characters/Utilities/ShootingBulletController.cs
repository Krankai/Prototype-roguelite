using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingBulletController : MonoBehaviour
{
    public List<int> ListBulletCounts = new() { 3, 5, 7, 9 };
    private int _currentIndex = 0;


    public void AdvanceBulletCount()
    {
        _currentIndex = (_currentIndex + 1) % ListBulletCounts.Count;
        var bulletCount = ListBulletCounts[_currentIndex];

        var listAbilities = LevelManager.Instance.Players[0].FindAbilities<CharacterHandleWeapon>();
        for (int i = 0, count = listAbilities.Count; i < count; ++i)
        {
            var weapon = listAbilities[i].CurrentWeapon as ProjectileWeapon;
            if (weapon != default)
            {
                weapon.ProjectilesPerShot = bulletCount;
            }
        }

        MMGameEvent.Trigger("UpdateActivatedWeapons");
    }
}
