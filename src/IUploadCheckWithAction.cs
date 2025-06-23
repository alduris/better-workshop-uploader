namespace BetterWorkshopUploader
{
    /// <summary>
    /// An upload check with a runnable action.
    /// </summary>
    internal interface IUploadCheckWithAction : IUploadCheck
    {
        /// <summary>
        /// Whether or not to allow the user to run the action. Checked when <see cref="IUploadCheck.RunCheck(ModManager.Mod)"/> is run.
        /// Action button will not appear unless true.
        /// </summary>
        public bool CanRunAction { get; }

        /// <summary>
        /// Action to run. Running the action will cause a reevaluation of checks after completion.
        /// </summary>
        public void RunAction();
    }
}
