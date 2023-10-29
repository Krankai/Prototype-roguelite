using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
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

    [Header("Sub Projectile")]
    public List<RangeShootAngle> ListShootAngles;


    public virtual void AdjustProjectileFormation(float originFromAngle, float originToAngle, float arcHeightAngle, float shootingRangeAngle)
    {
        // Main projectil
        var angle = FromShootAngle;
        angle.x = arcHeightAngle;
        angle.y = originFromAngle;
        FromShootAngle = angle;

        angle = ToShootAngle;
        angle.x = arcHeightAngle;
        angle.y = originToAngle;
        ToShootAngle = angle;


        // Sub-projectiles
        if (ListShootAngles.Count <= 0)
        {
            return;
        }

        for (int i = 0, count = ListShootAngles.Count; i < count; ++i)
        {
            var angleRangeData = ListShootAngles[i];

            if (i % ProjectilesPerShot == 0)
            {
                angle = angleRangeData.FromShootAngle;
                angle.x = arcHeightAngle;
                angle.y = originFromAngle - shootingRangeAngle / 2f;
                angleRangeData.FromShootAngle = angle;

                angle = angleRangeData.ToShootAngle;
                angle.x = arcHeightAngle;
                angle.y = originToAngle - shootingRangeAngle / 2f;
                angleRangeData.ToShootAngle = angle;
            }
            else if (i % ProjectilesPerShot == 1)
            {
                angle = angleRangeData.FromShootAngle;
                angle.x = arcHeightAngle;
                angle.y = originFromAngle + shootingRangeAngle / 2f;
                angleRangeData.FromShootAngle = angle;

                angle = angleRangeData.ToShootAngle;
                angle.x = arcHeightAngle;
                angle.y = originToAngle + shootingRangeAngle / 2f;
                angleRangeData.ToShootAngle = angle;
            }

            ListShootAngles[i] = angleRangeData;
        }
    }

    public override GameObject SpawnProjectile(Vector3 spawnPosition, int projectileIndex, int totalProjectiles, bool triggerObjectActivation = true)
    {
        var projectileGameObject = base.SpawnProjectile(spawnPosition, projectileIndex, totalProjectiles, triggerObjectActivation);

        var projectile = projectileGameObject.MMGetComponentNoAlloc<Projectile>();
        if (projectile == default)
        {
            return projectileGameObject;
        }

        if (projectileIndex == 0)
        {
            var angleX = UnityEngine.Random.Range(FromShootAngle.x, ToShootAngle.x);
            var rotateX = Quaternion.AngleAxis(angleX, transform.right);

            var angleY = UnityEngine.Random.Range(FromShootAngle.y, ToShootAngle.y);
            var rotateY = Quaternion.AngleAxis(angleY, transform.up);

            var angleZ = UnityEngine.Random.Range(FromShootAngle.z, ToShootAngle.z);
            var rotateZ = Quaternion.AngleAxis(angleZ, transform.forward);

            var direction = rotateX * rotateY * rotateZ * transform.forward;
            projectile.SetDirection(direction, transform.rotation, true);
        }
        else if (ListShootAngles.Count > 0)
        {
            CheckSettingAngleForProjectileDirection(projectile, projectileIndex, totalProjectiles);
        }

        return projectileGameObject;
    }

    public void CheckSettingAngleForProjectileDirection(Projectile projectile, int projectileIndex, int totalProjectiles)
    {
        var matchedIndexAngle = projectileIndex % (totalProjectiles - 1);
        var shootAngleRange = (matchedIndexAngle < ListShootAngles.Count)
            ? ListShootAngles[matchedIndexAngle]
            : new RangeShootAngle() { FromShootAngle = Vector3.zero, ToShootAngle = Vector3.zero };


        var fromShootAngle = shootAngleRange.FromShootAngle;
        var toShootAngle = shootAngleRange.ToShootAngle;

        var angleX = UnityEngine.Random.Range(fromShootAngle.x, toShootAngle.x);
        var rotateX = Quaternion.AngleAxis(angleX, transform.right);

        var angleY = UnityEngine.Random.Range(fromShootAngle.y, toShootAngle.y);
        var rotateY = Quaternion.AngleAxis(angleY, transform.up);

        var angleZ = UnityEngine.Random.Range(fromShootAngle.z, toShootAngle.z);
        var rotateZ = Quaternion.AngleAxis(angleZ, transform.forward);

        var direction = rotateX * rotateY * rotateZ * transform.forward;
        projectile.SetDirection(direction, transform.rotation, true);
    }

    //private void SetAngleForProjectileDirection(GameObject projectileGameObject, Vector3 fromShootAngle, Vector3 toShootAngle)
    //{
    //    var projectile = projectileGameObject.MMGetComponentNoAlloc<Projectile>();
    //    if (projectile == default)
    //    {
    //        return;
    //    }

    //    var angleX = UnityEngine.Random.Range(fromShootAngle.x, toShootAngle.x);
    //    var rotateX = Quaternion.AngleAxis(angleX, transform.right);

    //    var angleY = UnityEngine.Random.Range(fromShootAngle.y, toShootAngle.y);
    //    var rotateY = Quaternion.AngleAxis(angleY, transform.up);

    //    var angleZ = UnityEngine.Random.Range(fromShootAngle.z, toShootAngle.z);
    //    var rotateZ = Quaternion.AngleAxis(angleZ, transform.forward);

    //    var direction = rotateX * rotateY * rotateZ * transform.forward;
    //    projectile.SetDirection(direction, transform.rotation, true);
    //}

    //public override void WeaponUse()
    //{
    //    base.WeaponUse();

    //    // For now, only 1 instance of sub-projectiles is fired
    //    if (ListShootAngles.Count <= 0)
    //    {
    //        return;
    //    }

    //    int projectileIndex = ProjectilesPerShot;
    //    for (int i = 0, count = ListShootAngles.Count; i < count; ++i)
    //    {
    //        var rangeShootAngle = ListShootAngles[i];
    //        var projectileGameObject = SpawnProjectile(SpawnPosition, projectileIndex++, 1, true);

    //        SetAngleForProjectileDirection(projectileGameObject, rangeShootAngle.FromShootAngle, rangeShootAngle.ToShootAngle);
    //    }
    //}
}

[System.Serializable]
public struct RangeShootAngle
{
    public Vector3 FromShootAngle;
    public Vector3 ToShootAngle;
}
