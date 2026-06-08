using EOS;
using EOS.Attributes;
using Multiplayer.DataModel;
using Multiplayer.UGUI;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Multiplayer.Operations
{
    internal class OP_MainGUI : IOperation, IEventListener
    {
        private SteamNetworkManager _SNM => SteamNetworkManager.Instance;
        private MultiplayerMainUI _MMUI => MultiplayerRootObject.Instance.MainUI;
        public void Init()
        {
            EOSManager.AddListener(this);
        }
        public void Execute()
        {
            EOSManager.RemoveListener(this);
        }

        [EventListener(typeof(Signal_GetPlayerAvatar))]
        private void OnGetPlayerAvatar(CSteamID userID)
        {
            if (!_SNM.IsInLobby) return;
            var player = _SNM.NowLobby.GetPlayerName(userID);
            if (string.IsNullOrEmpty(player)) return;
            try
            {
                SteamFriends.RequestUserInformation(userID, false);
                var imageID = SteamFriends.GetLargeFriendAvatar(userID);

#if DEBUG
            Debug.Log("~~~~~~~~~~~~~~~~~~~~~ Starting Steam Avatar ~~~~~~~~~~~~~~~~~~~~~".DBugText());
            Debug.Log("ImageID: " + imageID);
#endif
                if (imageID != -1)
                {
                    SteamUtils.GetImageSize(imageID, out var pnWidth, out var pnHeight);
#if DEBUG
                    Debug.Log("W: " + pnWidth.ToString() + ", H: " + pnHeight.ToString());
#endif
                    byte[] array = new byte[(int)(pnWidth * pnHeight * 4)];
                    var flag = SteamUtils.GetImageRGBA(imageID, array, (int)(pnWidth * pnHeight * 4));
#if DEBUG
                    Debug.Log("Image downloaded: " + flag);
#endif
                    var Pixmap = new Texture2D((int)pnWidth, (int)pnHeight, TextureFormat.RGBA32, false);
                    Pixmap.LoadRawTextureData(FlipTextureVertically(array, (int)pnWidth, (int)pnHeight));
                    Pixmap.Apply();
                    _MMUI.PlayerAcatarDic[userID] = Pixmap;
#if DEBUG
                    Debug.Log("We have completed creating the Steam image".DBugText());
#endif
                    }
                }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        private static byte[] FlipTextureVertically(byte[] data, int width, int height)
        {
            int num = width * 4;
            byte[] array = new byte[data.Length];
            for (int i = 0; i < height; i++)
            {
                Buffer.BlockCopy(data, i * num, array, (height - 1 - i) * num, num);
            }
            return array;
        }

    }
}
