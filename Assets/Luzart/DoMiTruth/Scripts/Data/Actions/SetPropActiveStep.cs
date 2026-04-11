namespace Luzart
{
    using System.Collections;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Step_SetPropActive", menuName = "DoMiTruth/Actions/Set Prop Active")]
    public class SetPropActiveStep : ActionStepConfig
    {
        [Tooltip("SO của prop cần show/hide (match theo objectId)")]
        public InteractableObjectSO targetProp;

        [Tooltip("true = show, false = hide")]
        public bool setActive = true;

        public override ActionStepBehavior CreateBehavior() => new Behavior(this);

        private class Behavior : ActionStepBehavior
        {
            private readonly SetPropActiveStep config;
            public Behavior(SetPropActiveStep config) { this.config = config; }

            public override IEnumerator Execute()
            {
                if (config.targetProp == null) yield break;

                var ui = UIManager.Instance?.GetUiActive<UIInvestigation>(UIName.Investigation);
                if (ui == null) yield break;

                var prop = ui.FindPropByData(config.targetProp);
                if (prop != null)
                    prop.gameObject.SetActive(config.setActive);
            }
        }
    }
}
