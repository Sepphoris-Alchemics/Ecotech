using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verse;

namespace Ecotech
{
    public static class WeightedThingResultUtility
    {
        public static List<Thing> MakeRandomThings(this List<WeightedThingResult> potentialResults)
        {
            return potentialResults
                .RandomElementByWeight(result => result.weight)
                .things
                .Select(MakeThing)
                .ToList();
        }

        public static Thing MakeThing(this ThingDefCountRangeClass thingDefCountRange)
        {
            Thing thing = ThingMaker.MakeThing(thingDefCountRange.thingDef);
            thing.stackCount = thingDefCountRange.countRange.RandomInRange;
            return thing;
        }
    }
}
