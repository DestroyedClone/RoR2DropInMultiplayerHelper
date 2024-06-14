using RoR2;
using System.Runtime.CompilerServices;

namespace RoR2DropInMultiplayerHelper
{
    internal class ModCompat
    {
        public static bool loaded_Survariants = false;

        public static void Init()
        {
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("pseudopulse.Survariants"))
            {
                loaded_Survariants = true;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static bool Survariants_IsVariant(SurvivorDef survivorDef)
        {
            return Survariants.SurvivorVariantCatalog.SurvivorVariantReverseMap.ContainsKey(survivorDef);
        }
    }
}