using System.IO;
using Menu.Remix.MixedUI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BetterWorkshopUploader
{
    public static class Utils
    {
        public static void SaveModinfo(this ModManager.Mod mod)
        {
            // Do we need to make a new JSON object or is the user smart enough to have made their own modinfo
            JObject json;
            string path = Path.Combine(mod.basePath, "modinfo.json");
            if (File.Exists(path))
            {
                json = JObject.Parse(File.ReadAllText(path));
            }
            else
            {
                json = [];
            }

            // Add properties
            json["name"] = mod.name;
            //json["id"] = mod.id;
            json["version"] = mod.version;
            if (json.ContainsKey("target_game_version")) json["target_game_version"] = mod.targetGameVersion;
            json["requirements"] = new JArray(mod.requirements);
            json["requirements_names"] = new JArray(mod.requirementsNames);
            json["tags"] = new JArray(mod.tags);

            // Save
            File.WriteAllText(path, json.ToString(Formatting.Indented));
        }

        public static ulong GetValueULong(this OpTextBox element)
        {
            return ulong.TryParse(element.value, out ulong l) ? l : 0;
        }
    }
}
