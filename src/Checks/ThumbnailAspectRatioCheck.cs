using System.IO;
using UnityEngine;

namespace BetterWorkshopUploader.Checks
{
    public class ThumbnailAspectRatioCheck : IUploadCheck
    {
        public string Name => "Thumbnail 16:9 aspect ratio";

        public bool IsHiddenCheck => false;

        public bool? RunCheck(ModManager.Mod mod, BWUWorkshopData data)
        {
            var path = mod.GetThumbnailPath();
            if (File.Exists(path))
            {
                var thumb = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                AssetManager.SafeWWWLoadTexture(ref thumb, path, false, true);
                double ratio = (double)thumb.height / (double)thumb.width;
                try
                {
                    return ratio >= 0.5616 && ratio <= 0.5634;
                }
                finally
                {
                    Object.Destroy(thumb);
                }
            }
            return null;
        }
    }
}
