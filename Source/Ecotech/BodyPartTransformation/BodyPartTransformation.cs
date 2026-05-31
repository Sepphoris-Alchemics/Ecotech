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
    public class BodyPartTransformationDrop
    {
        public ThingDef thingDef;

        public IntRange countRange =
            new IntRange(1, 1);

        public float chance = 1f;
    }
    public class BodyPartTransformationModifier
    {
        public BodyPartDef bodyPart;
        public float severityMultiplier = 1f;
        public float yieldMultiplier = 1f;
        public bool sendLetter = true;
        public float overrideChance = 0f;

        public List<BodyPartTransformationDrop>
            overrideDrops =
                new List<BodyPartTransformationDrop>();
    }
    public class HediffCompProperties_BodyPartTransformation
        : HediffCompProperties
    {
        public List<BodyPartTransformationDrop>
            drops =
                new List<BodyPartTransformationDrop>();

        public List<BodyPartTransformationModifier>
            bodyPartModifiers =
                new List<BodyPartTransformationModifier>();

        public List<HediffDef> blockedByHediffs =
            new List<HediffDef>();

        public float transformSeverity = 1f;

        public bool systemicTransformation = false;

        public int checkIntervalTicks = 60;

        public bool sendLetter = true;

        public string letterLabelKey;

        public string letterTextKey;

        public bool sendLetterForPlayerFaction = true;

        public bool sendLetterForPrisonersOfColony = true;

        public bool sendLetterForHostiles = false;

        public bool sendLetterForOthers = false;
        public bool allowArtificialParts = false;
        //Just in case I want to take over artificial parts later, since Midas Touch would transmute metals.

        public bool destroyCorpse = false;

        public HediffCompProperties_BodyPartTransformation()
        {
            compClass =
                typeof(HediffComp_BodyPartTransformation);
        }
    }

    public class HediffComp_BodyPartTransformation
        : HediffComp
    {
        private bool transformed;

        private ThingDef lastTransmutedThing;

        private int lastTransmutedCount;

        public HediffCompProperties_BodyPartTransformation
            Props =>
                (HediffCompProperties_BodyPartTransformation)
                props;

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
            base.CompPostTick(
                ref severityAdjustment);

            if (transformed)
                return;

            if (Pawn == null)
                return;

            if (IsBlockedByProtectionHediff())
            {
                Pawn.health.RemoveHediff(parent);
                return;
            }
            if (Props.systemicTransformation)
            {
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

                return;
            }

            BodyPartRecord part =
                parent.Part;

            if (part == null)
                return;

            if (Pawn.health.hediffSet
                .PartIsMissing(part))
            {
                return;
            }

            BodyPartTransformationModifier
                modifier =
                    GetModifierForPart(part);
            if (modifier != null)
            {
                severityAdjustment *=
                    modifier.severityMultiplier;
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

            TransformBodyPart(
                part,
                modifier
            );
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

                //To-do: look into a better way for hediff handling
                if (Pawn.health.hediffSet
                    .GetFirstHediffOfDef(blocker) != null)
                {
                    return true;
                }
            }

            return false;
        }

        private void TransformWholeBody()
        {
            if (transformed)
                return;

            if (Pawn == null)
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

            string text =
                Props.letterTextKey
                    .Translate(
                        Pawn.Named("PAWN"),
                        parent.LabelCap
                            .Named("HEDIFF")
                    );

            Find.LetterStack.ReceiveLetter(
                label,
                text,
                LetterDefOf.NegativeEvent,
                Pawn
            );
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

        private BodyPartTransformationModifier
            GetModifierForPart(
                BodyPartRecord part)
        {
            if (Props.bodyPartModifiers == null)
                return null;

            foreach (
                BodyPartTransformationModifier
                modifier
                in Props.bodyPartModifiers)
            {
                if (modifier == null)
                    continue;

                if (modifier.bodyPart == null)
                    continue;

                if (modifier.bodyPart ==
                    part.def)
                {
                    return modifier;
                }
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

            if (Pawn.health.hediffSet
                .PartIsMissing(part))
            {
                return;
            }

            transformed = true;

            Map map = Pawn.MapHeld;

            IntVec3 position =
                Pawn.PositionHeld;

            if (map != null &&
                position.IsValid)
            {
                SpawnDrops(
                    map,
                    position,
                    modifier
                );
            }

            bool shouldSendLetter =
                modifier != null
                    ? modifier.sendLetter
                    : Props.sendLetter;

            if (shouldSendLetter &&
    ShouldSendTransmutationLetter())
            {
                SendLetter(
                    part,
                    lastTransmutedThing,
                    lastTransmutedCount
                );
            }

            Pawn.health.AddHediff(
                HediffDefOf.MissingBodyPart,
                part
            );

            Pawn.health.RemoveHediff(parent);

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

        private void SpawnDrops(
            Map map,
            IntVec3 position,
            BodyPartTransformationModifier modifier)
        {
            bool usedOverride = false;

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
            List<BodyPartTransformationDrop>
                dropList,
            Map map,
            IntVec3 position,
            float yieldMultiplier)
        {
            if (dropList == null)
                return;

            foreach (
                BodyPartTransformationDrop
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
                Props.letterLabelKey
                    .Translate();

            string itemLabel =
                transmutedThing != null
                    ? transmutedThing.label
                    : "unknown material";

            string itemText =
                itemLabel +
                " x" +
                transmutedCount;

            string text =
                Props.letterTextKey
                    .Translate(
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