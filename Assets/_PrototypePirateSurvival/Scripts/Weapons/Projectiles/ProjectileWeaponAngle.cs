using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileWeaponAngle : ProjectileWeapon
{
    [MMInspectorGroup("Projectiles", true, 22)]
    [Header("Shoot Angle")]
    // the range of shooting angle (in degrees) to apply for each projectile spawn
    [Tooltip("the range of shooting angle (in degrees) to apply for each projectile spawn")]
    public Vector3 FromShootAngle = Vector3.zero;
    public Vector3 ToShootAngle = Vector3.zero;

    private Vector3 _originalFaceDirection;

    protected override void Start()
    {
        base.Start();

        _originalFaceDirection = transform.forward;
    }

    public override GameObject SpawnProjectile(Vector3 spawnPosition, int projectileIndex, int totalProjectiles, bool triggerObjectActivation = true)
    {
        var projectileGameObject = base.SpawnProjectile(spawnPosition, projectileIndex, totalProjectiles, triggerObjectActivation);

        var projectile = projectileGameObject.MMGetComponentNoAlloc<Projectile>();
        if (projectile == default)
        {
            return projectileGameObject;
        }

        Vector3 angleSpread = Vector3.zero;

        // TODO: double-check if use y value (not tested yet!)
        var dotProduct = Vector3.Dot(_originalFaceDirection, transform.forward);
        if (dotProduct >= 0)
        {
            // same with original direction
            angleSpread.z = UnityEngine.Random.Range(FromShootAngle.z, ToShootAngle.z);
            angleSpread.x = UnityEngine.Random.Range(FromShootAngle.x, ToShootAngle.x);
            angleSpread.y = UnityEngine.Random.Range(FromShootAngle.y, ToShootAngle.y);
        }
        else
        {
            // opposite to original direction -> flip the angle
            angleSpread.z = -UnityEngine.Random.Range(FromShootAngle.z, ToShootAngle.z);
            angleSpread.x = -UnityEngine.Random.Range(FromShootAngle.x, ToShootAngle.x);
            angleSpread.y = -UnityEngine.Random.Range(FromShootAngle.y, ToShootAngle.y);
        }

        var spread = Quaternion.Euler(angleSpread);
        projectile.SetDirection(spread * transform.forward, transform.rotation, true);

        return projectileGameObject;
    }
}
