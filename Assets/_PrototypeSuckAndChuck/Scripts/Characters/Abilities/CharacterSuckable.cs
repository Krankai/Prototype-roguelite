using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpiritBomb.Prototype.SuckAndChuck
{
    /// <summary>
    /// To be used with CharacterSuckOnSight. Object with this component can be sucked
    /// </summary>
    public class CharacterSuckable : MonoBehaviour
    {
        [Header("Gained Stats")]
        // the points received when successfully sucked in this object
        [Tooltip("the points received when successfully sucked in this object")]
        [Min(1)]
        public int Points;

        [Header("Feedbacks")]
        // the feedback during sucking process
        [Tooltip("the feedback during sucking process")]
        public MMF_Player SuckingFeedback;


        public virtual void OnSucking(CharacterSuckOnSight suckOnSight)
        {
            if (SuckingFeedback != default)
            {
                SuckingFeedback.PlayFeedbacks();
            }

            suckOnSight.OnGainPoints(Points);
        }
    }
}
