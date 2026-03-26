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

            switch (data.interactType)
            {
                case InteractType.Clue:
                    HandleClue(data);
                    break;
                case InteractType.NPC:
                    HandleNPC(data);
                    break;
                case InteractType.LockedItem:
                    HandleLockedItem(data);
                    break;
                case InteractType.Decoration:
                    break;
            }
        }

        private void HandleClue(InteractableObjectSO data)
        {
            if (data.clue == null) return;

            GameDataManager.Instance.AddClue(data.clue.clueId);

            var ui = UIManager.Instance.ShowUI<UIClueDetail>(UIName.ClueDetail);
            if (ui != null)
            {
                ui.Init(data.clue);
            }
        }

        private void HandleNPC(InteractableObjectSO data)
        {
            if (data.dialogue == null) return;
            DialogueManager.Instance.StartDialogue(data.dialogue);
        }

        private void HandleLockedItem(InteractableObjectSO data)
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
