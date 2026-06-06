using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer.Connections
{
    public class SinglePlayer
    {
        public SinglePlayer(CSteamID steamID)
        {
            UserID = steamID;
            UserName = SteamFriends.GetFriendPersonaName(steamID).Trim();
            SteamFriends.RequestUserInformation(steamID, false); 
        }
        public bool IsUser(CSteamID player)
        {
            return UserID.GetAccountID() == player.GetAccountID();
        }
        public AccountID_t GetAccountID()
        {
            return UserID.GetAccountID();
        }
        public CSteamID UserID { get; private set; }
        public string UserName { get; private set; }
        public int IconID { get; set; } = -1;
    }
}
