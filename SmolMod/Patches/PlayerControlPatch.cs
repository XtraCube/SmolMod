using HarmonyLib;

namespace SmolMod.Patches;

[HarmonyPatch(typeof(PlayerControl),nameof(PlayerControl.Start))]
public static class PlayerControlPatch
{ 
    public static void Postfix(PlayerControl __instance)
    {
        __instance.UpdateSize();
    }
    
    public static void UpdateSize(this PlayerControl player)
    {
        player.cosmetics.SetScale(player.MyPhysics.Animations.DefaultPlayerScale, player.defaultCosmeticsScale);
    }
}