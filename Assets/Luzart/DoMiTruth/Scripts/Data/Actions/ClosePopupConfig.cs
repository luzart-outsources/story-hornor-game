namespace Luzart
{
    using System.Collections;
    using UnityEngine;

    [CreateAssetMenu(fileName = "ClosePopup", menuName = "DoMiTruth/Actions/Close Popup")]
    public class ClosePopupConfig : ActionStepConfig
    {
        [Tooltip("Leave as None to close the topmost active UI")]
        public UIName targetPopup = UIName.None;

        public override ActionStepBehavior CreateBehavior()
        {
            return new ClosePopupBehavior(targetPopup);
        }
    }

    public class ClosePopupBehavior : ActionStepBehavior
    {
        private readonly UIName targetPopup;

        public ClosePopupBehavior(UIName targetPopup)
        {
            this.targetPopup = targetPopup;
        }

        public override IEnumerator Execute()
        {
            if (targetPopup != UIName.None)
            {
                UIManager.Instance.HideUiActive(targetPopup);
            }
            else
            {
                var last = UIManager.Instance.GetLastUiActive();
                if (last != null) last.Hide();
            }
            yield return null;
        }
    }
}
