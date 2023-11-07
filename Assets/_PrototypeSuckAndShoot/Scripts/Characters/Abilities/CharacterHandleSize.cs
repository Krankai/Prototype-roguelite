using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using MoreMountains.Tools;

namespace SpiritBomb.Prototype.SuckAndShoot
{
    [MMHiddenProperties("AbilityStartFeedbacks", "AbilityStopFeedbacks")]
    public class CharacterHandleSize : CharacterAbility
    {
        [Header("Points")]
        [MMReadOnly, SerializeField]
        // the current count value used to determine model's scale and cone of vision's radius
        [Tooltip("the current count value used to determine model's scale and cone of vision's radius")]
        protected int _currentCount;

        [Header("Scale")]
        // the list of scale multipliers
        [Tooltip("the list of scale multipliers")]
        public List<FloatOnCondition> ListScaleMultipliers = new();

        // the associated handle weapon whose projectile will need to be scaled
        [Tooltip("the associated handle weapon whose projectile will need to be scaled")]
        public CharacterHandleWeapon HandleWeapon;

        // the list of scale multipliers for projectile
        [Tooltip("the list of scale multipliers for projectile")]
        public List<FloatOnCondition> ListProjectileScaleMultipliers = new();


        [Header("Animation")]
        // the easing used to animate scaling animation
        [Tooltip("the easing used to animate scaling animation")]
        [SerializeField]
        private Ease _easeType = Ease.Linear;

        // the animation duration for each scale
        [Tooltip("the animation duration for each scale")]
        [SerializeField]
        private float _duration;


        [Header("Cone of Vision")]
        // whether to adjust cone of vision
        [Tooltip("whether to adjust cone of vision")]
        public bool IsAdjustConeOfVision;

        // the associated cone of vision
        [MMCondition(nameof(IsAdjustConeOfVision), true)]
        [Tooltip("the associated cone of vision")]
        public MMConeOfVision CharacterConeOfVision;

        // the list of cone-of-vision radius
        [MMCondition(nameof(IsAdjustConeOfVision), true)]
        [Tooltip("the list of cone-of-vision radius")]
        public List<FloatOnCondition> ListRadius = new();



        protected CharacterConditionCountComparer _comparer;
        protected Vector3 _originalScale = Vector3.one;
        protected Vector3 _originalProjectileScale = Vector3.one;
        protected float _originalRadius = 1f;


        protected override void Initialization()
        {
            base.Initialization();

            _currentCount = 0;
            _comparer = new();

            ListScaleMultipliers.Sort(_comparer);
            ListRadius.Sort(_comparer);
            ListProjectileScaleMultipliers.Sort(_comparer);

            if (HandleWeapon == default)
            {
                HandleWeapon = _character.FindAbility<CharacterHandleWeapon>();
            }

            Invoke(nameof(LateInitialization), 1f);
        }

        protected virtual void LateInitialization()
        {
            if (_character.CharacterModel != default)
            {
                _originalScale = _character.CharacterModel.transform.localScale;
            }

            if (HandleWeapon != default && HandleWeapon.CurrentWeapon != default)
            {
                var pooler = HandleWeapon.CurrentWeapon.gameObject.MMGetComponentNoAlloc<DynamicScaleObjectPooler>();
                if (pooler != default)
                {
                    var poolableObject = pooler.GameObjectToPool.MMGetComponentNoAlloc<MMPoolableObject>();
                    if (poolableObject != default)
                    {
                        _originalProjectileScale = poolableObject.transform.localScale;
                    }

                    pooler.DynamicScale = _originalProjectileScale;
                }
            }

            if (CharacterConeOfVision == default)
            {
                CharacterConeOfVision = _character.gameObject.GetComponentInChildren<MMConeOfVision>();
            }

            if (CharacterConeOfVision != default)
            {
                _originalRadius = CharacterConeOfVision.VisionRadius;
            }

        }

        public virtual void OnUpdateSize(int addedCount)
        {
            Debug.LogError($"Added count: {addedCount}");
            _currentCount += addedCount;

            UpdateModelScale(_currentCount);
            UpdateConeOfVisionRadius(_currentCount);
            UpdateProjectileScale(_currentCount);
        }

        protected virtual float GetLargestMatchingScale(int countForScale)
        {
            for (int i = 0, count = ListScaleMultipliers.Count; i < count; ++i)
            {
                var scaleConditionData = ListScaleMultipliers[i];
                if (scaleConditionData.ConditionCount <= countForScale)
                {
                    return scaleConditionData.Value;
                }
            }

            return 1f;
        }

        protected virtual void UpdateModelScale(int countForScale)
        {
            var newScale = GetLargestMatchingScale(countForScale);
            var scaleModel = _character.CharacterModel;

            if (scaleModel == default)
            {
                scaleModel = _character.gameObject;
            }

            scaleModel.transform.DOScale(newScale, _duration).SetEase(_easeType);
        }

        protected virtual float GetLargestMatchingProjectileScale(int countForScale)
        {
            for (int i = 0, count = ListProjectileScaleMultipliers.Count; i < count; ++i)
            {
                var scaleConditionData = ListProjectileScaleMultipliers[i];
                if (scaleConditionData.ConditionCount <= countForScale)
                {
                    return scaleConditionData.Value;
                }
            }

            return 1f;
        }

        protected virtual void UpdateProjectileScale(int countForScale)
        {
            if (HandleWeapon.CurrentWeapon == default)
            {
                return;
            }

            var newScale = GetLargestMatchingProjectileScale(countForScale);

            var pooler = HandleWeapon.CurrentWeapon.gameObject.MMGetComponentNoAlloc<DynamicScaleObjectPooler>();
            if (pooler == default)
            {
                return;
            }

            pooler.DynamicScale = newScale * _originalProjectileScale;
        }

        protected virtual void UpdateConeOfVisionRadius(int countForRadius)
        {
            if (!IsAdjustConeOfVision || CharacterConeOfVision == default)
            {
                return;
            }

            var newRadius = GetLargestMatchingRadius(countForRadius);
            CharacterConeOfVision.VisionRadius = newRadius;
        }

        protected virtual float GetLargestMatchingRadius(int countForRadius)
        {
            for (int i = 0, count = ListRadius.Count; i < count; ++i)
            {
                var radiusConditionData = ListRadius[i];
                if (radiusConditionData.ConditionCount <= countForRadius)
                {
                    return radiusConditionData.Value;
                }
            }

            return _originalRadius;
        }


        int[] list = new int[] { 1, 3, 5, 7, 9, 12, 15 };
        int index = 0;

        [ContextMenu("TestUpdateSize")]
        private void TestUpdateSize()
        {
            var count = list[index++ % list.Length];
            Debug.Log($"Count: {count}");

            UpdateModelScale(count);
            UpdateConeOfVisionRadius(count);
        }
    }

    // float values associated with different condition count
    [System.Serializable]
    public struct FloatOnCondition
    {
        public float Value;
        public int ConditionCount;
    }

    public class CharacterConditionCountComparer : IComparer<FloatOnCondition>
    {
        public int Compare(FloatOnCondition x, FloatOnCondition y)
        {
            return y.ConditionCount.CompareTo(x.ConditionCount);
        }
    }
}
