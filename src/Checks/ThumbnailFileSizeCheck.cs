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
            string steam = null;

            if (!string.IsNullOrEmpty(original) && original.EndsWith("thumbnail.png", StringComparison.InvariantCultureIgnoreCase))
            {
                steam = original.Replace("thumbnail.png", "thumbnail-steam.png");
            }

            if (!string.IsNullOrEmpty(steam) && File.Exists(steam))
            {
                return new FileInfo(steam).Length < 1_000_000L;
            }

            if (!string.IsNullOrEmpty(original) && File.Exists(original))
            {
                return new FileInfo(original).Length < 1_000_000L;
            }

            return null;
        }
    }
}
