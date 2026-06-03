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
    
    public class BodyPartTransformationDrop_Full
    {
        public ThingDef thingDef;

        public IntRange countRange =
            new IntRange(1, 1);

        public float chance = 1f;
    }
    public class HediffCompProperties_BodyPartTransformation_Full
        : HediffCompProperties
    {
        public List<BodyPartTransformationDrop_Full>
            drops =
                new List<BodyPartTransformationDrop_Full>();
        public List<HediffDef> blockedByHediffs =
            new List<HediffDef>();
        public float transformSeverity = 1f;
        public int checkIntervalTicks = 60;
        public bool sendLetter = true;
        public bool sendLetterForPlayerFaction = true;

        public bool sendLetterForPrisonersOfColony = true;

        public bool sendLetterForHostiles = false;

        public bool sendLetterForOthers = false;
        public string letterLabelKey;

        public string letterTextKey;
        public bool destroyCorpse = false;

        public HediffCompProperties_BodyPartTransformation_Full()
        {
            compClass =
                typeof(HediffComp_BodyPartTransformation_Full);
        }
    }
    public class HediffComp_BodyPartTransformation_Full
        : HediffComp
    {
        private bool transformed;

        private ThingDef lastTransmutedThing;

        private int lastTransmutedCount;

        public HediffCompProperties_BodyPartTransformation_Full
            Props =>
                (HediffCompProperties_BodyPartTransformation_Full)
                props;

        public override void CompExposeData()
        {
            base.CompExposeData();

            Scribe_Values.Look(
                ref transformed,
                "transformed",
                false
            );
        }

        public override void CompPostTick(
            ref float severityAdjustment)
        {
            base.CompPostTick(
                ref severityAdjustment);

            if (transformed)
                return;

            if (Pawn == null)
                return;

            if (Pawn.Dead)
                return;

            if (IsBlockedByProtectionHediff())
            {
                Pawn.health.RemoveHediff(parent);
                return;
            }

            if (!Pawn.IsHashIntervalTick(
                Props.checkIntervalTicks))
            {
                return;
            }
            if (parent.Severity <
                Props.transformSeverity)
            {
                return;
            }

            TransformWholeBody();
        }

        private bool IsBlockedByProtectionHediff()
        {
            if (Props.blockedByHediffs == null)
                return false;

            if (Props.blockedByHediffs.Count == 0)
                return false;

            if (Pawn == null)
                return false;

            if (Pawn.health == null)
                return false;

            if (Pawn.health.hediffSet == null)
                return false;

            foreach (HediffDef blocker
                in Props.blockedByHediffs)
            {
                if (blocker == null)
                    continue;

                if (Pawn.health.hediffSet
                    .GetFirstHediffOfDef(blocker) != null)
                {
                    return true;
                }
            }

            return false;
        }

        private bool ShouldSendTransmutationLetter()
        {
            if (Pawn == null)
                return false;

            if (Pawn.IsPrisonerOfColony)
                return Props.sendLetterForPrisonersOfColony;

            if (Pawn.Faction == Faction.OfPlayer)
                return Props.sendLetterForPlayerFaction;

            if (Pawn.HostileTo(Faction.OfPlayer))
                return Props.sendLetterForHostiles;

            return Props.sendLetterForOthers;
        }

        private void TransformWholeBody()
        {
            if (transformed)
                return;

            if (Pawn == null)
                return;

            if (Pawn.Dead)
                return;

            transformed = true;

            Map map = Pawn.MapHeld;

            IntVec3 position =
                Pawn.PositionHeld;
            if (map != null &&
                position.IsValid)
            {
                SpawnDropList(
                    Props.drops,
                    map,
                    position,
                    1f
                );
            }
            if (Props.sendLetter &&
                ShouldSendTransmutationLetter())
            {
                SendWholeBodyLetter();
            }

            Pawn.Kill(null);
            if (Props.destroyCorpse)
            {
                Corpse corpse =
                    Pawn.Corpse;

                if (corpse != null &&
                    !corpse.Destroyed)
                {
                    corpse.Destroy();
                }
            }
        }

        private void SpawnDropList(
            List<BodyPartTransformationDrop_Full>
                dropList,
            Map map,
            IntVec3 position,
            float yieldMultiplier)
        {
            if (dropList == null)
                return;

            foreach (
                BodyPartTransformationDrop_Full
                entry
                in dropList)
            {
                if (entry == null)
                    continue;

                if (entry.thingDef == null)
                    continue;

                if (!Rand.Chance(
                    entry.chance))
                {
                    continue;
                }

                int count =
                    entry.countRange
                        .RandomInRange;

                count =
                    Mathf.RoundToInt(
                        count *
                        yieldMultiplier
                    );

                if (count <= 0)
                    continue;

                Thing thing =
                    ThingMaker.MakeThing(
                        entry.thingDef
                    );

                if (thing == null)
                    continue;

                thing.stackCount = count;

                lastTransmutedThing =
                    entry.thingDef;

                lastTransmutedCount =
                    count;

                GenPlace.TryPlaceThing(
                    thing,
                    position,
                    map,
                    ThingPlaceMode.Near
                );
            }
        }

        private void SendWholeBodyLetter()
        {
            if (string.IsNullOrEmpty(
                Props.letterLabelKey))
            {
                return;
            }

            if (string.IsNullOrEmpty(
                Props.letterTextKey))
            {
                return;
            }

            string label =
                Props.letterLabelKey
                    .Translate();

            string itemLabel =
                lastTransmutedThing != null
                    ? lastTransmutedThing.label
                    : "unknown material";

            string itemText =
                itemLabel +
                " x" +
                lastTransmutedCount;

            string text =
                Props.letterTextKey
                    .Translate(
                        Pawn.Named("PAWN"),
                        parent.LabelCap.Named("HEDIFF"),
                        itemText.Named("ITEM")
                    );

            Find.LetterStack.ReceiveLetter(
                label,
                text,
                LetterDefOf.NegativeEvent,
                Pawn
            );
        }
    }
}