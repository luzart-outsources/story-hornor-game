namespace Luzart
{
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Spawn thêm 1 prop mới vào room. Prop phải có sẵn prefab gán vào.
    /// </summary>
    [CreateAssetMenu(fileName = "Step_AddPropToRoom", menuName = "DoMiTruth/Actions/Add Prop To Room")]
    public class AddPropToRoomStep : ActionStepConfig
    {
        [Tooltip("Prefab của prop cần spawn (phải có InteractableObject component)")]
        public GameObject propPrefab;

        [Tooltip("Vị trí trong room (anchored position)")]
        public Vector2 position;

        public override ActionStepBehavior CreateBehavior() => new Behavior(this);

        private class Behavior : ActionStepBehavior
        {
            private readonly AddPropToRoomStep config;
            public Behavior(AddPropToRoomStep config) { this.config = config; }

            public override IEnumerator Execute()
            {
                if (config.propPrefab == null) yield break;

                var ui = UIManager.Instance?.GetUiActive<UIInvestigation>(UIName.Investigation);
                if (ui == null) yield break;

                var container = ui.GetRoomContainer();
                if (container == null) yield break;

                var go = Object.Instantiate(config.propPrefab, container);
                var rt = go.GetComponent<RectTransform>();
                if (rt != null)
                    rt.anchoredPosition = config.position;
            }
        }
    }
}
