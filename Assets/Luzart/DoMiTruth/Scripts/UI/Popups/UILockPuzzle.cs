namespace Luzart
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;
    using TMPro;
    using DG.Tweening;

    public class UILockPuzzle : UIBase
    {
        [Header("Panel Switching")]
        [SerializeField] private SelectSwitchGameObject panelSwitch;

        [Header("Passcode Panel")]
        [SerializeField] private TMP_Text txtPasscodeDisplay;
        [SerializeField] private Button[] numpadButtons; // 0-9
        [SerializeField] private Button btnDelete;
        [SerializeField] private Button btnConfirm;
        [SerializeField] private TMP_Text txtHint;
        [SerializeField] private TMP_Text txtError;
        [SerializeField] private int maxDigits = 6;

        [Header("Swipe Pattern Panel")]
        [SerializeField] private SwipePatternDot[] patternDots;
        [SerializeField] private Button btnPatternConfirm;
        [SerializeField] private Button btnPatternReset;
        [SerializeField] private TMP_Text txtPatternHint;
        [SerializeField] private TMP_Text txtPatternError;

        private LockConfigSO lockConfig;
        private Action onSuccess;
        private Action onFail;

        private string currentPasscode = "";
        private readonly List<int> currentPattern = new List<int>();
        private bool isDrawingPattern;

        protected override void Setup()
        {
            base.Setup();
            GameUtil.ButtonOnClick(btnConfirm, OnClickConfirm);
            GameUtil.ButtonOnClick(btnPatternConfirm, OnClickPatternConfirm);
            GameUtil.ButtonOnClick(btnPatternReset, OnClickPatternReset);

            if (btnDelete != null)
                GameUtil.ButtonOnClick(btnDelete, OnClickDelete);

            SetupNumpad();
            SetupPatternDots();
        }

        private void SetupNumpad()
        {
            if (numpadButtons == null) return;
            for (int i = 0; i < numpadButtons.Length; i++)
            {
                if (numpadButtons[i] == null) continue;
                int digit = i;
                GameUtil.ButtonOnClick(numpadButtons[i], () => OnNumpadPress(digit));
            }
        }

        private void OnNumpadPress(int digit)
        {
            if (currentPasscode.Length >= maxDigits) return;
            currentPasscode += digit.ToString();
            UpdatePasscodeDisplay();
            SoundManager.Instance?.PlayPasscodeInputSFX();
        }

        private void OnClickDelete()
        {
            if (currentPasscode.Length == 0) return;
            currentPasscode = currentPasscode.Substring(0, currentPasscode.Length - 1);
            UpdatePasscodeDisplay();
        }

        private void UpdatePasscodeDisplay()
        {
            if (txtPasscodeDisplay != null)
                txtPasscodeDisplay.text = currentPasscode;
        }

        private void SetupPatternDots()
        {
            if (patternDots == null) return;

            for (int i = 0; i < patternDots.Length; i++)
            {
                if (patternDots[i] == null) continue;

                int dotIndex = i;
                patternDots[i].Init(dotIndex, OnDotPointerDown, OnDotPointerEnter, OnDotPointerUp);
            }
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

            currentPasscode = "";
            UpdatePasscodeDisplay();

            if (txtPatternHint != null)
                txtPatternHint.text = config.hintText;

            if (txtPatternError != null)
                txtPatternError.gameObject.SetActive(false);

            ResetPattern();
        }

        #region Passcode

        private void OnClickConfirm()
        {
            if (lockConfig == null) return;

            switch (lockConfig.lockType)
            {
                case LockType.Passcode:
                    ValidatePasscode();
                    break;
                case LockType.SwipePattern:
                    break;
            }
        }

        private void ValidatePasscode()
        {
            if (string.Equals(currentPasscode, lockConfig.passcode, StringComparison.OrdinalIgnoreCase))
            {
                SoundManager.Instance?.PlaySafeOpenSFX();
                Hide();
                onSuccess?.Invoke();
            }
            else
            {
                SoundManager.Instance?.PlayPasscodeWrongSFX();
                OnWrongPasscode();
                onFail?.Invoke();
            }
        }

        private void OnWrongPasscode()
        {
            if (txtError != null)
                txtError.gameObject.SetActive(true);

            transform.DOShakePosition(0.3f, 10f, 20);

            currentPasscode = "";
            UpdatePasscodeDisplay();
        }

        #endregion

        #region Swipe Pattern

        private void OnDotPointerDown(int dotIndex)
        {
            isDrawingPattern = true;
            currentPattern.Clear();
            ResetDotVisuals();
            AddDotToPattern(dotIndex);
        }

        private void OnDotPointerEnter(int dotIndex)
        {
            if (!isDrawingPattern) return;
            if (currentPattern.Contains(dotIndex)) return;
            AddDotToPattern(dotIndex);
        }

        private void OnDotPointerUp(int dotIndex)
        {
            isDrawingPattern = false;
        }

        private void AddDotToPattern(int dotIndex)
        {
            currentPattern.Add(dotIndex);
            if (patternDots != null && dotIndex < patternDots.Length && patternDots[dotIndex] != null)
                patternDots[dotIndex].SetSelected(true);
        }

        private void OnClickPatternConfirm()
        {
            if (lockConfig == null) return;
            ValidatePattern();
        }

        private void ValidatePattern()
        {
            if (lockConfig.swipePattern == null || lockConfig.swipePattern.Length == 0)
            {
                Hide();
                onSuccess?.Invoke();
                return;
            }

            if (currentPattern.Count != lockConfig.swipePattern.Length)
            {
                OnWrongPattern();
                return;
            }

            for (int i = 0; i < currentPattern.Count; i++)
            {
                if (currentPattern[i] != lockConfig.swipePattern[i])
                {
                    OnWrongPattern();
                    return;
                }
            }

            Hide();
            onSuccess?.Invoke();
        }

        private void OnWrongPattern()
        {
            if (txtPatternError != null)
                txtPatternError.gameObject.SetActive(true);

            transform.DOShakePosition(0.3f, 10f, 20);

            onFail?.Invoke();
            ResetPattern();
        }

        private void OnClickPatternReset()
        {
            ResetPattern();
        }

        private void ResetPattern()
        {
            currentPattern.Clear();
            isDrawingPattern = false;
            ResetDotVisuals();
        }

        private void ResetDotVisuals()
        {
            if (patternDots == null) return;
            for (int i = 0; i < patternDots.Length; i++)
            {
                if (patternDots[i] != null)
                    patternDots[i].SetSelected(false);
            }
        }

        #endregion
    }
}
