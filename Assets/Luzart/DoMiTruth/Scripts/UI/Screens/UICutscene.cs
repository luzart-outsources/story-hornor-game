namespace Luzart
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.Video;

    public class UICutscene : UIBase
    {
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private RawImage videoDisplay;
        [SerializeField] private Button btnSkip;
        [SerializeField] private SelectToggleGameObject skipToggle;

        protected override void Setup()
        {
            base.Setup();
            GameUtil.ButtonOnClick(btnSkip, OnClickSkip);
        }

        public override void Show(System.Action onHideDone)
        {
            base.Show(onHideDone);

            if (skipToggle != null)
                skipToggle.Select(false);

            var config = GameFlowController.Instance.GameConfig;
            StartCoroutine(PlayCutscene(config));
        }

        private IEnumerator PlayCutscene(GameConfigSO config)
        {
            if (config.introCutscene != null && videoPlayer != null)
            {
                videoPlayer.clip = config.introCutscene;
                videoPlayer.Play();
            }

            yield return new WaitForSeconds(config.skipButtonDelay);

            if (skipToggle != null)
                skipToggle.Select(true);
            else if (btnSkip != null)
                btnSkip.gameObject.SetActive(true);

            if (videoPlayer != null && videoPlayer.clip != null)
            {
                float remaining = (float)(videoPlayer.clip.length - videoPlayer.time);
                yield return new WaitForSeconds(Mathf.Max(0, remaining));
            }
            else
            {
                yield return new WaitForSeconds(config.cutsceneDuration);
            }

            OnCutsceneEnd();
        }

        private void OnClickSkip()
        {
            StopAllCoroutines();
            OnCutsceneEnd();
        }

        private void OnCutsceneEnd()
        {
            if (videoPlayer != null)
                videoPlayer.Stop();
            GameFlowController.Instance.OnCutsceneComplete();
        }
    }
}
