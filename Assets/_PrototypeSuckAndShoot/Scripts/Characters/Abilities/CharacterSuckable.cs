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
        [MMReadOnly]
        public bool IsBeingSucked = false;

        [Header("Suck")]
        // the ratio of sucked time above which the suck action can no longer be cancelled
        [Tooltip("the ratio of sucked time above which the suck action can no longer be cancelled")]
        [Range(0, 1)]
        public float RatioTimeMaxCancellable = 0.9f;
        // whether to keep object stationary on being sucked
        [Tooltip("whether to keep object stationary on being sucked")]
        public bool IsStaticOnSucking = false;

        [Header("Points")]
        // the points received when successfully sucked in this object
        [Tooltip("the points received when successfully sucked in this object")]
        [Min(1)]
        public int Points;

        [Header("Feedbacks")]
        // the feedback during sucking process
        [Tooltip("the feedback during sucking process")]
        public MMF_Player SuckingFeedback;

        protected CharacterSuckOnSight _currentSucker;
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

        public virtual void OnSuckingComplete()
        {
            if (_currentSucker != default)
            {
                _currentSucker.OnGainPoints(Points);
            }

            if (SuckingFeedback != default)
            {
                SuckingFeedback.Events.OnPlay.RemoveAllListeners();

                //SuckingFeedback.StopFeedbacks();
                //SuckingFeedback.RestoreInitialValues();
            }

            _health.Kill();

            if (IsStaticOnSucking && _projectileMotionControl)
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

        public virtual void OnRestore()
        {
            IsBeingSucked = false;
            _currentSucker = default;
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
            bool isCancellable = SuckingFeedback.ElapsedTime < RatioTimeMaxCancellable * SuckingFeedback.TotalDuration;
            if (SuckingFeedback.IsPlaying && isCancellable)
            {
                Debug.Log("Cancel");

                SuckingFeedback.StopFeedbacks();
                SuckingFeedback.RestoreInitialValues();

                if (IsStaticOnSucking && _projectileMotionControl)
                {
                    _projectileMotionControl.ResumeControl();
                }
            }
        }
    }
}
