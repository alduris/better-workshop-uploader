namespace BetterWorkshopUploader
{
    /// <summary>
    /// An upload check with a runnable action.
    /// </summary>
    internal interface IUploadCheckWithAction : IUploadCheck
    {
        /// <summary>
        /// The text to display on the action button
        /// </summary>
        public string ActionText { get; }

        /// <summary>
        /// Whether or not to allow the user to run the action. Checked when <see cref="IUploadCheck.RunCheck(ModManager.Mod)"/> is run.
        /// Action button will not appear unless true.
        /// </summary>
        /// <param name="mod">The mod being checked</param>
        /// <param name="result">The result returned by <see cref="IUploadCheck.RunCheck(ModManager.Mod)"/></param>
        /// <returns>Whether to show the button</returns>
        public bool CanRunAction(ModManager.Mod mod, bool? result);

        /// <summary>
        /// Action to run. Running the action will cause a reevaluation of checks after completion.
        /// </summary>
        /// <param name="mod">The mod being checked</param>
        /// <param name="result">The result returned by <see cref="IUploadCheck.RunCheck(ModManager.Mod)"/></param>
        public void RunAction(ModManager.Mod mod, bool? result);
    }
}
