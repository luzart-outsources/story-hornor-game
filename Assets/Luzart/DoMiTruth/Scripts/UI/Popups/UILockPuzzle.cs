namespace Luzart
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using DG.Tweening;

    public class UILockPuzzle : UIBase
    {
        [Header("Panel Switching")]
        [SerializeField] private SelectSwitchGameObject panelSwitch;

        [Header("Passcode Panel")]
        [SerializeField] private TMP_InputField inputPasscode;
        [SerializeField] private Button btnConfirm;
        [SerializeField] private TMP_Text txtHint;
        [SerializeField] private TMP_Text txtError;

        private LockConfigSO lockConfig;
        private Action onSuccess;
        private Action onFail;

        protected override void Setup()
        {
            base.Setup();
            GameUtil.ButtonOnClick(btnConfirm, OnClickConfirm);
        }

        public void Init(LockConfigSO config, Action onSuccess, Action onFail)
        {
            lockConfig = config;
            this.onSuccess = onSuccess;
            this.onFail = onFail;

            if (panelSwitch != null)
                panelSwitch.Select((int)config.lockType);

            if (txtHint != null)
                txtHint.text = config.hintText;

            if (txtError != null)
                txtError.gameObject.SetActive(false);

            if (inputPasscode != null)
                inputPasscode.text = string.Empty;
        }

        private void OnClickConfirm()
        {
            if (lockConfig == null) return;

            switch (lockConfig.lockType)
            {
                case LockType.Passcode:
                    ValidatePasscode();
                    break;
                case LockType.SwipePattern:
                    // Future implementation
                    break;
            }
        }

        private void ValidatePasscode()
        {
            if (inputPasscode == null) return;

            string input = inputPasscode.text.Trim();
            if (string.Equals(input, lockConfig.passcode, StringComparison.OrdinalIgnoreCase))
            {
                Hide();
                onSuccess?.Invoke();
            }
            else
            {
                OnWrongAnswer();
                onFail?.Invoke();
            }
        }

        private void OnWrongAnswer()
        {
            if (txtError != null)
            {
                txtError.gameObject.SetActive(true);
            }

            transform.DOShakePosition(0.3f, 10f, 20);

            if (inputPasscode != null)
                inputPasscode.text = string.Empty;
        }
    }
}
