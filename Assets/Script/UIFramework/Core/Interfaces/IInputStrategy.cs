namespace UIFramework.Core
{
    /// <summary>
    /// Strategy interface for handling UI input
    /// </summary>
    public interface IInputStrategy
    {
        void Enable();
        void Disable();
        void ProcessInput();
    }
}
