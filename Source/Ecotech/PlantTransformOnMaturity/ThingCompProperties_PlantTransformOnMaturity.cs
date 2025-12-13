using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Ecotech
{
    public class ThingCompProperties_PlantTransformOnMaturity : CompProperties
    {
        private ThingDef transformedThing;
        private ThingDef transformedThingStuff;
        private bool randomTransformedThingStuff = false;
        private IntVec2 transformedPositionOffset = IntVec2.Zero;

        public ThingDef TransformedThingDef => transformedThing;
        public IntVec3 TransformedPositionOffset => transformedPositionOffset.ToIntVec3;

        public Thing MakeThing()
        {
            ThingDef stuffDef = null;
            if(transformedThing.MadeFromStuff)
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
            foreach(string error in base.ConfigErrors(parentDef))
            {
                yield return error;
            }
            if(transformedThing == null)
            {
                yield return $"Required field \"{nameof(transformedThing)}\" is not set";
            }
            else
            {
                if(transformedThing.MadeFromStuff)
                {
                    if(transformedThingStuff == null && !randomTransformedThingStuff)
                    {
                        yield return $"Must provide \"{nameof(transformedThingStuff)}\" or set \"{nameof(randomTransformedThingStuff)}\" to \"true\" if \"{nameof(transformedThing)}\" is stuffable";
                    }
                }
                if(!transformedThing.MadeFromStuff && randomTransformedThingStuff)
                {
                    yield return $"Cannot use \"{nameof(randomTransformedThingStuff)}\" if \"{nameof(transformedThing)}\" is not stuffable";
                }
            }
        }
        //Below are the changes I (Sepphoris) added. The code added is to allow the plant to acccept an 'infusion' (an item) during it's growth process. This infusion is read, then can have the plant turn into a different structure.
        //I'm not sure how this interacts with the ghost during growth, but all plants and buildings using this will be the same size, and usually have similar graphics.
        //I did this while you were occupied, to see if I could actually do something right with C#. I did get *alot* of help from ChatGPT, and from the Rimworld server coding wizards.
        //So if things appear weird, inefficient, or just downright wrong, you know why; it's ChatGPTs fault.
        //... and mine, since I caved and decided to use it. Feel free to yell at me for doing so.
        public List<SapTransformOption> sapTransformOptions;

        public ThingDef GetTransformedThingForSap(ThingDef infusedSap)
        {
            // If there is no infusion item, then it will just transform into the def that's already defined.
            if (infusedSap == null || sapTransformOptions == null)
                return transformedThing;

            foreach (SapTransformOption option in sapTransformOptions)
            {
                if (option.sap == infusedSap && option.result != null)
                    return option.result;
            }

            return transformedThing;
        }
        public Thing MakeThing(ThingDef infusedSap)
        {
            ThingDef thingDefToMake = GetTransformedThingForSap(infusedSap);

            ThingDef stuffDef = null;
            if (thingDefToMake.MadeFromStuff)
            {
                stuffDef = transformedThingStuff;
                if (stuffDef == null)
                {
                    stuffDef = GenStuff.RandomStuffFor(thingDefToMake);
                }
            }

            return ThingMaker.MakeThing(thingDefToMake, stuffDef);
        }
        public ThingCompProperties_PlantTransformOnMaturity()
        {
            compClass = typeof(ThingComp_PlantTransformOnMaturity);
        }
    }
}
