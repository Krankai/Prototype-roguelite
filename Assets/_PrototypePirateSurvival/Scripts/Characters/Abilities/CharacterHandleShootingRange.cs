using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;

[MMHiddenProperties("AbilityStartFeedbacks", "AbilityStopFeedbacks")]
[RequireComponent(typeof(CharacterHandleWeapon))]
public class CharacterHandleShootingRange : CharacterAbility
{
    [Header("Ability")]
    // handle weapon ability
    [Tooltip("handle weapon ability")]
    public CharacterHandleWeapon HandleWeaponAbility;

    [Header("Feedback")]
    public MMF_Player ShootingRangeFeedbacks;

    [Header("Output")]
    // shooting range
    [MMReadOnly]
    [Tooltip("shooting range")]
    public float ShootingRange;



    [Header("Debug")]
    [SerializeField, MMReadOnly]
    private float _initialVelocity;
    [SerializeField, MMReadOnly]
    private float _angle;
    [SerializeField, MMReadOnly]
    private float _gravityModifier;

    protected override void Initialization()
    {
        base.Initialization();

        if (HandleWeaponAbility == default)
        {
            HandleWeaponAbility = gameObject.MMGetComponentNoAlloc<CharacterHandleWeapon>();
        }

        Invoke(nameof(ComputeShootingRange), 1f);
    }

    public void ComputeShootingRange()
    {
        // Rough estimate: since start at initial height h0, the actual distance will be longer
        // x = (v0^2 * sin(2a)) / g

        if (HandleWeaponAbility == default)
        {
            return;
        }

        ProjectileWeapon weapon = HandleWeaponAbility.CurrentWeapon as ProjectileWeaponAngle;
        if (weapon == default)
        {
            weapon = HandleWeaponAbility.CurrentWeapon as ProjectileWeapon;
            if (weapon == default)
            {
                return;
            }
        }

        var projectileGameObject = weapon.ObjectPooler.GetPooledGameObject();
        if (projectileGameObject == default)
        {
            return;
        }

        var physicsProjectile = projectileGameObject.MMGetComponentNoAlloc<PhysicsProjectile>();
        var rigidBodyProjectile = projectileGameObject.MMGetComponentNoAlloc<Rigidbody>();


        // === Computation - Rough estimation only ===
        var initialVelocity = (physicsProjectile.InitialForce / rigidBodyProjectile.mass) * Time.fixedDeltaTime;
        _initialVelocity = initialVelocity;

        float angle = 0;
        if (weapon is ProjectileWeaponAngle)
        {
            var weaponAngle = weapon as ProjectileWeaponAngle;
            angle = Mathf.Max(Mathf.Abs(weaponAngle.FromShootAngle.x), Mathf.Abs(weaponAngle.ToShootAngle.x));
        }
        _angle = angle;

        var gravityModifier = Physics.gravity.magnitude;
        _gravityModifier = gravityModifier;

        weapon.DetermineSpawnPosition();


        var maxDistanceProjectileMotion = initialVelocity * initialVelocity * Mathf.Sin(2 * angle * Mathf.Deg2Rad) / gravityModifier;
        var distanceTillHitGround = weapon.SpawnPosition.y / Mathf.Tan(angle * Mathf.Deg2Rad);

        ShootingRange = weapon.SpawnPosition.x + maxDistanceProjectileMotion + distanceTillHitGround;

        TriggerFeedbacks();
    }

    private void TriggerFeedbacks()
    {
        var scaleFeedback = ShootingRangeFeedbacks.GetFeedbackOfType<MMF_Scale>();
        var scale = scaleFeedback.DestinationScale;

        scale.x = ShootingRange * 2;
        scale.y = ShootingRange * 2;
        scaleFeedback.DestinationScale = scale;

        ShootingRangeFeedbacks.PlayFeedbacks();
    }
}
