using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MonHunCollabRestored.Character;
using MonHunCollabRestored.Beambullet;
using Tangerine.Manager.Mod;
using BepInEx.Configuration;

namespace MonHunCollabRestored;

[BepInDependency(Tangerine.Plugin.GUID, BepInDependency.DependencyFlags.HardDependency)]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : TangerinePlugin
{
    private TangerineMod _tangerine = null;
    private static Harmony _harmony;
    internal static new ManualLogSource Log;

    internal static new ConfigFile Config;
    internal static ConfigEntry<bool> CH093_OnlineTTSMovement { get; set; }

    public override void Load(TangerineMod tangerine)
    {
        _tangerine = tangerine;

        // Plugin startup logic
        Plugin.Log = base.Log;
        Plugin.Config = base.Config;
        Log.LogInfo($"Tangerine plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        BeamBullet_Hooks.InitializeHarmony(_harmony);

        InitializeConfig();
        RestoreCharacters();
        
    }

    private void InitializeConfig()
    {
        CH093_OnlineTTSMovement = Config.Bind("Rathalos Armor X", "Online Movement", true, new ConfigDescription("Restore True Charge Slash to original Online movement"));
    }

    private void RestoreCharacters()
    {
        _tangerine.Character.AddController(72, typeof(CH091_Controller));  // Hunter R (Kamura)
        _tangerine.Character.AddController(71, typeof(CH092_Controller));  // Hunter V (Sinister)
        _tangerine.Character.AddController(73, typeof(CH093_Controller));  // Rathalos Armor X
        _tangerine.Character.AddController(101, typeof(CH106_Controller)); // Crimson Valstrax Zero
        _tangerine.Character.AddController(102, typeof(CH107_Controller)); // Zinogre Iris
        _tangerine.Character.AddController(126, typeof(CH129_Controller)); // Shagaru Armor X
        _tangerine.Character.AddController(127, typeof(CH130_Controller)); // Gore iCO
    }
}
