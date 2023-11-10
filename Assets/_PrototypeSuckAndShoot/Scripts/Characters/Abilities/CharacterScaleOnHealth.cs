using MoreMountains.Feedbacks;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpiritBomb.Prototype.SuckAndShoot
{
    public class CharacterScaleOnHealth : MonoBehaviour
    {
        [Header("Bindings")]
        // the transform of the model to be scaled
        [Tooltip("the transform of the model to be scaled")]
        public Transform ScaleModel;

        // the character's health component
        [Tooltip("the character's health component")]
        public Health CharacterHealth;

        // the character's auto-respawn component
        [Tooltip("the character's auto-respawn component")]
        public AutoRespawn CharacterAutoRespawn;

        // the associated feedback to update scale-on-health animation
        [Tooltip("the associated feedback to update scale-on-health animation")]
        public MMF_Player ScaleOnHeatlhFeedbackPlayer;

        protected Vector3 _originalScale;


        protected virtual void Start()
        {
            if (CharacterHealth == default)
            {
                CharacterHealth = gameObject.GetComponentInParent<Health>();
            }

            if (CharacterAutoRespawn == default)
            {
                CharacterAutoRespawn = gameObject.GetComponentInParent<AutoRespawn>();
            }

            if (ScaleModel == default)
            {
                ScaleModel = transform;
            }

            _originalScale = ScaleModel.localScale;

            if (CharacterAutoRespawn != default)
            {
                CharacterAutoRespawn.OnRevive += () => ScaleModel.localScale = _originalScale;
            }
        }

        public virtual void UpdateScaleOnHealthFeedback()
        {
            if (ScaleOnHeatlhFeedbackPlayer == default || ScaleOnHeatlhFeedbackPlayer.FeedbacksList.Count <= 0)
            {
                return;
            }

            var scaleFeedback = ScaleOnHeatlhFeedbackPlayer.FeedbacksList.Find(feedback => feedback is MMF_Scale
                && feedback.Label.Equals("ScaleOnHealth")) as MMF_Scale;

            if (scaleFeedback == default)
            {
                return;
            }

            
            var ratio = CharacterHealth.CurrentHealth * 1f / CharacterHealth.MaximumHealth;
            scaleFeedback.DestinationScale = ratio * _originalScale;
        }

        public virtual void UpdateScaleOnHealth()
        {
            if (CharacterHealth == default)
            {
                return;
            }

            var ratio = CharacterHealth.CurrentHealth * 1f / CharacterHealth.MaximumHealth;
            ratio = Mathf.Clamp(ratio, 0, 1);

            if (ScaleModel != default)
            {
                ScaleModel.localScale = ratio * _originalScale;
            }
            else
            {
                transform.localScale = ratio * _originalScale;
            }
        }
    }
}
