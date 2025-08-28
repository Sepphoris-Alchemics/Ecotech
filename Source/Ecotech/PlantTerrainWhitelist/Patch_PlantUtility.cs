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
    /// if (plantDef.plant.WildTerrainTags.Count > 0 && !plantDef.plant.WildTerrainTags.Overlaps(terrain.tags.OrElseEmptyEnumerable<string>()))
    /// {
    ///   if (!writeNoReason)
    ///     ...
    /// After:
    /// if(!Helper_PassesTerrainWhitelistChecks(plantDef, terrain)
    /// {
    ///   return HELPER_CreateAcceptanceReport(writeNoReason, plantDef, terrain);
    /// }
    /// if (plantDef.plant.WildTerrainTags.Count > 0 && !plantDef.plant.WildTerrainTags.Overlaps(terrain.tags.OrElseEmptyEnumerable<string>()))
    /// {
    ///   if (!writeNoReason)
    ///     ...
    /// </summary>
    [HarmonyDebug]
#pragma warning disable format
    [HarmonyPatch(typeof(PlantUtility), nameof(PlantUtility.CanEverPlantAt),
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
            if(writeNoReason)
            {
                return AcceptanceReport.WasRejected;
            }
            return "EcoTech_InspectString_PlantTerrainWhitelist_TerrainNotWhitelisted".Translate(
                terrain.LabelCap.Named("ACTUAL"),
                String.Join(", ", def.GetModExtension<ThingDefExtension_PlantTerrainWhitelist>().TerrainWhitelist.Select(t => t.LabelCap)).Named("EXPECTED")
            );
        }
    }
}
