using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterHandleSecondaryWeaponAlwaysShoot : CharacterHandleSecondaryWeapon
{
    protected override void HandleInput()
    {
        base.HandleInput();

        if (ForceAlwaysShoot)
        {
            ShootStart();
        }
    }
}
