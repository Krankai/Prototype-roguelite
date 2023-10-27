using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;

[MMHiddenProperties("AbilityStartFeedbacks", "AbilityStopFeedbacks")]
[RequireComponent(typeof(CharacterHandleWeapon))]
public class CharacterHandleShootingRange : CharacterAbility, MMEventListener<EnemyDetectEvent>
{
    [Header("Ability")]
    // handle weapon ability
    [Tooltip("handle weapon ability")]
    public CharacterHandleWeapon HandleWeaponAbility;

    [Header("Feedback")]
    public MMF_Player ShootingRangeFeedbacks;

    [Header("Output")]
    // shooting range
    [Tooltip("shooting range")]
    public float ShootingRange;
    // required initial force
    [Tooltip("required initial force"), MMReadOnly]
    public float InitialForce;

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

        //Invoke(nameof(ComputeShootingRange), 1f);
        TriggerFeedbacks();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        this.MMEventStartListening<EnemyDetectEvent>();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        this.MMEventStopListening<EnemyDetectEvent>();
    }


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

        ShootingRange = _weapon.SpawnPosition.x + maxDistanceProjectileMotion + distanceTillHitGround;

        TriggerFeedbacks();
    }

    public void ComputeRequiredInitialForce(float range)
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
        var maxDistanceProjectile = range - distanceTillHitGround - _weapon.SpawnPosition.x;
        maxDistanceProjectile += 0.5f;

        _initialVelocity = Mathf.Sqrt(maxDistanceProjectile * _gravityModifier / Mathf.Sin(2 * _angle * Mathf.Deg2Rad));
        InitialForce = _initialVelocity * _projectileRigidBody.mass / Time.fixedDeltaTime;
    }

    private void TriggerFeedbacks()
    {
        var listFeedbacks = ShootingRangeFeedbacks.GetFeedbacksOfType<MMF_Scale>();
        for (int i = 0, count = listFeedbacks.Count; i < count; ++i)
        {
            var scaleFeedback = listFeedbacks[i];

            var scale = scaleFeedback.DestinationScale;
            scale.x = ShootingRange * 2;
            scale.y = ShootingRange * 2;

            scaleFeedback.DestinationScale = scale;
        }

        ShootingRangeFeedbacks.PlayFeedbacks();
    }

    [ContextMenu("Test")]
    private void Test()
    {
        var npc = GameObject.Find("NPC");
        var range = npc.transform.position.x - _character.gameObject.transform.position.x;

        ComputeRequiredInitialForce(range);
    }

    public void OnMMEvent(EnemyDetectEvent eventType)
    {
        if (_weapon == default)
        {
            _weapon = HandleWeaponAbility.CurrentWeapon as ProjectileWeaponAngle;
            if (_weapon == default)
            {
                _weapon = HandleWeaponAbility.CurrentWeapon as ProjectileWeapon;
            }

            var projectileGameObject = (_weapon.ObjectPooler as MMSimpleObjectPooler).GameObjectToPool;
            if (projectileGameObject != default)
            {
                _projectilePhysics = projectileGameObject.MMGetComponentNoAlloc<PhysicsProjectile>();
                _projectileRigidBody = projectileGameObject.MMGetComponentNoAlloc<Rigidbody>();
            }
        }

        var enemyPosition = eventType.EnemyPosition;
        var range = enemyPosition.x - _weapon.transform.position.x;

        ComputeRequiredInitialForce(range);
    }
}
