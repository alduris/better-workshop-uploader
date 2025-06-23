using System.Collections.Generic;
using BetterWorkshopUploader.Checks;
using Menu;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace BetterWorkshopUploader
{
    public class WorkshopTab(OptionInterface owner) : OpTab(owner, "Workshop")
    {
        private ModManager.Mod activeMod;
        private List<IUploadCheck> checks;

        private OpLabel label_name, label_id, label_version;
        private OpScrollBox sbox_tags, sbox_checks;

        public void Initialize()
        {
            // Create checks
            checks = [
                new TestCheck(),
                new TestActionCheck(),
                ];

            // Create elements
            OpLabel titleLabel;

            AddItems([
                // Title
                titleLabel = new OpLabel(new Vector2(10f, 570f), new Vector2(580f, 30f), "WORKSHOP UPLOADER", FLabelAlignment.Center, true),

                // Lines
                new OpImage(new Vector2(0f, 559f), "pixel") { scale = new Vector2(600f, 2f), color = MenuColorEffect.rgbMediumGrey },   // top border
                new OpImage(new Vector2(299f, 600f), "pixel") { scale = new Vector2(2f, 550f), color = MenuColorEffect.rgbMediumGrey }, // middle vertical border
                new OpImage(new Vector2(306f, 90f), "pixel") { scale = new Vector2(280f, 2f), color = MenuColorEffect.rgbMediumGrey },  // upload border

                // Metadata verification
                // Name label
                // ID label
                // Version label
                // Tags scrollbox

                // Checks
                sbox_checks = new OpScrollBox(new Vector2(310f, 100f), new Vector2(280f, 460f), 0f, false, false, true),

                // Upload section
                // force workshop id
                // visibility
                // update description
                // upload button
                ]);

            titleLabel.label.shader = Custom.rainWorld.Shaders["MenuText"]; // shiny appearance
        }

        public void FillInModInfo(ModManager.Mod mod)
        {
            activeMod = mod;
        }

        public void Update()
        {
            //
        }

        private void ActuallyShowUploadDialogue()
        {
            ModdingMenu.instance.DisplayWorkshopUploadConfirmDialog(activeMod);
        }
    }
}
