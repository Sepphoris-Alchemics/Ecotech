using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;
using Verse.AI;

namespace Ecotech
{
    public class JobDriver_InfuseSapJob : JobDriver
    {
        private Thing Plant => job.targetA.Thing;
        private Thing Sap => job.targetB.Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Plant, job, 1, -1, null, errorOnFailed)
                && pawn.Reserve(Sap, job, 1, 1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnDestroyedOrNull(TargetIndex.B);

            // VERY IMPORTANT FOR 1.6
            job.count = 1;

            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            yield return Toils_General.Wait(300)
                .WithProgressBarToilDelay(TargetIndex.A);

            yield return new Toil
            {
                initAction = () =>
                {
                    CompSapInfusable comp = Plant.TryGetComp<CompSapInfusable>();
                    if (comp != null)
                    {
                        comp.infusedSapDef = Sap.def;
                        Plant.DirtyMapMesh(Plant.Map);
                        comp.ApplySapStyle();
                    }

                    // Consume carried sap safely
                    if (pawn.carryTracker.CarriedThing != null)
                    {
                        pawn.carryTracker.CarriedThing.Destroy();
                    }
                }
            };
        }
    }
}