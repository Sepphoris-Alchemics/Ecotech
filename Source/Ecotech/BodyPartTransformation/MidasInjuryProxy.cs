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
    public class HediffDefExtension_ApplyHediffToSamePart
        : DefModExtension
    {
        public HediffDef hediff;
        public float severityPerInjurySeverity = 1f;
        public float fixedSeverity = 0f;

        public float maxSeverityToAdd = 0f;
    }

    public class Hediff_Injury_ApplyHediffToSamePart
        : Hediff_Injury
    {
        private bool appliedProxyHediff;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(
                ref appliedProxyHediff,
                "appliedProxyHediff",
                false
            );
        }

        public override void PostAdd(
            DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);

            if (appliedProxyHediff)
                return;

            appliedProxyHediff = true;

            if (pawn == null)
                return;

            if (pawn.health == null)
                return;

            if (pawn.health.hediffSet == null)
                return;

            if (Part == null)
                return;

            if (pawn.health.hediffSet.PartIsMissing(Part))
                return;

            HediffDefExtension_ApplyHediffToSamePart extension =
                def.GetModExtension
                    <HediffDefExtension_ApplyHediffToSamePart>();

            if (extension == null)
                return;

            if (extension.hediff == null)
                return;

            float severityToAdd =
                extension.fixedSeverity;

            severityToAdd +=
                Severity *
                extension.severityPerInjurySeverity;

            if (extension.maxSeverityToAdd > 0f &&
                severityToAdd > extension.maxSeverityToAdd)
            {
                severityToAdd =
                    extension.maxSeverityToAdd;
            }

            if (severityToAdd <= 0f)
                return;

            ApplyOrIncreaseHediffOnSamePart(
                extension.hediff,
                severityToAdd
            );
        }

        private void ApplyOrIncreaseHediffOnSamePart(
            HediffDef hediffDef,
            float severityToAdd)
        {
            if (hediffDef == null)
                return;

            List<Hediff> hediffs =
                pawn.health.hediffSet.hediffs;

            for (int i = 0; i < hediffs.Count; i++)
            {
                Hediff existing =
                    hediffs[i];

                if (existing == null)
                    continue;

                if (existing.def == hediffDef &&
                    existing.Part == Part)
                {
                    existing.Severity +=
                        severityToAdd;

                    return;
                }
            }

            Hediff newHediff =
                HediffMaker.MakeHediff(
                    hediffDef,
                    pawn,
                    Part
                );

            newHediff.Severity =
                severityToAdd;

            pawn.health.AddHediff(
                newHediff,
                Part
            );
        }
    }
}