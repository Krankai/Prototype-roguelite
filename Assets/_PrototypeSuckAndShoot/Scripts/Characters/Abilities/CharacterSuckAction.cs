using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SpiritBomb.Prototype.SuckAndShoot
{
    public class CharacterSuckAction : MonoBehaviour, MMEventListener<MMGameEvent>
    {
        // === Range
        [Header("Range")]
        // the cone of vision range for sucking
        [Tooltip("the cone of vision range for sucking")]
        public MMConeOfVision SuckVision;

        // the number of Suckable objects can be sucked at the same time
        [Tooltip("the number of Suckable objects can be sucked at the same time")]
        [Min(1), Range(1, 10)]
        public int CanSuckCount = 1;

        // === Vision
        [Header("Vision")]
        // the material used for vision to indicate character can now suck objects
        [Tooltip("the material used for vision to indicate character can now suck objects")]
        public Material SuckableMaterial;

        // the material used for vision to indicate character cannot suck objects now
        [Tooltip("the material used for vision to indicate character cannot suck objects now")]
        public Material NonSuckableMaterial;

        // === Technical
        [Header("Technical")]
        // the frame count interval between each scan for suckable targets
        [Tooltip("the frame count interval between each scan for suckable targets")]
        [Min(0), Range(0, 60)]
        public int FrameCountInterval = 1;

        // === Feedbacks
        [Header("Feedbacks")]
        public MMF_Player SuckStartFeedback;
        public MMF_Player SuckCompleteFeedback;

        public UnityEvent OnSuckStartEvent;
        public UnityEvent<bool> OnSuckCompleteEvent;

        // === Debug
        [Header("Debug")]
        [MMReadOnly, SerializeField]
        protected List<CharacterSuckable> _listSucking = new();

        protected List<CharacterSuckable> _listSuckableTargets = new();
        internal CharacterSuckableDistanceComparerDesc _comparerDesc = new();

        protected MeshRenderer _suckVisionMeshRenderer;


        protected virtual void Start()
        {
            _suckVisionMeshRenderer = SuckVision.gameObject.MMGetComponentNoAlloc<MeshRenderer>();
        }

        protected void OnEnable()
        {
            this.MMEventStartListening();
        }

        protected void OnDisable()
        {
            this.MMEventStopListening();
        }

        protected virtual void LateUpdate()
        {
            if (SuckVision != default && Time.frameCount % FrameCountInterval == 0)
            {
                ScanForSuckableTargets();
            }
        }

        protected virtual void ScanForSuckableTargets()
        {
            if (_listSucking.Count >= CanSuckCount)
            {
                return;
            }

            _listSuckableTargets.Clear();

            for (int i = 0, count = SuckVision.VisibleTargets.Count; i < count; ++i)
            {
                var target = SuckVision.VisibleTargets[i];
                if (target.gameObject == default)
                {
                    continue;
                }

                var suckable = target.gameObject.MMGetComponentNoAlloc<CharacterSuckable>();
                if (suckable == default)
                {
                    suckable = target.gameObject.GetComponentInParent<CharacterSuckable>();
                    if (suckable == default)
                    {
                        continue;
                    }
                }

                _listSuckableTargets.Add(suckable);
            }

            if (_listSuckableTargets.Count > 1)
            {
                _listSuckableTargets.Sort(_comparerDesc);
            }
        }

        public virtual bool SuckTargets()
        {
            if (_listSuckableTargets.Count <= 0)
            {
                ScanForSuckableTargets();
            }

            if (_listSuckableTargets.Count <= 0)
            {
                OnSuckCompleteEvent?.Invoke(false);
                return false;
            }

            if (SuckStartFeedback != default)
            {
                SuckStartFeedback.PlayFeedbacks();
            }
            OnSuckStartEvent?.Invoke();

            for (int i = 0, count = _listSuckableTargets.Count; i < count; ++i)
            {
                if (_listSucking.Count >= CanSuckCount)
                {
                    break;
                }

                var suckableTarget = _listSuckableTargets[i];
                if (_listSucking.Contains(suckableTarget))
                {
                    continue;
                }

                suckableTarget.OnSucking(this);
            }

            return true;
        }

        public virtual void CancelSuckingTargets()
        {
            for (int i = 0, count = _listSucking.Count; i < count; ++i)
            {
                var sucking = _listSucking[i];
                if (sucking == default)
                {
                    continue;
                }

                sucking.CancelSucking();
            }

            ReleaseSuckingTargets();
            OnSuckCompleteEvent?.Invoke(false);
        }

        public virtual void ReleaseSuckingTargets()
        {
            _listSucking.Clear();
        }


        public virtual void OnSuckingComplete(CharacterSuckable suckable)
        {
            _listSucking.Remove(suckable);

            if (_listSucking.Count <= 0)
            {
                if (SuckCompleteFeedback != default)
                {
                    SuckCompleteFeedback.PlayFeedbacks();
                }
                OnSuckCompleteEvent?.Invoke(true);

                SetNonSuckableVision();

                // TODO: send event (or anything else) to CharacterShootAction to save suckable to be used as projectile
                SuckedTargetEvent.Trigger(suckable);
            }
        }

        public void OnMMEvent(MMGameEvent eventType)
        {
            if (eventType.EventName.Equals("ReleaseSuckedTargets", System.StringComparison.OrdinalIgnoreCase))
            {
                ReleaseSuckingTargets();
                SetSuckableVision();
            }
        }

        protected virtual void SetSuckableVision()
        {
            _suckVisionMeshRenderer.material = SuckableMaterial;
        }

        protected virtual void SetNonSuckableVision()
        {
            _suckVisionMeshRenderer.material = NonSuckableMaterial;
        }
    }

    class CharacterSuckableDistanceComparerDesc : IComparer<CharacterSuckable>
    {
        public int Compare(CharacterSuckable x, CharacterSuckable y)
        {
            return y.DistanceToSucker.CompareTo(x.DistanceToSucker);
        }
    }

    [System.Serializable]
    public struct SuckedTargetEvent
    {
        public CharacterSuckable Suckable;

        public SuckedTargetEvent(CharacterSuckable suckable)
        {
            Suckable = suckable;
        }

        static SuckedTargetEvent e;

        public static void Trigger(CharacterSuckable suckable)
        {
            e.Suckable = suckable;
            MMEventManager.TriggerEvent(e);
        }
    }
}