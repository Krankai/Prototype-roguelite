using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using MoreMountains.Tools;

namespace SpiritBomb.Prototype.SuckAndChuck
{
    [MMHiddenProperties("AbilityStartFeedbacks", "AbilityStopFeedbacks")]
    public class CharacterHandleSize : CharacterAbility
    {
        [Header("Scale")]
        // the list of scale multipliers
        [Tooltip("the list of scale multipliers")]
        public List<FloatOnCondition> ListScaleMultipliers = new();


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
        protected float _originalRadius = 1f;


        protected override void Initialization()
        {
            base.Initialization();

            _comparer = new();

            if (ListScaleMultipliers.Count >= 0)
            {
                ListScaleMultipliers.Sort(_comparer);
            }

            if (ListRadius.Count >= 0)
            {
                ListRadius.Sort(_comparer);
            }

            Invoke(nameof(LateCacheData), 1f);
        }

        protected virtual void LateCacheData()
        {
            if (_character.CharacterModel != default)
            {
                _originalScale = _character.CharacterModel.transform.localScale;
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

        public virtual void UpdateModelScale(int countForScale)
        {
            var newScale = GetLargestMatchingScale(countForScale);
            var scaleModel = _character.CharacterModel;

            if (scaleModel == default)
            {
                scaleModel = _character.gameObject;
            }

            scaleModel.transform.DOScale(newScale, _duration).SetEase(_easeType);
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
