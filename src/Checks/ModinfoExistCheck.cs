using System.IO;

namespace BetterWorkshopUploader.Checks
{
    public class ModinfoExistCheck : IUploadCheck
    {
        public string Name => "Has modinfo.json";

        public bool IsHiddenCheck => true;

        public bool? RunCheck(ModManager.Mod mod, BWUWorkshopData data)
        {
            return File.Exists(Path.Combine(mod.basePath, "modinfo.json"));
        }
    }
}
