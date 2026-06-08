using EOS;
using EOS.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Multiplayer
{
    public class Signal_OnUnityUpdate : IEventCode
    {
        [EventCodeMethod]
        public void OnUnityUpdate(GameObject triggerFrom)
        {
        }
    }
}
