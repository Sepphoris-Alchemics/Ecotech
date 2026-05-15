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

        public HediffCompProperties_DropOnDeath()
        {
            compClass = typeof(HediffComp_DropOnDeath);
        }
    }
    public class HediffComp_DropOnDeath : HediffComp
    {
        public HediffCompProperties_DropOnDeath Props =>
            (HediffCompProperties_DropOnDeath)props;

        public override void Notify_PawnDied(
            DamageInfo? dinfo,
            Hediff culprit = null)
        {
            base.Notify_PawnDied(dinfo, culprit);

            //Log.Message("DropOnDeath fired.");

            Pawn pawn = Pawn;

            if (pawn == null)
                return;

            Map map = pawn.Corpse?.Map ?? pawn.Map;

            IntVec3 position = pawn.Corpse?.Position ?? pawn.Position;

            if (map == null || !position.IsValid)
                return;

            if (Props.drops == null || Props.drops.Count == 0)
                return;

            foreach (ItemOnDeathHediff entry in Props.drops)
            {
                if (entry == null || entry.thingDef == null)
                    continue;

                float severity = parent.Severity;

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

                int count = entry.countRange.RandomInRange;

                if (count <= 0)
                    continue;

                Thing thing =
                    ThingMaker.MakeThing(entry.thingDef);

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