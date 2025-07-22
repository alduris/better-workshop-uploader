using System;
using System.Collections.Generic;
using Steamworks;

namespace BetterWorkshopUploader
{
    public static class BWUSteamManager
    {
        public static bool? AcceptedEULA { get; private set; } = null;

        private static Dictionary<string, SteamUGCDetails_t?> modIdInfoCache = [];
        private static Dictionary<ulong, SteamUGCDetails_t?> workshopIdInfoCache = [];

        private static CallResult<WorkshopEULAStatus_t> eulaResult;
        private static CallResult<SteamUGCDetails_t> itemDetailsResult;

        internal static void Initialize()
        {
            if (SteamManager.Initialized)
            {
                eulaResult ??= new();
                itemDetailsResult ??= new();
                Refresh();
            }
        }

        public static void Refresh()
        {
            AcceptedEULA = null;
            RequestEULAStatus();
        }

        public static void RequestEULAStatus() => eulaResult.Set(SteamUGC.GetWorkshopEULAStatus(), CallResult_WorkshopEULAStatus);
        public static void RequestItemExists(ModManager.Mod mod)
        {
            throw new NotImplementedException();
        }
        public static void RequestItemOwnership(ulong fileID)
        {
            throw new NotImplementedException();
        }

        public static event Action ReceivedNewData;

        private static void CallResult_WorkshopEULAStatus(WorkshopEULAStatus_t status, bool ioFailure)
        {
            AcceptedEULA = false;
            if (!ioFailure && status.m_eResult == EResult.k_EResultOK)
            {
                AcceptedEULA = true;
            }
            else
            {
                Plugin.Logger.LogDebug("EULA result returned not OK: " + status.m_eResult + (ioFailure ? " (IO failure)" : ""));
            }
            ReceivedNewData?.Invoke();
        }
    }
}
