using System;
using System.IO;

namespace BetterWorkshopUploader.Checks
{
    public class ThumbnailFileSizeCheck : IUploadCheck
    {
        public string Name => "Thumbnail file sizes under 1 MB";

        public bool IsHiddenCheck => false;

        public bool? RunCheck(ModManager.Mod mod, BWUWorkshopData data)
        {
            string original = mod.GetThumbnailPath();
            if (string.IsNullOrEmpty(original) || !File.Exists(original)) return null;

            if (new FileInfo(original).Length >= 1_000_000L) return false;

            if (original.EndsWith("thumbnail.png", StringComparison.InvariantCultureIgnoreCase))
            {
                string steam = original.Replace("thumbnail.png", "thumbnail-steam.png");

                if (File.Exists(steam) && new FileInfo(steam).Length >= 1_000_000L) return false;
            }
            return true;
        }
    }
}
