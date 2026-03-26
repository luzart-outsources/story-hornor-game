namespace Luzart
{
    using System.Collections;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Wait", menuName = "DoMiTruth/Actions/Wait")]
    public class WaitConfig : ActionStepConfig
    {
        public float duration = 1f;

        public override ActionStepBehavior CreateBehavior()
        {
            return new WaitBehavior(duration);
        }
    }

    public class WaitBehavior : ActionStepBehavior
    {
        private readonly float duration;

        public WaitBehavior(float duration)
        {
            this.duration = duration;
        }

        public override IEnumerator Execute()
        {
            yield return new WaitForSeconds(duration);
        }
    }
}
