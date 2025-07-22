using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace BetterWorkshopUploader
{
    /// <summary>
    /// Representation of Vigaro's workshopdata.json
    /// </summary>
    internal class VigaroWorkshopData
    {
        [JsonIgnore]
        internal string baseFolder;

        public string Title;
        public string Description;
        public string ID;
        public string Version;
        public string TargetGameVersion;
        public string Requirements;
        public string RequirementNames;
        public string Authors;
        public string Visibility;
        public List<string> Tags;
        public long WorkshopID = 0L;
        public bool UploadFilesOnly = false;
        public bool UploadThumbnail = true;

        private VigaroWorkshopData() { }

        private void UpdateData(ModManager.Mod mod)
        {
            try
            {
                baseFolder = mod.basePath;

                Title ??= mod.name ?? "UNKNOWN";
                Description ??= mod.description ?? "UNKNOWN";
                ID ??= mod.id ?? "UNKNOWN";
                Version ??= mod.version ?? "0.0";
                TargetGameVersion ??= RainWorld.GAME_VERSION_STRING;
                Requirements ??= string.Join(", ", mod.requirements);
                RequirementNames ??= string.Join(", ", mod.requirementsNames);
                Authors ??= mod.authors ?? "";
                Visibility ??= "Unlisted";
                Tags ??= mod.tags?.ToList() ?? [];
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError(e);
                throw;
            }
        }

        internal static string DataPath(ModManager.Mod mod) => Path.Combine(mod.basePath, "workshopdata.json");
        public static VigaroWorkshopData FromMod(ModManager.Mod mod)
        {
            VigaroWorkshopData data;
            if (File.Exists(DataPath(mod)))
            {
                data = JsonConvert.DeserializeObject<VigaroWorkshopData>(File.ReadAllText(DataPath(mod)));
            }
            else
            {
                data = new VigaroWorkshopData();
            }
            data.UpdateData(mod);
            return data;
        }
    }
}
