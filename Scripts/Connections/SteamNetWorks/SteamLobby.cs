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
        public SinglePlayer GetInsPlayer(CSteamID steamID)
        {
            if (InsPlayers.TryGetValue(steamID, out var player))
            {
                return player;
            }
            return null;
        }
        public CSteamID SteamID { get; set; }
        public CSteamID Owner { get; set; }
        public List<CSteamID> AllPlayers { get; private set; }      = new List<CSteamID>();
        public List<CSteamID> OtherPlayers { get; private set; }    = new List<CSteamID>();
        public List<CSteamID> NotOwnerPlayers { get; private set; } = new List<CSteamID>();
        public Dictionary<CSteamID, SinglePlayer> InsPlayers {  get; private set; } = new Dictionary<CSteamID, SinglePlayer>();
        public NetWorkSetting Setting { get; private set; }
    }
}
