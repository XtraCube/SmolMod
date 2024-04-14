// Make the lobby bigger and adjust spawn positions

using HarmonyLib;

namespace SmolMod.Patches;

[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
public static class LobbySizePatch
{
    public static void Postfix(LobbyBehaviour __instance)
    {
        __instance.transform.AdjustScale(SmolModPlugin.ScaleMod).AdjustPosition(SmolModPlugin.ScaleMod);

        for (var i = 0; i < __instance.SpawnPositions.Count; i++)
        {
            __instance.SpawnPositions[i] *= SmolModPlugin.ScaleMod;
        }
    }
}