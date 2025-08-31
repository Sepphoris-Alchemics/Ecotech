using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Ecotech
{
    /// <summary>
    /// Directly at the end of method, insert the following:
    /// HELPER_SetPlantFaction(plant, planter);
    /// </summary>
    [HarmonyPatch(typeof(CompPlantable), nameof(CompPlantable.DoPlant))]
    public static class Patch_CompPlantable
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SetFactionOfPlantToPlantersFaction(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher codeMatcher = new CodeMatcher(instructions)
                .End();
            List<System.Reflection.Emit.Label> labels = codeMatcher.Labels.ToList();
            codeMatcher.Labels.Clear();
            CodeInstruction newAnchorInstruction = CodeInstruction.LoadLocal(0);
            newAnchorInstruction.labels = labels;
            return codeMatcher.Insert(new CodeInstruction[]
                {
                    newAnchorInstruction,
                    CodeInstruction.LoadArgument(1),
                    CodeInstruction.Call(typeof(Patch_CompPlantable),nameof(HELPER_SetPlantFaction))
                })
                .Instructions();
        }

        private static void HELPER_SetPlantFaction(Plant plant, Pawn planter)
        {
            if(planter == null || plant == null)
            {
                return;
            }
            ThingComp_PlantTransformOnMaturity comp = plant.GetComp<ThingComp_PlantTransformOnMaturity>();
            if(comp == null)
            {
                return;
            }
            comp.PotentialTransformedFaction = planter.Faction;
        }
    }
}
