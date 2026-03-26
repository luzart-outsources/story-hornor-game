namespace Luzart
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using DG.Tweening;

    public class UINPCDialogue : UIBase
    {
        [SerializeField] private Image imgPortrait;
        [SerializeField] private TMP_Text txtName;
        [SerializeField] private TMP_Text txtDialogue;
        [SerializeField] private Button btnNext;

        protected override void Setup()
        {
            base.Setup();
            GameUtil.ButtonOnClick(btnNext, OnClickNext);
        }

        public void DisplayLine(DialogueLine line)
        {
            if (line.character != null)
            {
                if (imgPortrait != null && line.character.portrait != null)
                    imgPortrait.sprite = line.character.portrait;

                if (txtName != null)
                {
                    txtName.text = line.character.characterName;
                    txtName.color = line.character.nameColor;
                }
            }

            if (txtDialogue != null)
            {
                float speed = line.typingSpeed > 0 ? line.typingSpeed : 30f;
                var tweener = txtDialogue.DOSetTextCharByChar(line.text, speed);
                DialogueManager.Instance.SetTypingTweener(tweener);
            }
        }

        private void OnClickNext()
        {
            DialogueManager.Instance.ShowNextLine();
        }
    }
}
