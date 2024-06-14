using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using BepInEx.Configuration;
using RoR2;

namespace RoR2DropInMultiplayerHelper
{
    internal class ModCompat
    {
        public static bool loaded_Survariants = false;
        public static void Init(ConfigFile Config)
        {
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("pseudopulse.Survariants"))
            {
                Init_Survariants();
            }
        }

        //attr
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void Init_Survariants()
        {
            loaded_Survariants = true;
        }


        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static bool Survariants_IsVariant(SurvivorDef survivorDef)
        {
            return Survariants.SurvivorVariantCatalog.SurvivorVariantReverseMap.ContainsKey(survivorDef);
        }
    }
}
