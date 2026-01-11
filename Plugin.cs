using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace MarkysSuitDrinkSystem
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log;
        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;

            _harmony ??= new Harmony(PluginInfo.PLUGIN_GUID);
            _harmony.PatchAll();

            Log.LogInfo("MarkysSuitDrinkSystem loaded.");
        }

        private void OnDestroy()
        {
            _harmony.UnpatchSelf();

            Log.LogInfo("MarkysSuitDrinkSystem unloaded.");
        }
    }

    //[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    //public class Plugin : BaseUnityPlugin
    //{
    //    public static ManualLogSource Log;
    //    private Harmony _harmony;

    //    private void Awake()
    //    {
    //        Log = Logger;

    //        _harmony ??= new Harmony(PluginInfo.PLUGIN_GUID);
    //        _harmony.PatchAll();

    //        Log.LogInfo("MarkysSuitDrinkSystem loaded from SE.");
    //    }

    //    private void OnDestroy()
    //    {
    //        _harmony.UnpatchSelf();

    //        Log.LogInfo("MarkysSuitDrinkSystem unloaded from SE.");
    //    }
    //}
}