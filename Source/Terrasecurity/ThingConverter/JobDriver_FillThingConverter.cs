using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Ecotech
{
    public class JobDriver_FillThingConverter : JobDriver
    {
        TargetIndex buildingIndex = TargetIndex.A;
        TargetIndex fillThingIndex = TargetIndex.B;

        Building TargetBuilding => base.job.GetTarget(buildingIndex).Thing as Building;
        Thing TargetFillThing => base.job.GetTarget(fillThingIndex).Thing;
        ThingComp_ThingConverter TargetConverterComp => TargetBuilding.GetComp<ThingComp_ThingConverter>();

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if(!base.pawn.Reserve(TargetBuilding, base.job, errorOnFailed: errorOnFailed))
            {
                return false;
            }
            if(!base.pawn.Reserve(TargetFillThing, base.job, stackCount: base.job.count, errorOnFailed: errorOnFailed))
            {
                return false;
            }
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(buildingIndex);
            this.FailOnBurningImmobile(buildingIndex);

            yield return Toils_Goto.GotoThing(fillThingIndex, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(fillThingIndex)
                .FailOnSomeonePhysicallyInteracting(fillThingIndex);
            yield return Toils_Haul.StartCarryThing(fillThingIndex, subtractNumTakenFromJobCount: true)
                .FailOnDestroyedNullOrForbidden(fillThingIndex);
            yield return Toils_Goto.GotoThing(buildingIndex, PathEndMode.Touch);
            yield return Toils_General.Wait(TargetConverterComp.Props.timeToFillTicks, buildingIndex)
                .FailOnDestroyedNullOrForbidden(fillThingIndex)
                .FailOnDestroyedNullOrForbidden(buildingIndex)
                .FailOnCannotTouch(buildingIndex, PathEndMode.Touch)
                .WithProgressBarToilDelay(buildingIndex);
            Toil toil = ToilMaker.MakeToil();
            toil.initAction = delegate
            {
                TargetConverterComp.TryTake(TargetFillThing);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return toil;
            yield break;
        }
    }
}
