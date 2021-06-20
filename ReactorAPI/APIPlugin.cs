using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using ReactorAPI.UI;
using ReactorAPI.Options;
using static ReactorAPI.UI.ColourStorer;
using Reactor;
using UnityEngine;
using HarmonyLib;

namespace ReactorAPI
{
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id, BepInDependency.DependencyFlags.HardDependency)]
    [ReactorPluginSide(PluginSide.Both)]
    public partial class APIPlugin : BasePlugin
    {
        public const string Id = "com.reactor.api";

        public static APIPlugin Instance { get { return PluginSingleton<APIPlugin>.Instance; } }

        internal static ManualLogSource Logger { get { return Instance.Log; } }

        internal Harmony Harmony { get; } = new Harmony(Id);

        public override void Load()
        {
            Harmony.PatchAll();
            HudPosition.Load();
        }

    }

    [HarmonyPatch(typeof(VersionShower), "Start")]
    public static class VersionPatch
    {
        static void Postfix(VersionShower __instance)
        {
            var obj = new GameObject();
            foreach (GameObject gameObj in Object.FindObjectsOfType<GameObject>())
                if (gameObj.name.StartsWith("ReactorVersion"))
                    obj = gameObj;
            if (obj != null) GameObject.Destroy(obj);
            string text = $"\nv2021.6.15s  |  {Cyan}Reactor{Closer} {Purple}v1.0.0{Closer}\n{ColourStorer.ReactorAPI} {Purple}v1.0.0{Closer}";
            __instance.text.text = text;

        }

        public static readonly CustomOptionHeader Main = CustomOption.AddHeader(ColourStorer.ReactorAPI + "\nby " + SteamUnlocked21, true, false);
    }
}