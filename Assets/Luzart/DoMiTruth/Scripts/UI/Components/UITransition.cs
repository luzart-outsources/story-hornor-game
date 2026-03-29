namespace Luzart
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    using DG.Tweening;

    /// <summary>
    /// Circle Wipe transition — đen đóng từ ngoài vào giữa, rồi mở từ giữa ra ngoài.
    /// Dùng shader DoMiTruth/CircleWipe.
    /// Gắn trên 1 full-screen RawImage, nằm trên cùng (overlay).
    /// </summary>
    public class UITransition : MonoBehaviour
    {
        public static UITransition Instance { get; private set; }

        [SerializeField] private RawImage wipeImage;
        [SerializeField] private float closeDuration = 0.6f;
        [SerializeField] private float openDuration = 0.6f;
        [SerializeField] private float holdDuration = 0.2f;

        private Material wipeMat;
        private static readonly int ProgressID = Shader.PropertyToID("_Progress");

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (wipeImage != null)
            {
                // Tạo material instance riêng
                wipeMat = new Material(wipeImage.material);
                wipeImage.material = wipeMat;

                // Bắt đầu mở hoàn toàn (ẩn)
                wipeMat.SetFloat(ProgressID, 0f);
                wipeImage.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Chạy transition: đóng → gọi callback (chuyển màn) → mở.
        /// </summary>
        public void PlayTransition(Action onMidpoint, Action onComplete = null)
        {
            if (wipeMat == null || wipeImage == null)
            {
                // Không có material → gọi thẳng
                onMidpoint?.Invoke();
                onComplete?.Invoke();
                return;
            }

            wipeImage.gameObject.SetActive(true);
            wipeImage.raycastTarget = true; // block input trong lúc transition

            // Phase 1: Đóng (progress 0 → 1)
            wipeMat.SetFloat(ProgressID, 0f);

            DOTween.To(() => wipeMat.GetFloat(ProgressID),
                v => wipeMat.SetFloat(ProgressID, v),
                1f, closeDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    // Midpoint — màn hình đen hoàn toàn → chuyển scene
                    onMidpoint?.Invoke();

                    // Hold 1 chút rồi mở
                    DOVirtual.DelayedCall(holdDuration, () =>
                    {
                        // Phase 2: Mở (progress 1 → 0)
                        DOTween.To(() => wipeMat.GetFloat(ProgressID),
                            v => wipeMat.SetFloat(ProgressID, v),
                            0f, openDuration)
                            .SetEase(Ease.OutQuad)
                            .OnComplete(() =>
                            {
                                wipeImage.raycastTarget = false;
                                wipeImage.gameObject.SetActive(false);
                                onComplete?.Invoke();
                            });
                    });
                });
        }

        /// <summary>
        /// Chỉ đóng (đen hết) — dùng khi muốn tự control mở.
        /// </summary>
        public void Close(Action onComplete = null)
        {
            if (wipeMat == null || wipeImage == null)
            {
                onComplete?.Invoke();
                return;
            }

            wipeImage.gameObject.SetActive(true);
            wipeImage.raycastTarget = true;
            wipeMat.SetFloat(ProgressID, 0f);

            DOTween.To(() => wipeMat.GetFloat(ProgressID),
                v => wipeMat.SetFloat(ProgressID, v),
                1f, closeDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() => onComplete?.Invoke());
        }

        /// <summary>
        /// Chỉ mở (bỏ đen) — dùng sau khi đã Close.
        /// </summary>
        public void Open(Action onComplete = null)
        {
            if (wipeMat == null || wipeImage == null)
            {
                onComplete?.Invoke();
                return;
            }

            DOTween.To(() => wipeMat.GetFloat(ProgressID),
                v => wipeMat.SetFloat(ProgressID, v),
                0f, openDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    wipeImage.raycastTarget = false;
                    wipeImage.gameObject.SetActive(false);
                    onComplete?.Invoke();
                });
        }

        private void OnDestroy()
        {
            if (wipeMat != null)
                Destroy(wipeMat);
        }
    }
}
