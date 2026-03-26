namespace Luzart
{
    using System.Collections;
    using UnityEngine;

    [CreateAssetMenu(fileName = "CollectClue", menuName = "DoMiTruth/Actions/Collect Clue")]
    public class CollectClueConfig : ActionStepConfig
    {
        public ClueSO clue;

        public override ActionStepBehavior CreateBehavior()
        {
            return new CollectClueBehavior(clue);
        }
    }

    public class CollectClueBehavior : ActionStepBehavior
    {
        private readonly ClueSO clue;

        public CollectClueBehavior(ClueSO clue)
        {
            this.clue = clue;
        }

        public override IEnumerator Execute()
        {
            if (clue != null)
            {
                GameDataManager.Instance.AddClue(clue.clueId);
            }
            yield return null;
        }
    }
}
