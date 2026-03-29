namespace Luzart
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "NewClue", menuName = "DoMiTruth/Clue")]
    public class ClueSO : ScriptableObject
    {
        public string clueId;
        public string clueName;
        public Sprite clueImage;
        [TextArea(3, 6)] public string description;
        public ClueCategory category;
        public string foundLocation;
    }

    public enum ClueCategory
    {
        Physical = 0,
        Document = 1,
        Testimony = 2,
        Photo = 3,
    }
}
