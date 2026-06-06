using EOS;
using EOS.Attributes;
using Multiplayer.Connections;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Multiplayer.Operations
{
    internal class OP_SessionAndMSG : IOperation, IEventListener
    {
        private SteamNetworkManager _SNM => SteamNetworkManager.Instance;
        public void Init()
        {
            EOSManager.AddListener(this);
        }
        public void Execute()
        {
            EOSManager.RemoveListener(this);
        }



        [EventListener(typeof(Signal_OnUnityUpdate))]
        private void OnUnityUpdate(GameObject triggerFrom)
        {
            while(ReceivePackage()) continue;
        }

        [EventListener(typeof(Siganl_SendPackage))]
        public void SendPackage(object msgData)
        {
            if (_SNM.NowLobby == null)
            {
                return;
            }
            var data = new byte[0];
            if (msgData is string str)
            {
                data = Encoding.UTF8.GetBytes(str);
            }
            else if (msgData is JObject jObject) 
            {
                data = Encoding.UTF8.GetBytes(jObject.ToString());
            }
            if (data.Length < 1) return;
            if (_SNM.IsNowLocalLobby)
            {
                for (int i = 0; i < _SNM.NowLobby.NotOwnerPlayers.Count; i++)
                {
                    TrySendPack(_SNM.NowLobby.NotOwnerPlayers[i], data);
                }
            }
            else
            {
                TrySendPack(_SNM.NowLobby.Owner, data);
            }
        }
        private void TrySendPack(CSteamID steamID, byte[] data)
        {
            try
            {
                var result = SteamNetworking.SendP2PPacket(steamID, data, (uint)data.Length, EP2PSend.k_EP2PSendReliable);
                EOSManager.BroadCast<Signal_OnSendPackageOnce>(data, steamID, result);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EOSManager.BroadCast<Signal_OnSendPackageOnce>(data, steamID, false);
            }
        }

        public bool ReceivePackage()
        {
            if (_SNM.NowLobby == null)
            {
                return false;
            }
            var result = false;
            try
            {
                result = SteamNetworking.IsP2PPacketAvailable(out uint num) && num > 0;
                if (result)
                {
                    byte[] array = new byte[num];
                    if (SteamNetworking.ReadP2PPacket(array, num, out var num2, out var csteamID))
                    {
#if DEBUG
                        Debug.Log("Received a package from " + csteamID.ToString());
#endif
                        var jObject = JsonConvert.DeserializeObject<JObject>(Encoding.UTF8.GetString(array));
                        EOSManager.BroadCast<Signal_OnReceivePackage>(jObject, csteamID);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            return result;
        }


        [EventListener(typeof(Signal_OnP2PSessionRequest))]
        private void CheckP2PSessionRequest(CSteamID userID)
        {
            if (_SNM.NowLobby == null)
            {
                return;
            }
            if (_SNM.NowLobby.IsPlayerInLobby(userID))
            {
                SteamNetworking.AcceptP2PSessionWithUser(userID);
            }
        }
    }
}
