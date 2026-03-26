namespace Luzart
{
    using UnityEngine;

    /// <summary>
    /// Abstract SO config for action steps in action chains.
    /// Each concrete config holds data and creates a matching behavior via CreateBehavior().
    /// Strategy Pattern: Config (data) + Behavior (logic).
    /// </summary>
    public abstract class ActionStepConfig : ScriptableObject
    {
        public abstract ActionStepBehavior CreateBehavior();
    }
}
