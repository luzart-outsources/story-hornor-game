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

        [Header("Briefing")]
        [Tooltip("Dialogue khi Công an giao nhiệm vụ sau Cutscene. Nếu null sẽ bỏ qua.")]
        public DialogueSequenceSO briefingDialogue;
        [Tooltip("Danh sách NPC full body hiển thị trong màn Briefing.")]
        public List<BriefingCharacterSO> briefingCharacters = new List<BriefingCharacterSO>();

        [Header("Maps")]
        public List<MapSO> allMaps = new List<MapSO>();

        [Header("Investigation")]
        public float panSpeed = 5f;
        public float panEdgeThreshold = 50f;

        [Header("Dialogue")]
        public float defaultTypingSpeed = 30f;

        [Header("Effects")]
        public float clueCollectFlyDuration = 0.8f;

        [Header("Sound Effects")]
        [Tooltip("Sound khi dialogue text hiện ra")]
        public AudioClip sfxDialogue;
        [Tooltip("Sound khi bấm nút ở menu (Settings, Guide, Quit)")]
        public AudioClip sfxMenuClick;
        [Tooltip("Sound khi tương tác đồ vật ingame")]
        public AudioClip sfxInteract;
        [Tooltip("Sound khi nhập mật khẩu két sắt")]
        public AudioClip sfxPasscodeInput;
        [Tooltip("Sound khi sai mật khẩu")]
        public AudioClip sfxPasscodeWrong;
        [Tooltip("Sound khi mở két sắt thành công")]
        public AudioClip sfxSafeOpen;
    }
}
