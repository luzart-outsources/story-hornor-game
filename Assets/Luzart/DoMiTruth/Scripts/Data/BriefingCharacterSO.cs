namespace Luzart
{
    using UnityEngine;

    /// <summary>
    /// Config riêng cho NPC full body trong màn Briefing.
    /// Tách biệt khỏi DialogueCharacterSO để không ảnh hưởng hệ thống dialogue.
    /// </summary>
    [CreateAssetMenu(fileName = "NewBriefingChar", menuName = "DoMiTruth/Briefing Character")]
    public class BriefingCharacterSO : ScriptableObject
    {
        [Tooltip("Link đến DialogueCharacterSO để map characterId.")]
        public DialogueCharacterSO character;

        [Header("Full Body Display")]
        public Sprite fullBodySprite;
        [Tooltip("Optional — nếu null thì dùng fullBodySprite tĩnh.")]
        public RuntimeAnimatorController fullBodyAnimator;
    }
}
