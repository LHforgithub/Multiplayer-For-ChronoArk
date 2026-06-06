using EOS;
using EOS.Attributes;
using Multiplayer.Connections;
using Newtonsoft.Json.Linq;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Multiplayer
{
    public class Signal_OnUnityUpdate : IEventCode
    {
        [EventCodeMethod]
        public void OnUnityUpdate(GameObject triggerFrom)
        {
        }
    }

    public class Signal_RequestRefreshUGUI : IEventCode
    {
        [EventCodeMethod]
        public void RequestRefreshUGUI(uint delayFrame = 0x10) 
        { 
        }
    }

    //Connections Related
    public class Signal_OnP2PSessionRequest : IEventCode
    {
        [EventCodeMethod]
        public void OnP2PSessionRequest(CSteamID userID)
        {
        }
    }
    public class Signal_OnReceivePackage : IEventCode
    {
        [EventCodeMethod]
        public void OnReceivePackage(JObject data, CSteamID fromUser)
        {
        }
    }
    public class Siganl_SendPackage : IEventCode
    {
        /// <summary>
        /// 按规则送信，内容必须为JObject格式的Json编码才能被解析
        /// </summary>
        /// <param name="msgData">只能是<see cref="JObject"/>、<see cref="string"/>类型其中之一</param>
        [EventCodeMethod]
        public void TriggerSendPackage(object msgData)
        {
        }
    }
    public class Signal_OnSendPackageOnce : IEventCode 
    {
        [EventCodeMethod]
        public void OnSendPackage(byte[] data, CSteamID toUser, bool result)
        {
        }
    }
    public class Signal_PlayerSingleMSG : IEventCode 
    {
        [EventCodeMethod]
        public void PlayerSingleMSG(CSteamID fromUser, string msg)
        {

        }
    }

    //Steam Lobby
    public class Signal_InitializeLobbyMetadata : IEventCode 
    {
        [EventCodeMethod]
        public  void InitializeLobbyMetadata(CSteamID steamID, ref SteamLobby lobby)
        {
        }
    }
    public class Signal_CreateLobby : IEventCode
    {
        [EventCodeMethod]
        public void TriggerCreateLobby()
        {
        }
    }
    public class Signal_OnCreateLobby : IEventCode
    {
        [EventCodeMethod]
        public void OnCreateLobby(CSteamID lobbyID, bool result)
        {
        }
    }
    public class Signal_JoinLobby : IEventCode
    {
        [EventCodeMethod]
        public void TriggerJoinLobby(SteamLobby lobby)
        {
        }
    }
    public class Signal_OnJoinLobby : IEventCode
    {
        [EventCodeMethod]
        public void OnJoinLobby(CSteamID lobbyID, bool result)
        {
        }
    }
    public class Signal_LeaveLobby : IEventCode
    {
        [EventCodeMethod]
        public void TriggerLeaveLobby()
        {
        }
    }
    public class Signal_OnLeaveLobby : IEventCode
    {
        [EventCodeMethod]
        public void OnLeaveLobby(SteamLobby lobby)
        {
        }
    }
    public class Signal_DisbandNowLobby : IEventCode
    {
        [EventCodeMethod]
        public void TriggerDisbandNowLobby()
        {
        }
    }
    public class Signal_ChangeNowLobby : IEventCode
    {
        [EventCodeMethod]
        public void ChangeNowLobby(SteamLobby lobby)
        {
        }
    }
    public class Signal_InvitePlayerToLobby : IEventCode 
    {
        [EventCodeMethod]
        public void InvitePlayerToLobby(CSteamID userID)
        {

        }
    }
    public class Signal_KickPlayerFromLobby : IEventCode
    {
        [EventCodeMethod]
        public void KickPlayerFromLobby(CSteamID userID)
        {

        }
    }
    public class Signal_ChangeLobbyOwner : IEventCode 
    {
        [EventCodeMethod]
        public void ChangeLobbyOwner(CSteamID userID) 
        { 

        }
    }
    public class Signal_GetSteamLobbyList : IEventCode 
    {
        [EventCodeMethod]
        public void GetSteamLobbyList()
        {

        }
    }
    public class Signal_OnGetSteamLobbyList : IEventCode
    {
        [EventCodeMethod]
        public void OnGetSteamLobbyList(List<CSteamID> lobbies)
        {
        }
    }
    
    //Player Information
    public class Signal_GetPlayerAvatar : IEventCode
    {
        [EventCodeMethod]
        public void OnGetPlayerAvatar(CSteamID userID)
        {

        }
    }
}
