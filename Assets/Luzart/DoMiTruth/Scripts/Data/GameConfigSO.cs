namespace Luzart
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Video;

    [CreateAssetMenu(fileName = "GameConfig", menuName = "DoMiTruth/Game Config")]
    public class GameConfigSO : ScriptableObject
    {
        [Header("Cutscene")]
        public VideoClip introCutscene;
        public float cutsceneDuration = 30f;
        public float skipButtonDelay = 3f;

        [Header("Maps")]
        public List<MapSO> allMaps = new List<MapSO>();

        [Header("Investigation")]
        public float panSpeed = 5f;
        public float panEdgeThreshold = 50f;

        [Header("Dialogue")]
        public float defaultTypingSpeed = 30f;

        [Header("Effects")]
        public float clueCollectFlyDuration = 0.8f;
    }
}
