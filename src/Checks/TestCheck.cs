namespace BetterWorkshopUploader.Checks
{
    /// <summary>
    /// Test case that only passes if the person running it is signed into Steam as Alduris
    /// </summary>
    internal class TestCheck : IUploadCheck
    {
        public string Name => "TEST";

        public bool IsHiddenCheck => false;

        public bool? RunCheck(ModManager.Mod mod, BWUWorkshopData data)
        {
            // Checks if the user is Alduris
            return RainWorldSteamManager.ownerUserID == 76561199088708941LU;
        }
    }
}
