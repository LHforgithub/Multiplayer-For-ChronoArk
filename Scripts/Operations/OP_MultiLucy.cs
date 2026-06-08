using EOS;
using EOS.Attributes;
using Multiplayer.UGUI;
using Steamworks;
using Multiplayer.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Multiplayer.Operations
{
    internal class OP_MultiLucy : IOperation, IEventListener
    {
        private SteamNetworkManager _SNM => SteamNetworkManager.Instance;
        private MultiLucyControlManager _MLCM => MultiLucyControlManager.Instance;
        public void Init()
        {
            EOSManager.AddListener(this);
        }
        public void Execute()
        {
            EOSManager.RemoveListener(this);
        }

        private void CleanupAllRemotePlayers()
        {
            foreach (ulong steamId in _MLCM.CreatedRemotePlayers)
            {
                if (_MLCM.OtherPlayerControllers.TryGetValue(steamId, out var controller))
                {
                    if (controller != null && controller.gameObject != null)
                    {
                        GameObject.Destroy(controller.gameObject);
                    }
                }
            }

            _MLCM.OtherPlayerControllers.Clear();
            _MLCM.SyncBuffers.Clear();
            _MLCM.CreatedRemotePlayers.Clear();
            _MLCM.PendingPlayers.Clear();
            if (_MLCM.RemotePlayerTemplate != null)
            {
                GameObject.Destroy(_MLCM.RemotePlayerTemplate);
                _MLCM.RemotePlayerTemplate = null;
            }
            _MLCM.IsRetrying = false;
        }

        private void CleanupRemotePlayer(ulong steamId)
        {
            if (_MLCM.OtherPlayerControllers.TryGetValue(steamId, out var controller))
            {
                if (controller != null && controller.gameObject != null)
                {
                    GameObject.Destroy(controller.gameObject);
                }
            }

            UnregisterPlayerController(steamId);
            _MLCM.CreatedRemotePlayers.Remove(steamId);
            _MLCM.PendingPlayers.RemoveAll(id => id == steamId);
        }
        public void InitializeRemotePlayers()
        {
            if (FieldSystem.instance == null || FieldSystem.instance.Playercontrol == null)
            {
#if DEBUG
                Debug.LogWarning("[MultiLucyControlManager] Cannot initialize remote players yet: Playercontrol not found.".DBugText());
#endif
                _MLCM.IsRetrying = true;
                return;
            }

            CapturePlayerTemplate();

            foreach (CSteamID player in _SNM.NowLobby.AllPlayers)
            {
                if (player != null && _SNM.LocalPlayer != null && _SNM.LocalPlayer == player)
                    continue;

                TryCreateRemotePlayer(player.m_SteamID);
                EnsureRemotePlayerController(player.m_SteamID);
            }
        }
        public void TryCreateRemotePlayer(ulong steamId)
        {
            if (_MLCM.CreatedRemotePlayers.Contains(steamId))
                return;

            _MLCM.CreatedRemotePlayers.Add(steamId);
            CapturePlayerTemplate();

            if (FieldSystem.instance == null || FieldSystem.instance.Playercontrol == null)
            {
                if (!_MLCM.PendingPlayers.Contains(steamId))
                {
                    _MLCM.PendingPlayers.Add(steamId);
#if DEBUG
                    Debug.Log(("[MultiLucyControlManager] FieldSystem not ready, queued SteamID: " + steamId + " for later creation.").DBugText());
#endif
                }
                _MLCM.IsRetrying = true;
                return;
            }

            EnsureRemotePlayerController(steamId);
        }

        [EventListener(typeof(Signal_OnUnityUpdate))]
        private void RetryLoop()
        {
            if (!_MLCM.IsRetrying || _MLCM.PendingPlayers.Count == 0)
                return;

            _MLCM.LastRetryTime = Time.time;

            if (Time.time - _MLCM.LastRetryTime < _MLCM.RetryInterval)
            {
                return;
            }

            _MLCM.LastRetryTime = Time.time;

            if (FieldSystem.instance == null || FieldSystem.instance.Playercontrol == null)
            {
#if DEBUG
                Debug.Log("[MultiLucyControlManager] Waiting for FieldSystem to be ready...".DBugText());
#endif
                return;
            }

            CapturePlayerTemplate();

            var toCreate = new List<ulong>(_MLCM.PendingPlayers);
            _MLCM.PendingPlayers.Clear();

            foreach (ulong steamId in toCreate)
                EnsureRemotePlayerController(steamId);
        }

        private void CapturePlayerTemplate()
        {
            if (_MLCM.RemotePlayerTemplate != null) return;
            if (FieldSystem.instance == null || FieldSystem.instance.Playercontrol == null) return;

            var original = FieldSystem.instance.Playercontrol.gameObject;
            _MLCM.RemotePlayerTemplate = GameObject.Instantiate(original);
            _MLCM.RemotePlayerTemplate.name = "_RemotePlayerTemplate";
            _MLCM.RemotePlayerTemplate.SetActive(false);
            GameObject.DontDestroyOnLoad(_MLCM.RemotePlayerTemplate);
#if DEBUG
            Debug.Log("[MultiLucyControlManager] Captured hidden remote player template.".DBugText());
#endif
            
        }

        private void EnsureRemotePlayerController(ulong steamId)
        {
            if (_MLCM.OtherPlayerControllers.TryGetValue(steamId, out var existing) && existing != null && existing.gameObject != null)
                return;

            _MLCM.OtherPlayerControllers.Remove(steamId);

            if (_MLCM.RemotePlayerTemplate == null)
            {
                CapturePlayerTemplate();
                if (_MLCM.RemotePlayerTemplate == null)
                {
#if DEBUG
                    Debug.LogWarning(("[MultiLucyControlManager] Cannot create remote player for " + steamId + ": template not available.").DBugText());
#endif
                    
                    return;
                }
            }

            CreateRemotePlayerController(steamId);
        }

        private void CreateRemotePlayerController(ulong steamId)
        {
            if (_MLCM.RemotePlayerTemplate == null)
            {
#if DEBUG
                Debug.LogError("[MultiLucyControlManager] Remote player template not available.".DBugText());
#endif
                return;
            }

            GameObject remoteObj = GameObject.Instantiate(_MLCM.RemotePlayerTemplate);
            remoteObj.name = "RemotePlayer_" + steamId;
            remoteObj.SetActive(true);

            PlayerController remoteController = remoteObj.GetComponent<PlayerController>();
            if (remoteController == null)
            {
#if DEBUG
                Debug.LogError("[MultiLucyControlManager] PlayerController component not found on instantiated prefab.".DBugText());
#endif
                
                GameObject.Destroy(remoteObj);
                return;
            }

            if (remoteController.Spinedata != null)
            {
                remoteController.Spinedata.AnimationName = "standing";
                remoteController.Spinedata.loop = true;
                remoteController.Spinedata.timeScale = 1f;
            }

            if (remoteController.LucyCharMiantr != null)
                remoteController.LucyCharMiantr.localPosition = Vector3.zero;

            if (remoteController.rigiedbody != null)
                remoteController.rigiedbody.velocity = Vector2.zero;

            remoteController.Movevec = Vector2.zero;
            remoteController.DonUpdate = false;
            remoteController.enabled = true;

            RegisterPlayerController(steamId, remoteController);

            CreatePlayerNameTag(remoteObj, steamId);
#if DEBUG
            Debug.Log(("[MultiLucyControlManager] Created remote player controller for SteamID: " + steamId).DBugText());
#endif
            
        }

        private void CreatePlayerNameTag(GameObject parent, ulong steamId)
        {
            GameObject tagObj = new GameObject("NameTag_" + steamId);
            tagObj.transform.SetParent(parent.transform);
            tagObj.transform.localPosition = new Vector3(0f, 2.5f, 0f);
            tagObj.transform.localScale = Vector3.one;
#if DEBUG
            Debug.Log(("[NameTag] Created NameTag for SteamID: " + steamId + ", parent: " + parent.name + ", localPos: " + tagObj.transform.localPosition).DBugText());
#endif
            

            GameObject avatarObj = new GameObject("Avatar");
            avatarObj.transform.SetParent(tagObj.transform, false);
            avatarObj.transform.localPosition = Vector3.zero;
            avatarObj.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
            SpriteRenderer avatarRenderer = avatarObj.AddComponent<SpriteRenderer>();

            GameObject nameObj = new GameObject("NameText");
            nameObj.transform.SetParent(tagObj.transform, false);
            nameObj.transform.localPosition = new Vector3(0.3f, 0f, 0f);
            TextMesh nameText = nameObj.AddComponent<TextMesh>();
            nameText.fontSize = 28;
            nameText.anchor = TextAnchor.MiddleCenter;
            nameText.alignment = TextAlignment.Center;
            nameText.color = Color.white;
            nameText.text = "Loading...";
            nameText.characterSize = 0.08f;
            nameText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            PlayerNameTag tag = tagObj.AddComponent<PlayerNameTag>();
            tag.SteamId = steamId;

#if DEBUG
            Debug.Log(("[NameTag] NameTag components added").DBugText());
#endif
            
        }



        private void RegisterPlayerController(ulong steamId, PlayerController controller)
        {
            _MLCM.OtherPlayerControllers[steamId] = controller;
        }

        private void UnregisterPlayerController(ulong steamId)
        {
            _MLCM.OtherPlayerControllers.Remove(steamId);
            _MLCM.SyncBuffers.Remove(steamId);
        }
    }
}
