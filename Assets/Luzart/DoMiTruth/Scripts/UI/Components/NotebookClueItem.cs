namespace Luzart
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class NotebookClueItem : MonoBehaviour
    {
        [SerializeField] private Image imgClue;
        [SerializeField] private TMP_Text txtName;
        [SerializeField] private TMP_Text txtCategory;
        [SerializeField] private Button btnDetail;

        private ClueSO clueData;

        public void Init(ClueSO clue)
        {
            clueData = clue;

            if (imgClue != null && clue.clueImage != null)
                imgClue.sprite = clue.clueImage;

            if (txtName != null)
                txtName.text = clue.clueName;

            if (txtCategory != null)
                txtCategory.text = clue.category.ToString();

            if (btnDetail != null)
                GameUtil.ButtonOnClick(btnDetail, OnClickDetail);
        }

        private void OnClickDetail()
        {
            var ui = UIManager.Instance.ShowUI<UIClueDetail>(UIName.ClueDetail);
            if (ui != null)
            {
                ui.Init(clueData);
            }
        }
    }
}
