namespace Luzart
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.UI;

    public class UIBase : MonoBehaviour
    {
        public UIName uiName = UIName.None;
        public Button closeBtn;
        public bool isCache = false;

        [Header("Animation (Optional)")]
        [SerializeField] protected TweenAnimationCaller showAnimation;
        [SerializeField] protected TweenAnimationCaller hideAnimation;

        protected bool isSetup = false;
        protected System.Action onHideDone;

        protected virtual void Setup()
        {
            if (closeBtn != null)
            {
                GameUtil.ButtonOnClick(closeBtn, OnClickClose);
            }
        }

        public virtual void Show(System.Action onHideDone)
        {
            if (!isSetup)
            {
                isSetup = true;
                Setup();
            }
            gameObject.SetActive(true);
            InitActionOnHideDone(onHideDone);

            if (showAnimation != null)
            {
                showAnimation.CallShow();
            }
        }

        public void InitActionOnHideDone(System.Action onHideDone)
        {
            this.onHideDone = onHideDone;
        }

        public virtual void RefreshUI()
        {
        }

        public virtual void Hide()
        {
            UIManager.Instance.RemoveActiveUI(uiName);
            if (gameObject == null) return;

            if (hideAnimation != null)
            {
                hideAnimation.CallShow();
                StartCoroutine(DelayHideAfterAnimation());
            }
            else
            {
                ExecuteHide();
            }
        }

        private IEnumerator DelayHideAfterAnimation()
        {
            // Wait for hide animation to finish
            while (hideAnimation != null && hideAnimation.IsPlaying())
            {
                yield return null;
            }
            ExecuteHide();
        }

        private void ExecuteHide()
        {
            if (gameObject == null) return;

            if (!isCache)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
            onHideDone?.Invoke();
            onHideDone = null;
        }

        public virtual void OnClickClose()
        {
            Hide();
        }
    }
}
