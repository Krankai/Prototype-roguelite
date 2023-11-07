using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;

namespace SpiritBomb.Prototype.SuckAndChuck
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


        protected virtual void Start()
        {
            Initialization();
        }

        protected virtual void Initialization()
        {
            _health = gameObject.GetComponentInParent<Health>();
            _health.OnRevive += OnRestore;
        }

        public virtual void OnSucking(CharacterSuckOnSight suckOnSight)
        {
            if (IsBeingSucked) return;

            if (SuckingFeedback != default)
            {
                SuckingFeedback.Events.OnPlay.AddListener(() => OnStart(suckOnSight));
                SuckingFeedback.Events.OnComplete.AddListener(OnSuckingComplete);

                SuckingFeedback.PlayFeedbacks();
            }
            else
            {
                OnSuckingComplete();
            }
        }

        protected virtual void OnSuckingComplete()
        {
            if (_currentSucker != default)
            {
                _currentSucker.OnGainPoints(Points);
            }

            if (SuckingFeedback != default)
            {
                SuckingFeedback.Events.OnPlay.RemoveAllListeners();
                SuckingFeedback.Events.OnComplete.RemoveListener(OnSuckingComplete);

                //SuckingFeedback.StopFeedbacks();
                SuckingFeedback.RestoreInitialValues();
            }

            _health.Kill();
        }

        public virtual void OnStart(CharacterSuckOnSight suckOnSight)
        {
            Debug.Log("Suck");

            //SuckingFeedback.Events.OnPlay.RemoveAllListeners();

            IsBeingSucked = true;
            _currentSucker = suckOnSight;
        }

        public virtual void OnRestore()
        {
            IsBeingSucked = false;
            _currentSucker = default;
        }

        public virtual void CancelSucking()
        {
            bool isCancellable = SuckingFeedback.ElapsedTime < RatioTimeMaxCancellable * SuckingFeedback.TotalDuration;
            if (SuckingFeedback.IsPlaying && isCancellable)
            {
                Debug.Log("Cancel");

                SuckingFeedback.StopFeedbacks();
                SuckingFeedback.RestoreInitialValues();
            }
        }
    }
}
