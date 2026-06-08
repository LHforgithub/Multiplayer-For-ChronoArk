using EOS;
using EOS.Attributes;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer
{

    public class Signal_RequestRefreshUGUI : IEventCode
    {
        [EventCodeMethod]
        public void RequestRefreshUGUI(uint delayFrame = 0x10)
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
