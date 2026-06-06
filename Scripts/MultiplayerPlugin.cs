using BasicMethods;
using ChronoArkMod;
using ChronoArkMod.ModData;
using ChronoArkMod.ModData.Settings;
using ChronoArkMod.Plugin;
using EOS;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer
{
    public class MultiplayerPlugin : ChronoArkPlugin
    {
        public override void Dispose()
        {
            harmony?.UnpatchSelf();
        }

        public override void Initialize()
        {
            harmony = new Harmony(base.GetGuid());
            harmony.PatchAll();
        }
        public static ModInfo ModInfo = ModManager.getModInfo(ModResourcesManager.MOD_KEY_ID);
        private Harmony harmony;
    }
}
