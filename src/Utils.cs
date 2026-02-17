using System;
using System.IO;
using Menu.Remix.MixedUI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Steamworks;

namespace BetterWorkshopUploader
{
    public static class Utils
    {
        public static string SteamThumbnailPath(this ModManager.Mod mod, bool forceAsSteamPng = false)
        {
            var baseName = Path.Combine(mod.basePath, "thumbnail-steam");
            if (forceAsSteamPng || File.Exists(baseName + ".png"))
                return baseName + ".png";
            if (File.Exists(baseName + ".jpg"))
                return baseName + ".jpg";
            if (File.Exists(baseName + ".jpeg")) // because apparently both are valid :rolling_eyes:
                return baseName + ".jpeg";
            if (File.Exists(baseName + ".gif"))
                return baseName + ".gif";

            return mod.GetThumbnailPath();
        }

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

        public static ERemoteStoragePublishedFileVisibility ToSteamVisibility(this Visibility val)
        {
            return val switch
            {
                Visibility.Public => ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic,
                Visibility.Unlisted or Visibility.DontChange => ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityUnlisted,
                Visibility.FriendsOnly => ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityFriendsOnly,
                Visibility.Private => ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate,
                _ => throw new InvalidOperationException()
            };
        }
    }
}
