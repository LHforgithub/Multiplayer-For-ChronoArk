using ChronoArkMod.Plugin;
using EOS;
using Multiplayer.DataModel;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Multiplayer.UGUI
{
    public class MultiplayerMainUI : MonoBehaviour
    {
        private bool windowShow = false;
        private Rect windowRect = new Rect(980f, 520f, 640f, 800f);
        private Vector2 lobbyScroll;
        private GUIStyle titleStyle;
        private GUIStyle subTitleStyle;
        private GUIStyle playerNameStyle;
        private GUIStyle mutedStyle;
        private GUIStyle badgeStyle;
        private GUIStyle voteBadgeStyle;
        private Texture2D fallbackAvatar;
        private Texture2D panelTexture;
        private Texture2D rowTexture;
        private Texture2D ownerTexture;
        private Texture2D voteTexture;

        //
        public Dictionary<CSteamID, Texture2D> PlayerAcatarDic { get; private set; } = new Dictionary<CSteamID, Texture2D>();

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                windowShow = !windowShow;
            }
        }

        private void OnGUI()
        {
            if (!windowShow)
            {
                return;
            }

            windowRect = GUILayout.Window(ModResourcesManager.MAIN_UGUI_WINDOW_ID, windowRect, Window, "Multiplayer");
        }

        private void Window(int id)
        {
            EnsureGuiResources();
            DrawHeader();

            if (!SteamManager.Initialized)
            {
                GUILayout.Label("Steam is not initialized.", mutedStyle);
                GUI.DragWindow();
                return;
            }

            if (!SteamNetworkManager.Instance.IsInLobby)
            {
                DrawNoLobby();
                GUI.DragWindow();
                return;
            }
            
            DrawLobby();
            GUI.DragWindow();
        }

        private void DrawHeader()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Multiplayer", titleStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label("F: Toggle", mutedStyle, GUILayout.Width(90f));
            GUILayout.EndHorizontal();
            GUILayout.Space(6f);
        }

        private void DrawNoLobby()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label(ModResourcesManager.LOBBY_NOT_JOINED_MSG, subTitleStyle);

            // Mode toggle
            GUILayout.BeginHorizontal();
            //useLanMode = GUILayout.Toggle(useLanMode, "LAN Mode");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Label(ModResourcesManager.LOBBY_CREATE_MSG, mutedStyle);
            GUILayout.Space(8f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(ModResourcesManager.LOBBY_CREATE_BUTTON, GUILayout.Height(34f)) || (windowShow && Input.GetKeyDown(KeyCode.Return)))
            {
                EOSManager.BroadCast<Signal_CreateLobby>();
            }
            if (GUILayout.Button(ModResourcesManager.LOBBY_REFRESH_LIST_BUTTON, GUILayout.Height(34f)))
            {
                EOSManager.BroadCast<Signal_GetSteamLobbyList>();
            }
            GUILayout.EndHorizontal();
            DrawLobbyList();
            GUILayout.EndVertical();
        }

        private void DrawLobbyList()
        {
            if (SteamNetworkManager.Instance.LobbiesList == null || SteamNetworkManager.Instance.LobbiesList.Count == 0)
            {
                return;
            }

            GUILayout.Space(10f);
            GUILayout.Label(ModResourcesManager.LOBBY_AVAILABLE_LIST_MSG, subTitleStyle);
            for (int i = 0; i < SteamNetworkManager.Instance.LobbiesList.Count; i++)
            {
                SteamLobby lobby = SteamNetworkManager.Instance.LobbiesList[i];
                GUILayout.BeginHorizontal(GUI.skin.box);
                var str1 = string.Format(
                    "{0}  ({1}/{2})", 
                    string.IsNullOrEmpty(lobby.Setting.Name) ? lobby.GetPlayerName(lobby.Owner) + ModResourcesManager.LOBBY_OWNER_ROOM_MSG : lobby.Setting.Name, 
                    lobby.AllPlayers.Count, 
                    lobby.Setting.MaxPlayer
                );
                if (!lobby.Setting.Joinable)
                {
                    GUI.enabled = false;
                    str1 += "  " + ModResourcesManager.LOBBY_DISALLOWED_JOIN_MSG;
                }
                GUILayout.Label(str1, playerNameStyle);
                if (GUILayout.Button(ModResourcesManager.LOBBY_JOIN_BUTTON, GUILayout.Width(70f)))
                {
                    EOSManager.BroadCast<Signal_JoinLobby>(lobby);
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }
        }
        private void DrawLobby()
        {
            string lobbyName, lobbyOwner, lobbyType, lobbyJoinable;
            int lobbyCapacity;
            bool isLobbyOwner;
            SteamLobby lobby = SteamNetworkManager.Instance.NowLobby;
            //lobby.FetchAllMetadata();
            lobbyName = string.IsNullOrWhiteSpace(lobby.Setting.Name) ? lobby.GetPlayerName(lobby.Owner) + ModResourcesManager.LOBBY_OWNER_ROOM_MSG : lobby.Setting.Name;
            lobbyOwner = lobby.GetPlayerName(lobby.Owner) ?? "Unknown";
            lobbyCapacity = lobby.Setting.MaxPlayer;
            switch (lobby.Setting.LobbyType)
            {
                case ELobbyType.k_ELobbyTypePrivate:
                    lobbyType = "Private";
                    break;
                case ELobbyType.k_ELobbyTypeFriendsOnly:
                    lobbyType = "Friends Only";
                    break;
                case ELobbyType.k_ELobbyTypePublic:
                    lobbyType = "Public";
                    break;
                case ELobbyType.k_ELobbyTypeInvisible:
                    lobbyType = "Invisible";
                    break;
                case ELobbyType.k_ELobbyTypePrivateUnique:
                    lobbyType = "Private Unique";
                    break;
                default:
                    lobbyType = "Unknown";
                    break;
            }
            lobbyJoinable = lobby.Setting.Joinable.ToString();
            isLobbyOwner = SteamNetworkManager.Instance.IsNowLocalLobby;

            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label(lobbyName, subTitleStyle);
            var str0 = string.Format(
                "Owner: {0}     Players: {1}/{2}     Visibility: {3}    Joinable: {4}", 
                lobbyOwner, 
                lobby.AllPlayers.Count, 
                lobbyCapacity, 
                lobbyType,
                lobbyJoinable
                );
            GUILayout.Label(str0, mutedStyle);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(ModResourcesManager.LOBBY_LEAVE_BUTTON, GUILayout.Width(78f), GUILayout.Height(30f)))
            {
                EOSManager.BroadCast<Signal_LeaveLobby>();
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                return;
            }

            GUI.enabled = isLobbyOwner;
            if (GUILayout.Button(ModResourcesManager.LOBBY_DISBAND_BUTTON, GUILayout.Width(86f), GUILayout.Height(30f)))
            {
                EOSManager.BroadCast<Signal_DisbandNowLobby>();
                GUI.enabled = true;
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                return;
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            GUILayout.Space(8f);
            DrawPlayers();
            GUI.enabled = isLobbyOwner;
            GUILayout.Space(8f);
            DrawLobbySettings();
            GUI.enabled = true;
            //GUILayout.Space(8f);
            //DrawVotePanel();
            GUILayout.EndVertical();
        }
        private void DrawLobbySettings()
        {
            DrawChangeMaxLimit();
            DrawSetVisiable();
            DrawSetJoinable();
        }
        private void DrawChangeMaxLimit()
        {

        }
        private void DrawSetVisiable()
        {

        }
        private void DrawSetJoinable()
        {

        }

        private void DrawPlayers()
        {
            lobbyScroll = GUILayout.BeginScrollView(lobbyScroll, false, true, GUILayout.ExpandHeight(true));
            foreach (var player in SteamNetworkManager.Instance.NowLobby.AllPlayers)
            {
                DrawPlayerRow(player);
            }
            GUILayout.EndScrollView();
        }

        private void DrawPlayerRow(CSteamID player)
        {
            SteamLobby lobby = SteamNetworkManager.Instance.NowLobby;

            Rect rowRect = GUILayoutUtility.GetRect(1f, 54f, GUILayout.ExpandWidth(true));
            GUI.DrawTexture(rowRect, rowTexture);

            Rect avatarRect = new Rect(rowRect.x + 8f, rowRect.y + 7f, 40f, 40f);
            GUI.DrawTexture(avatarRect, PlayerAcatarDic.TryGetValue(player,out var pixmap) ? pixmap : fallbackAvatar, ScaleMode.ScaleToFit);
            
            Rect nameRect = new Rect(rowRect.x + 58f, rowRect.y + 8f, rowRect.width - 170f, 22f);
            GUI.Label(nameRect, string.IsNullOrEmpty(lobby.GetPlayerName(player)) ? "Unknown Player" : lobby.GetPlayerName(player), playerNameStyle);

            if (lobby.Owner == player)
            {
                Rect ownerRect = new Rect(rowRect.x + 58f, rowRect.y + 31f, 54f, 18f);
                GUI.DrawTexture(ownerRect, ownerTexture);
                GUI.Label(ownerRect, "Owner", badgeStyle);
            }
            else
            {
                if(lobby.Owner == SteamNetworkManager.Instance.LocalPlayer)
                {
                    Rect changeOwnerButton = new Rect(rowRect.x + rowRect.width - 58f, rowRect.y + 56f, 54f, 18f);
                    if(GUI.Button(changeOwnerButton, ModResourcesManager.LOBBY_CHANGE_OWNER_BUTTON))
                    {
                        EOSManager.BroadCast<Signal_ChangeLobbyOwner>(player);
                    }
                }
            }
            /*
            if (VoteManager.Instance.HasPlayerVotedYes(player))
            {
                Rect voteRect = new Rect(rowRect.x + rowRect.width - 46f, rowRect.y + 12f, 30f, 30f);
                GUI.DrawTexture(voteRect, voteTexture);
                int voteCount = VoteManager.Instance.GetActiveVotes().Count(vote => VoteManager.Instance.HasPlayerVotedYes(player, vote.voteTheme));
                GUI.Label(voteRect, voteCount > 1 ? "\u2713" + voteCount : "\u2713", voteBadgeStyle);
            }
            */
        }
        /*
        private void DrawVotePanel()
        {
            IReadOnlyList<VoteManager.VoteSession> sessions = VoteManager.Instance.GetActiveVotes();
            GUILayout.BeginVertical(GUI.skin.box);
            if (sessions.Count == 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("No active vote", mutedStyle);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                return;
            }

            foreach (VoteManager.VoteSession session in sessions)
            {
                DrawVoteSession(session);
            }
            GUILayout.EndVertical();
        }

        private void DrawVoteSession(VoteManager.VoteSession session)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Vote: " + GetVoteTitle(session.voteTheme), subTitleStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label(string.Format("{0}/{1}", VoteManager.Instance.GetYesVoteCount(session.voteTheme), session.GetTotalPlayerCount()), mutedStyle, GUILayout.Width(50f));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUI.enabled = VoteManager.Instance.UnlockYesVoteButton(session.voteTheme);
            if (GUILayout.Button("Agree", GUILayout.Height(28f)))
            {
                VoteManager.Instance.Vote(session.voteTheme);
            }
            GUI.enabled = VoteManager.Instance.HasLocalPlayerVotedYes(session.voteTheme);
            if (GUILayout.Button("Cancel", GUILayout.Height(28f)))
            {
                VoteManager.Instance.Vote(session.voteTheme, true);
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private static string GetVoteTitle(VoteManager.VoteTheme voteTheme)
        {
            switch (voteTheme)
            {
                case VoteManager.VoteTheme.TurnEnd:
                    return "End Turn";
                case VoteManager.VoteTheme.NextStage:
                    return "Next Stage";
                case VoteManager.VoteTheme.FirstStage:
                    return "Start First Stage";
                case VoteManager.VoteTheme.EnterCrimson:
                    return "Crimson Wilderness";
                case VoteManager.VoteTheme.EnterAzar:
                    return "Ultimate Azar";
                default:
                    return voteTheme.ToString();
            }
        }
        */
        private void EnsureGuiResources()
        {
            if (titleStyle != null)
            {
                return;
            }

            panelTexture = MakeTexture(new Color(0.08f, 0.09f, 0.11f, 0.92f));
            rowTexture = MakeTexture(new Color(0.16f, 0.17f, 0.2f, 0.92f));
            ownerTexture = MakeTexture(new Color(0.24f, 0.31f, 0.45f, 0.95f));
            //voteTexture = MakeTexture(new Color(0.16f, 0.62f, 0.29f, 0.95f));
            fallbackAvatar = MakeTexture(new Color(0.24f, 0.25f, 0.28f, 1f));

            GUI.skin.window.normal.background = panelTexture;
            GUI.skin.box.normal.background = panelTexture;

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            subTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.92f, 0.94f, 0.98f, 1f) }
            };
            playerNameStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                clipping = TextClipping.Clip,
                normal = { textColor = Color.white }
            };
            mutedStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                normal = { textColor = new Color(0.72f, 0.75f, 0.8f, 1f) }
            };
            badgeStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 11,
                normal = { textColor = new Color(0.88f, 0.92f, 1f, 1f) }
            };
            //voteBadgeStyle = new GUIStyle(GUI.skin.label)
            //{
            //    alignment = TextAnchor.MiddleCenter,
            //    fontSize = 20,
            //    fontStyle = FontStyle.Bold,
            //    normal = { textColor = Color.white }
            //};
        }

        private static Texture2D MakeTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}
