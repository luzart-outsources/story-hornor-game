using UnityEngine;

namespace Luzart.Core
{
    /// <summary>
    /// Helper component để giữ GameObject persist giữa các scene loads.
    /// Attach vào [GameManagers] GameObject để prevent destruction khi load scene mới.
    /// </summary>
    public class DontDestroyOnLoadHelper : MonoBehaviour
    {
        /// <summary>
        /// Called when the script instance is being loaded.
        /// Ensures this GameObject persists across scene loads.
        /// </summary>
        private void Awake()
        {
            // Check if another instance already exists
            GameObject[] managers = GameObject.FindGameObjectsWithTag(gameObject.tag);
            if (managers.Length > 1)
            {
                // Another instance exists, destroy this duplicate
                Destroy(gameObject);
                return;
            }

            // Mark this GameObject to not be destroyed when loading new scenes
            DontDestroyOnLoad(gameObject);
            
            Debug.Log($"[DontDestroyOnLoadHelper] {gameObject.name} will persist across scenes.");
        }
    }
}
