namespace Luzart
{
    using System.Collections;
    using UnityEngine;

    [CreateAssetMenu(fileName = "ShowDialogue", menuName = "DoMiTruth/Actions/Show Dialogue")]
    public class ShowDialogueConfig : ActionStepConfig
    {
        public DialogueSequenceSO dialogue;

        public override ActionStepBehavior CreateBehavior()
        {
            return new ShowDialogueBehavior(dialogue);
        }
    }

    public class ShowDialogueBehavior : ActionStepBehavior
    {
        private readonly DialogueSequenceSO dialogue;

        public ShowDialogueBehavior(DialogueSequenceSO dialogue)
        {
            this.dialogue = dialogue;
        }

        public override IEnumerator Execute()
        {
            if (dialogue == null) yield break;

            bool done = false;
            DialogueManager.Instance.StartDialogue(dialogue, () => done = true);

            while (!done)
            {
                yield return null;
            }
        }
    }
}
