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
    public class ItemOnDeathHediff
    {
        public ThingDef thingDef;
        public IntRange countRange = new IntRange(1, 1);
        public float chance = 1f;

        public float? minSeverity;
        public float? maxSeverity;

    }
    public class HediffCompProperties_DropOnDeath : HediffCompProperties
    {
        public List<ItemOnDeathHediff> drops =
            new List<ItemOnDeathHediff>();

        public bool enablePeriodicDrops = false;

        public int periodicDropInterval = 60000;

        public HediffCompProperties_DropOnDeath()
        {
            compClass = typeof(HediffComp_DropOnDeath);
        }
    }
    public class HediffComp_DropOnDeath : HediffComp
    {
        public HediffCompProperties_DropOnDeath Props =>
            (HediffCompProperties_DropOnDeath)props;

        public override void CompPostTick(
            ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (!Props.enablePeriodicDrops)
                return;
            if (Props.periodicDropInterval <= 0)
                return;

            Pawn pawn = Pawn;
            if (pawn == null || pawn.Map == null)
                return;
            if (pawn.Dead)
                return;
            if (Find.TickManager.TicksGame %
                Props.periodicDropInterval != 0)
            {
                return;
            }

            ExecuteDrops(
                pawn.Map,
                pawn.Position
            );
        }
        public override void Notify_PawnDied(
            DamageInfo? dinfo,
            Hediff culprit = null)
        {
            base.Notify_PawnDied(dinfo, culprit);

            Pawn pawn = Pawn;

            if (pawn == null)
                return;

            Map map = pawn.Corpse?.Map ?? pawn.Map;

            IntVec3 position =
                pawn.Corpse?.Position ?? pawn.Position;

            if (map == null || !position.IsValid)
                return;

            ExecuteDrops(
                map,
                position
            );
        }
        private void ExecuteDrops(
            Map map,
            IntVec3 position)
        {
            if (Props.drops == null ||
                Props.drops.Count == 0)
            {
                return;
            }

            float severity = parent.Severity;

            foreach (ItemOnDeathHediff entry in Props.drops)
            {
                if (entry == null ||
                    entry.thingDef == null)
                {
                    continue;
                }

                if (entry.minSeverity.HasValue &&
                    severity < entry.minSeverity.Value)
                {
                    continue;
                }

                if (entry.maxSeverity.HasValue &&
                    severity > entry.maxSeverity.Value)
                {
                    continue;
                }

                if (!Rand.Chance(entry.chance))
                    continue;

                int count =
                    entry.countRange.RandomInRange;

                if (count <= 0)
                    continue;

                Thing thing =
                    ThingMaker.MakeThing(
                        entry.thingDef);

                if (thing == null)
                    continue;

                thing.stackCount = count;

                GenPlace.TryPlaceThing(
                    thing,
                    position,
                    map,
                    ThingPlaceMode.Near
                );
            }
        }
    }
}