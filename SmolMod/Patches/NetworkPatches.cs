using HarmonyLib;
using UnityEngine;

namespace SmolMod.Patches;

// Patch Vector2 network operations to support players without the mod
[HarmonyPatch(typeof(NetHelpers))]
public static class NetworkPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(NetHelpers.WriteVector2))]
    public static void WriteVector2Prefix(ref Vector2 vec)
    {
        vec /= SmolModPlugin.ScaleMod;
    }
        
    [HarmonyPostfix]
    [HarmonyPatch(nameof(NetHelpers.ReadVector2))]
    public static void ReadVector2Prefix(ref Vector2 __result)
    {
        __result *= SmolModPlugin.ScaleMod;
    }
}