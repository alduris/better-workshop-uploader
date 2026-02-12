using System.IO;
using Menu.Remix;
using UnityEngine;

namespace BetterWorkshopUploader.Checks
{
    internal class SteamThumbnailCheck : IUploadCheckWithAction
    {
        private const string DIALOG_TEXT = "This action will copy your existing thumbnail to thumbnail-steam.png and then resize the original thumbnail to be " +
            "426x240, which reduces lag when loading the Remix menu. Press CONTINUE if you are okay with Better Workshop Uploader performing this action.";

        public string Name => "Separate Steam thumbnail";

        public bool IsHiddenCheck => false;

        public string ActionText => "COPY";

        public bool? RunCheck(ModManager.Mod mod, BWUWorkshopData data)
        {
            if (!File.Exists(mod.GetThumbnailPath()))
            {
                return null;
            }

            return mod.GetThumbnailPath() != mod.SteamThumbnailPath();
        }

        public bool CanRunAction(ModManager.Mod mod, BWUWorkshopData data, bool? result)
        {
            return result is false;
        }

        public void RunAction(ModManager.Mod mod, BWUWorkshopData data, bool? result)
        {
            if (result is null) return;

            ConfigConnector.CreateDialogBoxMultibutton(DIALOG_TEXT, ["CONTINUE", "CANCEL"], [() => CopyAndResize(mod), () => { }]);
        }

        private void CopyAndResize(ModManager.Mod mod)
        {
            // Get orig thumbnail and resize texture
            var resizedTex = new Texture2D(0, 0, TextureFormat.ARGB32, false);
            resizedTex.LoadImage(File.ReadAllBytes(mod.GetThumbnailPath()));
            bool overwrite = false;
            if (resizedTex.width != 426 && resizedTex.height != 240)
            {
                TextureScale.Bilinear(resizedTex, 426, 240);
                overwrite = true;
            }

            // Copy old thumbnail to Steam version
            File.Copy(mod.GetThumbnailPath(), mod.SteamThumbnailPath(true), true);

            // Overwrite old thumbnail
            if (overwrite)
            {
                File.WriteAllBytes(mod.GetThumbnailPath(), resizedTex.EncodeToPNG());
            }

            // Destroy texture object
            Object.Destroy(resizedTex);
        }
    }
}
