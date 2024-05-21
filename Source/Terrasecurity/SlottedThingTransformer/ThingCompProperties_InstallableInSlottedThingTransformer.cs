using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verse;

namespace Terrasecurity
{
    public class ThingCompProperties_InstallableInSlottedThingTransformer : CompProperties
    {
        public ThingCompProperties_InstallableInSlottedThingTransformer()
        {
            compClass = typeof(ThingComp_InstallableInSlottedThingTransformer);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (string error in base.ConfigErrors(parentDef))
            {
                yield return error;
            }
            PostGameLoadConfigErrors.AddDefToCheckForRecipe(parentDef);
        }
    }
}
