using System;
using Steamworks;

namespace BetterWorkshopUploader.Checks
{
    internal class SteamEULACheck : IUploadCheckWithAction
    {
        public SteamEULACheck()
        {
            BWUSteamManager.ReceivedNewData += BWUSteamManager_ReceivedNewData;
        }

        private void BWUSteamManager_ReceivedNewData()
        {
            assumeAccepted = false;
        }

        private bool assumeAccepted = false;

        public string Name => "Has accepted Steam EULA";

        public bool IsHiddenCheck => false;

        public bool? RunCheck(ModManager.Mod mod, BWUWorkshopData data)
        {
            if (assumeAccepted) return true;
            if (!SteamManager.Initialized) return null;
            return BWUSteamManager.AcceptedEULA;
        }

        public string ActionText => "OPEN";

        public bool CanRunAction(ModManager.Mod mod, BWUWorkshopData data, bool? result)
        {
            return result == false;
        }

        public void RunAction(ModManager.Mod mod, BWUWorkshopData data, bool? result)
        {
            assumeAccepted = true;
            SteamFriends.ActivateGameOverlayToWebPage("https://steamcommunity.com/sharedfiles/workshoplegalagreement", EActivateGameOverlayToWebPageMode.k_EActivateGameOverlayToWebPageMode_Default);
        }
    }
}
