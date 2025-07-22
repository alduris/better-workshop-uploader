using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BetterWorkshopUploader.Checks
{
    public class ModRequirementsCheck : IUploadCheckWithAction
    {
        public string Name => "Mod requirements match names";

        public bool IsHiddenCheck => false;

        public bool? RunCheck(ModManager.Mod mod, BWUWorkshopData data)
        {
            // Edge cases
            if (!File.Exists(Path.Combine(mod.basePath, "modinfo.json"))) return null;
            if (mod.requirements.Length != mod.requirementsNames.Length) return false;
            if (mod.requirements.Length == 0) return null;

            // Check requirements match
            for (int i = 0; i < mod.requirements.Length; i++)
            {
                var details = ModManager.ActiveMods.FirstOrDefault(x => x.id == mod.requirements[i]);
                if (details == null || details.name != mod.requirementsNames[i])
                {
                    return false;
                }
            }

            return true;
        }

        public string ActionText => "FIX";

        public bool CanRunAction(ModManager.Mod mod, BWUWorkshopData data, bool? result)
        {
            return result == false && !data.FromCurrentSession;
        }

        public void RunAction(ModManager.Mod mod, BWUWorkshopData data, bool? result)
        {
            Dictionary<string, string> modMap = ModManager.ActiveMods.ToDictionary(x => x.id, y => y.name);
            mod.requirementsNames = [.. mod.requirements.Select(x => modMap[x])];
        }
    }
}
