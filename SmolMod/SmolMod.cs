using BepInEx;
using BepInEx.Logging;
using BepInEx.IL2CPP;
using HarmonyLib;
using UnityEngine;
using Reactor;
using System.Linq;
using Hazel;
using System.Reflection;
using System.Collections.Generic;
using System;

namespace SmolMod
{
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    [ReactorPluginSide(PluginSide.ClientOnly)]
    public class SmolMod : BasePlugin
    {
        public const string Id = "com.xtracube.smolmod";
        public Harmony Harmony { get; } = new Harmony(Id);
        public static ManualLogSource Logger;

        public static readonly float playerSize = 2f;
        public static readonly float mapScale = 7.4f;
        public static readonly float petSize = 1.2f;
        public static readonly float iconSize = 0.6f;
        //public static float networkSize = 2f;

        public override void Load()
        {
            Logger = Log;
            Harmony.PatchAll();
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
        public static class PlayerControlFixedUpdatePatch
        {
            public static void Postfix(PlayerControl __instance)
            {
                try
                {
                    if (__instance.CurrentPet.transform.localScale.x < petSize)
                    {
                        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        {
                            player.CurrentPet.transform.localScale = new Vector3(petSize, petSize, player.CurrentPet.transform.localScale.z);
                        }
                    }
                }
                catch { }
            }
        }

        //Network patches commented out until full testing is complete

        /*[HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.Method_8))]
        public static class CustomNetworkTransformWriteVector2Patch
        {
            public static bool Prefix(CustomNetworkTransform __instance, [HarmonyArgument(0)] Vector2 vec,[HarmonyArgument(1)] MessageWriter writer)
            {
                if (AmongUsClient.Instance.GameState == InnerNetClient.Nested_0.Started)
                {
                    ushort value = (ushort)(ReverseLerp(__instance.XRange, vec.x /= networkSize) * 65535f);
                    ushort value2 = (ushort)(ReverseLerp(__instance.YRange, vec.y /= networkSize) * 65535f);
                    writer.Write(value);
                    writer.Write(value2);
                    return false;
                }
                return true;
            }
            private static float ReverseLerp(FloatRange floatRange, float t)
            {
                return Mathf.Clamp((t - floatRange.min) / floatRange.Width, 0f, 1f);
            }
        }*/

        /*[HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.Deserialize))]
        public static class CustomNetworkTransformDeserializePatch
        {
            public static bool Prefix(CustomNetworkTransform __instance, [HarmonyArgument(0)] MessageReader reader, [HarmonyArgument(1)] bool initialState)
            {
                if (AmongUsClient.Instance.GameState == InnerNetClient.Nested_0.Started)
                {
                    if (initialState)
                    {
                        __instance.Field_11 = reader.ReadUInt16();
                        __instance.targetSyncPosition = (__instance.transform.position = ReadVector2(reader, __instance));
                        __instance.targetSyncVelocity = ReadVector2(reader, __instance);
                        return false;
                    }
                    ushort newSid = reader.ReadUInt16();
                    if (!SidGreaterThan(newSid, __instance.Field_11))
                    {
                        return false;
                    }
                    __instance.Field_11 = newSid;
                    if (!__instance.isActiveAndEnabled)
                    {
                        return false;
                    }
                    __instance.targetSyncPosition = ReadVector2(reader, __instance);
                    __instance.targetSyncVelocity = ReadVector2(reader, __instance);
                    if (Vector2.Distance(__instance.Field_8.position, __instance.targetSyncPosition) > __instance.snapThreshold)
                    {
                        if (__instance.Field_8)
                        {
                            __instance.Field_8.position = __instance.targetSyncPosition;
                            __instance.Field_8.velocity = __instance.targetSyncVelocity;
                        }
                        else
                        {
                            __instance.transform.position = __instance.targetSyncPosition;
                        }
                    }
                    if (__instance.interpolateMovement == 0f && __instance.Field_8)
                    {
                        __instance.Field_8.position = __instance.targetSyncPosition;
                    }
                    return false;
                }
                return true;
            }
        }*/

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
        public static class ShipStatusAwakePatch
        {
            public static void Prefix(ShipStatus __instance)
            {
                try
                {
                    SetMapSize(__instance);
                }
                catch { }
            }
        }

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Begin))]
        public static class ShipStatusBeginPatch
        {
            public static void Prefix(ShipStatus __instance)
            {
                try
                {
                    SetMapSize(__instance);
                }
                catch { }    
            }        
        }

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.FixedUpdate))]
        public static class MapSizePatch
        {
            public static void Postfix(ShipStatus __instance)
            {
                try
                {
                    if (__instance.MapScale < mapScale)
                    {
                        SetMapSize(__instance);
                    }
                }
                catch { }
            }
        }    
        
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
        public static class MeetingHudPlayerSizePatch
        {
            public static void Postfix(MeetingHud __instance)
            {   try
                {

                    foreach (var playerVoteArea in __instance.playerStates)
                    {
                        playerVoteArea.PlayerIcon.transform.localScale = new Vector3(iconSize, iconSize, playerVoteArea.PlayerIcon.transform.localScale.z);
                    }
                }
                catch { }
            }
        }

        [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.FixedUpdate))]
        public static class MapBehaviourUpdatePatch
        {
            public static void Postfix(MapBehaviour __instance)
            {
                try
                {
                    __instance.HerePoint.transform.localScale = new Vector3(iconSize, iconSize, __instance.HerePoint.transform.localScale.z);
                }
                catch { }
            }
        }

        public static void SetMapSize(ShipStatus __instance)
        {
            __instance.transform.localScale = new Vector3(2, 2, 1.02f);
            __instance.SpawnRadius = 3.1f;
            SetDistances();
            switch (__instance.Type)
            {
                case ShipStatus.Nested_0.Ship:
                    //networkSize = 2.4f;
                    __instance.MapScale = mapScale;
                    return;

                case ShipStatus.Nested_0.Pb:
                    //networkSize = 2.1f;
                    __instance.InitialSpawnCenter = new Vector2(33.325f, -5.140f);
                    __instance.MeetingSpawnCenter = new Vector2(35.584f, -32.949f);
                    __instance.MeetingSpawnCenter2 = new Vector2(35.584f, -35.34f);
                    __instance.MapScale = mapScale + 2.6f;
                    return;
                    
                case ShipStatus.Nested_0.Hq:
                    //networkSize = 2;
                    __instance.InitialSpawnCenter = new Vector2(-8.961f, 4.133f);
                    __instance.MeetingSpawnCenter = new Vector2(50f, 2.985f);
                    __instance.MeetingSpawnCenter2 = new Vector2(50f, 2.985f);
                    __instance.MapScale = mapScale + 3.4f;
                    return;
            }
        }
        public static void SetDistances()
        {
            var useMod = 1.6f;            
            try
            {
                foreach (var body in UnityEngine.Object.FindObjectsOfType<Console>().ToArray())
                {
                    body.usableDistance = useMod;
                }
                foreach (var body in UnityEngine.Object.FindObjectsOfType<GEAHFGDIGIC>().ToArray())
                {
                    body.usableDistance = useMod;
                }
                foreach (var body in UnityEngine.Object.FindObjectsOfType<SystemConsole>().ToArray())
                {
                    body.usableDistance = 3f;
                }
                foreach (var body in UnityEngine.Object.FindObjectsOfType<DeconControl>().ToArray())
                {
                    body.usableDistance = useMod;
                }
            }
            catch { }
        }

        private static Vector2 ReadVector2(MessageReader reader, CustomNetworkTransform __instance)
        {
            float v = reader.ReadUInt16() / 65535f;
            float v2 = reader.ReadUInt16() / 65535f;
            var vector2 =  new Vector2(Mathf.Lerp(__instance.XRange.min, __instance.XRange.max, v), Mathf.Lerp(__instance.YRange.min, __instance.YRange.max, v2));
            return vector2;// *= networkSize;
        }

        private static bool SidGreaterThan(ushort newSid, ushort prevSid)
        {
            ushort num = (ushort)(prevSid + 32767);
            if (prevSid < num)
            {
                return newSid > prevSid && newSid <= num;
            }
            return newSid > prevSid || newSid <= num;
        }
    }

}

