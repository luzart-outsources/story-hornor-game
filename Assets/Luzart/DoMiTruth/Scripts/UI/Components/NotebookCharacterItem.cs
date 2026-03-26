namespace Luzart
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class NotebookCharacterItem : MonoBehaviour
    {
        [SerializeField] private Image imgPortrait;
        [SerializeField] private TMP_Text txtName;

        public void Init(DialogueCharacterSO character)
        {
            if (imgPortrait != null && character.portrait != null)
                imgPortrait.sprite = character.portrait;

            if (txtName != null)
            {
                txtName.text = character.characterName;
                txtName.color = character.nameColor;
            }
        }
    }
}
