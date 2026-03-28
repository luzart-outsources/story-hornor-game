namespace Luzart
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "NewCharacter", menuName = "DoMiTruth/Dialogue Character")]
    public class DialogueCharacterSO : ScriptableObject
    {
        public string characterId;
        public string characterName;

        [Header("Sprites (fallback khi không có Animator)")]
        public Sprite portrait;
        public Sprite fullBodySprite;

        [Header("Animation (optional — nếu null thì dùng Sprite tĩnh)")]
        [Tooltip("AnimatorController cho portrait nhỏ trong dialogue box.")]
        public RuntimeAnimatorController portraitAnimator;
        [Tooltip("AnimatorController cho full body NPC.")]
        public RuntimeAnimatorController fullBodyAnimator;

        public Color nameColor = Color.white;
    }
}
