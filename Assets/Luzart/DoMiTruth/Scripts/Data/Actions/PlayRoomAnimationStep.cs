namespace Luzart
{
    using System.Collections;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Step_PlayRoomAnimation", menuName = "DoMiTruth/Actions/Play Room Animation")]
    public class PlayRoomAnimationStep : ActionStepConfig
    {
        [Tooltip("Prop để play animation trên đó (null = tìm Animator bất kỳ trong room)")]
        public InteractableObjectSO targetProp;

        [Tooltip("AnimatorController mới để set lên Animator của prop")]
        public RuntimeAnimatorController animatorController;

        [Tooltip("Chờ animation xong mới tiếp tục chain (dựa trên duration)")]
        public float waitDuration = 0f;

        public override ActionStepBehavior CreateBehavior() => new Behavior(this);

        private class Behavior : ActionStepBehavior
        {
            private readonly PlayRoomAnimationStep config;
            public Behavior(PlayRoomAnimationStep config) { this.config = config; }

            public override IEnumerator Execute()
            {
                if (config.animatorController == null) yield break;

                Animator anim = null;

                if (config.targetProp != null)
                {
                    var ui = UIManager.Instance?.GetUiActive<UIInvestigation>(UIName.Investigation);
                    var prop = ui?.FindPropByData(config.targetProp);
                    if (prop != null)
                        anim = prop.GetComponentInChildren<Animator>();
                }

                if (anim != null)
                {
                    anim.runtimeAnimatorController = config.animatorController;
                    anim.enabled = true;
                }

                if (config.waitDuration > 0f)
                    yield return new WaitForSeconds(config.waitDuration);
            }
        }
    }
}
