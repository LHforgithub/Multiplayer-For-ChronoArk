using EOS;
using EOS.Attributes;
using Newtonsoft.Json.Linq;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer
{

    //Connections Related
    public class Signal_OnP2PSessionRequest : IEventCode
    {
        [EventCodeMethod]
        public void OnP2PSessionRequest(CSteamID userID)
        {
        }
    }
    public class Signal_OnReceivePackage : IEventCode
    {
        [EventCodeMethod]
        public void OnReceivePackage(JObject data, CSteamID fromUser)
        {
        }
    }
    public class Signal_SendPackage : IEventCode
    {
        /// <summary>
        /// 按规则送信，内容必须为能解析为JObject格式的Json编码的实例才能被解析
        /// </summary>
        /// <param name="msgData">只能是<see cref="JObject"/>、<see cref="string"/>类型其中之一</param>
        [EventCodeMethod]
        public void TriggerSendPackage(object msgData)
        {
        }
    }
    public class Signal_OnSendPackageOnce : IEventCode
    {
        [EventCodeMethod]
        public void OnSendPackage(byte[] data, CSteamID toUser, bool result)
        {
        }
    }
    public class Signal_PlayerSingleMSG : IEventCode
    {
        [EventCodeMethod]
        public void PlayerSingleMSG(CSteamID fromUser, string msg)
        {

        }
    }
}
