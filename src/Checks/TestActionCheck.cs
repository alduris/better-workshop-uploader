namespace BetterWorkshopUploader.Checks
{
    /// <summary>
    /// Test case that switches the check state when the button is pressed
    /// </summary>
    internal class TestActionCheck : IUploadCheckWithAction
    {
        public string Name { get; } = "TEST (with action)";
        public bool IsHiddenCheck => false;

        private int state = UnityEngine.Random.Range(0, 3);

        public bool? RunCheck(ModManager.Mod mod, BWUWorkshopData data)
        {
            // Depends on the state
            return state switch
            {
                0 => true,
                1 => false,
                _ => null,
            };
        }

        public string ActionText { get; } = "SWITCH";

        public bool CanRunAction(ModManager.Mod mod, BWUWorkshopData data, bool? result) => true;

        public void RunAction(ModManager.Mod mod, BWUWorkshopData data, bool? result)
        {
            // Switches what the check will return
            state = (state + 1) % 3;
        }
    }
}
