using EOS;
using EOS.Attributes;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.Connections
{
    public class SteamNetworkManager : Singleton<SteamNetworkManager>
    {
        public void Init()
        {
            if (IsSteamInit)
            {
                return;
            }
            LocalPlayer = SteamUser.GetSteamID();
            IsSteamInit = true;
        }

        public void Execute()
        {
            IsSteamInit = false;
        }


        public bool IsSteamInit { get; private set; } = false;
        public List<SteamLobby> LobbiesList { get; private set; } = new List<SteamLobby>();
        public CSteamID LocalPlayer { get; private set; }
        public NetWorkSetting Setting { get; set; } = new NetWorkSetting();
        public SteamLobby NowLobby { get; set; } = null;
        public bool IsNowLocalLobby => IsSteamInit && NowLobby != null && NowLobby.IsOwner(LocalPlayer);
        public bool IsInLobby => IsSteamInit && NowLobby != null && NowLobby.IsPlayerInLobby(LocalPlayer);
    }
}
