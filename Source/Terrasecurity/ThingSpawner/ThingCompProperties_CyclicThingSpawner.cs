using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verse;

namespace Terrasecurity
{
    public class ThingCompProperties_CyclicThingSpawner : CompProperties
    {
        public IntRange cycleDurationRangeTicks = new IntRange(250, 500);
        List<WeightedThingResult> potentialSpawnedThings;
        public bool showNotificationWhenSpawningThings = false;

        public List<Thing> ProduceRandomItems()
        {
            return potentialSpawnedThings.MakeRandomThings();
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (string error in base.ConfigErrors(parentDef))
            {
                yield return error;
            }
            if (potentialSpawnedThings.NullOrEmpty())
            {
                yield return $"List \"{nameof(potentialSpawnedThings)}\" is null or empty";
            }
        }
    }
}
