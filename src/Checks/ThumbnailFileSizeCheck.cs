using System;
using System.IO;

namespace BetterWorkshopUploader.Checks
{
    public class ThumbnailFileSizeCheck : IUploadCheck
    {
        public string Name => "Thumbnail file size under 1 MB";

        public bool IsHiddenCheck => false;

        public bool? RunCheck(ModManager.Mod mod, BWUWorkshopData data)
        {
            string original = mod.SteamThumbnailPath();
            if (string.IsNullOrEmpty(original) || !File.Exists(original)) return null;

            if (new FileInfo(original).Length >= 1_000_000L) return false;

            return true;
        }
    }
}
