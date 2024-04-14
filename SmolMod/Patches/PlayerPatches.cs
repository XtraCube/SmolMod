using HarmonyLib;

namespace SmolMod.Patches;

public static class PlayerPatches
{
    // Multiply local player speed and max report distance by Scale Modifier
    // This works with the CustomNetworkTransform patches to add compatibility with users who don't have the mod
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
    public static class PlayerReportPatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            __instance.MaxReportDistance *= SmolModPlugin.ScaleMod;
            __instance.MyPhysics.Speed *= SmolModPlugin.ScaleMod;
        }
    }

    // Multiply light source distance by ScaleMod
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
    public static class PlayerLightPatch
    {
        public static void Postfix(ref float __result)
        {
            __result *= SmolModPlugin.ScaleMod;
        }
    }

    // Make pets bigger to make the players look smaller
    [HarmonyPatch(typeof(PetBehaviour), nameof(PetBehaviour.Start))]
    public static class PetSizePatch
    {
        public static void Postfix(PetBehaviour __instance)
        {
            __instance.transform.AdjustScale(SmolModPlugin.ScaleMod);
        }
    }
}