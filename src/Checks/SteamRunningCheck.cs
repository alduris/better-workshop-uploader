namespace BetterWorkshopUploader.Checks
{
    internal class SteamRunningCheck : IUploadCheck
    {
        public string Name => "Steam is running";

        public bool IsHiddenCheck => true;

        public bool? RunCheck(ModManager.Mod mod, BWUWorkshopData data)
        {
            return SteamManager.Initialized;
        }
    }
}
