using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpiritBomb.Prototype.SuckAndChuck
{
    /// <summary>
    /// To be used with CharacterSuckable. Object with this component will suck 
    /// </summary>
    public class CharacterSuckOnSight : MonoBehaviour
    {
        // the cone of vision used to detect suckable objects
        [Tooltip("the cone of vision used to detect suckable objects")]
        public MMConeOfVision SuckConeOfVision;

        // the number of objects can be sucked at a time
        [Tooltip("the number of objects can be sucked at a time")]
        [Min(1)]
        public int CountSucking = 1;

        // the frame interval to scan for sucking targets
        [Tooltip("the frame interval to scan for sucking targets")]
        [Min(1), Range(1, 30)]
        public int FrameInterval = 3;


        protected virtual void LateUpdate()
        {
            if (Time.frameCount % FrameInterval == 0 && SuckConeOfVision != default)
            {
                ScanForSuckTargets();
            }
        }

        protected virtual void ScanForSuckTargets()
        {
            var listTargets = SuckConeOfVision.VisibleTargets;
            for (int i = 0, count = listTargets.Count; i < count; ++i)
            {

            }
        }

        protected virtual void Suck(CharacterSuckable suckable)
        {
            if (suckable.gameObject != default)
            {
                suckable.OnSucking(this);
            }
        }

        public virtual void OnGainPoints(int points)
        {
            // TODO
        }
    }
}
