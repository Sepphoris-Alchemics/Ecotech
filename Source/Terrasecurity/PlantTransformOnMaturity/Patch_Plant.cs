using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verse;

namespace Terrasecurity
{
    [HarmonyPatch(typeof(Plant), nameof(Plant.TickLong))]
    public static class Patch_Plant
    {
        [HarmonyPostfix]
        public static void CallTransformComp(Plant __instance)
        {
            try
            {
                ThingComp_PlantTransformOnMaturity comp = __instance.GetComp<ThingComp_PlantTransformOnMaturity>();
                if(comp == null)
                {
                    return;
                }
                comp.TransformTickLong();
            }
            catch(Exception ex)
            {
                Log.Error($"Caught unhandled exception: {ex}");
            }
        }
    }
}
