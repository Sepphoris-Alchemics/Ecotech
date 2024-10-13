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
    public class WorkGiver_EmptyThingConverter : WorkGiver_Scanner
    {
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return pawn.Map.listerBuildings.allBuildingsColonist
                .Where(building => building.def.HasComp(typeof(ThingComp_ThingConverter)));
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if(!(t is ThingWithComps thingWithComps))
            {
                return false;
            }
            ThingComp_ThingConverter converterComp = thingWithComps.GetComp<ThingComp_ThingConverter>();
            return converterComp.CanBeEmptiedBy(pawn);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is ThingWithComps thingWithComps))
            {
                return null;
            }
            ThingComp_ThingConverter converterComp = thingWithComps.GetComp<ThingComp_ThingConverter>();
            if (!converterComp.CanBeEmptiedBy(pawn))
            {
                return null;
            }

            return JobMaker.MakeJob(Common.emptyThingConverterJobDef, t);
        }
    }
}
