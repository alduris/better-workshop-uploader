using System;
using System.Diagnostics;
using System.IO;
using Menu.Remix;
using Newtonsoft.Json.Linq;

namespace BetterWorkshopUploader.Checks
{
    public class ValidJsonCheck : IUploadCheckWithAction
    {
        private const string DIALOG_TEXT = "Your modinfo.json has invalid syntax! The game's--for lack of better words--shitty parser doesn't care about that, " +
            "but it can be problematic. The most likely cause is you forgot to put a comma at the end of a line, or perhaps you added an extra comma where there " +
            "shouldn't have been.";
        public string Name => "Valid syntax in modinfo.json";

        public bool IsHiddenCheck => false;

        public string ActionText => "HELP";

        public bool? RunCheck(ModManager.Mod mod, BWUWorkshopData data)
        {
            var path = Path.Combine(mod.basePath, "modinfo.json");
            if (!File.Exists(path)) return null;
            try
            {
                _ = JObject.Parse(File.ReadAllText(path));
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e);
                UnityEngine.Debug.LogException(e);
                Plugin.Logger.LogError(e);
                return false;
            }
        }

        public bool CanRunAction(ModManager.Mod mod, BWUWorkshopData data, bool? result)
        {
            return result == false;
        }

        public void RunAction(ModManager.Mod mod, BWUWorkshopData data, bool? result)
        {
            ConfigConnector.CreateDialogBoxMultibutton(DIALOG_TEXT, ["OPEN VALIDATOR", "DISMISS"], [OpenValidator, () => { }]);
        }

        private static void OpenValidator()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://odu.github.io/slingjsonlint/",
                UseShellExecute = true
            });
        }
    }
}
