using ChronoArkMod.Plugin;
using EOS;
using Multiplayer.Operations;
using Multiplayer.DataModel;
using Multiplayer.UGUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Multiplayer
{
    public class MultiplayerRootObject : ChronoArkPluginMonoBehaviour
    {
        public static MultiplayerRootObject Instance { get; private set; } = null;
        public MultiplayerMainUI MainUI { get; private set; } = null;
        private bool steamInitialized;
        private List<IOperation> _operations = new List<IOperation>();

        private void Awake()
        {
            if(Instance != null)
            {
                DestroyImmediate(this);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(this);
            EOSManager.MergeToSingleton();
            MainUI = gameObject.AddComponent<MultiplayerMainUI>();
            DontDestroyOnLoad(MainUI);
            StartCoroutine(InitializeSteamWhenReady());
        }
        private IEnumerator InitializeSteamWhenReady()
        {
            var delay = new WaitForFixedUpdate();
            while (true)
            {
                if (steamInitialized)
                {
                    yield break;
                }
                if (SteamManager.Initialized)
                {
#if DEBUG
                    Debug.Log("Multiplayer Steam Initializing".DBugText());
#endif
                    SteamNetworkManager.Instance.Init();
                    SteamEventHandler.Instance.Init();
                    var opTypes = Assembly.GetAssembly(typeof(SteamNetworkManager)).GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Contains(typeof(IOperation)));
                    foreach (var opType in opTypes)
                    {
                        var op = Activator.CreateInstance(opType) as IOperation;
                        op.Init();
#if DEBUG
                        Debug.Log(("Multiplayer Create Operation Instance :" + op.GetType().FullName).DBugText());
#endif
                        _operations.Add(op);
                    }
                    steamInitialized = true;
#if DEBUG
                    Debug.Log("Multiplayer Steam Initialized".DBugText());
#endif
                    yield break;
                }
                yield return delay;
            }
        }

        private void Update()
        {
            EOSManager.BroadCast<Signal_OnUnityUpdate>(gameObject);
        }

        private void OnDestroy()
        {
            if (steamInitialized)
            {

            }
            //退出顺序： 显示层->逻辑层->数据层->事件层
            if (MainUI != null)
            {
                DestroyImmediate(MainUI);
            }
            for (int i = 0; i < _operations.Count; i++)
            {
                _operations[i].Execute();
            }
            SteamEventHandler.Instance.Execute();
            SteamNetworkManager.Instance.Execute();
            EOSManager.SpliteFromSingleton();
        }
    }
}
