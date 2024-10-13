using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verse;

namespace Ecotech
{
    public class ThingCompProperties_PlantTransformOnMaturity : CompProperties
    {
        private ThingDef transformedThing;
        private ThingDef transformedThingStuff;
        private bool randomTransformedThingStuff = false;

        public Thing MakeThing()
        {
            ThingDef stuffDef = null;
            if (transformedThing.MadeFromStuff)
            {
                stuffDef = transformedThingStuff;
                if(stuffDef == null)
                {
                    stuffDef = GenStuff.RandomStuffFor(transformedThing);
                }
            }
            return ThingMaker.MakeThing(transformedThing, stuffDef);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (string error in base.ConfigErrors(parentDef))
            {
                yield return error;
            }
            if (transformedThing == null)
            {
                yield return $"Required field \"{nameof(transformedThing)}\" is not set";
            }
            else
            {
                if (transformedThing.MadeFromStuff)
                {
                    if(transformedThingStuff == null && !randomTransformedThingStuff)
                    {
                        yield return $"Must provide \"{nameof(transformedThingStuff)}\" or set \"{nameof(randomTransformedThingStuff)}\" to \"true\" if \"{nameof(transformedThing)}\" is stuffable";
                    }
                }
                if (!transformedThing.MadeFromStuff && randomTransformedThingStuff)
                {
                    yield return $"Cannot use \"{nameof(randomTransformedThingStuff)}\" if \"{nameof(transformedThing)}\" is not stuffable";
                }
            }
        }
    }
}
