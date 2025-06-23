namespace BetterWorkshopUploader.Checks
{
    internal class TestCheck : IUploadCheck
    {
        public string DisplayText => "TEST";

        public bool IsHiddenCheck => false;

        public bool? RunCheck(ModManager.Mod mod)
        {
            // Checks if the user is Alduris
            return RainWorldSteamManager.ownerUserID == 76561199088708941LU;
        }
    }
}
