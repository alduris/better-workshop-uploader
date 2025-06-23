namespace BetterWorkshopUploader.Checks
{
    internal class TestActionCheck : IUploadCheckWithAction
    {
        public bool CanRunAction => true;

        public string DisplayText => "TEST (with action)";

        public bool IsHiddenCheck => false;

        private int state = UnityEngine.Random.Range(0, 3);

        public void RunAction()
        {
            // Switches what the check will return
            state = (state + 1) % 3;
        }

        public bool? RunCheck(ModManager.Mod mod)
        {
            // Depends on the state
            return state switch
            {
                0 => true,
                1 => false,
                _ => null,
            };
        }
    }
}
