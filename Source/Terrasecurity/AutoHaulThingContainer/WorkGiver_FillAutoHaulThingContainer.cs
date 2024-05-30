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
    public class WorkGiver_FillAutoHaulThingContainer : WorkGiver_Scanner
    {
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            var buildings = pawn.Map.listerBuildings.allBuildingsColonist
                .Where(building => IsValidWorkBuilding(building, pawn));
            //Log.Message($"work things: {string.Join(", ", buildings)}");
            return buildings;
        }

        protected virtual bool IsValidWorkBuilding(Building building, Pawn pawn)
        {
            if (!building.def.HasAssignableCompFrom(typeof(ThingComp_AutoHaulThingContainer)))
            {
                return false;
            }
            ThingComp_AutoHaulThingContainer containerComp = building.GetComp<ThingComp_AutoHaulThingContainer>();
            if(containerComp == null)
            {
                return false;
            }
            if (!containerComp.ShouldFill(pawn))
            {
                return false;
            }
            return true;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if(!(t is ThingWithComps thingWithComps))
            {
                return false;
            }
            ThingComp_AutoHaulThingContainer autoHaulComp = thingWithComps.GetComp<ThingComp_AutoHaulThingContainer>();
            AcceptanceReport fillReport = autoHaulComp.ShouldFill(pawn);
            if (!fillReport)
            {
                JobFailReason.Is(fillReport.Reason);
                return fillReport;
            }
            if (autoHaulComp.FindHaulThingFor(pawn) == null)
            {
                return false;
            }
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is ThingWithComps thingWithComps))
            {
                return null;
            }
            ThingComp_AutoHaulThingContainer autoHaulThingContainerComp = thingWithComps.GetComp<ThingComp_AutoHaulThingContainer>();
            if (!autoHaulThingContainerComp.ShouldFill(pawn))
            {
                return null;
            }
            Thing thingToHaul = autoHaulThingContainerComp.FindHaulThingFor(pawn);
            if(thingToHaul == null)
            {
                return null;
            }

            Job job = JobMaker.MakeJob(JobDefOf.HaulToContainer, thingToHaul, t);
            job.count = autoHaulThingContainerComp.HaulCountFor(thingToHaul);
            return job;
        }
    }
}
