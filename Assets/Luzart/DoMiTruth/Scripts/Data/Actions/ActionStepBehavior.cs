namespace Luzart
{
    using System.Collections;

    /// <summary>
    /// Abstract behavior for action steps.
    /// Created by ActionStepConfig.CreateBehavior().
    /// Execute() is an IEnumerator for coroutine-based sequential execution.
    /// </summary>
    public abstract class ActionStepBehavior
    {
        public abstract IEnumerator Execute();
    }
}
