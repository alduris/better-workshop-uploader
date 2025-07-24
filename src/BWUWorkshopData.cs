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

        public string Name;
        public string ID;
        public string Version;
        public ulong WorkshopID = 0L;
        public string LatestGameVersion;
        public bool UpdateDescription;
        public bool MarkAsPublic = true;
        public HashSet<string> Tags;
        internal long LastUpdate = Plugin.sessionId;

        [JsonIgnore]
        public bool FromCurrentSession => LastUpdate == Plugin.sessionId;

        private BWUWorkshopData() { }

        private void UpdateData(ModManager.Mod mod)
        {
            // Fill in basic information if possible
            try
            {
                baseFolder = mod.basePath;

                Name ??= mod.name;
                ID = mod.id;
                Version = mod.version;
                Tags ??= [.. mod.tags];
                LatestGameVersion = RainWorld.GAME_VERSION_STRING;
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

            Name = dataToAdapt.Title;
            ID = dataToAdapt.ID;
            Version = dataToAdapt.Version;
            WorkshopID = dataToAdapt.WorkshopID < 0 ? 0L : (ulong)dataToAdapt.WorkshopID;
            LatestGameVersion = dataToAdapt.TargetGameVersion;
            UpdateDescription = !dataToAdapt.UploadFilesOnly;
            MarkAsPublic = dataToAdapt.Visibility == "Public";
            Tags = [.. dataToAdapt.Tags];
            LastUpdate = Plugin.sessionId;
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
            LastUpdate = Plugin.sessionId;
            JObject json = JObject.FromObject(this);
            File.WriteAllText(DataPath(baseFolder), json.ToString(Formatting.Indented));
            // File.WriteAllText(DataPath(baseFolder), JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
