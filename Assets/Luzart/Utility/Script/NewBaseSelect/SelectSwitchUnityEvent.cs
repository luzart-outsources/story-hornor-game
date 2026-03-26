namespace Luzart
{
    public class SelectSwitchUnityEvent : SelectSwitch
    {
        public UnityEngine.Events.UnityEvent[] onSelect;
        public override void Select(int value)
        {
            foreach (var selectEvent in onSelect)
            {
                selectEvent?.Invoke();
            }
        }
    }
}
