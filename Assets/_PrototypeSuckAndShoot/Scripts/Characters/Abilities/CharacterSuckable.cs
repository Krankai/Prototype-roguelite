using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;

namespace SpiritBomb.Prototype.SuckAndShoot
{
    /// <summary>
    /// To be used with CharacterSuckOnSight. Object with this component can be sucked
    /// </summary>
    public class CharacterSuckable : MonoBehaviour
    {
        public float DistanceToSucker => _currentSucker != default
            ? Vector3.Distance(transform.position, _currentSucker.transform.position) : 0f;

        [MMReadOnly]
        public bool IsBeingSucked = false;


        // === Suck
        [Header("Suck")]
        // the duration in which object will be fully sucked
        [Tooltip("the duration in which object will be fully sucked")]
        public float SuckDuration = 1f;

        // the ratio of sucked time above which the suck action can no longer be cancelled
        [Tooltip("the ratio of sucked time above which the suck action can no longer be cancelled")]
        [Range(0, 1)]
        public float RatioTimeMaxCancellable = 0.9f;

        // whether to keep object stationary on being sucked
        [Tooltip("whether to keep object stationary on being sucked")]
        public bool IsStaticOnSucking = false;


        // === Points
        [Header("Points")]
        // the points received when successfully sucked in this object
        [Tooltip("the points received when successfully sucked in this object")]
        [Min(1)]
        public int Points;


        // === As Projectile
        [Header("As Projectile")]
        // the associated model to be used as projectile once being sucked
        [Tooltip("the associated model to be used as projectile once being sucked")]
        public GameObject SuckableAsProjectilePrefab;

        // the id used to name instantiated project tile object (for later pooling)
        [Tooltip("the id used to name instantiated project tile object (for later pooling)")]
        public string SuckableAsProjectileID;

        // the scale for the projectile prefab
        [Tooltip("the scale for the projectile prefab")]
        public Vector3 ScaleSuckableAsProjectile = Vector3.one;

        // the offset position for the projectile prefab
        [Tooltip("the offset position for the projectile prefab")]
        public Vector3 OffsetSuckableAsProjectile = Vector3.zero;

        // the rotation for the projectile prefab
        [Tooltip("the rotation for the projectile prefab")]
        public Vector3 RotationSuckableAsProjectile = Vector3.zero;


        // === Feedbacks
        [Header("Feedbacks")]
        // the feedback during sucking process
        [Tooltip("the feedback during sucking process")]
        public MMF_Player SuckingFeedback;

        protected CharacterSuckOnSight _currentSucker;
        protected CharacterSuckAction _currentSuckerAction;
        protected Health _health;
        protected ProjectileMotionControl _projectileMotionControl;


        protected virtual void Start()
        {
            Initialization();
        }

        protected virtual void Initialization()
        {
            _health = gameObject.MMGetComponentNoAlloc<Health>();
            if (_health == default)
            {
                _health = gameObject.GetComponentInParent<Health>();
            }

            _health.OnRevive += OnRestore;


            if (IsStaticOnSucking)
            {
                _projectileMotionControl = gameObject.MMGetComponentNoAlloc<ProjectileMotionControl>();
            }

            SyncFeedbackDuration();
        }

        protected virtual void SyncFeedbackDuration()
        {
            if (SuckingFeedback == default)
            {
                return;
            }

            var listFeedbacks = SuckingFeedback.FeedbacksList;
            for (int i = 0, count = listFeedbacks.Count; i < count; ++i)
            {
                var feedback = listFeedbacks[i];
                if (feedback == default || !feedback.Active)
                {
                    continue;
                }

                feedback.FeedbackDuration = SuckDuration;
                //if (feedback is MMF_Scale)
                //{
                //    var scaleFeedback = feedback as MMF_Scale;
                //    scaleFeedback.AnimateScaleDuration = SuckDuration;
                //}
                //else if (feedback is MMF_Flicker)
                //{
                //    var flickerFeedback = feedback as MMF_Flicker;
                //    flickerFeedback.FlickerDuration = SuckDuration;
                //}
            }
        }

        public virtual void OnSucking(CharacterSuckOnSight suckOnSight)
        {
            if (IsBeingSucked) return;

            if (SuckingFeedback != default)
            {
                SuckingFeedback.Events.OnPlay.AddListener(() => OnStart(suckOnSight));
                SuckingFeedback.PlayFeedbacks();
            }
            else
            {
                OnSuckingComplete();
            }
        }

        public virtual void OnSucking(CharacterSuckAction suckAction)
        {
            if (IsBeingSucked) return;

            if (SuckingFeedback != default)
            {
                SuckingFeedback.Events.OnPlay.AddListener(() => OnStartSucking(suckAction));
                SuckingFeedback.PlayFeedbacks();
            }
            else
            {
                OnStartSucking(suckAction);
                OnSuckingComplete();
                OnRestore();
            }
        }

        public virtual void OnSuckingComplete()
        {
            if (_currentSucker != default)
            {
                _currentSucker.OnSuckComplete(this);
            }

            if (_currentSuckerAction != default)
            {
                _currentSuckerAction.OnSuckingComplete(this);
            }

            if (SuckingFeedback != default)
            {
                SuckingFeedback.Events.OnPlay.RemoveAllListeners();
            }

            _health.Kill();

            if (IsStaticOnSucking && _projectileMotionControl != default)
            {
                _projectileMotionControl.ResumeControl();
            }
        }

        public virtual void OnStart(CharacterSuckOnSight suckOnSight)
        {
            Debug.Log("Suck");

            //SuckingFeedback.Events.OnPlay.RemoveAllListeners();

            IsBeingSucked = true;
            _currentSucker = suckOnSight;

            if (IsStaticOnSucking && _projectileMotionControl)
            {
                _projectileMotionControl.PauseControl();
            }
        }

        public virtual void OnStartSucking(CharacterSuckAction suckAction)
        {
            Debug.Log("Start sucking");

            IsBeingSucked = true;
            _currentSuckerAction = suckAction;
        }

        public virtual void OnRestore()
        {
            IsBeingSucked = false;
            _currentSucker = default;
            _currentSuckerAction = default;
        }

        //public float time;
        //public float total;

        //private void LateUpdate()
        //{
        //    time = SuckingFeedback.ElapsedTime;
        //    total = SuckingFeedback.TotalDuration;
        //}

        public virtual void CancelSucking()
        {
            if (!IsBeingSucked)
            {
                return;
            }

            bool isCancellable = SuckingFeedback.ElapsedTime < RatioTimeMaxCancellable * SuckingFeedback.TotalDuration;
            if (SuckingFeedback.IsPlaying && isCancellable)
            {
                Debug.Log("Cancel");

                SuckingFeedback.StopFeedbacks();
                SuckingFeedback.RestoreInitialValues();
            }

            if (IsStaticOnSucking && _projectileMotionControl)
            {
                _projectileMotionControl.ResumeControl();
            }
        }
    }
}
