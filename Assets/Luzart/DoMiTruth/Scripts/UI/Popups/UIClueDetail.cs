namespace Luzart
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class UIClueDetail : UIBase
    {
        [SerializeField] private Image imgClue;
        [SerializeField] private TMP_Text txtClueName;
        [SerializeField] private TMP_Text txtDescription;
        [SerializeField] private TMP_Text txtCategory;

        [Header("Fly Animation Target")]
        [SerializeField] private Transform notebookTarget;

        private ClueSO currentClue;
        private System.Action onCloseCallback;

        public void Init(ClueSO clue, System.Action onClose = null)
        {
            currentClue = clue;
            onCloseCallback = onClose;

            if (imgClue != null && clue.clueImage != null)
                imgClue.sprite = clue.clueImage;

            if (txtClueName != null)
                txtClueName.text = clue.clueName;

            if (txtDescription != null)
                txtDescription.text = clue.description;

            if (txtCategory != null)
                txtCategory.text = clue.category.ToString();
        }

        public override void OnClickClose()
        {
            if (currentClue != null && currentClue.clueImage != null && notebookTarget != null)
            {
                var config = GameFlowController.Instance.GameConfig;
                float duration = config != null ? config.clueCollectFlyDuration : 0.8f;

                ClueCollectAnimation.Play(
                    currentClue.clueImage,
                    imgClue != null ? imgClue.transform.position : transform.position,
                    notebookTarget,
                    duration,
                    () => base.OnClickClose()
                );
            }
            else
            {
                base.OnClickClose();
            }

            onCloseCallback?.Invoke();
            onCloseCallback = null;
        }
    }
}
