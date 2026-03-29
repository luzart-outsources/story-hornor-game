namespace Luzart
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class InvestigationManager : Singleton<InvestigationManager>
    {
        private RoomSO currentRoom;

        public RoomSO CurrentRoom => currentRoom;

        public void LoadRoom(RoomSO room)
        {
            if (room == null) return;
            currentRoom = room;

            var ui = UIManager.Instance.ShowUI<UIInvestigation>(UIName.Investigation);
            if (ui != null)
            {
                ui.LoadRoom(room);
            }

            if (room.entryDialogue != null)
            {
                DialogueManager.Instance.StartDialogue(room.entryDialogue);
            }
        }

        public void OnObjectClicked(InteractableObject obj)
        {
            if (obj == null || obj.Data == null) return;

            var data = obj.Data;

            if (data.isOneTimeOnly && GameDataManager.Instance.HasInteracted(data.objectId))
                return;

            if (data.isOneTimeOnly)
            {
                GameDataManager.Instance.MarkInteracted(data.objectId);
            }

            if (data is ClueInteractableSO clueData)
                HandleClue(clueData);
            else if (data is NPCInteractableSO npcData)
                HandleNPC(npcData);
            else if (data is LockedItemInteractableSO lockData)
                HandleLockedItem(lockData);
            // DecorationInteractableSO → không làm gì
        }

        private void HandleClue(ClueInteractableSO data)
        {
            if (data.clue == null) return;

            GameDataManager.Instance.AddClue(data.clue.clueId);

            var ui = UIManager.Instance.ShowUI<UIClueDetail>(UIName.ClueDetail);
            if (ui != null)
            {
                ui.Init(data.clue);
            }
        }

        private void HandleNPC(NPCInteractableSO data)
        {
            var dialogueUI = UIManager.Instance.ShowUI<UINPCDialogue>(UIName.NPCDialogue);
            if (dialogueUI == null) return;

            // Branching dialogue (có dialogue tree)
            if (data.dialogueTree != null)
            {
                dialogueUI.StartBranchingDialogue(
                    data.dialogueTree,
                    data.npcFullBodySprite,
                    data.npcFullBodyAnimator,
                    onComplete: null
                );
                return;
            }

            // Fallback: linear dialogue
            if (data.fallbackDialogue != null)
            {
                var npcChar = data.fallbackDialogue.lines.Count > 0
                    ? data.fallbackDialogue.lines[0].character
                    : null;

                dialogueUI.StartLinearDialogue(
                    data.fallbackDialogue,
                    data.npcFullBodySprite,
                    data.npcFullBodyAnimator,
                    npcChar,
                    onComplete: null
                );
            }
        }

        private void HandleLockedItem(LockedItemInteractableSO data)
        {
            if (data.lockConfig == null) return;

            if (GameDataManager.Instance.IsItemUnlocked(data.objectId))
            {
                StartCoroutine(ExecuteActionChain(data.onUnlockSuccess));
                return;
            }

            var ui = UIManager.Instance.ShowUI<UILockPuzzle>(UIName.LockPuzzle);
            if (ui != null)
            {
                ui.Init(
                    data.lockConfig,
                    onSuccess: () =>
                    {
                        GameDataManager.Instance.UnlockItem(data.objectId);
                        StartCoroutine(ExecuteActionChain(data.onUnlockSuccess));
                    },
                    onFail: () =>
                    {
                        StartCoroutine(ExecuteActionChain(data.onUnlockFail));
                    }
                );
            }
        }

        public IEnumerator ExecuteActionChain(List<ActionStepConfig> configs)
        {
            if (configs == null) yield break;

            foreach (var config in configs)
            {
                if (config == null) continue;
                var behavior = config.CreateBehavior();
                yield return behavior.Execute();
            }
        }
    }
}
