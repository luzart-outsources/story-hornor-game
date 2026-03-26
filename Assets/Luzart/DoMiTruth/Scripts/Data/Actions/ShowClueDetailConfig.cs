namespace Luzart
{
    using System.Collections;
    using UnityEngine;

    [CreateAssetMenu(fileName = "ShowClueDetail", menuName = "DoMiTruth/Actions/Show Clue Detail")]
    public class ShowClueDetailConfig : ActionStepConfig
    {
        public ClueSO clue;

        public override ActionStepBehavior CreateBehavior()
        {
            return new ShowClueDetailBehavior(clue);
        }
    }

    public class ShowClueDetailBehavior : ActionStepBehavior
    {
        private readonly ClueSO clue;

        public ShowClueDetailBehavior(ClueSO clue)
        {
            this.clue = clue;
        }

        public override IEnumerator Execute()
        {
            if (clue == null) yield break;

            var ui = UIManager.Instance.ShowUI<UIClueDetail>(UIName.ClueDetail);
            if (ui != null)
            {
                ui.Init(clue);
            }
            yield return null;
        }
    }
}
