using System.IO;

namespace BetterWorkshopUploader.Checks
{
    public class ThumbnailFileSizeCheck : IUploadCheck
    {
        public string Name => "Thumbnail file size under 1 MB";

        public bool IsHiddenCheck => false;

        public bool? RunCheck(ModManager.Mod mod, BWUWorkshopData data)
        {
            var path = mod.GetThumbnailPath();
            if (File.Exists(path))
            {
                return new FileInfo(path).Length < 1000000L;
            }
            return null;
        }
    }
}
