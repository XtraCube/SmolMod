using HarmonyLib;

namespace SmolMod.Patches;

// Fix zipline locations
[HarmonyPatch(typeof(ZiplineBehaviour), nameof(ZiplineBehaviour.Awake))]
public static class ZiplinePatch
{
    public static void Prefix(ZiplineBehaviour __instance)
    {
        __instance.landingPositionTop.position /= SmolModPlugin.ScaleMod;
        __instance.landingPositionBottom.position /= SmolModPlugin.ScaleMod;
    }
}