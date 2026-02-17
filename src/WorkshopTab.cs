using System;
using System.Collections.Generic;
using System.Linq;
using BetterWorkshopUploader.Checks;
using Menu;
using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using RWCustom;
using UnityEngine;
using LabelVAlignment = Menu.Remix.MixedUI.OpLabel.LabelVAlignment;

namespace BetterWorkshopUploader
{
    public class WorkshopTab(OptionInterface owner) : OpTab(owner, "Workshop")
    {
        private const int CHECK_FREQUENCY = 80;

        private static readonly List<string> DefaultTags = [
            "Arenas",
            "Regions",
            "Campaigns",
            "Creatures",
            "Game Mechanics",
            "Items",
            "Cosmetics",
            "Game Modes",
            "Dependency",
            "Accessibility",
            "Translations",
            "Tools",
            "Custom Slugcat",
            "Base",
            "Downpour",
            "Watcher"
            ];

        private ModManager.Mod activeMod;
        internal BWUWorkshopData activeData;
        private List<IUploadCheck> checks;

        private OpLabel label_name, label_id, label_version;
        private OpScrollBox sbox_tags, sbox_checks;
        private OpCheckBox cbox_updatedescr, cbox_updatetitle, cbox_updatedeps;
        private OpHoldButton button_upload;
        private OpTextBox input_id;
        private OpResourceSelector combo_visibility;

        private int checkCountdown = CHECK_FREQUENCY;

        internal ulong DesiredID => activeData.WorkshopID;
        internal bool UpdateTitle
        {
            get => activeData.UpdateTitle;
            set
            {
                activeData.UpdateTitle = value;
                cbox_updatetitle.SetValueBool(value);
            }
        }

        internal bool UpdateDescription
        {
            get => activeData.UpdateDescription;
            set
            {
                activeData.UpdateDescription = value;
                cbox_updatedescr.SetValueBool(value);
            }
        }
        internal bool UpdateDependencies
        {
            get => cbox_updatedeps.GetValueBool();
            set => cbox_updatedeps.SetValueBool(value);
        }

        internal Visibility MarkedVisibility
        {
            get => (Visibility)Enum.Parse(typeof(Visibility), combo_visibility.value);
            set
            {
                if (value != activeData.Visibility)
                {
                    activeData.Visibility = value;
                    combo_visibility.value = value.ToString();
                }
            }
        }

        internal ulong SetNewWorkshopID
        {
            set
            {
                activeData.WorkshopID = value;
                input_id.value = value.ToString();
                activeData.Save();
            }
        }

        public void Initialize()
        {
            // Create checks
            checks = [
                new SteamRunningCheck(),
                new ThumbnailAspectRatioCheck(),
                new ThumbnailFileSizeCheck(),
                new SteamThumbnailCheck(),
                new ModinfoExistCheck(),
                new ValidJsonCheck(),
                new ModRequirementsCheck(),
                new ModTargetVersionCheck(),
                ];

            // Create elements
            OpLabel titleLabel;

            AddItems([
                // Title
                titleLabel = new OpLabel(new Vector2(10f, 570f), new Vector2(580f, 30f), Translate("WORKSHOP UPLOADER"), FLabelAlignment.Center, true),

                // Lines
                new OpImage(new Vector2(0f, 559f), "pixel") { scale = new Vector2(600f, 2f), color = MenuColorEffect.rgbMediumGrey },   // top border
                new OpImage(new Vector2(299f, 0f), "pixel") { scale = new Vector2(2f, 550f), color = MenuColorEffect.rgbMediumGrey },   // middle vertical border
                new OpImage(new Vector2(306f, 239f), "pixel") { scale = new Vector2(292f, 2f), color = MenuColorEffect.rgbMediumGrey }, // upload border

                // Metadata verification
                label_name = new OpLabel(new Vector2(10f, 520f), new Vector2(280f, 30f), Translate("NAME HERE")) { verticalAlignment = LabelVAlignment.Center },
                label_id = new OpLabel(new Vector2(10f, 490f), new Vector2(280f, 30f), Translate("ID HERE")) { verticalAlignment = LabelVAlignment.Center },
                label_version = new OpLabel(new Vector2(10f, 460f), new Vector2(280f, 30f), Translate("VERSION HERE")) { verticalAlignment = LabelVAlignment.Center },
                sbox_tags = new OpScrollBox(new Vector2(10f, 10f), new Vector2(280f, 440f), 0f, false, false, true),

                // Checks
                sbox_checks = new OpScrollBox(new Vector2(310f, 300f), new Vector2(280f, 260f), 0f, false, false, false),

                // Upload section
                new OpLabel(new Vector2(310f, 200f), new Vector2(0f, 30f), Translate("WORKSHOP INFO"), FLabelAlignment.Left, true) { verticalAlignment = LabelVAlignment.Center },
                input_id = new OpTextBox(new Configurable<string>("0"), new Vector2(430f, 163f), 160f) { accept = Plugin.ACCEPT_ULONG },
                new OpLabel(new Vector2(310f, 160f), new Vector2(0f, 30f), Translate("Workshop ID:"), FLabelAlignment.Left, false) { bumpBehav = input_id.bumpBehav },
                cbox_updatetitle = new OpCheckBox(new Configurable<bool>(false), new Vector2(564f, 133f)),
                new OpLabel(new Vector2(310f, 130f), new Vector2(0f, 30f), Translate("Update workshop title:"), FLabelAlignment.Left) { verticalAlignment = LabelVAlignment.Center, bumpBehav = cbox_updatetitle.bumpBehav },
                cbox_updatedescr = new OpCheckBox(new Configurable<bool>(false), new Vector2(564f, 103f)),
                new OpLabel(new Vector2(310f, 100f), new Vector2(0f, 30f), Translate("Update workshop description:"), FLabelAlignment.Left) { verticalAlignment = LabelVAlignment.Center, bumpBehav = cbox_updatedescr.bumpBehav },
                // 100f
                cbox_updatedeps = new OpCheckBox(new Configurable<bool>(false), new Vector2(564f, 73f)),
                new OpLabel(new Vector2(310f, 70f), new Vector2(0f, 30f), "Update workshop dependencies:", FLabelAlignment.Left) { verticalAlignment = LabelVAlignment.Center, bumpBehav= cbox_updatedeps.bumpBehav },
                combo_visibility = new OpResourceSelector(new Configurable<Visibility>(Visibility.DontChange), new Vector2(470f, 43f), 120f),
                new OpLabel(new Vector2(310f, 40f), new Vector2(0f, 30f), Translate("Set item visibility:"), FLabelAlignment.Left) { verticalAlignment = LabelVAlignment.Center, bumpBehav = combo_visibility.bumpBehav },
                button_upload = new OpHoldButton(new Vector2(400f, 10f), new Vector2(100f, 24f), Translate("UPLOAD"), 40) { colorEdge = new Color(0.7f, 0.85f, 1f) }
                ]);

            titleLabel.label.shader = Custom.rainWorld.Shaders["MenuText"]; // shiny appearance

            cbox_updatetitle.OnChange += UpdateTitleCheckbox_OnChange;
            cbox_updatedescr.OnChange += UpdateDescriptionCheckbox_OnChange;
            combo_visibility.OnChange += VisibilityComboBox_OnChange;
            input_id.OnChange += IdInput_OnChange;

            button_upload.OnPressDone += UploadButton_OnPressDone;
        }

        public void FillInModInfo(ModManager.Mod mod)
        {
            activeMod = mod;
            activeData = BWUWorkshopData.FromMod(activeMod);

            // Fill in mod info
            label_name.text = Translate(mod.name); // only this needs to be translated
            label_id.text = mod.id;
            label_version.text = mod.version;
            UpdateTags();
            sbox_tags.ScrollToTop();

            input_id.value = activeData.WorkshopID.ToString();
            if (mod.workshopId > 0 && activeData.WorkshopID == 0)
            {
                activeData.WorkshopID = mod.workshopId;
                input_id.value = activeData.WorkshopID.ToString();
            }
            else
            {
                mod.workshopId = activeData.WorkshopID;
            }

            cbox_updatetitle.SetValueBool(activeData.UpdateTitle);
            cbox_updatedescr.SetValueBool(activeData.UpdateDescription);
            cbox_updatedeps.SetValueBool(activeData.WorkshopID == 0);
            combo_visibility.value = activeData.Visibility.ToString();

            // Other stuff
            RunChecks();
            activeData.Save();
        }

        public void Update()
        {
            // Secret bump version functionality
            if (label_version.MouseOver)
            {
                int tick = Input.GetMouseButtonDown(0) ? 1 : (Input.GetMouseButtonDown(1) ? -1 : 0);
                if (tick != 0)
                {
                    var version = new Version(activeMod.version);
                    if (tick < 0)
                    {
                        int major = version.Major;
                        int minor = version.Minor;
                        int build = version.Build;

                        if (build <= 0)
                        {
                            if (minor <= 0)
                            {
                                if (major > 0)
                                {
                                    major += tick;
                                }
                            }
                            else
                            {
                                minor += tick;
                            }
                        }
                        else
                        {
                            build += tick;
                        }
                        version = new Version(major, minor, build);
                    }
                    else
                    {
                        version = new Version(version.Major, version.Minor, version.Build + tick);
                    }
                    activeMod.version = version.ToString();
                    activeMod.SaveModinfo();
                    activeData.Save();

                    label_version.text = version.ToString();
                }
            }

            // Run checks
            checkCountdown--;
            if (checkCountdown <= 0)
            {
                RunChecks();
                checkCountdown = CHECK_FREQUENCY;
            }
        }

        private void UpdateTags()
        {
            // Clear out old elements
            foreach (var item in sbox_tags.items)
            {
                item.Deactivate();
                _RemoveItem(item);
            }
            sbox_tags.items.Clear();
            sbox_tags.SetContentSize(0f);
            float y = sbox_tags.size.y - (10f - 6f);

            // Add default tags
            HashSet<string> defaultTagsAdded = [.. DefaultTags.Intersect(activeMod.tags, StringComparer.OrdinalIgnoreCase)];
            foreach (string tag in DefaultTags)
            {
                y -= 30f;
                var cbox = new OpCheckBox(new Configurable<bool>(defaultTagsAdded.Contains(tag)), new Vector2(10f, y));
                var label = new OpLabel(new Vector2(40f, y), new Vector2(0f, 24f), tag, FLabelAlignment.Left, false) { verticalAlignment = LabelVAlignment.Center };

                cbox.OnValueChanged += (_, v, o) =>
                {
                    if (v != o)
                    {
                        if (cbox.GetValueBool())
                        {
                            activeData.Tags.Add(tag);
                        }
                        else
                        {
                            activeData.Tags.Remove(tag);
                        }
                        activeMod.tags = [.. activeData.Tags];
                        activeData.Save();
                        activeMod.SaveModinfo();
                    }
                };

                sbox_tags.AddItems(cbox, label);
            }

            // Add custom tags
            List<string> customTagsAdded = [.. activeMod.tags.Except(DefaultTags, StringComparer.OrdinalIgnoreCase)];
            foreach (string tag in customTagsAdded)
            {
                y -= 30f;
                var button = new OpSimpleImageButton(new Vector2(10f, y), new Vector2(24f, 24f), "Menu_Symbol_Clear_All") { colorEdge = new Color(1f, 0.7f, 0.7f) };
                var label = new OpLabel(new Vector2(40f, y), new Vector2(0f, 24f), tag, FLabelAlignment.Left, false) { verticalAlignment = LabelVAlignment.Center };

                button.OnClick += (_) =>
                {
                    activeData.Tags.Remove(tag);
                    activeMod.tags = [.. activeData.Tags];
                    activeData.Save();
                    activeMod.SaveModinfo();
                    UpdateTags();
                };

                sbox_tags.AddItems(button, label);
            }

            // Tag adder
            y -= 30f;
            var addButton = new OpSimpleImageButton(new Vector2(10f, y), new Vector2(24f, 24f), "Menu_Symbol_CheckBox") { colorEdge = new Color(0.7f, 1f, 0.7f) };
            var addTextbox = new OpTextBox(new Configurable<string>(""), new Vector2(40f, y), sbox_tags.size.x - 70f);

            addButton.OnClick += (_) =>
            {
                if (addTextbox.value != null && addTextbox.value.Length > 0)
                {
                    // Check that it isn't already added
                    if (!activeData.Tags.Any(x => x.Equals(addTextbox.value, StringComparison.OrdinalIgnoreCase)))
                    {
                        string tagToAdd = addTextbox.value;
                        if (DefaultTags.Contains(addTextbox.value, StringComparer.OrdinalIgnoreCase))
                        {
                            // If it's a default tag, change it to match the case
                            tagToAdd = DefaultTags[DefaultTags.FindIndex(x => x.Equals(tagToAdd, StringComparison.OrdinalIgnoreCase))];
                        }
                        activeData.Tags.Add(tagToAdd);
                        activeMod.tags = [.. activeData.Tags];
                        activeData.Save();
                        activeMod.SaveModinfo();
                        UpdateTags();
                    }
                    addTextbox.value = "";
                }
            };

            sbox_tags.AddItems(addButton, addTextbox);

            // Set size
            sbox_tags.SetContentSize(sbox_tags.size.y - y + 10f, true);
        }

        private void RunChecks()
        {
            // Importante
            float oldHeight = sbox_checks.contentSize;
            float oldScroll = sbox_checks.scrollOffset;

            // Clear out old elements
            foreach (var item in sbox_checks.items)
            {
                item.Deactivate();
                _RemoveItem(item);
            }
            sbox_checks.items.Clear();
            sbox_checks.SetContentSize(0f);

            // Run checks
            float boxWidth = sbox_checks.size.x - 10f;
            float y = sbox_checks.size.y - 40f; // 30f for height of label + 10f margin
            sbox_checks.AddItems(new OpLabel(0f, y, "CHECKS", true));
            y -= 6f;
            foreach (var check in checks)
            {
                bool? result = check.RunCheck(activeMod, activeData);

                if (result == false || !check.IsHiddenCheck)
                {
                    // Setup
                    var action = check as IUploadCheckWithAction;
                    bool canRunAction = action?.CanRunAction(activeMod, activeData, result) ?? false;
                    float height = action is not null && canRunAction ? 36f : 24f;
                    float buttonWidth = action is not null && canRunAction ? Mathf.Max(24f, LabelTest.GetWidth(action.ActionText, false) + 9f) : 0f;
                    y -= height;

                    // Create labels
                    string resultString = result switch
                    {
                        true => "Pass",
                        false => "Fail",
                        _ => "N/A"
                    };
                    var label_checkName = new OpLabel(new Vector2(0f, y), new Vector2(boxWidth - buttonWidth - 6f, height), check.Name, FLabelAlignment.Left, false)
                    {
                        verticalAlignment = LabelVAlignment.Center
                    };
                    var label_checkResult = new OpLabel(new Vector2(0f, y), new Vector2(boxWidth - buttonWidth - 6f, height), resultString, FLabelAlignment.Right, false)
                    {
                        color = result switch
                        {
                            true => new Color(0f, 0.8f, 0f),
                            false => new Color(1f, 0.1f, 0.1f),
                            null => MenuColorEffect.rgbDarkGrey,
                        },
                        verticalAlignment = LabelVAlignment.Center
                    };

                    sbox_checks.AddItems(label_checkName, label_checkResult);

                    if (action is not null && canRunAction)
                    {
                        var button = new OpSimpleButton(new Vector2(boxWidth - buttonWidth, y + height / 2f - 12f), new Vector2(buttonWidth, 24f), action.ActionText);
                        button.OnClick += (_) =>
                        {
                            action.RunAction(activeMod, activeData, result);
                            RunChecks();
                        };
                        label_checkName.bumpBehav = button.bumpBehav;
                        sbox_checks.AddItems(button);
                    }
                }
            }

            // Maybe more stuff here in the future (workshop verification?)

            // Fix size
            sbox_checks.SetContentSize(sbox_checks.size.y - y + 10f);

            // Do not flicker
            sbox_checks.SetContentSize(sbox_checks.size.y - y + 20f, true);
            sbox_checks.ScrollOffset = oldScroll + (oldHeight - sbox_checks.contentSize);
            foreach (UIelement element in sbox_checks.items)
            {
                element.lastScreenPos = element.pos;
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void UpdateTitleCheckbox_OnChange()
        {
            activeData.UpdateTitle = cbox_updatetitle.GetValueBool();
            activeData.Save();
        }

        private void UpdateDescriptionCheckbox_OnChange()
        {
            activeData.UpdateDescription = cbox_updatedescr.GetValueBool();
            activeData.Save();
        }

        private void VisibilityComboBox_OnChange()
        {
            activeData.Visibility = Enum.TryParse<Visibility>(combo_visibility.value, out var v) ? v : Visibility.DontChange;
            activeData.Save();
        }
        
        private void IdInput_OnChange()
        {
            if (input_id.GetValueULong() != activeData.WorkshopID)
            {
                activeData.WorkshopID = input_id.GetValueULong();
                activeData.Save();
            }
        }

        private void UploadButton_OnPressDone(UIfocusable trigger)
        {
            trigger.held = false;
            ActuallyShowUploadDialogue();
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void ActuallyShowUploadDialogue()
        {
            ModdingMenu.instance.DisplayWorkshopUploadConfirmDialog(activeMod);
        }

        public string Translate(string text) => Custom.rainWorld.inGameTranslator.TryTranslate(text, out var translated) ? translated : text;
    }
}
