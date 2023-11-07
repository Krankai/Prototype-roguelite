using MoreMountains.Feedbacks;
using MoreMountains.Tools;

namespace SpiritBomb.Prototype.SuckAndShoot
{
    public class AIDecisionBeingSucked : AIDecision
    {
        protected CharacterSuckable Suckable;


        public override void Initialization()
        {
            base.Initialization();

            Suckable = transform.parent.gameObject.MMFGetComponentNoAlloc<CharacterSuckable>();
        }

        public override bool Decide()
        {
            return (Suckable != default && Suckable.IsBeingSucked);
        }
    }
}
