using HarmonyLib;
using UnityEngine;

namespace SmolMod.Patches;

public static class ShipPatches
{
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
            __result *= SmolModPlugin.ScaleMod;
        }
    }
        
    // Adjust ShipStatus size, MapScale, Spawn Radius, Spawn Centers, and IUsables with usableDistance fields
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
    public static class ShipSizePatch
    {
        public static void Postfix(ShipStatus __instance)
        {
            var scaleMod = SmolModPlugin.ScaleMod;
            
            __instance.transform.AdjustScale(scaleMod).AdjustPosition(scaleMod);
            __instance.MapScale *= scaleMod;
            __instance.SpawnRadius *= scaleMod;
            __instance.InitialSpawnCenter *= scaleMod;
            __instance.MeetingSpawnCenter *= scaleMod;
            __instance.MeetingSpawnCenter2 *= scaleMod;

            foreach (var t in __instance.DummyLocations)
            {
                t.AdjustScale(scaleMod).AdjustPosition(scaleMod);
            }
            
            // feel free to find a better way to do this and PR it for me thx!
            foreach (var console in Object.FindObjectsOfType<Console>(true))
            {
                console.usableDistance *= scaleMod;
            }

            foreach (var deconControl in Object.FindObjectsOfType<DeconControl>(true))
            {
                deconControl.usableDistance *= scaleMod;
            }

            foreach (var mapConsole in Object.FindObjectsOfType<MapConsole>(true))
            {
                mapConsole.usableDistance *= scaleMod;
            }

            foreach (var openDoorConsole in Object.FindObjectsOfType<OpenDoorConsole>(true))
            {
                openDoorConsole.usableDisance *= scaleMod;
            }

            foreach (var platformConsole in Object.FindObjectsOfType<PlatformConsole>(true))
            {
                platformConsole.usableDistance *= scaleMod;
            }

            foreach (var systemConsole in Object.FindObjectsOfType<SystemConsole>(true))
            {
                systemConsole.usableDistance *= scaleMod;
            }

            foreach (var ziplineConsole in Object.FindObjectsOfType<ZiplineConsole>(true))
            {
                ziplineConsole.usableDistance *= scaleMod;
            }
        }
    }
}
