using HarmonyLib;
using UnityEngine;

namespace SmolMod.Patches;

public static class AirShipPatches
{
    // Adjust spawn positions on airship
    [HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.Begin))]
    public static class AirShipSpawnGamePatch
    {
        public static void Postfix(SpawnInMinigame __instance)
        {
            foreach (var location in __instance.Locations)
            {
                location.Location *= SmolModPlugin.ScaleMod;
            }
        }
    }
    
    // Replace hardcoded 3f with scaled value
    [HarmonyPatch(typeof(MovingPlatformBehaviour), nameof(MovingPlatformBehaviour.Use), typeof(PlayerControl))]
    public static class MovingPlatformPatch
    {
        public static bool Prefix(MovingPlatformBehaviour __instance, PlayerControl player)
        {
            var vector = __instance.transform.position - player.transform.position;
            if (player.Data.IsDead || player.Data.Disconnected)
            {
                return false;
            }
            if (__instance.Target || vector.magnitude > 3f * SmolModPlugin.ScaleMod)
            {
                return false;
            }
            __instance.IsDirty = true;
            __instance.StartCoroutine(__instance.UsePlatform(player));

            return false;
        }
    }
    
    // Replace Platform CanUse hardcoded 2f with scaled value
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
                       Vector2.Distance(__instance.Platform.transform.position, __instance.transform.position) < 2f * SmolModPlugin.ScaleMod;
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
}