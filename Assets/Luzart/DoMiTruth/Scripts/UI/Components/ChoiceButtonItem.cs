namespace Luzart
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class ChoiceButtonItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text txtLabel;
        [SerializeField] private Button btnSelect;

        private Action onClick;

        public void Init(string label, Action onClickCallback)
        {
            onClick = onClickCallback;

            if (txtLabel != null)
                txtLabel.text = label;

            if (btnSelect != null)
                GameUtil.ButtonOnClick(btnSelect, OnClick);
        }

        private void OnClick()
        {
            onClick?.Invoke();
        }
    }
}
