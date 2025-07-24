using System;
using System.Reflection;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Logging;
using Menu.Remix;
using Menu.Remix.MixedUI;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Steamworks;
using UnityEngine;

// Allows access to private members
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace BetterWorkshopUploader;

[BepInPlugin("alduris.betterworkshop", "Better Workshop Uploader", "1.0")]
internal sealed class Plugin : BaseUnityPlugin
{
    public static new ManualLogSource Logger;
    public static int workshopTabIndex = -1;
    public static readonly long sessionId = DateTime.Now.Ticks;
    public static WorkshopTab workshopTabInstance;

    internal static readonly OpTextBox.Accept ACCEPT_ULONG = (OpTextBox.Accept)("BWU.ulong".GetHashCode());

    private static bool initialized = false;

    public void OnEnable()
    {
        Logger = base.Logger;
        On.RainWorld.OnModsInit += RainWorld_OnModsInit; // menu crashes if we have it in OnEnable
    }

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        if (initialized) return;
        initialized = true;
        try
        {
            // Run our menu instead of the normal dialogue
            On.Menu.Remix.InternalOI_Stats.Initialize += InternalOI_Stats_Initialize;
            On.Menu.Remix.InternalOI_Stats.Update += InternalOI_Stats_Update;

            On.Menu.Remix.InternalOI_Stats.ShowDialogWorkshopUpload += InternalOI_Stats_ShowDialogWorkshopUpload;

            // Remix menu element changes
            On.Menu.Remix.MixedUI.OpTextBox.NumberTypingClean += OpTextBox_NumberTypingClean;
            _ = new ILHook(typeof(OpTextBox).GetProperty(nameof(OpTextBox.value)).GetSetMethod(), OpTextBox_value_set);

            // Changes to the workshop upload script
            On.SteamWorkshopUploader.Update += SteamWorkshopUploader_Update;
            On.RainWorldSteamManager.OnCreateItemResult += RainWorldSteamManager_OnCreateItemResult;
            IL.RainWorldSteamManager.UploadWorkshopMod += RainWorldSteamManager_UploadWorkshopMod;
        }
        catch (Exception e)
        {
            Logger.LogError(e);
        }
    }

    private void InternalOI_Stats_Initialize(On.Menu.Remix.InternalOI_Stats.orig_Initialize orig, InternalOI_Stats self)
    {
        orig(self);

        if (SteamManager.Initialized)
        {
            workshopTabIndex = self.Tabs.Length;
            Array.Resize(ref self.Tabs, self.Tabs.Length + 1);

            workshopTabInstance = new WorkshopTab(self);
            self.Tabs[workshopTabIndex] = workshopTabInstance;
            workshopTabInstance.Initialize();
        }
    }

    private void InternalOI_Stats_Update(On.Menu.Remix.InternalOI_Stats.orig_Update orig, InternalOI_Stats self)
    {
        orig(self);

        if (SteamManager.Initialized && ConfigContainer.activeTab is WorkshopTab tab)
        {
            tab.Update();
        }
    }

    private void InternalOI_Stats_ShowDialogWorkshopUpload(On.Menu.Remix.InternalOI_Stats.orig_ShowDialogWorkshopUpload orig, InternalOI_Stats self, UIfocusable trigger)
    {
        if (SteamManager.Initialized)
        {
            var tab = self.Tabs[workshopTabIndex] as WorkshopTab;
            tab.FillInModInfo(self.previewMod);
            ConfigContainer._ChangeActiveTab(workshopTabIndex);
        }
        else
        {
            orig(self, trigger);
        }
    }

    private void OpTextBox_NumberTypingClean(On.Menu.Remix.MixedUI.OpTextBox.orig_NumberTypingClean orig, OpTextBox self)
    {
        if (self.accept == ACCEPT_ULONG)
        {
            if (self.value.Length < 1 || self.value.Contains("-"))
            {
                self.value = "0";
            }

            if (ulong.TryParse(self.value, out ulong l))
            {
                self.value = l.ToString();
            }
            else
            {
                self.value = "0";
            }
            Logger.LogDebug($"NEW VALUE AS ulong: {ulong.Parse(self.value)}");
        }
        else
        {
            orig(self);
        }
    }

    private void OpTextBox_value_set(ILContext il)
    {
        var c = new ILCursor(il);

        // Save the beginning so we can have a
        var begin = c.MarkLabel();

        // Run our custom logic
        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldarg_1);
        c.EmitDelegate((OpTextBox self, string value) =>
        {
            // Return value: whether or not the string `value` is suitable to be the new value string
            if (self.value == value) return false;

            // Check for whitespace
            char[] array = value.ToCharArray();
            for (int i = 0; i < array.Length; i++)
            {
                if (char.IsWhiteSpace(array[i]))
                {
                    return false;
                }
            }

            // Check if it parses okay (then reject if not)
            if (!ulong.TryParse(value, out _) && value.Length != 0)
                return false;

            // Trim to max length
            if (value.Length > self.maxLength)
            {
                value = value.Substring(0, self.maxLength);
                if (self.value == value)
                {
                    return false;
                }
            }

            // Play sound if necessary
            if (self._KeyboardOn && Input.anyKey && !Input.GetKey(KeyCode.Backspace))
            {
                self.PlaySound(SoundID.MENU_Checkbox_Uncheck);
            }

            // Accept string
            return true;
        });
        var acceptLabel = c.MarkLabel();

        // Call base.value = value;
        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldarg_1);
        c.Emit(OpCodes.Call, typeof(UIconfig).GetProperty(nameof(UIconfig.value)).GetSetMethod());

        // Emit return;
        c.Emit(OpCodes.Ret);
        var returnPos = c.Prev;
        var notULongPos = c.Next;

        // If our delegate returns false, just jump to return without calling base.value = value
        c.GotoLabel(acceptLabel);
        c.Emit(OpCodes.Brfalse, returnPos);

        // Now we add our logic at the very start to check if it's our ulong stuff to begin with.
        // Creates: if (this.accept != ACCEPT_ULONG) goto notULongPos;
        c.GotoLabel(begin);
        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate((OpTextBox self) => self.accept == ACCEPT_ULONG);
        c.Emit(OpCodes.Brfalse, notULongPos);
    }

    private void SteamWorkshopUploader_Update(On.SteamWorkshopUploader.orig_Update orig, SteamWorkshopUploader self)
    {
        var steamManager = self.menu.manager.mySteamManager;
        if (self.currentStep == SteamWorkshopUploader.UploadStep.CHECK_EXISTS && !steamManager.isCurrentlyQuerying && workshopTabInstance != null)
        {
            try
            {
                ulong id = workshopTabInstance.DesiredID;
                if (id >= 2920438669UL) // this is UwU mod by Henpemaz, which is the first publicly uploaded mod to the Workshop.
                {
                    steamManager.lastQueryOwners.Clear();
                    steamManager.lastQueryFiles.Clear();
                    steamManager.lastQueryOwners.Add(RainWorldSteamManager.ownerUserID);
                    steamManager.lastQueryFiles.Add(new(id));
                }
            }
            catch { }
        }
        orig(self);
    }

    private void RainWorldSteamManager_OnCreateItemResult(On.RainWorldSteamManager.orig_OnCreateItemResult orig, RainWorldSteamManager self, CreateItemResult_t callback, bool ioFailure)
    {
        if (callback.m_eResult == EResult.k_EResultOK)
        {
            workshopTabInstance.SetNewWorkshopID = callback.m_nPublishedFileId.m_PublishedFileId;
        }
        orig(self, callback, ioFailure);
    }

    private void RainWorldSteamManager_UploadWorkshopMod(ILContext il)
    {
        var c = new ILCursor(il);

        // Override public (since we're already doing an IL hook here)
        c.Emit(OpCodes.Ldarg_2);
        c.EmitDelegate((bool orig) => !workshopTabInstance.MarkAsPublic); // arg 2 is unlisted
        c.Emit(OpCodes.Starg, 2);

        // Don't replace description if we don't want to
        c.GotoNext(x => x.MatchCallOrCallvirt(typeof(SteamUGC).GetMethod(nameof(SteamUGC.SetItemDescription), BindingFlags.Static | BindingFlags.Public)));
        c.GotoNext(MoveType.After, x => x.MatchPop());
        var brTo = c.MarkLabel();
        c.Index--;
        c.GotoPrev(MoveType.AfterLabel, x => x.MatchLdarg(0), x => x.MatchLdfld<RainWorldSteamManager>(nameof(RainWorldSteamManager.updateHandle)));
        c.EmitDelegate(() => workshopTabInstance.UpdateDescription);
        c.Emit(OpCodes.Brfalse, brTo);
    }
}
