using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer.Connections
{
    public class NetWorkSetting
    {
        public string Name { get; set; } = "New Room";
        public int MaxPlayer {
            get
            {
                return _maxPlayer;
            }
            set
            {
                if (value < 2)
                {
                    _maxPlayer = 2;
                    return;
                }
                if (value > 4)
                {
                    _maxPlayer = 4;
                    return;
                }
                _maxPlayer = value;
            }
        }
        private int _maxPlayer = 2;
        public ELobbyType LobbyType { get; set; } = ELobbyType.k_ELobbyTypePublic;
        public bool Joinable { get; set; } = true;
    }
}
