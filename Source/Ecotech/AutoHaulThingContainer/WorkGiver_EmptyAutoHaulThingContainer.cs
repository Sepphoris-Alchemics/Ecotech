using HarmonyLib;
using Mono.Unix.Native;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Ecotech
{
    public class WorkGiver_EmptyAutoHaulThingContainer : WorkGiver_Scanner
    {
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return pawn.Map.listerBuildings.allBuildingsColonist
                .Where(building => building.def.HasAssignableCompFrom(typeof(ThingComp_AutoHaulThingContainer)));
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if(!(t is ThingWithComps thingWithComps))
            {
                return false;
            }
            ThingComp_AutoHaulThingContainer autoHaulComp = thingWithComps.GetComp<ThingComp_AutoHaulThingContainer>();
            AcceptanceReport emptyReport = autoHaulComp.ShouldEmpty(pawn);
            if (!emptyReport)
            {
                JobFailReason.Is(emptyReport.Reason);
                return emptyReport;
            }
            if (!StoreUtility.TryFindBestBetterStorageFor(autoHaulComp.ContainedThing, pawn, pawn.Map, StoragePriority.Unstored, pawn.Faction, out _, out _))
            {
                JobFailReason.Is(HaulAIUtility.NoEmptyPlaceLowerTrans, null);
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
            if (!autoHaulThingContainerComp.ShouldEmpty(pawn))
            {
                return null;
            }
            if (!StoreUtility.TryFindBestBetterStorageFor(autoHaulThingContainerComp.ContainedThing, pawn, pawn.Map, StoragePriority.Unstored, pawn.Faction, out IntVec3 storeCell, out _))
            {
                return null;
            }

            Job job = JobMaker.MakeJob(JobDefOf.EmptyThingContainer, t, autoHaulThingContainerComp.ContainedThing, storeCell);
            return job;
        }
    }
}
