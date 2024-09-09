using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.GravityHelperGV;

public class GravityHelperGVModule : EverestModule
{
    public static GravityHelperGVModule Instance { get; private set; }

    public override Type SettingsType => typeof(GravityHelperGVModuleSettings);
    public static GravityHelperGVModuleSettings Settings => (GravityHelperGVModuleSettings)Instance._Settings;

    public override Type SessionType => typeof(GravityHelperGVModuleSession);
    public static GravityHelperGVModuleSession Session => (GravityHelperGVModuleSession)Instance._Session;

    public override Type SaveDataType => typeof(GravityHelperGVModuleSaveData);
    public static GravityHelperGVModuleSaveData SaveData => (GravityHelperGVModuleSaveData)Instance._SaveData;

    public GravityHelperGVModule()
    {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(GravityHelperGVModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(GravityHelperGVModule), LogLevel.Info);
#endif
    }

    public override void Load()
    {
        On.Celeste.LevelLoader.ctor += LevelLoad;
        On.Celeste.OverworldLoader.ctor += LevelUnload;
    }
    public override void Unload()
    {
        On.Celeste.LevelLoader.ctor -= LevelLoad;
        On.Celeste.OverworldLoader.ctor -= LevelUnload;
    }


    static bool hooksLoaded = false;
    private static void LevelLoad(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startPosition)
    {
        orig(self, session, startPosition);
        if (session.MapData.Levels.Any(level => level.Entities.Any(data => data.Name == "GravityHelperGV/GravityArrow")))
        {
            if(!hooksLoaded) {
                hooksLoaded = true;
                LoadHooks();
            }
        }
    }
    private static void LevelUnload(On.Celeste.OverworldLoader.orig_ctor orig, OverworldLoader self, Overworld.StartMode startMode, HiresSnow snow)
    {
        orig(self, startMode, snow);
        if (hooksLoaded)
        {
            UnloadHooks();
            hooksLoaded = false;
        }
    }

    public static void LoadHooks()
    {
        Logger.Info("GHGV", "Loading hooks");
        GravityComponent.SetHooks();
        ControlHooks.Load();
        PlayerHooks.Load();
        PlayerHairHooks.Load();
        SpringHooks.Load();
        SolidHooks.Load();
        MoonBlockHooks.Load();
        MiscHooks.Load();
        HoldableHooks.Load();
        BadelineChaserHooks.Load();
    }
    public static void UnloadHooks()
    {
        Logger.Info("GHGV", "Unloading hooks");
        GravityComponent.RemoveHooks();
        ControlHooks.Unload();
        PlayerHooks.Unload();
        PlayerHairHooks.Unload();
        SpringHooks.Unload();
        SolidHooks.Unload();
        MoonBlockHooks.Unload();
        MiscHooks.Unload();
        HoldableHooks.Unload();
        BadelineChaserHooks.Unload();
    }
}
