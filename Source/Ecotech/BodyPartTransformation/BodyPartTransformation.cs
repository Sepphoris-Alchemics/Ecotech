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
     * Standard drop entry.
     */
    public class BodyPartTransformationDrop
    {
        public ThingDef thingDef;

        public IntRange countRange = new IntRange(1, 1);

        public float chance = 1f;
    }

    /*
     * Per-body-part modifiers.
     */
    public class BodyPartTransformationModifier
    {
        public BodyPartDef bodyPart;

        /*
         * Severity gain multiplier.
         */
        public float severityMultiplier = 1f;

        /*
         * Material yield multiplier.
         */
        public float yieldMultiplier = 1f;

        /*
         * Optional letter override.
         */
        public bool sendLetter = true;

        /*
         * Chance to replace normal drops
         * with override drops.
         */
        public float overrideChance = 0f;

        /*
         * Special replacement drops.
         */
        public List<BodyPartTransformationDrop> overrideDrops =
            new List<BodyPartTransformationDrop>();
    }

    /*
     * Hediff properties.
     */
    public class HediffCompProperties_BodyPartTransformation
        : HediffCompProperties
    {
        public List<BodyPartTransformationDrop> drops =
            new List<BodyPartTransformationDrop>();

        public List<BodyPartTransformationModifier>
            bodyPartModifiers =
                new List<BodyPartTransformationModifier>();

        /*
         * Severity required before transformation.
         */
        public float transformSeverity = 1f;

        /*
         * Tick interval.
         * 60 = once per second.
         */
        public int checkIntervalTicks = 60;

        /*
         * Default fallback letter behavior.
         */
        public bool sendLetter = true;

        /*
         * Translation keys.
         */
        public string letterLabelKey;

        public string letterTextKey;

        /*
         * Future-proofing.
         */
        public bool allowArtificialParts = false;
        public bool destroyCorpse = false;

        public HediffCompProperties_BodyPartTransformation()
        {
            compClass =
                typeof(HediffComp_BodyPartTransformation);
        }
    }

    /*
     * Main comp.
     */
    public class HediffComp_BodyPartTransformation
        : HediffComp
    {
        private bool transformed;
        private ThingDef lastTransmutedThing;
        private int lastTransmutedCount;

        public HediffCompProperties_BodyPartTransformation Props =>
            (HediffCompProperties_BodyPartTransformation)props;

        private Pawn Pawn => parent.pawn;

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
            base.CompPostTick(ref severityAdjustment);

            if (transformed)
                return;

            if (Pawn == null)
                return;

            BodyPartRecord part = parent.Part;

            if (part == null)
                return;

            /*
             * Already gone.
             */
            if (Pawn.health.hediffSet.PartIsMissing(part))
                return;

            /*
             * Modifier lookup.
             */
            BodyPartTransformationModifier modifier =
                GetModifierForPart(part);

            /*
             * Severity scaling.
             */
            if (modifier != null)
            {
                severityAdjustment *=
                    modifier.severityMultiplier;
            }

            /*
             * Performance-safe checks.
             */
            if (!Pawn.IsHashIntervalTick(
                Props.checkIntervalTicks))
            {
                return;
            }

            /*
             * Final transformation threshold.
             */
            if (parent.Severity <
                Props.transformSeverity)
            {
                return;
            }

            TransformBodyPart(
                part,
                modifier
            );
        }

        private BodyPartTransformationModifier
            GetModifierForPart(
                BodyPartRecord part)
        {
            if (Props.bodyPartModifiers == null)
                return null;

            foreach (
                BodyPartTransformationModifier modifier
                in Props.bodyPartModifiers)
            {
                if (modifier == null)
                    continue;

                if (modifier.bodyPart == null)
                    continue;

                if (modifier.bodyPart == part.def)
                    return modifier;
            }

            return null;
        }

        private void TransformBodyPart(
            BodyPartRecord part,
            BodyPartTransformationModifier modifier)
        {
            if (transformed)
                return;

            if (Pawn == null)
                return;

            if (part == null)
                return;

            if (Pawn.health.hediffSet.PartIsMissing(part))
                return;

            transformed = true;

            Map map = Pawn.MapHeld;

            IntVec3 position = Pawn.PositionHeld;

            /*
             * Spawn drops BEFORE destruction.
             */
            if (map != null && position.IsValid)
            {
                SpawnDrops(
                    map,
                    position,
                    modifier
                );
            }

            /*
             * Optional letter.
             */
            bool shouldSendLetter =
                modifier != null
                    ? modifier.sendLetter
                    : Props.sendLetter;

            if (shouldSendLetter)
            {
                SendLetter(
                    part,
                    lastTransmutedThing,
                    lastTransmutedCount
                );
            }

            /*
             * Catastrophic destruction.
             */
            DamageInfo dinfo =
                new DamageInfo(
                    DamageDefOf.Cut,
                    99999f,
                    999f,
                    -1f,
                    null,
                    part
                );

            Pawn.health.AddHediff(
                HediffDefOf.MissingBodyPart,
                part
            );

            Pawn.health.RemoveHediff(parent);
            
            if (Props.destroyCorpse)
{
    Corpse corpse = Pawn.Corpse;

    if (corpse != null &&
        !corpse.Destroyed)
    {
        corpse.Destroy();
    }
}
        }

        private void SpawnDrops(
            Map map,
            IntVec3 position,
            BodyPartTransformationModifier modifier)
        {
            bool usedOverride = false;

            /*
             * Try override drops first.
             */
            if (modifier != null &&
                modifier.overrideDrops != null &&
                modifier.overrideDrops.Count > 0)
            {
                float overrideChance =
                    Mathf.Clamp01(
                        modifier.overrideChance
                    );

                if (Rand.Chance(overrideChance))
                {
                    SpawnDropList(
                        modifier.overrideDrops,
                        map,
                        position,
                        1f
                    );

                    usedOverride = true;
                }
            }

            /*
             * Skip normal drops if override succeeded.
             */
            if (usedOverride)
                return;

            SpawnDropList(
                Props.drops,
                map,
                position,
                modifier != null
                    ? modifier.yieldMultiplier
                    : 1f
            );
        }

        private void SpawnDropList(
            List<BodyPartTransformationDrop> dropList,
            Map map,
            IntVec3 position,
            float yieldMultiplier)
        {
            if (dropList == null)
                return;

            foreach (
                BodyPartTransformationDrop entry
                in dropList)
            {
                if (entry == null)
                    continue;

                if (entry.thingDef == null)
                    continue;

                if (!Rand.Chance(entry.chance))
                    continue;

                int count =
                    entry.countRange.RandomInRange;

                count =
                    Mathf.RoundToInt(
                        count * yieldMultiplier
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

                lastTransmutedThing = entry.thingDef;

                lastTransmutedCount = count;

                GenPlace.TryPlaceThing(
                    thing,
                    position,
                    map,
                    ThingPlaceMode.Near
                );
            }
        }

        private void SendLetter(
            BodyPartRecord part,
            ThingDef transmutedThing,
            int transmutedCount)
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
                Props.letterLabelKey.Translate();

            string itemLabel =
                transmutedThing != null
                    ? transmutedThing.label
                    : "unknown material";

            string itemText =
                itemLabel + " x" + transmutedCount;

            string text =
                Props.letterTextKey.Translate(
                    Pawn.Named("PAWN"),
                    part.Label.Named("BODYPART"),
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