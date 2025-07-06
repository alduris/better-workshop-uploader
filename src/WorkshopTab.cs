using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BetterWorkshopUploader.Checks;
using Menu;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;
using LabelVAlignment = Menu.Remix.MixedUI.OpLabel.LabelVAlignment;

namespace BetterWorkshopUploader
{
    public class WorkshopTab(OptionInterface owner) : OpTab(owner, "Workshop")
    {
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
        private List<IUploadCheck> checks;

        private OpLabel label_name, label_id, label_version;
        private OpScrollBox sbox_tags, sbox_checks;
        private OpCheckBox cbox_update;

        private FileSystemWatcher modWatcher;

        public void Initialize()
        {
            // Create checks
            checks = [
                new TestCheck(),
                new TestActionCheck(),
                ];

            // Check for stuff we want to know about
            // TODO: workshop stuff (if exists, is a contributor, etc)

            // Create elements
            OpLabel titleLabel;

            AddItems([
                // Title
                titleLabel = new OpLabel(new Vector2(10f, 570f), new Vector2(580f, 30f), "WORKSHOP UPLOADER", FLabelAlignment.Center, true),

                // Lines
                new OpImage(new Vector2(0f, 559f), "pixel") { scale = new Vector2(600f, 2f), color = MenuColorEffect.rgbMediumGrey },   // top border
                new OpImage(new Vector2(299f, 0f), "pixel") { scale = new Vector2(2f, 550f), color = MenuColorEffect.rgbMediumGrey }, // middle vertical border
                new OpImage(new Vector2(306f, 90f), "pixel") { scale = new Vector2(280f, 2f), color = MenuColorEffect.rgbMediumGrey },  // upload border

                // Metadata verification
                label_name = new OpLabel(new Vector2(10f, 520f), new Vector2(280f, 30f), "NAME HERE") { verticalAlignment = LabelVAlignment.Center },
                label_id = new OpLabel(new Vector2(10f, 490f), new Vector2(280f, 30f), "ID HERE") { verticalAlignment = LabelVAlignment.Center },
                label_version = new OpLabel(new Vector2(10f, 460f), new Vector2(280f, 30f), "VERSION HERE") { verticalAlignment = LabelVAlignment.Center },
                cbox_update = new OpCheckBox(new Configurable<bool>(false), new Vector2(10f, 433f)),
                new OpLabel(new Vector2(40f, 430f), new Vector2(250f, 30f), "Update workshop description", FLabelAlignment.Left) { verticalAlignment = LabelVAlignment.Center },
                sbox_tags = new OpScrollBox(new Vector2(10f, 10f), new Vector2(280f, 410f), 0f, false, false, true),

                // Checks
                sbox_checks = new OpScrollBox(new Vector2(310f, 100f), new Vector2(280f, 460f), 0f, false, false, true),

                // Upload section
                // force workshop id
                // visibility
                // upload button
                ]);

            titleLabel.label.shader = Custom.rainWorld.Shaders["MenuText"]; // shiny appearance
        }

        public void FillInModInfo(ModManager.Mod mod)
        {
            activeMod = mod;
            modWatcher?.Dispose();

            // Fill in mod info
            label_name.text = Translate(mod.name); // only this needs to be translated
            label_id.text = mod.id;
            label_version.text = mod.version;
            UpdateTags();

            // Other stuff
            RunChecks();

            // Watch the mod's files for updates
            modWatcher = new FileSystemWatcher(mod.basePath);
            modWatcher.Changed += ModWatcher_Changed;
            modWatcher.Created += ModWatcher_Created;
            modWatcher.Deleted += ModWatcher_Deleted;
            modWatcher.Renamed += ModWatcher_Renamed;
            modWatcher.Error += ModWatcher_Error;
            modWatcher.NotifyFilter = NotifyFilters.Attributes
                                    | NotifyFilters.CreationTime
                                    | NotifyFilters.DirectoryName
                                    | NotifyFilters.FileName
                                    | NotifyFilters.LastWrite
                                    | NotifyFilters.Security
                                    | NotifyFilters.Size;
            modWatcher.IncludeSubdirectories = false;
            modWatcher.EnableRaisingEvents = true;
        }

        public void Update()
        {
            //
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
#warning Implement!
                        throw new NotImplementedException("Tag toggle not implemented!");
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
#warning Implement!
                    throw new NotImplementedException("Tag remove not implemented!");
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
#warning Implement!
                    if (DefaultTags.Contains(addTextbox.value, StringComparer.OrdinalIgnoreCase))
                    {
                        throw new NotImplementedException("Default tag text toggle not implemented!");
                    }
                    else
                    {
                        throw new NotImplementedException("Custom tag adding not implemented");
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
            // Clear out old elements
            foreach (var item in sbox_checks.items)
            {
                item.Deactivate();
                _RemoveItem(item);
            }
            sbox_checks.items.Clear();
            sbox_checks.SetContentSize(0f);

            // Run checks
            float boxWidth = sbox_checks.size.x - 30f; // 20f for scrollbar + 10f margin
            float y = sbox_checks.size.y - 40f; // 30f for height of label + 10f margin
            sbox_checks.AddItems(new OpLabel(0f, y, "CHECKS", true));
            y -= 6f;
            foreach (var check in checks)
            {
                bool? result = check.RunCheck(activeMod);

                if (result == false || !check.IsHiddenCheck)
                {
                    // Setup
                    var action = check as IUploadCheckWithAction;
                    float height = action is not null && action.CanRunAction(activeMod, result) ? 30f : 20f;
                    float buttonWidth = action is not null ? Mathf.Max(24f, LabelTest.GetWidth(action.ActionText, false) + 9f) : 0f;
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
                        verticalAlignment = OpLabel.LabelVAlignment.Center
                    };
                    var label_checkResult = new OpLabel(new Vector2(0f, y), new Vector2(boxWidth - buttonWidth - 6f, height), resultString, FLabelAlignment.Right, false)
                    {
                        color = result switch
                        {
                            true => new Color(0f, 0.8f, 0f),
                            false => new Color(1f, 0.1f, 0.1f),
                            null => MenuColorEffect.rgbDarkGrey,
                        },
                        verticalAlignment = OpLabel.LabelVAlignment.Center
                    };

                    sbox_checks.AddItems(label_checkName, label_checkResult);

                    if (action is not null)
                    {
                        var button = new OpSimpleButton(new Vector2(boxWidth - buttonWidth, y + 3f), new Vector2(buttonWidth, 24f), action.ActionText);
                        button.OnClick += (_) =>
                        {
                            action.RunAction(activeMod, result);
                            RunChecks();
                        };
                        sbox_checks.AddItems(button);
                    }
                }
            }

            // Maybe more stuff here in the future (workshop verification?)

            // Fix size
            sbox_checks.SetContentSize(sbox_checks.size.y - y + 10f);
        }

        private void ActuallyShowUploadDialogue()
        {
            ModdingMenu.instance.DisplayWorkshopUploadConfirmDialog(activeMod);
        }

        public string Translate(string text) => Custom.rainWorld.inGameTranslator.TryTranslate(text, out var translated) ? translated : text;

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void ModWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.Name != "bwudata.json")
                RunChecks();
        }

        private void ModWatcher_Created(object sender, FileSystemEventArgs e)
        {
            if (e.Name != "bwudata.json")
                RunChecks();
        }

        private void ModWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            RunChecks();
        }

        private void ModWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            RunChecks();
        }

        private void ModWatcher_Error(object sender, ErrorEventArgs e)
        {
            Plugin.Logger.LogError(e.GetException());
        }
    }
}
