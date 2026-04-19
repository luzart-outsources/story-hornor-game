namespace Luzart
{
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// ActionStep: Hiện detective monologue nhỏ ở dưới màn hình.
    /// Dùng cho thám tử tự nói/nhận xét ngắn trong investigation.
    /// Chờ player đóng xong mới tiếp tục chain.
    /// </summary>
    [CreateAssetMenu(fileName = "Step_ShowMonologue", menuName = "DoMiTruth/Actions/Show Detective Monologue")]
    public class ShowMonologueStep : ActionStepConfig
    {
        [Tooltip("Dialogue sequence cho monologue. Thường chỉ 1-2 dòng.")]
        public DialogueSequenceSO dialogue;

        public override ActionStepBehavior CreateBehavior() => new Behavior(this);

        private class Behavior : ActionStepBehavior
        {
            private readonly ShowMonologueStep config;
            public Behavior(ShowMonologueStep config) { this.config = config; }

            public override IEnumerator Execute()
            {
                if (config.dialogue == null) yield break;

                bool done = false;

                var ui = UIManager.Instance.ShowUI<UIDetectiveMonologue>(UIName.DetectiveMonologue);
                if (ui != null)
                {
                    ui.StartMonologue(config.dialogue, () => done = true);
                }
                else
                {
                    // Fallback: dùng DialogueManager nếu chưa có prefab
                    DialogueManager.Instance.StartDialogue(config.dialogue, () => done = true);
                }

                while (!done)
                    yield return null;
            }
        }
    }
}
