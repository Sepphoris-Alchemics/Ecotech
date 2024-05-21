using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verse;

namespace Terrasecurity
{
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.GetGizmos))]
    public static class Patch_EquipmentTracker
    {
        [HarmonyPostfix]
        public static IEnumerable<Gizmo> AddLifespanGizmos(IEnumerable<Gizmo> __result, Pawn_EquipmentTracker __instance)
        {
            foreach (Gizmo gizmo in __result)
            {
                yield return gizmo;
            }
            // if multiple pawns are selected, do not draw the gizmos for each pawn
            if (Find.Selector.SingleSelectedThing != __instance.pawn)
            {
                yield break;
            }
            foreach (ThingWithComps equipment in __instance.AllEquipmentListForReading)
            {
                foreach (ThingComp equipmentComp in equipment.AllComps)
                {
                    if (equipmentComp is ThingComp_EquippedLifespan lifespanComp)
                    {
                        yield return lifespanComp.TimeSpanReadoutGizmo;
                    }
                    if(equipmentComp is ThingComp_InstallableInSlottedThingTransformer installableInTransformerComp)
                    {
                        yield return installableInTransformerComp.InstallGizmo;
                    }
                }
            }
        }
    }
}
