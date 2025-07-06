using BepInEx;
using BepInEx.Logging;
using Menu.Remix;
using System;
using System.Security.Permissions;

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

    public void OnEnable()
    {
        Logger = base.Logger;
        On.RainWorld.OnModsInit += RainWorld_OnModsInit; // menu crashes if we have it in OnEnable
    }

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        try
        {
            On.Menu.Remix.InternalOI_Stats.Initialize += InternalOI_Stats_Initialize;
            On.Menu.Remix.InternalOI_Stats.Update += InternalOI_Stats_Update;

            On.Menu.Remix.InternalOI_Stats.ShowDialogWorkshopUpload += InternalOI_Stats_ShowDialogWorkshopUpload;
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

            var tab = new WorkshopTab(self);
            self.Tabs[workshopTabIndex] = tab;
            tab.Initialize();
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

    private void InternalOI_Stats_ShowDialogWorkshopUpload(On.Menu.Remix.InternalOI_Stats.orig_ShowDialogWorkshopUpload orig, InternalOI_Stats self, Menu.Remix.MixedUI.UIfocusable trigger)
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
}
