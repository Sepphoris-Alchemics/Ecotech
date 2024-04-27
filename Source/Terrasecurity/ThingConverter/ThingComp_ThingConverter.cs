using HarmonyLib;
using NAudio.Utils;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Drawing;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Terrasecurity
{
    public class ThingComp_ThingConverter : ThingComp
    {
        int currentConversionTicks = 0;
        int RemainingConversionTicks => Props.conversionDurationTicks - currentConversionTicks;
        float ConversionProgress => 1 - ((float)RemainingConversionTicks / Props.conversionDurationTicks);
        int inputCount = 0;
        int InputCountRequiredToStart => Props.InputThing.count - inputCount;
        string FormattedInputContents => $"{inputCount}/{Props.InputThing.count} x {Props.InputThing.thingDef.label}";
        string FormattedOutputContents
        {
            get
            {
                if (outputContents.NullOrEmpty())
                {
                    return "";
                }
                return string.Join(", ", outputContents.Select(output => output.LabelCap));
            }
        }
        List<Thing> outputContents = new List<Thing>();
        bool isCurrentlyConverting = false;
        public ThingCompProperties_ThingConverter Props => props as ThingCompProperties_ThingConverter;
        
        public bool CanBeFilledOrEmtpiedBy(Pawn pawn)
        {
            if (isCurrentlyConverting)
            {
                JobFailReason.Is("CurrentlyConverting");
                return false;
            }
            if (pawn.Map.designationManager.DesignationOn(parent, DesignationDefOf.Deconstruct) != null)
            {
                JobFailReason.Is("DesignatedForDeconstruct");
                return false;
            }
            if (parent.IsForbidden(pawn))
            {
                JobFailReason.Is("IsForbidden");
                return false;
            }
            if (!pawn.CanReserve(parent))
            {
                JobFailReason.Is("CannotReserve");
                return false;
            }
            return true;
        }

        public bool CanBeFilledBy(Pawn pawn, out Thing thingToFillWith, out int thingCount)
        {
            thingToFillWith = null;
            thingCount = 0;

            if (!CanBeFilledOrEmtpiedBy(pawn))
            {
                return false;
            }
            if (!outputContents.NullOrEmpty())
            {
                JobFailReason.Is("NeedsToBeEmptied");
                return false;
            }
            if (inputCount >= Props.InputThing.count)
            {
                JobFailReason.Is("AlreadyFull");
                return false;
            }
            thingToFillWith = FindInputFor(pawn, out thingCount);
            if (thingToFillWith == null)
            {
                JobFailReason.Is("NoItemToFillWith");
                return false;
            }
            return true;
        }

        public bool CanBeEmptiedBy(Pawn pawn)
        {
            if (!CanBeFilledOrEmtpiedBy(pawn))
            {
                return false;
            }
            if (outputContents.NullOrEmpty())
            {
                return false;
            }
            return true;
        }

        private Thing FindInputFor(Pawn pawn, out int thingCount)
        {
            ThingRequest request = ThingRequest.ForDef(Props.InputThing.thingDef);
            TraverseParms traverseParms = TraverseParms.For(pawn);
            Predicate<Thing> validator = (Thing thing) => IsValidInput(pawn, thing);
            Thing foundThing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, request, PathEndMode.ClosestTouch, traverseParms, validator: validator);
            if(foundThing == null)
            {
                thingCount = 0;
                return null;
            }
            thingCount = Math.Min(foundThing.stackCount, InputCountRequiredToStart);
            return foundThing;
        }
        private bool IsValidInput(Pawn seeker, Thing thing)
        {
            if (thing.IsForbidden(seeker))
            {
                return false;
            }
            if (!seeker.CanReserve(thing))
            {
                return false;
            }
            return true;
        }

        private bool ContainsAllThingsRequiredForConversion => inputCount >= Props.InputThing.count;

        public override void CompTickRare()
        {
            base.CompTickRare();
            if (!isCurrentlyConverting)
            {
                return;
            }
            currentConversionTicks += GenTicks.TickRareInterval;
            if (currentConversionTicks > Props.conversionDurationTicks)
            {
                FinishConversion();
            }
        }

        public AcceptanceReport TryStartConversion()
        {
            if (isCurrentlyConverting)
            {
                return "AlreadyConverting";
            }
            if (!ContainsAllThingsRequiredForConversion)
            {
                return "NotContainingRequiredThings";
            }
            isCurrentlyConverting = true;
            return true;
        }

        public AcceptanceReport TryTake(Thing thing)
        {
            if (thing.def != Props.InputThing.thingDef)
            {
                return "InvalidThingDef";
            }
            int requiredCount = Props.InputThing.count - inputCount;
            int countToTake = Math.Min(requiredCount, thing.stackCount);
            inputCount += countToTake;
            if (countToTake >= thing.stackCount)
            {
                thing.Destroy();
            }
            if(InputCountRequiredToStart == 0)
            {
                TryStartConversion();
            }
            return true;
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            SpawnContents(parent.Position, previousMap);
        }

        public void SpawnContents(IntVec3 position, Map map)
        {
            ThingDef thingDef = Props.InputThing.thingDef;
            int stackCountPerThing = thingDef.stackLimit;
            while (inputCount > 0)
            {
                int countToRemove = Math.Min(inputCount, stackCountPerThing);
                Thing thingToSpawn = ThingMaker.MakeThing(thingDef);
                thingToSpawn.stackCount = countToRemove;
                GenSpawn.Spawn(thingToSpawn, position, map);
                inputCount -= countToRemove;
            }
            foreach (Thing item in outputContents)
            {
                GenSpawn.Spawn(item, position, map);
            }
            outputContents.Clear();
        }

        private void FinishConversion()
        {
            currentConversionTicks = 0;
            isCurrentlyConverting = false;
            inputCount = 0;
            outputContents = Props.ProduceRandomItems();
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder text = new StringBuilder();
            text.Append(base.CompInspectStringExtra());

            if (text.Length != 0)
            {
                text.AppendLine();
            }

            if (!outputContents.NullOrEmpty())
            {
                text.AppendLine("Terrasecurity_InspectString_ConverterFinished".Translate());
                text.Append("Terrasecurity_InspectString_ConverterContents".Translate(FormattedOutputContents.Named("CONTENTS")));
            }
            else if (isCurrentlyConverting)
            {
                text.AppendLine("Terrasecurity_InspectString_ConverterTimeRemaining".Translate(RemainingConversionTicks.ToStringTicksToPeriod().Named("TIME")));
                text.Append("Terrasecurity_InspectString_ConverterContents".Translate(FormattedInputContents.Named("CONTENTS")));
            }
            else
            {
                text.Append("Terrasecurity_InspectString_ConverterContents".Translate(FormattedInputContents.Named("CONTENTS")));
            }

            return text.ToString();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            if (!DebugSettings.ShowDevGizmos)
            {
                yield break;
            }

            yield return new Command_Action()
            {
                defaultLabel = "Dev: Fill",
                action = () =>
                {
                    inputCount = Props.InputThing.count;
                    AcceptanceReport report = TryStartConversion();
                    if (!report)
                    {
                        Log.Warning($"could not start conversion: {report.Reason}");
                    }
                }
            };
            yield return new Command_Action()
            {
                defaultLabel = "Dev: Finish",
                action = () =>
                {
                    if (!isCurrentlyConverting)
                    {
                        Log.Warning($"Coult not finish conversion");
                    }
                    FinishConversion();
                }
            };
            yield return new Command_Action()
            {
                defaultLabel = "Dev: Empty",
                action = () =>
                {
                    SpawnContents(parent.Position, parent.Map);
                }
            };
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (Props.drawFillableBar)
            {
                DrawFillableBar();
            }
        }

        private void DrawFillableBar()
        {
            Vector3 drawPos = parent.DrawPos;
            drawPos += Props.fillableBarDrawOffset;
            GenDraw.DrawFillableBar(new GenDraw.FillableBarRequest
            {
                center = drawPos,
                size = Props.fillableBarSize,
                fillPercent = ConversionProgress,
                filledMat = Props.FillableBarFilledMaterial,
                unfilledMat = Props.FillableBarBackgroundMaterial,
                margin = 0.1f,
                rotation = Rot4.North
            });
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref currentConversionTicks, nameof(currentConversionTicks));
            Scribe_Values.Look(ref inputCount, nameof(inputCount));
            Scribe_Collections.Look(ref outputContents, nameof(outputContents), LookMode.Deep);
            Scribe_Values.Look(ref isCurrentlyConverting, nameof(isCurrentlyConverting));
        }
    }
}
