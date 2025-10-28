namespace MultiXIVLauncher.Utils.Interfaces
{
    /// <summary>
    /// Interface for views that support saving their current state or configuration.
    /// </summary>
    public interface ISavableView
    {
        /// <summary>
        /// Saves the current view state or user settings.
        /// </summary>
        void Save();
    }
}
