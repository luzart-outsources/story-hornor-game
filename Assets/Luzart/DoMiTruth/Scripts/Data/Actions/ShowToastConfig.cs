namespace Luzart
{
    using System.Collections;
    using UnityEngine;

    [CreateAssetMenu(fileName = "ShowToast", menuName = "DoMiTruth/Actions/Show Toast")]
    public class ShowToastConfig : ActionStepConfig
    {
        public string message;

        public override ActionStepBehavior CreateBehavior()
        {
            return new ShowToastBehavior(message);
        }
    }

    public class ShowToastBehavior : ActionStepBehavior
    {
        private readonly string message;

        public ShowToastBehavior(string message)
        {
            this.message = message;
        }

        public override IEnumerator Execute()
        {
            UIManager.Instance.ShowToast(message);
            yield return null;
        }
    }
}
