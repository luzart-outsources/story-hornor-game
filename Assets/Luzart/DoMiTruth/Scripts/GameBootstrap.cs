namespace Luzart
{
    using UnityEngine;

    public class GameBootstrap : MonoBehaviour
    {
        private void Start()
        {
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
