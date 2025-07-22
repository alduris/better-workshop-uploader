using System;
using System.IO;

namespace BetterWorkshopUploader.Checks
{
    public class ModVersionCheck : IUploadCheckWithAction
    {
        public string Name => "Mod version changed";

        public bool IsHiddenCheck => false;

        public bool? RunCheck(ModManager.Mod mod, BWUWorkshopData data)
        {
            if (!File.Exists(Path.Combine(mod.basePath, "modinfo.json"))) return null;
            return mod.version == data.LatestGameVersion || data.FromCurrentSession;
        }

        public string ActionText => "FIX";

        public bool CanRunAction(ModManager.Mod mod, BWUWorkshopData data, bool? result)
        {
            return result == false;
        }

        public void RunAction(ModManager.Mod mod, BWUWorkshopData data, bool? result)
        {
            try
            {
                var version = new Version(mod.version);
                version = new Version(version.Major, version.Minor, version.Build + 1);
                mod.version = version.ToString();
                data.Version = mod.version;
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError(ex);
            }
        }
    }
}
