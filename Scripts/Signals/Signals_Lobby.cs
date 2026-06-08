using EOS;
using EOS.Attributes;
using Multiplayer.DataModel;
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
}
