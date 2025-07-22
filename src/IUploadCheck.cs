namespace BetterWorkshopUploader
{
    /// <summary>
    /// Check to run before mod upload to display a pass/fail result
    /// </summary>
    internal interface IUploadCheck
    {
        /// <summary>
        /// Display text
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Makes the check only display in the event that the check fails
        /// </summary>
        public bool IsHiddenCheck { get; }

        /// <summary>
        /// Runs the check on the given mod
        /// </summary>
        /// <param name="mod">The mod to check</param>
        /// <param name="data">The workshop data file</param>
        /// <returns>Pass or fail. Returning null means the check is not applicable.</returns>
        public bool? RunCheck(ModManager.Mod mod, BWUWorkshopData data);
    }
}
