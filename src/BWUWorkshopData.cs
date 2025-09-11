using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BetterWorkshopUploader
{
    public class BWUWorkshopData
    {
        [JsonIgnore]
        internal string baseFolder;

        public ulong WorkshopID = 0L;
        public string LatestGameVersion;
        public bool UpdateTitle = true;
        public bool UpdateDescription = true;
        public bool MarkAsPublic = true;
        public HashSet<string> Tags;

        private BWUWorkshopData() { }

        private void UpdateData(ModManager.Mod mod)
        {
            // Fill in basic information if possible
            try
            {
                baseFolder = mod.basePath;

                Tags ??= [.. mod.tags];
                LatestGameVersion = Plugin.GameVersion;
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError(e);
                throw;
            }
        }

        private BWUWorkshopData(VigaroWorkshopData dataToAdapt)
        {
            baseFolder = dataToAdapt.baseFolder;

            WorkshopID = dataToAdapt.WorkshopID < 0 ? 0L : (ulong)dataToAdapt.WorkshopID;
            LatestGameVersion = dataToAdapt.TargetGameVersion;
            UpdateDescription = !dataToAdapt.UploadFilesOnly;
            UpdateTitle = !dataToAdapt.UploadFilesOnly;
            MarkAsPublic = dataToAdapt.Visibility == "Public";
            Tags = [.. dataToAdapt.Tags];
        }

        internal static string DataPath(string basePath) => Path.Combine(basePath, "bwu.json");
        internal static string DataPath(ModManager.Mod mod) => DataPath(mod.basePath);

        public static BWUWorkshopData FromMod(ModManager.Mod mod)
        {
            BWUWorkshopData data;
            if (!File.Exists(DataPath(mod)) && File.Exists(VigaroWorkshopData.DataPath(mod)))
            {
                data = new BWUWorkshopData(VigaroWorkshopData.FromMod(mod));
            }
            else if (File.Exists(DataPath(mod)))
            {
                data = JsonConvert.DeserializeObject<BWUWorkshopData>(File.ReadAllText(DataPath(mod)));
            }
            else
            {
                data = new BWUWorkshopData();
            }
            data.UpdateData(mod);
            return data;
        }

        public void Save()
        {
            JObject json = JObject.FromObject(this);
            File.WriteAllText(DataPath(baseFolder), json.ToString(Formatting.Indented));
            // File.WriteAllText(DataPath(baseFolder), JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
