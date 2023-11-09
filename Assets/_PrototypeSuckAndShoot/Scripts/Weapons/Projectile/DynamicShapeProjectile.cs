using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpiritBomb.Prototype.SuckAndShoot
{
    public class DynamicShapeProjectile : Projectile
    {
        [Header("Health")]
        // the base health for projectile, into which all damages on its associated model will be redirected
        [Tooltip("the base health for projectile, into which all damages on its associated model will be redirected")]
        [MMReadOnlyWhenPlaying]
        public Health BaseHealth;

        [Header("Model")]
        // the transform to which projectile's model will be attached
        [Tooltip("the transform to which projectile's model will be attached")]
        public Transform ModelAttachement;

        // whether to destroy model after done using, or keep it there for pooling
        [Tooltip("whether to destroy model after done using, or keep it there for pooling")]
        public bool IsDestroyModel;


        [Header("Homing Mode")]
        // whether to steer projectile slightly towards enemies within range (= homing projectile)
        [Tooltip("whether to steer projectile slightly towards enemies within range (= homing projectile)")]
        public bool IsHomingMode;

        [MMCondition(nameof(IsHomingMode), true)]
        // the frames interval to detect target in homing mode
        [Tooltip("the frames interval to detect target in homing mode")]
        public int FrameCountInterval = 1;

        [MMCondition(nameof(IsHomingMode), true)]
        // the target layer(s) to search for
        [Tooltip("the target layer(s) to search for")]
        public LayerMask TargetLayerMask;

        [MMCondition(nameof(IsHomingMode), true)]
        // the range radius to detect target
        [Tooltip("the range radius to detect target")]
        public float DetectRadius = 5;

        [MMCondition(nameof(IsHomingMode), true)]
        // the maximum number of targets within the overlap detection that can be acquired
        [Tooltip("the maximum number of targets within the overlap detection that can be acquired")]
        public int DetectOverlapMaximum = 5;

        [MMCondition(nameof(IsHomingMode), true)]
        // the speed at which projectile will steer its rotation towards target
        [Tooltip("the speed at which projectile will steer its rotation towards target")]
        public float SteeringSpeed = 5f;

        [MMCondition(nameof(IsHomingMode), true)]
        // the minimum angle difference to target direction below which steering is no longer needed
        [Tooltip("the minimum angle difference to target direction below which steering is no longer needed")]
        [Range(0, 360)]
        public float CutOffSteerAngle = 1f;

        [MMCondition(nameof(IsHomingMode), true)]
        // the maximum angle (both side of projectile direction) above which projectile will not be steered
        [Tooltip("the maximum angle (both side of projectile direction) above which projectile will not be steered")]
        [Range(0, 360)]
        public float MaxApplySteeringAngle = 30f;


        [Header("Debug")]
        [SerializeField]
        protected bool _isDebugOn;

        [SerializeField, MMReadOnly]
        protected float _differenceAngle;

        [SerializeField, MMReadOnly]
        protected bool _isFinishSteering = false;

        protected MeshRenderer _defaultMeshRenderer;

        protected Vector3 _raycastOrigin;
        protected Collider[] _hitColliers;
        protected List<Collider> _listDetectedColliders;


        protected override void Initialization()
        {
            base.Initialization();

            if (BaseHealth == default)
            {
                BaseHealth = gameObject.MMGetComponentAroundOrAdd<Health>();
            }

            if (_defaultMeshRenderer == default)
            {
                _defaultMeshRenderer = gameObject.MMGetComponentNoAlloc<MeshRenderer>();
            }

            _defaultMeshRenderer.enabled = false;
            _damageOnTouch.enabled = false;

            _hitColliers = new Collider[DetectOverlapMaximum];
            _listDetectedColliders = new();
        }

        public override void Destroy()
        {
            if (IsDestroyModel)
            {
                ModelAttachement.MMDestroyAllChildren();
            }
            else
            {
                for (int i = 0, count = ModelAttachement.childCount; i < count; ++i)
                {
                    var child = ModelAttachement.GetChild(i);
                    if (child == default)
                    {
                        continue;
                    }

                    child.gameObject.SetActive(false);
                }
            }

            _isFinishSteering = false;

            base.Destroy();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (IsHomingMode && Time.frameCount % FrameCountInterval == 0)
            {
                SteerTowardsTargets();
            }
        }

        public virtual void EnableDefaultShape()
        {
            _defaultMeshRenderer.enabled = true;
            _damageOnTouch.enabled = true;
        }

        protected virtual void SteerTowardsTargets()
        {
            if (_isFinishSteering)
            {
                return;
            }

            _raycastOrigin = _collider.bounds.center;

            int countFoundColliders = Physics.OverlapSphereNonAlloc(_raycastOrigin, DetectRadius, _hitColliers, TargetLayerMask);
            if (countFoundColliders <= 0)
            {
                return;
            }

            _listDetectedColliders.Clear();
            for (int i = 0, length = _hitColliers.Length; i < length; ++i)
            {
                if (_hitColliers[i] == default || _hitColliers[i].gameObject == gameObject)
                {
                    continue;
                }

                _listDetectedColliders.Add(_hitColliers[i]);
            }

            _listDetectedColliders.Sort(delegate (Collider a, Collider b)
            {
                var distanceToA = Vector3.Distance(transform.position, a.transform.position);
                var distanceToB = Vector3.Distance(transform.position, b.transform.position);

                return distanceToA.CompareTo(distanceToB);
            });

            int count = Mathf.Min(countFoundColliders, DetectOverlapMaximum);
            for (int i = 0; i < count; ++i)
            {
                var targetCollider = _listDetectedColliders[i];

                var directionToTarget = Vector3.ClampMagnitude(targetCollider.bounds.center - _collider.bounds.center, Direction.magnitude);
                _differenceAngle = Vector3.Angle(Direction, directionToTarget);
                if (_differenceAngle > MaxApplySteeringAngle)
                {
                    continue;
                }

                var newDirection = Vector3.Slerp(Direction, directionToTarget, Time.deltaTime * SteeringSpeed);
                SetDirection(newDirection, transform.rotation);

                _differenceAngle = Vector3.Angle(Direction, directionToTarget);
                if (_differenceAngle <= CutOffSteerAngle)
                {
                    _isFinishSteering = true;
                }

                break;
            }
        }

        private void OnDrawGizmos()
        {
            if (!_isDebugOn)
            {
                return;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawLine(_raycastOrigin, _raycastOrigin + 3 * Direction);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_raycastOrigin, DetectRadius);
        }
    }
}
