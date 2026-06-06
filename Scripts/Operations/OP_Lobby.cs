using EOS;
using EOS.Attributes;
using Multiplayer.Connections;
using Multiplayer.UGUI;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileTypes;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

namespace Multiplayer.Operations
{
    internal class OP_Lobby : IOperation, IEventListener
    {
        private SteamNetworkManager _SNM => SteamNetworkManager.Instance;
        private MultiplayerMainUI _MMUI => MultiplayerRootObject.Instance.MainUI;
        public void Init()
        {
            EOSManager.AddListener(this);
        }
        public void Execute()
        {
            if (_SNM.NowLobby != null)
            {
                LeaveLobby();
            }
            EOSManager.RemoveListener(this);
        }
        [EventListener(typeof(Signal_InitializeLobbyMetadata))]
        private void InitializeLobbyMetadata(CSteamID steamID, ref SteamLobby lobby)
        {
            try
            {
#if DEBUG
                Debug.Log("Operation : Initialize Lobby Metadata");
#endif
                lobby = new SteamLobby(steamID);
                lobby.Owner = SteamMatchmaking.GetLobbyOwner(steamID);
                if (_SNM.NowLobby == null && lobby.Owner == _SNM.LocalPlayer)
                {
                    SetLobbyData(lobby);
                }
                lobby.Setting.Name = SteamMatchmaking.GetLobbyData(steamID, nameof(NetWorkSetting.Name));
                if (Enum.TryParse<ELobbyType>(SteamMatchmaking.GetLobbyData(steamID, nameof(NetWorkSetting.LobbyType)), out var etype))
                {
                    lobby.Setting.LobbyType = etype;
                }
                if (bool.TryParse(SteamMatchmaking.GetLobbyData(steamID, nameof(NetWorkSetting.Joinable)), out var jable))
                {
                    lobby.Setting.Joinable = jable;
                }
                else
                {
                    lobby.Setting.Joinable = false;
                }
                lobby.Setting.MaxPlayer = SteamMatchmaking.GetLobbyMemberLimit(steamID);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            UpdateLobbyMembers(lobby);
        }
        private void UpdateLobbyMembers(SteamLobby lobby)
        {
            try
            {
                int num = SteamMatchmaking.GetNumLobbyMembers(lobby.SteamID);
#if DEBUG
                Debug.Log("Get Members in lobby: " + num.ToString());
#endif
                lobby.AllPlayers.Clear();
                lobby.OtherPlayers.Clear();
                lobby.NotOwnerPlayers.Clear();
                lobby.InsPlayers.Clear();
                var localID = SteamNetworkManager.Instance.LocalPlayer.GetAccountID();
                var ownerID = lobby.Owner.GetAccountID();
                for (int i = 0; i < num; i++)
                {
                    var memberSteamID = SteamMatchmaking.GetLobbyMemberByIndex(lobby.SteamID, i);
                    lobby.AllPlayers.Add(memberSteamID);
                    var maID = memberSteamID.GetAccountID();
                    if (ownerID != maID)
                    {
                        lobby.NotOwnerPlayers.Add(memberSteamID);
                    }
                    if (localID != maID)
                    {
                        lobby.OtherPlayers.Add(memberSteamID);
                    }
                    lobby.InsPlayers[memberSteamID] = new SinglePlayer(memberSteamID);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void AddPlayer(CSteamID userID)
        {
            if (!_SNM.IsNowLocalLobby) return;
            if (_SNM.NowLobby.IsPlayerInLobby(userID)) return;
#if DEBUG
            Debug.Log("Operation : Add Player To Now Lobby");
#endif
            _SNM.NowLobby.AllPlayers.Add(userID);
            _SNM.NowLobby.NotOwnerPlayers.Add(userID);
            if (userID != SteamNetworkManager.Instance.LocalPlayer)
            {
                _SNM.NowLobby.OtherPlayers.Add(userID);
            }
            _SNM.NowLobby.InsPlayers[userID] = new SinglePlayer(userID);
        }

        public void RemovePlayer(CSteamID userID)
        {
            if (!_SNM.IsNowLocalLobby) return;
            if (!_SNM.NowLobby.IsPlayerInLobby(userID)) return;
#if DEBUG
            Debug.Log("Operation : Remove Player From Now Lobby");
#endif
            _SNM.NowLobby.AllPlayers.Remove(userID);
            _SNM.NowLobby.NotOwnerPlayers.Remove(userID);
            _SNM.NowLobby.OtherPlayers.Remove(userID);
            _SNM.NowLobby.InsPlayers.Remove(userID);
        }
        private void SetLobbyData(SteamLobby lobby)
        {
            SteamMatchmaking.SetLobbyData(lobby.SteamID, nameof(NetWorkSetting.Name), lobby.Setting.Name);
            SteamMatchmaking.SetLobbyData(lobby.SteamID, nameof(NetWorkSetting.LobbyType), lobby.Setting.LobbyType.ToString());
            SteamMatchmaking.SetLobbyData(lobby.SteamID, nameof(NetWorkSetting.Joinable), lobby.Setting.Joinable.ToString());
            SteamMatchmaking.SetLobbyType(lobby.SteamID, lobby.Setting.LobbyType);
            SteamMatchmaking.SetLobbyMemberLimit(lobby.SteamID, lobby.Setting.MaxPlayer);
        }
        private void ResetLobbyData()
        {
            if (!_SNM.IsNowLocalLobby)
            {
                return;
            }
#if DEBUG
            Debug.Log("Operation : Reset Lobby Custom Data");
#endif
            SteamMatchmaking.SetLobbyData(_SNM.NowLobby.SteamID, nameof(NetWorkSetting.Name), _SNM.NowLobby.Setting.Name);
            SteamMatchmaking.SetLobbyData(_SNM.NowLobby.SteamID, nameof(NetWorkSetting.LobbyType), _SNM.NowLobby.Setting.LobbyType.ToString());
            SteamMatchmaking.SetLobbyData(_SNM.NowLobby.SteamID, nameof(NetWorkSetting.Joinable), _SNM.NowLobby.Setting.Joinable.ToString());
            SteamMatchmaking.SetLobbyType(_SNM.NowLobby.SteamID, _SNM.NowLobby.Setting.LobbyType);
            SteamMatchmaking.SetLobbyMemberLimit(_SNM.NowLobby.SteamID, _SNM.NowLobby.Setting.MaxPlayer);
        }

        [EventListener(typeof(Signal_CreateLobby))]
        private void CreateLobby()
        {
#if DEBUG
            Debug.Log("Operation : Create Lobby");
#endif
            SteamMatchmaking.CreateLobby(_SNM.Setting.LobbyType, _SNM.Setting.MaxPlayer);
        }

        [EventListener(typeof(Signal_JoinLobby))]
        private void JoinLobby(SteamLobby lobby)
        {
            if (lobby == null)
            {
                return;
            }
            if (_SNM.NowLobby != null)
            {
                LeaveLobby();
            }
#if DEBUG
            Debug.Log("Operation : Join Lobby : " + lobby.SteamID.m_SteamID);
#endif
            SteamMatchmaking.JoinLobby(lobby.SteamID);
        }

        [EventListener(typeof(Signal_LeaveLobby))]
        private void LeaveLobby()
        {
            if (_SNM.NowLobby == null)
            {
                return;
            }
#if DEBUG
            Debug.Log("Operation : Leave Lobby : " + _SNM.NowLobby.SteamID.m_SteamID);
#endif
            SteamMatchmaking.LeaveLobby(_SNM.NowLobby.SteamID);
            EOSManager.BroadCast<Signal_OnLeaveLobby>(_SNM.NowLobby);
            ChangeNowLobby(null);
        }

        [EventListener(typeof(Signal_DisbandNowLobby))]
        private void DisbandNowLobby()
        {
            if (!_SNM.IsNowLocalLobby)
            {
                return;
            }
#if DEBUG
            Debug.Log("Operation : Disband Lobby");
#endif
            for (int i = 0; i < _SNM.NowLobby.NotOwnerPlayers.Count; i++)
            {
                var userID = _SNM.NowLobby.NotOwnerPlayers[i];
                var data = ModResourcesManager.LOBBY_DISBAND_MSG.ToByteArray();
                SteamMatchmaking.SendLobbyChatMsg(userID, data, data.Length);
                KickPlayer(userID);
            }
            SteamMatchmaking.SetLobbyJoinable(_SNM.NowLobby.SteamID, false);
            SteamMatchmaking.SetLobbyType(_SNM.NowLobby.SteamID, ELobbyType.k_ELobbyTypeInvisible);
            LeaveLobby();
        }
        [EventListener(typeof(Signal_InvitePlayerToLobby))]
        private void InvitePlayer(CSteamID userID)
        {
            if(!_SNM.IsInLobby) 
            { 
                return; 
            }
            SteamMatchmaking.InviteUserToLobby(_SNM.NowLobby.SteamID, userID);
        }
        [EventListener(typeof(Signal_ChangeLobbyOwner))]
        private void ChangeOwner(CSteamID userID)
        {
            if (!_SNM.IsNowLocalLobby)
            {
                return;
            }
            SteamMatchmaking.SetLobbyOwner(_SNM.NowLobby.SteamID, userID);
        }
        [EventListener(typeof(Signal_KickPlayerFromLobby))]
        private void KickPlayer(CSteamID userID)
        {
            if (!_SNM.IsNowLocalLobby)
            {
                return;
            }
            if (!_SNM.NowLobby.AllPlayers.Contains(userID))
            {
                return;
            }
#if DEBUG
            Debug.Log("Operation : Kick Player : " + _SNM.NowLobby.GetInsPlayer(userID).UserName);
#endif
            var data = (ModResourcesManager.KICK_PLAYER_MSG_HEAD + userID.m_SteamID.ToString()).ToByteArray();
            SteamMatchmaking.SendLobbyChatMsg(userID, data, data.Length);
        }

        [EventListener(typeof(Signal_ChangeNowLobby))]
        private void ChangeNowLobby(SteamLobby lobby)
        {
#if DEBUG
            Debug.Log("Operation : Change Lobby To : " + (lobby?.SteamID.m_SteamID.ToString() ?? "Null"));
#endif
            _SNM.NowLobby = lobby;
            if (lobby == null) return;
            _MMUI.PlayerAcatarDic.Clear();
            for (int i = 0; i < _SNM.NowLobby.AllPlayers.Count; i++) 
            {
                EOSManager.BroadCast<Signal_GetPlayerAvatar>(_SNM.NowLobby.AllPlayers[i]);
            }
            EOSManager.BroadCast<Signal_RequestRefreshUGUI>();
        }

        [EventListener(typeof(Signal_GetSteamLobbyList))]
        private void GetSteamLobbyList()
        {
#if DEBUG
            Debug.Log("Operation : Get Steam Lobby List.");
#endif
            SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide);
            SteamMatchmaking.RequestLobbyList();
        }

        [EventListener(typeof(Signal_OnGetSteamLobbyList))]
        private void OnGetSteamLobbyList(List<CSteamID> lobbies)
        {
#if DEBUG
            Debug.Log("Operation : Get Steam Lobby List, Count : " + lobbies.Count + " Set to SNM.");
#endif
            _SNM.LobbiesList.Clear();
            for (int i = 0; i < lobbies.Count; i++)
            {
                var newLobby = new SteamLobby(lobbies[i]);
                EOSManager.BroadCast<Signal_InitializeLobbyMetadata>(newLobby);
                _SNM.LobbiesList.Add(newLobby);
            }
        }
    }
}
