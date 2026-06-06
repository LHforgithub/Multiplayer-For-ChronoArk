using EOS;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Multiplayer.Connections
{
    public class SteamLobby
    {
        public SteamLobby(CSteamID cSteamID)
        {
            SteamID = cSteamID;
            Setting = new NetWorkSetting();
        }
        public bool IsOwner(CSteamID steamID)
        {
            return Owner.GetAccountID() == steamID.GetAccountID();
        }
        public bool IsPlayerInLobby(CSteamID steamID)
        {
            return AllPlayers.Contains(steamID);
        }
        public string GetPlayerName(CSteamID steamID)
        {
            return PlayersName.TryGetValue(steamID, out var result) ? result : string.Empty;
        }
        public CSteamID SteamID { get; set; }
        public CSteamID Owner { get; set; }
        public List<CSteamID> AllPlayers { get; private set; }      = new List<CSteamID>();
        public List<CSteamID> OtherPlayers { get; private set; }    = new List<CSteamID>();
        public List<CSteamID> NotOwnerPlayers { get; private set; } = new List<CSteamID>();
        public Dictionary<CSteamID, string> PlayersName {  get; private set; } = new Dictionary<CSteamID, string>();
        public NetWorkSetting Setting { get; private set; }
    }
}
