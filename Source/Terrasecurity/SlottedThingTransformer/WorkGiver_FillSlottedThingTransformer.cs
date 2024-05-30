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
    public class WorkGiver_FillSlottedThingTransformer : WorkGiver_FillAutoHaulThingContainer
    {
        override protected bool IsValidWorkBuilding(Building building, Pawn pawn)
        {
            if (!building.def.HasAssignableCompFrom(typeof(ThingComp_SlottedThingTransformer)))
            {
                return false;
            }
            ThingComp_SlottedThingTransformer containerComp = building.GetComp<ThingComp_SlottedThingTransformer>();
            if (containerComp == null)
            {
                return false;
            }
            AcceptanceReport report = containerComp.ShouldFill(pawn);
            if (!report)
            {
                Log.Message($"{building} is not valid: {report.Reason}");
                return false;
            }
            return true;
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            if (!pawn.Map.designationManager.AnySpawnedDesignationOfDef(Common.installInSlottedThingTransformerDesignation))
            {
                return true;
            }
            if(base.ShouldSkip(pawn, forced))
            {
                return true;
            }
            return false;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is ThingWithComps thingWithComps))
            {
                return false;
            }
            ThingComp_SlottedThingTransformer transformerComp = thingWithComps.GetComp<ThingComp_SlottedThingTransformer>();
            AcceptanceReport fillReport = transformerComp.ShouldFill(pawn);
            Log.Message($"Report on {t}: {fillReport.Reason}");
            if (!fillReport)
            {
                JobFailReason.Is(fillReport.Reason);
                return fillReport;
            }
            if (transformerComp.FindHaulThingFor(pawn) == null)
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
            ThingComp_SlottedThingTransformer transformerComp = thingWithComps.GetComp<ThingComp_SlottedThingTransformer>();
            if (!transformerComp.ShouldFill(pawn))
            {
                return null;
            }
            Thing thingToHaul = transformerComp.FindHaulThingFor(pawn);
            if (thingToHaul == null)
            {
                return null;
            }

            Job job = JobMaker.MakeJob(Common.insertIntoSlottedTransformerJobDef, thingToHaul, t);
            job.count = transformerComp.HaulCountFor(thingToHaul);
            return job;
        }
    }
}
