using HarmonyLib;
using UnityEngine;

namespace SmolMod.Patches;

[HarmonyPatch(typeof(PlayerControl),nameof(PlayerControl.Start))]
public static class PlayerControlPatch
{ 
    // adapted from https://github.com/Among-Us-Modding/Laboratory/blob/main/Laboratory/Player/SizeModifier.cs
    private static readonly Vector3 DefaultSize = new (0.7f, 0.7f, 1f);
    
    public static void Postfix(PlayerControl __instance)
    {
        __instance.UpdateSize();
    }
    
    public static void UpdateSize(this PlayerControl player)
    {
        var size = DefaultSize / SmolModPlugin.ScaleMod;

        player.Collider.Cast<CircleCollider2D>().radius = 0.2233912f / (size.x / DefaultSize.x);
        player.transform.localScale = size;
    }
}