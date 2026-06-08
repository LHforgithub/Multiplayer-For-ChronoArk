using EOS;
using HarmonyLib;
using Multiplayer.DataModel;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Multiplayer.UGUI
{
    [HarmonyPatch]
    public class MultiLucyControlManager : Singleton<MultiLucyControlManager>
    {

        /// <summary>
        /// 渲染延迟（秒）。建议设为平均 RTT 的 50%~70%，通常 0.08~0.15s
        /// </summary>
        public const float InterpolationDelay = 0.12f;
        private const int MaxBufferSize = 48;
        private const float SendInterval = 0.05f;

        private static float _lastSendTime = 0f;


        public GameObject RemotePlayerTemplate;
        public readonly HashSet<ulong> CreatedRemotePlayers = new HashSet<ulong>();
        public readonly List<ulong> PendingPlayers = new List<ulong>();
        public bool IsRetrying = false;
        public readonly float RetryInterval = 1f;
        public float LastRetryTime = 0f;

        public struct SyncPacket
        {
            public string Type;
            public float Timestamp;
            public Vector2 WorldPosition;
            public float JumpLocalY;
            public bool IsMoving;
            public bool FacingRight;
            public string SkinName;
        }

        public readonly Dictionary<ulong, List<SyncPacket>> SyncBuffers = new Dictionary<ulong, List<SyncPacket>>();
        public readonly Dictionary<ulong, PlayerController> OtherPlayerControllers = new Dictionary<ulong, PlayerController>();
        public readonly Dictionary<ulong, string> RemotePlayerSkins = new Dictionary<ulong, string>();


        [HarmonyPatch(typeof(PlayerController), "Update")]
        [HarmonyPrefix]
        private static bool PC_Update_Prefix(PlayerController __instance)
        {
            if (Instance.IsLocalPlayer(__instance)) return true;

            __instance.DonUpdate = true;
            return true;
        }

        [HarmonyPatch(typeof(PlayerController), "Update")]
        [HarmonyPostfix]
        private static void PC_Update_Postfix(PlayerController __instance)
        {
            if (Instance.IsLocalPlayer(__instance)) return;

            ulong steamId = Instance.GetPlayerSteamId(__instance);
            if (steamId == 0) return;

            if (Instance.SyncBuffers.TryGetValue(steamId, out var buffer) && buffer.Count > 0)
            {
                var state = Instance.GetInterpolatedState(buffer, Time.time);
                if (state.HasValue)
                {
                    if (__instance.Spinedata != null)
                    {
                        if (state.Value.IsMoving)
                        {
                            __instance.Spinedata.AnimationName = "walking";
                            __instance.Spinedata.loop = true;
                            __instance.Spinedata.timeScale = 1f;
                        }
                        else
                        {
                            __instance.Spinedata.AnimationName = "standing";
                            __instance.Spinedata.loop = true;
                        }
                    }

                    __instance.Right = state.Value.FacingRight;
                }
            }
        }
        [HarmonyPatch(typeof(PlayerController), "FixedUpdate")]
        [HarmonyPrefix]
        private static bool PC_FixedUpdate_Prefix(PlayerController __instance)
        {
            if (Instance.IsLocalPlayer(__instance)) return true;

            ulong steamId = Instance.GetPlayerSteamId(__instance);
            if (steamId == 0) return true;

            if (Instance.SyncBuffers.TryGetValue(steamId, out var buffer) && buffer.Count > 0)
            {
                var state = Instance.GetInterpolatedState(buffer, Time.time);
                if (state.HasValue)
                {
                    float dist = Vector2.Distance(__instance.transform.position, state.Value.WorldPosition);
                    if (dist > 1.5f)
                    {
                        __instance.transform.position = state.Value.WorldPosition;
                    }
                    else
                    {
                        __instance.transform.position = Vector2.Lerp(__instance.transform.position, state.Value.WorldPosition, 10f * Time.fixedDeltaTime);
                    }
                }
            }

            __instance.Movevec = Vector2.zero;
            if (__instance.rigiedbody != null) __instance.rigiedbody.velocity = Vector2.zero;
            return false;
        }

        [HarmonyPatch(typeof(PlayerController), "FixedUpdate")]
        [HarmonyPostfix]
        private static void PC_FixedUpdate_Postfix(PlayerController __instance)
        {
            if (!SteamNetworkManager.Instance.IsInLobby) return;
            if (!Instance.IsLocalPlayer(__instance)) return;

            if (Time.time - _lastSendTime >= SendInterval)
            {
                _lastSendTime = Time.time;
                float jumpY = __instance.LucyCharMiantr?.localPosition.y ?? 0f;
                bool isMoving = __instance.Movevec != Vector2.zero;
                bool facingRight = __instance.Spinedata != null && __instance.Spinedata.transform.localScale.x > 0;
                string skinName = __instance.Spinedata != null ? __instance.Spinedata.initialSkinName : "skin_1";
                EOSManager.BroadCast<Signal_SendPackage>(new SyncPacket()
                {
                    Type = nameof(SyncPacket),
                    Timestamp = Time.time,
                    WorldPosition = new Vector2(__instance.transform.position.x, __instance.transform.position.y),
                    JumpLocalY = jumpY,
                    IsMoving = isMoving,
                    FacingRight = facingRight,
                    SkinName = skinName,
                });
                //MessageDispatcher.Send(new PlayerPositionMessage { X = __instance.transform.position.x, Y = __instance.transform.position.y, JumpY = jumpY, Timestamp = Time.time, IsMoving = isMoving, FacingRight = facingRight, SkinName = skinName });
            }
        }

        [HarmonyPatch(typeof(PlayerJump), "Update")]
        [HarmonyPrefix]
        private static bool PJ_Update_Prefix(PlayerJump __instance)
        {
            if (__instance.MainCont == null) return true;
            return Instance.IsLocalPlayer(__instance.MainCont);
        }

        [HarmonyPatch(typeof(PlayerJump), nameof(PlayerJump.FixedUpdate))]
        [HarmonyPrefix]
        private static bool PJ_FixedUpdate_Prefix(PlayerJump __instance)
        {
            if (__instance.MainCont == null || Instance.IsLocalPlayer(__instance.MainCont)) return true;

            ulong steamId = Instance.GetPlayerSteamId(__instance.MainCont);
            if (steamId == 0) return true;

            var childTr = __instance.MainCont.LucyCharMiantr;
            if (childTr == null) return true;

            if (Instance.SyncBuffers.TryGetValue(steamId, out var buffer) && buffer.Count > 0)
            {
                var state = Instance.GetInterpolatedState(buffer, Time.time);
                if (state.HasValue)
                {
                    float targetY = state.Value.JumpLocalY;
                    float currentY = childTr.localPosition.y;
                    childTr.localPosition = new Vector3(0f, Mathf.Lerp(currentY, targetY, 8f * Time.fixedDeltaTime), 0f);
                }
            }

            __instance.JumpSpeed = 0f;
            return false;
        }


        [HarmonyPatch(typeof(PlayerController), "OnTriggerStay2D")]
        [HarmonyPrefix]
        private static bool PC_OnTriggerStay2D_Prefix(PlayerController __instance)
        {
            if (Instance.IsLocalPlayer(__instance)) return true;

            if (__instance.Coll != null)
            {
                __instance.Emoji.Off();
                __instance.Coll = null;
            }
            return false;
        }

        [HarmonyPatch(typeof(PlayerController), "OnTriggerExit2D")]
        [HarmonyPrefix]
        private static bool PC_OnTriggerExit2D_Prefix(PlayerController __instance)
        {
            if (Instance.IsLocalPlayer(__instance)) return true;

            if (__instance.Coll != null)
            {
                __instance.Emoji.Off();
                __instance.Coll = null;
            }
            return false;
        }

        [HarmonyPatch(typeof(EventObject), "OnTriggerExit2D")]
        [HarmonyPrefix]
        private static bool EO_OnTriggerExit2D_Prefix(EventObject __instance, Collider2D coll)
        {
            if (coll.gameObject.tag != "Player") return true;

            PlayerController collPlayer = coll.gameObject.GetComponent<PlayerController>();
            if (collPlayer != null && Instance.IsLocalPlayer(collPlayer)) return true;

            return false;
        }


        private bool IsLocalPlayer(PlayerController controller)
        {
            if (!SteamNetworkManager.Instance.IsInLobby) return true;
            if (SteamNetworkManager.Instance.LocalPlayer == null) return true;

            foreach (var kvp in OtherPlayerControllers)
            {
                if (kvp.Value == controller)
                {
                    return false;
                }
            }
            return true;
        }
        private ulong GetPlayerSteamId(PlayerController controller)
        {
            foreach (var kvp in OtherPlayerControllers)
            {
                if (kvp.Value == controller)
                {
                    return kvp.Key;
                }
            }
            return 0;
        }
        private SyncPacket? GetInterpolatedState(List<SyncPacket> buffer, float currentTime)
        {
            if (buffer == null || buffer.Count == 0) return null;
            if (buffer.Count < 2) return buffer[buffer.Count - 1];

            float targetTime = currentTime - InterpolationDelay;
            SyncPacket a = buffer[0], b = buffer[1];
            bool found = false;

            for (int i = 1; i < buffer.Count; i++)
            {
                if (buffer[i].Timestamp >= targetTime)
                {
                    a = buffer[i - 1];
                    b = buffer[i];
                    found = true;
                    break;
                }
            }

            if (!found) return buffer[buffer.Count - 1];

            float t = Mathf.Clamp01((targetTime - a.Timestamp) / Mathf.Max(0.0001f, b.Timestamp - a.Timestamp));
            return new SyncPacket
            {
                Timestamp = targetTime,
                WorldPosition = Vector2.Lerp(a.WorldPosition, b.WorldPosition, t),
                JumpLocalY = Mathf.Lerp(a.JumpLocalY, b.JumpLocalY, t),
                IsMoving = b.IsMoving,
                FacingRight = b.FacingRight
            };
        }


    }
}
