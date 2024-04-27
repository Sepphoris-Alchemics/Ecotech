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
    public class JobDriver_EmptyThingConverter : JobDriver
    {
        TargetIndex buildingIndex = TargetIndex.A;

        Building TargetBuilding => base.job.GetTarget(buildingIndex).Thing as Building;
        ThingComp_ThingConverter TargetConverterComp => TargetBuilding.GetComp<ThingComp_ThingConverter>();

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if(!base.pawn.Reserve(TargetBuilding, base.job, errorOnFailed: errorOnFailed))
            {
                return false;
            }
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(buildingIndex);
            this.FailOnBurningImmobile(buildingIndex);

            yield return Toils_Goto.GotoThing(buildingIndex, PathEndMode.Touch);
            yield return Toils_General.Wait(TargetConverterComp.Props.timeToEmptyTicks, buildingIndex)
                .FailOnDestroyedNullOrForbidden(buildingIndex)
                .FailOnCannotTouch(buildingIndex, PathEndMode.Touch)
                .WithProgressBarToilDelay(buildingIndex);
            Toil toil = ToilMaker.MakeToil();
            toil.initAction = delegate
            {
                TargetConverterComp.SpawnContents(base.pawn.Position, base.pawn.Map);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return toil;
            yield break;
        }
    }
}
