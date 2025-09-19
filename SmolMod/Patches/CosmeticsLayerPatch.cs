using System.Numerics;
using HarmonyLib;

namespace SmolMod.Patches;

[HarmonyPatch(typeof(CosmeticsLayer), nameof(CosmeticsLayer.SetScale))]
public static class CosmeticsLayerPatch
{
    public static void Prefix(ref Vector3 playerScale, ref Vector3 cosmeticsScale)
    {
        playerScale /= SmolModPlugin.ScaleMod;
        cosmeticsScale /= SmolModPlugin.ScaleMod;
    }
}