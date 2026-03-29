namespace Luzart
{
    using UnityEngine;

    public class GameBootstrap : MonoBehaviour
    {
        [Header("Transition")]
        [SerializeField] private UITransition transitionPrefab;

        private void Start()
        {
            // Spawn transition overlay (nếu chưa có)
            if (UITransition.Instance == null && transitionPrefab != null)
            {
                var t = Instantiate(transitionPrefab);
                DontDestroyOnLoad(t.gameObject);
            }

            if (GameDataManager.Instance.HasSaveData())
            {
                GameFlowController.Instance.ShowMainMenu();
            }
            else
            {
                GameFlowController.Instance.ShowMainMenu();
            }
        }
    }
}
