using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;
using static CustomEvents;

[MMHiddenProperties("AbilityStartFeedbacks", "AbilityStopFeedbacks")]
[RequireComponent(typeof(CharacterHandleWeapon))]
public class CharacterHandleShootingRange : CharacterAbility, MMEventListener<EnemyDetectEvent>
{
    [Header("Ability")]
    // handle weapon ability
    [Tooltip("handle weapon ability")]
    public CharacterHandleWeapon HandleWeaponAbility;
    // handle secondary weapon ability
    [Tooltip("handle secondary weapon ability")]
    public CharacterHandleSecondaryWeapon HandleSecondaryWeaponAbility;

    [Header("Feedback")]
    public MMF_Player ShootingRangeFeedbacks;

    [Header("Settings")]
    // shooting range radius
    [Tooltip("shooting range radius")]
    public float ShootingRangeRadius;
    // angle for shooting range arc
    [Tooltip("angle for shooting range arc")]
    public float ShootingRangeAngle = 60f;

    [Header("Output")]
    // required initial force
    [Tooltip("required initial force"), MMReadOnly]
    public float InitialForce;
    // required angle to lineup projectile
    [Tooltip("required angle to lineup projectile"), MMReadOnly]
    public float ProjectileAngleY;

    [Header("Debug")]
    [SerializeField, MMReadOnly]
    private float _initialVelocity;
    [SerializeField, MMReadOnly]
    private float _angle;
    [SerializeField, MMReadOnly]
    private float _gravityModifier;

    private ProjectileWeapon _weapon;
    private PhysicsProjectile _projectilePhysics;
    private Rigidbody _projectileRigidBody;


    protected override void Initialization()
    {
        base.Initialization();

        if (HandleWeaponAbility == default)
        {
            HandleWeaponAbility = _character.FindAbility<CharacterHandleWeapon>();
        }

        if (HandleSecondaryWeaponAbility == default)
        {
            HandleSecondaryWeaponAbility = _character.FindAbility<CharacterHandleSecondaryWeapon>();
        }

        TriggerFeedbacks();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        this.MMEventStartListening();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        this.MMEventStopListening();
    }

    private void ComputeRequiredInitialForce(float range)
    {
        // v = sqrt(x * g / sin2a) -> F = v * mass / Time.fixedDeltaTime

        var gravityModifier = Physics.gravity.magnitude;
        _gravityModifier = gravityModifier;

        float angle = 0;
        if (_weapon is ProjectileWeaponAngle)
        {
            var weaponAngle = _weapon as ProjectileWeaponAngle;
            angle = Mathf.Max(Mathf.Abs(weaponAngle.FromShootAngle.x), Mathf.Abs(weaponAngle.ToShootAngle.x));
        }
        _angle = angle;

        _weapon.DetermineSpawnPosition();

        var distanceTillHitGround = _weapon.SpawnPosition.y / Mathf.Tan(angle * Mathf.Deg2Rad);
        var relativeWeaponPositionX = _weapon.SpawnPosition.x - _character.transform.position.x;
        var maxDistanceProjectile = range - distanceTillHitGround - relativeWeaponPositionX;
        maxDistanceProjectile += 0.5f;

        _initialVelocity = Mathf.Sqrt(maxDistanceProjectile * _gravityModifier / Mathf.Sin(2 * _angle * Mathf.Deg2Rad));
        InitialForce = _initialVelocity * _projectileRigidBody.mass / Time.fixedDeltaTime;
    }

    private void SetForceToGetToShootingRange()
    {
        var dynamicForceObjectPooler = _weapon.gameObject.MMGetComponentNoAlloc<DynamicForcePhysicObjectPooler>();
        if (dynamicForceObjectPooler != default)
        {
            dynamicForceObjectPooler.AppliedInitialForce = InitialForce;
        }
    }

    private void ComputeRequiredAngleForProjectile(Vector3 enemyPosition)
    {
        var weaponPoint = _weapon.transform.position;
        var weaponAimDirection = _weapon.transform.forward;

        enemyPosition.y = weaponPoint.y;

        var distanceDirectionToEnemy = (enemyPosition - weaponPoint).normalized;
        ProjectileAngleY = Vector3.SignedAngle(weaponAimDirection, distanceDirectionToEnemy, _weapon.transform.up);
    }

    private void SetAngleToLineUpProjectile()
    {
        if (_weapon is not ProjectileWeaponAngle)
        {
            return;
        }

        var weaponAngle = _weapon as ProjectileWeaponAngle;

        var fromAngle = weaponAngle.FromShootAngle;
        fromAngle.y = ProjectileAngleY;
        weaponAngle.FromShootAngle = fromAngle;

        var toAngle = weaponAngle.ToShootAngle;
        toAngle.y = ProjectileAngleY;
        weaponAngle.ToShootAngle = toAngle;
    }

    private void TriggerFeedbacks()
    {
        var listScaleFeedbacks = ShootingRangeFeedbacks.GetFeedbacksOfType<MMF_Scale>();
        for (int i = 0, count = listScaleFeedbacks.Count; i < count; ++i)
        {
            var scaleFeedback = listScaleFeedbacks[i];

            var scale = scaleFeedback.DestinationScale;
            scale.x = ShootingRangeRadius * 2;
            scale.y = ShootingRangeRadius * 2;

            scaleFeedback.DestinationScale = scale;
        }

        var listFloatControllerFeedback = ShootingRangeFeedbacks.GetFeedbacksOfType<MMF_FloatController>();
        for (int i = 0, count = listFloatControllerFeedback.Count; i < count; ++i)
        {
            var floatControllerFeedback = listFloatControllerFeedback[i];

            if (floatControllerFeedback.Label.Equals("RadiusController", System.StringComparison.OrdinalIgnoreCase))
            {
                floatControllerFeedback.ToDestinationValue = ShootingRangeRadius;
            }
            else if (floatControllerFeedback.Label.Equals("AngleController", System.StringComparison.OrdinalIgnoreCase))
            {
                floatControllerFeedback.ToDestinationValue = ShootingRangeAngle;
            }
        }

        ShootingRangeFeedbacks.PlayFeedbacks();
    }

    public void OnMMEvent(EnemyDetectEvent eventType)
    {
        RefreshTargetWeapon(eventType.IsFromSecondaryWeapon);
        isFromSecondaryWeapon = eventType.IsFromSecondaryWeapon;

        var enemyPosition = eventType.EnemyPosition;
        var range = Vector3.Distance(enemyPosition, _weapon.transform.position);

        ComputeRequiredInitialForce(Mathf.Abs(range));
        SetForceToGetToShootingRange();

        ComputeRequiredAngleForProjectile(eventType.EnemyPosition);
        SetAngleToLineUpProjectile();

        // TODO: auto shoot???
    }

    private bool isFromSecondaryWeapon = false;

    [ContextMenu("Test")]
    private void Test()
    {
        if (isFromSecondaryWeapon)
        {
            HandleSecondaryWeaponAbility.ShootStart();
        }
        else
        {
            HandleWeaponAbility.ShootStart();
        }
    }

    private void RefreshTargetWeapon(bool isSecondary)
    {
        _weapon = isSecondary
            ? HandleSecondaryWeaponAbility.CurrentWeapon as ProjectileWeaponAngle
            : HandleWeaponAbility.CurrentWeapon as ProjectileWeaponAngle;

        if (_weapon == default)
        {
            _weapon = isSecondary
                ? HandleSecondaryWeaponAbility.CurrentWeapon as ProjectileWeapon
                : HandleWeaponAbility.CurrentWeapon as ProjectileWeapon;
        }

        var projectileGameObject = (_weapon.ObjectPooler as MMSimpleObjectPooler).GameObjectToPool;
        if (projectileGameObject != default)
        {
            _projectilePhysics = projectileGameObject.MMGetComponentNoAlloc<PhysicsProjectile>();
            _projectileRigidBody = projectileGameObject.MMGetComponentNoAlloc<Rigidbody>();
        }
    }


    #region Obsolete
    // Obsolete
    public void ComputeShootingRange()
    {
        ProjectileWeapon _weapon = HandleWeaponAbility.CurrentWeapon as ProjectileWeaponAngle;
        if (_weapon == default)
        {
            _weapon = HandleWeaponAbility.CurrentWeapon as ProjectileWeapon;
        }

        var projectileGameObject = _weapon.ObjectPooler.GetPooledGameObject();
        if (projectileGameObject != default)
        {
            _projectilePhysics = projectileGameObject.MMGetComponentNoAlloc<PhysicsProjectile>();
            _projectileRigidBody = projectileGameObject.MMGetComponentNoAlloc<Rigidbody>();
        }

        // ===


        // Rough estimate: since start at initial height h0, the actual distance will be longer
        // x = (v0^2 * sin(2a)) / g

        if (HandleWeaponAbility == default)
        {
            return;
        }


        // === Computation - Rough estimation only ===
        var initialVelocity = (_projectilePhysics.InitialForce / _projectileRigidBody.mass) * Time.fixedDeltaTime;
        _initialVelocity = initialVelocity;

        float angle = 0;
        if (_weapon is ProjectileWeaponAngle)
        {
            var weaponAngle = _weapon as ProjectileWeaponAngle;
            angle = Mathf.Max(Mathf.Abs(weaponAngle.FromShootAngle.x), Mathf.Abs(weaponAngle.ToShootAngle.x));
        }
        _angle = angle;

        var gravityModifier = Physics.gravity.magnitude;
        _gravityModifier = gravityModifier;

        _weapon.DetermineSpawnPosition();


        var maxDistanceProjectileMotion = initialVelocity * initialVelocity * Mathf.Sin(2 * angle * Mathf.Deg2Rad) / gravityModifier;
        var distanceTillHitGround = _weapon.SpawnPosition.y / Mathf.Tan(angle * Mathf.Deg2Rad);

        ShootingRangeRadius = _weapon.SpawnPosition.x + maxDistanceProjectileMotion + distanceTillHitGround;

        TriggerFeedbacks();
    }
    #endregion Obsolete
}
