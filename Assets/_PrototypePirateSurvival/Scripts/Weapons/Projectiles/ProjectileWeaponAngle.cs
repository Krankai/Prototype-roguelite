using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
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

        // TODO: double-check if use x and y value (not correct yet!)
        //Vector3 angleSpread = Vector3.zero;
        //var dotProduct = Vector3.Dot(_originalFaceDirection, transform.forward);
        //if (dotProduct >= 0)
        //{
        //    // same with original direction
        //    angleSpread.z = UnityEngine.Random.Range(FromShootAngle.z, ToShootAngle.z);
        //    angleSpread.x = UnityEngine.Random.Range(FromShootAngle.x, ToShootAngle.x);
        //    angleSpread.y = UnityEngine.Random.Range(FromShootAngle.y, ToShootAngle.y);
        //}
        //else
        //{
        //    // opposite to original direction -> flip the angle
        //    angleSpread.z = -UnityEngine.Random.Range(FromShootAngle.z, ToShootAngle.z);
        //    angleSpread.x = -UnityEngine.Random.Range(FromShootAngle.x, ToShootAngle.x);
        //    angleSpread.y = -UnityEngine.Random.Range(FromShootAngle.y, ToShootAngle.y);
        //}

        //var spread = Quaternion.Euler(angleSpread);
        //projectile.SetDirection(spread * transform.forward, transform.rotation, true);
        //Debug.Log($"Direction: {spread * transform.forward}");

        var angleX = UnityEngine.Random.Range(FromShootAngle.x, ToShootAngle.x);
        var rotateX = Quaternion.AngleAxis(angleX, transform.right);

        var angleY = UnityEngine.Random.Range(FromShootAngle.y, ToShootAngle.y);
        var rotateY = Quaternion.AngleAxis(angleY, transform.up);

        var angleZ = UnityEngine.Random.Range(FromShootAngle.z, ToShootAngle.z);
        var rotateZ = Quaternion.AngleAxis(angleZ, transform.forward);

        var direction = rotateX * rotateY * rotateZ * transform.forward;
        projectile.SetDirection(direction, transform.rotation, true);
        //Debug.Log($"Direction: {direction}");

        return projectileGameObject;
    }
}
