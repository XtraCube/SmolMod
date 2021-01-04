using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using UnityEngine;
using Reactor;
using System.Linq;

namespace SmolMod
{
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    [ReactorPluginSide(PluginSide.Client)]
    public class SmolMod : BasePlugin
    {
        public const string Id = "com.xtracube.smol";
        public Harmony Harmony { get; } = new Harmony(Id);

        public static float playerSize = 2f;
        public static float mapScale = 7.4f;
        public static float petSize = 1.2f;
        public static float iconSize = 0.6f;

        public override void Load()
        {
            Harmony.PatchAll();
        }             

        [HarmonyPatch(typeof(PlayerControl),nameof(PlayerControl.FixedUpdate))]
        public static class PlayerControlFixedUpdatePatch
        {
            public static void Postfix( PlayerControl __instance)
            {
                if (__instance.CurrentPet.transform.localScale.x < petSize)
                {
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    {
                        player.CurrentPet.transform.localScale = new Vector3(petSize, petSize, player.CurrentPet.transform.localScale.z);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
        public static class ShipStatusAwakePatch
        {
            public static void Prefix(ShipStatus __instance)
            {
                SetMapSize(__instance);
            }
        }

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Begin))]
        public static class ShipStatusBeginPatch
        {
            public static void Prefix(ShipStatus __instance)
            {
                SetMapSize(__instance);
            }
        }

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.FixedUpdate))]
        public static class MapSizePatch
        {
            public static void Postfix(ShipStatus __instance)
            {
                if (__instance.MapScale < mapScale)
                {
                    SetMapSize(__instance);
                }
            }
        }    
        
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
        public static class MeetingHudPlayerSizePatch
        {
            public static void Postfix(MeetingHud __instance)
            {
                foreach (var playerVoteArea in __instance.playerStates)
                {
                    playerVoteArea.PlayerIcon.transform.localScale = new Vector3(iconSize, iconSize, playerVoteArea.PlayerIcon.transform.localScale.z);
                }
            }
        }        

        [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.FixedUpdate))]
        public static class MapBehaviourUpdatePatch
        {
            public static void Postfix(MapBehaviour __instance)
            {
                __instance.HerePoint.transform.localScale = new Vector3(iconSize, iconSize, __instance.HerePoint.transform.localScale.z);
            }
        }

        public static void SetMapSize(ShipStatus __instance)
        {
            __instance.transform.localScale = new Vector3(2, 2, 2);
            __instance.SpawnRadius = 3.1f;
            SetDistances();
            switch (__instance.Type)
            {
                case ShipStatus.Nested_0.Ship:
                    __instance.MapScale = mapScale;
                    return;

                case ShipStatus.Nested_0.Pb:
                    __instance.InitialSpawnCenter = new Vector2(33.325f, -5.140f);
                    __instance.MeetingSpawnCenter = new Vector2(35.584f, -32.949f);
                    __instance.MeetingSpawnCenter2 = new Vector2(35.584f, -35.34f);
                    __instance.MapScale = mapScale + 2.6f;
                    return;
                    
                case ShipStatus.Nested_0.Hq:
                    __instance.InitialSpawnCenter = new Vector2(-8.961f, 4.133f);
                    __instance.MeetingSpawnCenter = new Vector2(50f, 2.985f);
                    __instance.MeetingSpawnCenter2 = new Vector2(50f, 2.985f);
                    __instance.MapScale = mapScale + 3.4f;
                    return;
            }
        }
        public static void SetDistances()
        {
            var useMod = 1.6f;            
            try
            {
                foreach (var body in Object.FindObjectsOfType<Console>().ToArray())
                {
                    body.usableDistance = useMod;
                }
                foreach (var body in Object.FindObjectsOfType<GEAHFGDIGIC>().ToArray())
                {
                    body.usableDistance = useMod;
                }
                foreach (var body in Object.FindObjectsOfType<SystemConsole>().ToArray())
                {
                    body.usableDistance = 3f;
                }
                foreach (var body in Object.FindObjectsOfType<DeconControl>().ToArray())
                {
                    body.usableDistance = useMod;
                }
            }
            catch { }
        }

    }
}

