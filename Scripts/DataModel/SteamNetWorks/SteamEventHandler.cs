using EOS;
using Mono.Cecil;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileTypes;
using UnityEngine;

namespace Multiplayer.DataModel
{
    public class SteamEventHandler : Singleton<SteamEventHandler>
    {
        public bool IsSteamEventHandlerInit { get; private set; } = false;
        
        public void Init()
        {
            if (IsSteamEventHandlerInit)
            {
                return;
            }
            LobbyInvite = Callback<LobbyInvite_t>.Create(new Callback<LobbyInvite_t>.DispatchDelegate(OnLobbyInvite));
            LobbyEnter = Callback<LobbyEnter_t>.Create(new Callback<LobbyEnter_t>.DispatchDelegate(OnLobbyEnter));
            LobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(new Callback<LobbyDataUpdate_t>.DispatchDelegate(OnLobbyDataUpdate));
            LobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(new Callback<LobbyChatUpdate_t>.DispatchDelegate(OnLobbyChatUpdate));
            LobbyChatMsg = Callback<LobbyChatMsg_t>.Create(new Callback<LobbyChatMsg_t>.DispatchDelegate(OnLobbyChatMessage));
            LobbyMatchList = Callback<LobbyMatchList_t>.Create(new Callback<LobbyMatchList_t>.DispatchDelegate(OnLobbyMatchList));
            LobbyCreated = Callback<LobbyCreated_t>.Create(new Callback<LobbyCreated_t>.DispatchDelegate(OnLobbyCreated));
            GameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(new Callback<GameLobbyJoinRequested_t>.DispatchDelegate(OnGameLobbyJoinRequested));
            AvatarImageLoaded = Callback<AvatarImageLoaded_t>.Create(new Callback<AvatarImageLoaded_t>.DispatchDelegate(OnAvatarImageLoaded));
            PersonaStateChange = Callback<PersonaStateChange_t>.Create(new Callback<PersonaStateChange_t>.DispatchDelegate(OnPersonaStateChange));
            P2PSessionConnectFail = Callback<P2PSessionConnectFail_t>.Create(new Callback<P2PSessionConnectFail_t>.DispatchDelegate(OnP2PSessionConnectFail));
            P2PSessionRequest = Callback<P2PSessionRequest_t>.Create(new Callback<P2PSessionRequest_t>.DispatchDelegate(OnP2PSessionRequest));
            IsSteamEventHandlerInit = true;
        }
        public void Execute()
        {
            LobbyInvite.Dispose();
            LobbyEnter.Dispose();
            LobbyDataUpdate.Dispose();
            LobbyChatUpdate.Dispose();
            LobbyChatMsg.Dispose();
            LobbyMatchList.Dispose();
            LobbyCreated.Dispose();
            GameLobbyJoinRequested.Dispose();
            AvatarImageLoaded.Dispose();
            PersonaStateChange.Dispose();
            P2PSessionConnectFail.Dispose();
            P2PSessionRequest.Dispose();
            IsSteamEventHandlerInit = false;
        }
        private void OnLobbyInvite(LobbyInvite_t callback)
        {
            var user = callback.m_ulSteamIDUser;
            var lobby = callback.m_ulSteamIDLobby;
            var gameID = callback.m_ulGameID;
#if DEBUG
            Debug.Log(("Got Invited! :) -  ID: " + lobby.ToString()).DBugText());
#endif
        }

        public static void OnLobbyEnter(LobbyEnter_t callback)      //进入大厅时
        {
            var lobby = callback.m_ulSteamIDLobby;
            var unused = callback.m_rgfChatPermissions;
            var blocked = callback.m_bLocked;
            var successEnum = callback.m_EChatRoomEnterResponse;
            var lobbyID = new CSteamID(lobby);
#if DEBUG
            Debug.Log(("Entered Lobby: " + successEnum.ToString() + " - " + lobby.ToString()).DBugText());
#endif
            if (!blocked && successEnum == 1)
            {
                EOSManager.BroadCast<Signal_OnJoinLobby>(lobbyID, true);
            }
            else
            {
                EOSManager.BroadCast<Signal_OnJoinLobby>(null, false);
                Debug.Log(">>>".DBugText());
            }

            //更新大厅信息。
            //var eventParams = new object[2] { lobbyID, null };
            //EOSManager.BroadCast<Signal_InitializeLobbyMetadata>(eventParams);
            //if (eventParams[1] != null)
            //{
            //    EOSManager.BroadCast<Signal_ChangeNowLobby>(eventParams[1]);
            //}
        }

        public static void OnLobbyDataUpdate(LobbyDataUpdate_t callback)    //大厅元数据改变时。进入大厅、创建房间均会触发此回调。
        {
            var lobby = callback.m_ulSteamIDLobby;
            var playerUpdated = callback.m_ulSteamIDMember;
            var success = callback.m_bSuccess;
            var lobbyID = new CSteamID(lobby);
            if (success > 0)
            {
#if DEBUG
                Debug.Log("Lobby Data Updated for some reason".DBugText());
#endif
                //更新大厅信息。
                var eventParams = new object[2] { lobbyID, null };
                EOSManager.BroadCast<Signal_InitializeLobbyMetadata>(eventParams);
                if (eventParams[1] != null)
                {
                    EOSManager.BroadCast<Signal_ChangeNowLobby>(eventParams[1]);
                }
            }
        }
        public static void OnLobbyChatUpdate(LobbyChatUpdate_t callback)    //大厅角色数据改变时
        {
            var lobby = callback.m_ulSteamIDLobby;
            var targetPlayer = callback.m_ulSteamIDUserChanged;
            var causePlayer = callback.m_ulSteamIDMakingChange;
            var even = (EChatMemberStateChange)callback.m_rgfChatMemberStateChange;
            var lobbyID = new CSteamID(lobby);
            var targetPlayerID = new CSteamID(targetPlayer);
            var causePlayerID = new CSteamID(causePlayer);
#if DEBUG
            Debug.Log("Lobby Character Data Updated for some reason".DBugText());
#endif
            //更新大厅信息。
            var eventParams = new object[2] { lobbyID, null };
            EOSManager.BroadCast<Signal_InitializeLobbyMetadata>(eventParams);
            if (eventParams[1] != null)
            {
                EOSManager.BroadCast<Signal_ChangeNowLobby>(eventParams[1]);
            }
        }

        public static void OnLobbyChatMessage(LobbyChatMsg_t callback)  //获取聊天消息时
        {
            var lobby = callback.m_ulSteamIDLobby;
            var chatter = callback.m_ulSteamIDUser;
            var chatType = callback.m_eChatEntryType;
            var chatIndex = callback.m_iChatID;
            var lobbyID = new CSteamID(lobby);
            var chatterID = new CSteamID(chatter);
#if DEBUG
            Debug.Log("Lobby Chat Message".DBugText());
#endif
            var chatMSG = new byte[4000];
            SteamMatchmaking.GetLobbyChatEntry(chatterID, (int)chatIndex, out var _, chatMSG, 4000, out var _);
            var byteHead = new byte[4];
            Buffer.BlockCopy(chatMSG, 0, byteHead, 0, 4);
            if (byteHead.ToUInt32() == ModResourcesManager.BYTE_MSG_HEAD)   //接收到自定义二进制数据
            {

            }
            else
            {
                var msg = chatMSG.ToUTF8String().TrimEnd();
                if (string.IsNullOrWhiteSpace(msg)) return;
                EOSManager.BroadCast<Signal_PlayerSingleMSG>(chatterID, msg);
            }
        }

        public static void OnLobbyMatchList(LobbyMatchList_t callback)  //搜索大厅列表时
        {
            var lobbiesMatching = callback.m_nLobbiesMatching;
#if DEBUG
            Debug.Log(("Lobby Match List: " + lobbiesMatching.ToString()).DBugText());
#endif
            var lobbies = new List<CSteamID>();
            for (int i = 0; i < lobbiesMatching; i++)
            {
                var lobby = SteamMatchmaking.GetLobbyByIndex(i);
                lobbies.Add(lobby);
            }
            EOSManager.BroadCast<Signal_OnGetSteamLobbyList>(lobbies);
        }
        public static void OnLobbyCreated(LobbyCreated_t callback)      //创建大厅时
        {
            var result = callback.m_eResult;
            var lobby = callback.m_ulSteamIDLobby;
            var lobbyID = new CSteamID(lobby);
#if DEBUG
            Debug.Log(string.Concat(new string[]
            {
                "Lobby Created: ",
                result.ToString(),
                " - Steam ID- ",
                lobbyID.ToString()
            }).DBugText());
#endif
            EOSManager.BroadCast<Signal_OnCreateLobby>(lobbyID, result==EResult.k_EResultOK);
            //更新大厅信息。
            //var eventParams = new object[2] { lobbyID, null };
            //EOSManager.BroadCast<Signal_InitializeLobbyMetadata>(eventParams);
            //if (eventParams[1] != null)
            //{
            //    EOSManager.BroadCast<Signal_ChangeNowLobby>(eventParams[1]);
            //}
        }
        public static void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)  //受邀请加入大厅时
        {
            var steamIDLobby = callback.m_steamIDLobby;
            var steamIDFriend = callback.m_steamIDFriend;
#if DEBUG
            Debug.Log(("Entered via invite/join - " + steamIDLobby.ToString() + " - ID: " + steamIDLobby.ToString()).DBugText());
#endif
            SteamMatchmaking.JoinLobby(steamIDLobby);
        }

        public static void OnAvatarImageLoaded(AvatarImageLoaded_t callback)    //下载玩家头像完成时
        {
            var steamID = callback.m_steamID;
            var image = callback.m_iImage;
            var width = callback.m_iWide;
            var height = callback.m_iTall;
#if DEBUG
            Debug.Log(("Steam Avatar is downloaded! " + steamID.ToString() + " - size: " + width.ToString()).DBugText());
#endif
            EOSManager.BroadCast<Signal_GetPlayerAvatar>(steamID);
        }
        public static void OnPersonaStateChange(PersonaStateChange_t callback)  //获取其它玩家信息完成时
        {
            var steamID = callback.m_ulSteamID;
            var change = callback.m_nChangeFlags;
        }
        /*
            EPersonaChange
            用在 PersonaStateChange_t::m_nChangeFlags 中，描述用户发生了哪些变更。
            这些标识描述了客户端所知的最近变更，因此启动时您便能看到每位好友的名称、头像以及关系变更。

            k_EPersonaChangeName	0x0001	
            k_EPersonaChangeStatus	0x0002	
            k_EPersonaChangeComeOnline	0x0004	
            k_EPersonaChangeGoneOffline	0x0008	
            k_EPersonaChangeGamePlayed	0x0010	
            k_EPersonaChangeGameServer	0x0020	
            k_EPersonaChangeAvatar	0x0040	
            k_EPersonaChangeJoinedSource	0x0080	
            k_EPersonaChangeLeftSource	0x0100	
            k_EPersonaChangeRelationshipChanged	0x0200	
            k_EPersonaChangeNameFirstSet	0x0400	
            k_EPersonaChangeFacebookInfo	0x0800	
            k_EPersonaChangeNickname	0x1000	
            k_EPersonaChangeSteamLevel	0x2000
        */
        
        public static void OnP2PSessionConnectFail(P2PSessionConnectFail_t callback)
        {
            var remoteID = callback.m_steamIDRemote;
            var paramP2PSessionError = callback.m_eP2PSessionError;
#if DEBUG
            Debug.Log(("OnP2PSessionConnectFail - Remote ID: " + remoteID.ToString() + " - Error: " + paramP2PSessionError.ToString()).DBugText());
#endif
        }

        public static void OnP2PSessionRequest(P2PSessionRequest_t callback)        //接收到P2P连接请求时
        {
            var paramSteamID = callback.m_steamIDRemote;
#if DEBUG
            Debug.Log(("onP2PSessionRequest - Remote ID: " + paramSteamID.ToString()).DBugText());
#endif
            EOSManager.BroadCast<Signal_OnP2PSessionRequest>(paramSteamID);
        }



        protected static Callback<LobbyInvite_t> LobbyInvite;
        protected static Callback<LobbyEnter_t> LobbyEnter;
        protected static Callback<LobbyDataUpdate_t> LobbyDataUpdate;
        protected static Callback<LobbyChatUpdate_t> LobbyChatUpdate;
        protected static Callback<LobbyChatMsg_t> LobbyChatMsg;
        protected static Callback<LobbyMatchList_t> LobbyMatchList;
        protected static Callback<LobbyCreated_t> LobbyCreated;
        protected static Callback<GameLobbyJoinRequested_t> GameLobbyJoinRequested;
        protected static Callback<AvatarImageLoaded_t> AvatarImageLoaded;
        protected static Callback<PersonaStateChange_t> PersonaStateChange;
        protected static Callback<P2PSessionConnectFail_t> P2PSessionConnectFail;
        protected static Callback<P2PSessionRequest_t> P2PSessionRequest;
    }
}
