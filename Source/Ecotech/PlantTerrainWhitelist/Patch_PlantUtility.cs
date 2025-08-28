using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace Ecotech
{
    /// <summary>
    /// Before:
    /// else if (plantDef.plant.terrainBlacklist != null && plantDef.plant.terrainBlacklist.Contains(terrain))
    /// After:
    /// else if (plantDef.plant.terrainBlacklist != null && !HELPER_PassesTerrainWhitelistChecks(plantDef, terrain) && plantDef.plant.terrainBlacklist.Contains(terrain))
    /// </summary>
    [HarmonyDebug]
    [HarmonyPatch(typeof(PlantUtility), nameof(PlantUtility.CanEverPlantAt),
#pragma warning disable format
        new Type[] {         typeof(ThingDef),    typeof(IntVec3),     typeof(Map),         typeof(Thing),    typeof(bool),        typeof(bool),        typeof(bool) },
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, })]
#pragma warning restore format
    public static class Patch_PlantUtility
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> CheckTerrainWhitelist(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            return new CodeMatcher(instructions, generator)
                .SearchForward(ci => ci.opcode == OpCodes.Ldloc_2)
                .Advance(2)
                .CreateLabel(out Label hookLabel)
                .Insert(new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldloc_0),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_PlantUtility), nameof(HELPER_PassesTerrainWhitelistChecks))),
                    new CodeInstruction(OpCodes.Brtrue_S, hookLabel),
                    new CodeInstruction(OpCodes.Ldarg_S, 6),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldloc_0),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_PlantUtility), nameof(HELPER_CreateAcceptanceReport))),
                    new CodeInstruction(OpCodes.Ret)

                }).Instructions();
        }

        private static bool HELPER_PassesTerrainWhitelistChecks(ThingDef def, TerrainDef terrain)
        {
            ThingDefExtension_PlantTerrainWhitelist modExtension = def.GetModExtension<ThingDefExtension_PlantTerrainWhitelist>();
            if(modExtension == null)
            {
                return true;
            }
            return modExtension.TerrainWhitelist.Contains(terrain);
        }

        private static AcceptanceReport HELPER_CreateAcceptanceReport(bool writeNoReason, ThingDef def, TerrainDef terrain)
        {
            return "EcoTech_InspectString_PlantTerrainWhitelist_TerrainNotWhitelisted".Translate(
                terrain.LabelCap.Named("ACTUAL"),
                String.Join(", ", def.GetModExtension<ThingDefExtension_PlantTerrainWhitelist>().TerrainWhitelist.Select(t => t.LabelCap)).Named("EXPECTED")
            );
        }
    }
}
