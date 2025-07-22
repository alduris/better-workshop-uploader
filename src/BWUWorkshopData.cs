using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BetterWorkshopUploader
{
    public class BWUWorkshopData
    {
        internal readonly string baseFolder;

        public string Name;
        public string ID;
        public string Version;
        public long WorkshopID;
        public string LatestGameVersion;
        public bool UpdateDescription;
        public List<string> Tags;
        internal long LastUpdate;

        internal bool AdaptedFromVigaro;

        public BWUWorkshopData(ModManager.Mod mod)
        {
            // Fill in basic information if possible
            try
            {
                baseFolder = mod.basePath;
                Dictionary<string, object> data = [];
                var path = Path.Combine(mod.basePath, "bwu.json");
                if (File.Exists(path))
                    data = (Dictionary<string, object>)Json.Deserialize(File.ReadAllText(path));

                Name = TryGetValue(nameof(Name), mod.name);
                ID = TryGetValue(nameof(ID), mod.id);
                Version = TryGetValue(nameof(Version), mod.version);
                WorkshopID = TryGetValue(nameof(WorkshopID), -1L);
                LatestGameVersion = TryGetValue(nameof(LatestGameVersion), mod.targetGameVersion ?? RainWorld.GAME_VERSION_STRING);
                UpdateDescription = TryGetValue(nameof(UpdateDescription), false);
                Tags = TryGetList(nameof(Tags), mod.tags?.ToList() ?? []);
                LastUpdate = TryGetValue(nameof(LastUpdate), Plugin.sessionId);

                AdaptedFromVigaro = TryGetValue(nameof(AdaptedFromVigaro), false);

                T TryGetValue<T>(string name, T defaultValue)
                {
                    return data.TryGetValue(name, out object boxedValue) && boxedValue is T parsedValue ? parsedValue : defaultValue;
                }
                List<T> TryGetList<T>(string name, List<T> defaultValue)
                {
                    return TryGetValue<List<object>>(name, null)?.Cast<T>().ToList() ?? defaultValue;
                }
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError(e);
                throw;
            }
        }

        internal BWUWorkshopData(VigaroWorkshopData dataToAdapt)
        {
            baseFolder = dataToAdapt.baseFolder;

            Name = dataToAdapt.Title;
            ID = dataToAdapt.ID;
            Version = dataToAdapt.Version;
            WorkshopID = dataToAdapt.WorkshopID;
            LatestGameVersion = dataToAdapt.TargetGameVersion;
            UpdateDescription = !dataToAdapt.UploadFilesOnly;
            Tags = [.. dataToAdapt.Tags];
            LastUpdate = Plugin.sessionId;
            AdaptedFromVigaro = true;
        }

        public bool FromCurrentSession => LastUpdate == Plugin.sessionId;

        public void Save()
        {
            Dictionary<string, object> data = [];
            data[nameof(Name)] = Name;
            data[nameof(ID)] = ID;
            data[nameof(WorkshopID)] = WorkshopID;
            data[nameof(LatestGameVersion)] = LatestGameVersion;
            data[nameof(UpdateDescription)] = UpdateDescription;
            data[nameof(Tags)] = Tags;
            data[nameof(LastUpdate)] = LastUpdate;

            if (AdaptedFromVigaro)
                data[nameof(AdaptedFromVigaro)] = AdaptedFromVigaro;

            throw new NotImplementedException("JSON saving is not implemented yet!");
        }
    }
}
