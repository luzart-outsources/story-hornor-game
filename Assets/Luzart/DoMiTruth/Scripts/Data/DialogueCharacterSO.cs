namespace Luzart
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "NewCharacter", menuName = "DoMiTruth/Dialogue Character")]
    public class DialogueCharacterSO : ScriptableObject
    {
        public string characterId;
        public string characterName;
        public Sprite portrait;
        public Color nameColor = Color.white;
    }
}
