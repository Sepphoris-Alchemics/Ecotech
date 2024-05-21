using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Terrasecurity
{
    public class JobDriver_InsertIntoSlottedThingTransformer : JobDriver
    {
        TargetIndex thingIndex = TargetIndex.A;
        TargetIndex buildingIndex = TargetIndex.B;

        Building TargetBuilding => base.job.GetTarget(buildingIndex).Thing as Building;
        Thing TargetThing => base.job.GetTarget(thingIndex).Thing;
        ThingComp_SlottedThingTransformer TransformerComp => TargetBuilding.GetComp<ThingComp_SlottedThingTransformer>();

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if(!base.pawn.Reserve(TargetBuilding, base.job, errorOnFailed: errorOnFailed))
            {
                return false;
            }
            if(!base.pawn.Reserve(TargetThing, base.job, stackCount: base.job.count, errorOnFailed: errorOnFailed))
            {
                return false;
            }
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(buildingIndex);
            this.FailOnBurningImmobile(buildingIndex);

            yield return Toils_Goto.GotoThing(thingIndex, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(thingIndex)
                .FailOnSomeonePhysicallyInteracting(thingIndex);
            yield return Toils_Haul.StartCarryThing(thingIndex, subtractNumTakenFromJobCount: true)
                .FailOnDestroyedNullOrForbidden(thingIndex);
            yield return Toils_Goto.GotoThing(buildingIndex, PathEndMode.Touch);
            yield return Toils_General.Wait(TransformerComp.TransformerProps.timeToInsertTicks, buildingIndex)
                .FailOnDestroyedNullOrForbidden(thingIndex)
                .FailOnDestroyedNullOrForbidden(buildingIndex)
                .FailOnCannotTouch(buildingIndex, PathEndMode.Touch)
                .WithProgressBarToilDelay(buildingIndex);
            Toil toil = ToilMaker.MakeToil();
            toil.initAction = delegate
            {
                TransformerComp.TryTake(TargetThing);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return toil;
            yield break;
        }
    }
}
