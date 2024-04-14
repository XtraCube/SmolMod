using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Hazel;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using UnityEngine;

namespace SmolMod;

[BepInAutoPlugin("dev.xtracube.smolmod")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[ReactorModFlags(ModFlags.None)]
public partial class SmolModPlugin : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);

    public ConfigEntry<float> ConfigScale { get; private set; }
    
    public static float ScaleMod => PluginSingleton<SmolModPlugin>.Instance.ConfigScale.Value;
    
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

            foreach (var console in Object.FindObjectsOfType<Console>())
            {
                console.usableDistance *= ScaleMod;
            }

            foreach (var deconControl in Object.FindObjectsOfType<DeconControl>())
            {
                deconControl.usableDistance *= ScaleMod;
            }

            foreach (var mapConsole in Object.FindObjectsOfType<MapConsole>())
            {
                mapConsole.usableDistance *= ScaleMod;
            }

            foreach (var openDoorConsole in Object.FindObjectsOfType<OpenDoorConsole>())
            {
                openDoorConsole.usableDisance *= ScaleMod;
            }

            foreach (var platformConsole in Object.FindObjectsOfType<PlatformConsole>())
            {
                platformConsole.usableDistance *= ScaleMod;
            }

            foreach (var systemConsole in Object.FindObjectsOfType<SystemConsole>())
            {
                systemConsole.usableDistance *= ScaleMod;
            }

            foreach (var ziplineConsole in Object.FindObjectsOfType<ZiplineConsole>())
            {
                ziplineConsole.usableDistance *= ScaleMod;
            }
        }
    }
    
    // patch UsableDistance getters for IUsables without a usableDistance field
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

    // Patch CustomNetworkTransform to add compatibility to those without the mod
    [HarmonyPatch(typeof(CustomNetworkTransform))]
    public static class CustomNetTransformPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(CustomNetworkTransform.Serialize))]
        public static bool SerializePrefix(CustomNetworkTransform __instance, MessageWriter writer, bool initialState, ref bool __result)
        {
            if (__instance.isPaused)
            { 
                __result = false;
                return false;
            }
            if (initialState)
            {
                writer.Write(__instance.lastSequenceId);
                NetHelpers.WriteVector2(__instance.body.position / ScaleMod, writer);
                __result = true;
                return false;
            }
            if (!__instance.isActiveAndEnabled)
            {
                __instance.ClearDirtyBits();
                __result = false;
                return false;
            }
            if (__instance.sendQueue.Count == 0)
            {
                __result = false;
                return false;
            }
            __instance.lastSequenceId += 1;
            writer.Write(__instance.lastSequenceId);
            var num = (ushort)__instance.sendQueue.Count;
            writer.WritePacked(num);
            foreach (var vector in __instance.sendQueue)
            {
                NetHelpers.WriteVector2(vector/ScaleMod, writer);
                __instance.lastPosSent = vector/ScaleMod;
            }
            __instance.sendQueue.Clear();
            __instance.lastSequenceId += (ushort)(num - 1);
            __instance.DirtyBits -= 1U;
            __result = true;
            return false;
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(nameof(CustomNetworkTransform.Deserialize))]
        public static bool DeserializePrefix(CustomNetworkTransform __instance, MessageReader reader, bool initialState)
        {
            if (__instance.isPaused)
            {
                return false;
            }

            if (initialState)
            {
                __instance.lastSequenceId = reader.ReadUInt16();
                __instance.transform.position = NetHelpers.ReadVector2(reader)*ScaleMod;
                __instance.incomingPosQueue.Clear();
                __instance.incomingPosQueue.Enqueue(__instance.transform.position);
                return false;
            }

            if (__instance.AmOwner)
            {
                return false;
            }

            var num = reader.ReadUInt16();
            var num2 = reader.ReadPackedInt32();
            Vector2 vector;
            if (__instance.incomingPosQueue.Count > 0)
            {
                vector = __instance.incomingPosQueue.ToArray().Last() * ScaleMod;
            }
            else
            {
                vector = __instance.body.position;
            }

            for (var i = 0; i < num2; i++)
            {
                var num3 = (ushort)(num + i);
                var vector2 = NetHelpers.ReadVector2(reader)*ScaleMod;
                if (!NetHelpers.SidGreaterThan(num3, __instance.lastSequenceId))
                {
                    continue;
                }
                __instance.lastSequenceId = num3;
                __instance.incomingPosQueue.Enqueue(vector2);
                __instance.debugTargetPositions.AddPt(vector2);
                vector = vector2;
            }

            __instance.debugTruePositions.AddPt(__instance.transform.position);
            if (!__instance.IsInMiddleOfAnimationThatMakesPlayerInvisible())
            {
                return false;
            }
            
            __instance.tempSnapPosition = new Il2CppSystem.Nullable<Vector2>(vector);
            return false;

        }
    }
}
