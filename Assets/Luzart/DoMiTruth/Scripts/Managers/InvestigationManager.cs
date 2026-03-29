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
                HandleClue(clueData, data.isOneTimeOnly ? obj : null);
            else if (data is NPCInteractableSO npcData)
                HandleNPC(npcData, data.isOneTimeOnly ? obj : null);
            else if (data is LockedItemInteractableSO lockData)
                HandleLockedItem(lockData, data.isOneTimeOnly ? obj : null);
            // DecorationInteractableSO → không làm gì
        }

        private void HandleClue(ClueInteractableSO data, InteractableObject objToHide = null)
        {
            if (data.clue == null) return;

            GameDataManager.Instance.AddClue(data.clue.clueId);

            var ui = UIManager.Instance.ShowUI<UIClueDetail>(UIName.ClueDetail);
            if (ui != null)
            {
                ui.Init(data.clue, () => HideIfNeeded(objToHide));
            }
        }

        private void HandleNPC(NPCInteractableSO data, InteractableObject objToHide = null)
        {
            var dialogueUI = UIManager.Instance.ShowUI<UINPCDialogue>(UIName.NPCDialogue);
            if (dialogueUI == null) return;

            System.Action onComplete = () => HideIfNeeded(objToHide);

            // Branching dialogue (có dialogue tree)
            if (data.dialogueTree != null)
            {
                dialogueUI.StartBranchingDialogue(
                    data.dialogueTree,
                    data.npcFullBodySprite,
                    data.npcFullBodyAnimator,
                    onComplete: onComplete
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
                    onComplete: onComplete
                );
            }
        }

        private void HandleLockedItem(LockedItemInteractableSO data, InteractableObject objToHide = null)
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
                        HideIfNeeded(objToHide);
                        StartCoroutine(ExecuteActionChain(data.onUnlockSuccess));
                    },
                    onFail: () =>
                    {
                        StartCoroutine(ExecuteActionChain(data.onUnlockFail));
                    }
                );
            }
        }

        private void HideIfNeeded(InteractableObject obj)
        {
            if (obj != null)
                obj.gameObject.SetActive(false);
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
