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
     * Represents a single possible drop entry.
     * Each entry rolls independently.
     */
    public class ItemOnDeathHediff
    {
        // Item to spawn
        public ThingDef thingDef;

        // Min/max amount to spawn
        public IntRange countRange = new IntRange(1, 1);

        // Independent chance to spawn (0.0 -> 1.0)
        public float chance = 1f;
    }

    /*
     * XML properties class for the HediffComp.
     */
    public class HediffCompProperties_DropOnDeath : HediffCompProperties
    {
        // List of possible drops
        public List<ItemOnDeathHediff> drops =
            new List<ItemOnDeathHediff>();

        public HediffCompProperties_DropOnDeath()
        {
            compClass = typeof(HediffComp_DropOnDeath);
        }
    }

    /*
     * HediffComp that spawns items when the pawn dies.
     */
    public class HediffComp_DropOnDeath : HediffComp
    {
        public HediffCompProperties_DropOnDeath Props =>
            (HediffCompProperties_DropOnDeath)props;

        public override void Notify_PawnDied(
            DamageInfo? dinfo,
            Hediff culprit = null)
        {
            base.Notify_PawnDied(dinfo, culprit);

            Log.Message("DropOnDeath fired.");

            Pawn pawn = Pawn;

            if (pawn == null)
                return;

            Map map = pawn.Corpse?.Map ?? pawn.Map;

            IntVec3 position = pawn.Corpse?.Position ?? pawn.Position;

            // No valid spawn location
            if (map == null || !position.IsValid)
                return;

            if (Props.drops == null || Props.drops.Count == 0)
                return;

            foreach (ItemOnDeathHediff entry in Props.drops)
            {
                if (entry == null || entry.thingDef == null)
                    continue;

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