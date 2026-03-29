namespace Luzart
{
    using UnityEngine;

    public class InteractableObjectSO : ScriptableObject
    {
        public string objectId;
        public Vector2 hitboxSize = new Vector2(100f, 100f);
        public bool isOneTimeOnly;
        public Sprite highlightSprite;
    }
}
