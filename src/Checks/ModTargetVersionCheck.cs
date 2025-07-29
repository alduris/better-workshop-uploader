using System.IO;
using Newtonsoft.Json.Linq;

namespace BetterWorkshopUploader.Checks
{
    public class ModTargetVersionCheck : IUploadCheckWithAction
    {
        public string Name => "Target version matches game";

        public bool IsHiddenCheck => false;

        public bool? RunCheck(ModManager.Mod mod, BWUWorkshopData data)
        {
            var path = Path.Combine(mod.basePath, "modinfo.json");
            if (!File.Exists(path)) return null;
            var json = JObject.Parse(File.ReadAllText(path));
            if (!json.ContainsKey("target_game_version")) return null;
            return mod.targetGameVersion == Plugin.GameVersion;
        }

        public string ActionText => "FIX";

        public bool CanRunAction(ModManager.Mod mod, BWUWorkshopData data, bool? result)
        {
            return result == false;
        }

        public void RunAction(ModManager.Mod mod, BWUWorkshopData data, bool? result)
        {
            mod.targetGameVersion = Plugin.GameVersion;
            data.LatestGameVersion = Plugin.GameVersion;
            mod.SaveModinfo();
            data.Save();
        }
    }
}
