using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BetterWorkshopUploader
{
    /// <summary>
    /// Representation of Vigaro's workshopdata.json
    /// </summary>
    internal class VigaroWorkshopData
    {
        internal readonly string baseFolder;
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
        public long WorkshopID;
        public bool UploadFilesOnly;
        public bool UploadThumbnail;

        public VigaroWorkshopData(ModManager.Mod mod)
        {
            try
            {
                var data = (Dictionary<string, object>)Json.Deserialize(File.ReadAllText(Path.Combine(mod.basePath, "workshopdata.json")));
                baseFolder = mod.basePath;

                if (data.ContainsKey(nameof(Title)))             Title             = (string)data[nameof(Title)]             ?? mod.name              ?? "UNKNOWN";
                if (data.ContainsKey(nameof(Description)))       Description       = (string)data[nameof(Description)]       ?? mod.description       ?? "UNKNOWN";
                if (data.ContainsKey(nameof(ID)))                ID                = (string)data[nameof(ID)]                ?? mod.id                ?? "UNKNOWN";
                if (data.ContainsKey(nameof(Version)))           Version           = (string)data[nameof(Version)]           ?? mod.version           ?? "0.0";
                if (data.ContainsKey(nameof(TargetGameVersion))) TargetGameVersion = (string)data[nameof(TargetGameVersion)] ?? mod.targetGameVersion ?? RainWorld.GAME_VERSION_STRING;
                if (data.ContainsKey(nameof(Requirements)))      Requirements      = (string)data[nameof(Requirements)]      ?? string.Join(", ", mod.requirements);
                if (data.ContainsKey(nameof(RequirementNames)))  RequirementNames  = (string)data[nameof(RequirementNames)]  ?? string.Join(", ", mod.requirementsNames);
                if (data.ContainsKey(nameof(Authors)))           Authors           = (string)data[nameof(Authors)]           ?? mod.authors ?? "";
                if (data.ContainsKey(nameof(Visibility)))        Visibility        = (string)data[nameof(Visibility)]        ?? "Unlisted";
                if (data.ContainsKey(nameof(Tags)))              Tags              = ((List<object>)data[nameof(Tags)])?.Cast<string>().Where(x => x != null).ToList() ?? [];
                if (data.ContainsKey(nameof(WorkshopID)))        WorkshopID        = data[nameof(WorkshopID)] is long l ? l : 0;
                if (data.ContainsKey(nameof(UploadFilesOnly)))   UploadFilesOnly   = data[nameof(UploadFilesOnly)] is bool b1 && b1;
                if (data.ContainsKey(nameof(UploadThumbnail)))   UploadThumbnail   = data[nameof(UploadThumbnail)] is bool b2 && b2;
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError(e);
                throw;
            }
        }
    }
}
