namespace Luzart
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.UI;

    [CreateAssetMenu(fileName = "Step_ChangeBackground", menuName = "DoMiTruth/Actions/Change Room Background")]
    public class ChangeRoomBackgroundStep : ActionStepConfig
    {
        [Tooltip("Sprite background mới của room")]
        public Sprite newBackground;

        public override ActionStepBehavior CreateBehavior() => new Behavior(this);

        private class Behavior : ActionStepBehavior
        {
            private readonly ChangeRoomBackgroundStep config;
            public Behavior(ChangeRoomBackgroundStep config) { this.config = config; }

            public override IEnumerator Execute()
            {
                if (config.newBackground == null) yield break;

                // Tìm Image background trong room instance hiện tại
                var ui = UIManager.Instance?.GetUiActive<UIInvestigation>(UIName.Investigation);
                if (ui == null) yield break;

                var bgImage = ui.GetRoomBackground();
                if (bgImage != null)
                    bgImage.sprite = config.newBackground;
            }
        }
    }
}
