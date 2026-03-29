namespace Luzart
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "NewCharacter", menuName = "DoMiTruth/Dialogue Character")]
    public class DialogueCharacterSO : ScriptableObject
    {
        public string characterId;
        public string characterName;

        [Header("Portrait (dialogue box)")]
        public Sprite portrait;
        [Tooltip("Optional — nếu null thì dùng Sprite tĩnh.")]
        public RuntimeAnimatorController portraitAnimator;

        public Color nameColor = Color.white;
        public string foundLocation;
    }
}
