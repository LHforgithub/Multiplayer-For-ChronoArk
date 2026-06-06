using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer
{
    internal class ModResourcesManager
    {
        public const string MOD_KEY_ID = "Multiplayer";
        public const string KICK_PLAYER_MSG_HEAD = "</SMSG text=\"KICK\">";
        public const int MAIN_UGUI_WINDOW_ID = 0x481c4155;
        public const uint BYTE_MSG_HEAD = 0xEE10D6F0;


        //UGUI Translation
        public static string LOBBY_NOT_JOINED_MSG = "Does not in any lobby.";
        public static string LOBBY_CREATE_MSG = "Create a Steam lobby to start multiplayer synchronization.";
        public static string LOBBY_CREATE_BUTTON = "Create Lobby";
        public static string LOBBY_REFRESH_LIST_BUTTON = "Refresh Lobbies";
        public static string LOBBY_AVAILABLE_LIST_MSG = "Available Lobbies";
        public static string LOBBY_OWNER_ROOM_MSG = "'s Lobby";
        public static string LOBBY_DISALLOWED_JOIN_MSG = "Disallowed Join.";
        public static string LOBBY_DISBAND_MSG = "Owner had disband this lobby";
        public static string LOBBY_JOIN_BUTTON = "Join";
        public static string LOBBY_LEAVE_BUTTON = "Leave";
        public static string LOBBY_DISBAND_BUTTON = "Disband";
        public static string LOBBY_CHANGE_OWNER_BUTTON = "Change Owner";
    }
}
