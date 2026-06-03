using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;
using Verse.AI;
using UnityEngine;

namespace Ecotech
{
    /*
     * Mod extension for DamageDefs that should apply
     * a Hediff to the actual injured body part.
     */
    public class DamageDefExtension_ApplyHediffToInjuredPart
        : DefModExtension
    {
        public HediffDef hediff;

        /*
         * Severity added per point of damage dealt.
         */
        public float severityPerDamageDealt = 0f;

        /*
         * Optional flat severity added on hit.
         */
        public float fixedSeverity = 0f;
    }

    /*
     * Damage worker for Midas projectiles.
     *
     * This applies normal injury first, then applies
     * the configured Hediff to the body part that was
     * actually injured.
     */
    public class DamageWorker_MidasAddInjury
        : DamageWorker_AddInjury
    {
        public override DamageResult Apply(
            DamageInfo dinfo,
            Thing victim)
        {
            Pawn pawn =
                victim as Pawn;

            List<Hediff> beforeHediffs = null;

            if (pawn != null &&
                pawn.health != null &&
                pawn.health.hediffSet != null)
            {
                beforeHediffs =
                    pawn.health.hediffSet
                        .hediffs
                        .ToList();
            }

            /*
             * Let vanilla injury logic happen first.
             */
            DamageResult result =
                base.Apply(
                    dinfo,
                    victim
                );

            if (pawn == null)
                return result;

            if (pawn.health == null)
                return result;

            if (pawn.Dead)
                return result;

            DamageDefExtension_ApplyHediffToInjuredPart
                extension =
                    def.GetModExtension
                        <DamageDefExtension_ApplyHediffToInjuredPart>();

            if (extension == null)
                return result;

            if (extension.hediff == null)
                return result;

            BodyPartRecord injuredPart =
                FindNewInjuredPart(
                    pawn,
                    beforeHediffs
                );

            /*
             * If no injured part could be found, do not
             * apply the Hediff. This prevents accidental
             * whole-body application.
             */
            if (injuredPart == null)
                return result;

            float severityToAdd =
                extension.fixedSeverity;

            if (extension.severityPerDamageDealt > 0f)
            {
                float damageAmount =
                    result.totalDamageDealt > 0f
                        ? result.totalDamageDealt
                        : dinfo.Amount;

                severityToAdd +=
                    damageAmount *
                    extension.severityPerDamageDealt;
            }

            if (severityToAdd <= 0f)
                return result;

            ApplyOrIncreaseHediff(
                pawn,
                injuredPart,
                extension.hediff,
                severityToAdd
            );

            return result;
        }

        /*
         * Finds the body part of the new injury created
         * by this damage event.
         */
        private BodyPartRecord FindNewInjuredPart(
            Pawn pawn,
            List<Hediff> beforeHediffs)
        {
            if (pawn == null)
                return null;

            if (pawn.health == null)
                return null;

            if (pawn.health.hediffSet == null)
                return null;

            List<Hediff> currentHediffs =
                pawn.health.hediffSet.hediffs;

            for (int i = currentHediffs.Count - 1;
                i >= 0;
                i--)
            {
                Hediff hediff =
                    currentHediffs[i];

                if (hediff == null)
                    continue;

                if (beforeHediffs != null &&
                    beforeHediffs.Contains(hediff))
                {
                    continue;
                }

                if (hediff is Hediff_Injury &&
                    hediff.Part != null)
                {
                    return hediff.Part;
                }
            }

            return null;
        }

        /*
         * Adds the transmutation Hediff to the injured part,
         * or increases severity if that same Hediff already
         * exists on that same part.
         */
        private void ApplyOrIncreaseHediff(
            Pawn pawn,
            BodyPartRecord part,
            HediffDef hediffDef,
            float severityToAdd)
        {
            if (pawn == null)
                return;

            if (part == null)
                return;

            if (hediffDef == null)
                return;

            Hediff existingHediff = null;

            List<Hediff> hediffs =
                pawn.health.hediffSet.hediffs;

            for (int i = 0; i < hediffs.Count; i++)
            {
                Hediff hediff =
                    hediffs[i];

                if (hediff == null)
                    continue;

                if (hediff.def == hediffDef &&
                    hediff.Part == part)
                {
                    existingHediff = hediff;
                    break;
                }
            }

            if (existingHediff != null)
            {
                existingHediff.Severity +=
                    severityToAdd;

                return;
            }

            Hediff newHediff =
                HediffMaker.MakeHediff(
                    hediffDef,
                    pawn,
                    part
                );

            newHediff.Severity =
                severityToAdd;

            pawn.health.AddHediff(
                newHediff,
                part
            );
        }
    }
}