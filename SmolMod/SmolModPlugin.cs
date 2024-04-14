﻿using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SmolMod;

[BepInAutoPlugin("dev.xtracube.smolmod")]
[BepInProcess("Among Us.exe")]
public partial class SmolModPlugin : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);
    
    public static ConfigEntry<float> ConfigScale { get; private set; }
    
    public static float ScaleMod => ConfigScale.Value;
    
    public override void Load()
    {
        ConfigScale = Config.Bind("Scale", "GlobalModifier", 1.5f, "Scale modifier used for SmolMod scaling");
        Harmony.PatchAll();
    }

    // Multiply local player speed and max report distance by Scale Modifier
    // This works with the CustomNetworkTransform patches to add compatibility with users who don't have the mod
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
    public static class PlayerSpeedPatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            __instance.MaxReportDistance *= ScaleMod;
            __instance.MyPhysics.Speed *= ScaleMod;
        }
    }

    // Multiply light source distance by ScaleMod
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
    public static class PlayerLightPatch
    {
        public static void Postfix(ref float __result)
        {
            __result *= ScaleMod;
        }
    }

    // Make pets bigger to make the players look smaller
    [HarmonyPatch(typeof(PetBehaviour), nameof(PetBehaviour.Start))]
    public static class PetSizePatch
    {
        public static void Postfix(PetBehaviour __instance)
        {
            __instance.transform.AdjustScale(ScaleMod);
        }
    }

    // Make the lobby bigger and adjust spawn positions
    [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
    public static class LobbySizePatch
    {
        public static void Postfix(LobbyBehaviour __instance)
        {
            __instance.transform.AdjustScale(ScaleMod).AdjustPosition(ScaleMod);
            for (var i = 0; i < __instance.SpawnPositions.Count; i++)
            {
                __instance.SpawnPositions[i] *= ScaleMod;
            }
        }
    }
    
    // Adjust spawn positions on airship
    [HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.Begin))]
    public static class AirShipSpawnGamePatch
    {
        public static void Postfix(SpawnInMinigame __instance)
        {
            foreach (var location in __instance.Locations)
            {
                location.Location *= ScaleMod;
            }
        }
    }
    
    // Fix zipline locations
    [HarmonyPatch(typeof(ZiplineBehaviour), nameof(ZiplineBehaviour.Awake))]
    public static class ZiplinePatch
    {
        public static void Prefix(ZiplineBehaviour __instance)
        {
            __instance.landingPositionTop.position /= ScaleMod;
            __instance.landingPositionBottom.position /= ScaleMod;
        }
    }
    
    // Fix moving platform on Airship
    [HarmonyPatch(typeof(PlatformConsole), nameof(PlatformConsole.CanUse))]
    public static class PlatformConsolePatch
    {
        public static bool Prefix(PlatformConsole __instance, GameData.PlayerInfo pc, ref float __result, ref bool canUse, ref bool couldUse)
        {
            var num = float.MaxValue;
            var @object = pc.Object;
            couldUse = !pc.IsDead &&
                       @object.CanMove &&
                       !__instance.Platform.InUse &&
                       Vector2.Distance(__instance.Platform.transform.position, __instance.transform.position) < @object.MaxReportDistance;
            canUse = couldUse;
            if (canUse)
            {
                var truePosition = @object.GetTruePosition();
                var position = __instance.transform.position;
                num = Vector2.Distance(truePosition, position);
                canUse &= num <= __instance.UsableDistance && !PhysicsHelpers.AnythingBetween(truePosition, position, Constants.ShipOnlyMask, false);
            }
            __result = num;
            
            
            return false;
        }
    }
    
    
    
    
    // Adjust ShipStatus size, MapScale, Spawn Radius, Spawn Centers, and IUsables with usableDistance fields
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
    public static class ShipSizePatch
    {
        public static void Postfix(ShipStatus __instance)
        {
            __instance.transform.AdjustScale(ScaleMod).AdjustPosition(ScaleMod);
            __instance.MapScale *= ScaleMod;
            __instance.SpawnRadius *= ScaleMod;
            __instance.InitialSpawnCenter *= ScaleMod;
            __instance.MeetingSpawnCenter *= ScaleMod;
            __instance.MeetingSpawnCenter2 *= ScaleMod;

            foreach (var t in __instance.DummyLocations)
            {
                t.AdjustScale(ScaleMod).AdjustPosition(ScaleMod);
            }
            
            // feel free to find a better way to do this and PR it for me thx!
            foreach (var console in Object.FindObjectsOfType<Console>(true))
            {
                console.usableDistance *= ScaleMod;
            }

            foreach (var deconControl in Object.FindObjectsOfType<DeconControl>(true))
            {
                deconControl.usableDistance *= ScaleMod;
            }

            foreach (var mapConsole in Object.FindObjectsOfType<MapConsole>(true))
            {
                mapConsole.usableDistance *= ScaleMod;
            }

            foreach (var openDoorConsole in Object.FindObjectsOfType<OpenDoorConsole>(true))
            {
                openDoorConsole.usableDisance *= ScaleMod;
            }

            foreach (var platformConsole in Object.FindObjectsOfType<PlatformConsole>(true))
            {
                platformConsole.usableDistance *= ScaleMod;
            }

            foreach (var systemConsole in Object.FindObjectsOfType<SystemConsole>(true))
            {
                systemConsole.usableDistance *= ScaleMod;
            }

            foreach (var ziplineConsole in Object.FindObjectsOfType<ZiplineConsole>(true))
            {
                ziplineConsole.usableDistance *= ScaleMod;
            }
        }
    }
    
    // for some reason, using TargetMethods crashes the game.
    // patch UsableDistance getters for IUsables without a usableDistance field.
    [HarmonyPatch(typeof(DoorConsole), "UsableDistance", MethodType.Getter)]
    [HarmonyPatch(typeof(Ladder), "UsableDistance", MethodType.Getter)]
    [HarmonyPatch(typeof(OptionsConsole), "UsableDistance", MethodType.Getter)]
    [HarmonyPatch(typeof(Vent), "UsableDistance", MethodType.Getter)]
    public static class UsableDistancePatch
    {
        public static void Postfix(ref float __result)
        {
            __result *= ScaleMod;
        }
    }

    // Patch Vector2 network operations to support players without the mod
    [HarmonyPatch(typeof(NetHelpers))]
    public static class ReadAndWriteVectorPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(NetHelpers.WriteVector2))]
        public static void WriteVector2Prefix(ref Vector2 vec)
        {
            vec /= ScaleMod;
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(nameof(NetHelpers.ReadVector2))]
        public static void ReadVector2Prefix(ref Vector2 __result)
        {
            __result *= ScaleMod;
        }
    }
}
