using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using Verse;
using Color = UnityEngine.Color;

namespace Ecotech
{
    public class ThingCompProperties_ThingConverter : CompProperties
    {
        private List<ThingDefCountClass> inputThings;
        public ThingDefCountClass InputThing => inputThings[0];
        public int conversionDurationTicks = GenTicks.TickLongInterval;
        List<WeightedThingResult> potentialResults;
        public int timeToFillTicks = 200;
        public int timeToEmptyTicks = 200;
        public bool drawFillableBar = false;
        public Vector3 fillableBarDrawOffset = Vector3.zero;
        public Vector2 fillableBarSize = Vector2.one;
        private Color fillableBarBackgroundColor = Color.black;
        private Color fillableBarFilledColor = Color.white;

        public Material FillableBarBackgroundMaterial;
        public Material FillableBarFilledMaterial;

        public ThingCompProperties_ThingConverter()
        {
            FillableBarBackgroundMaterial = SolidColorMaterials.SimpleSolidColorMaterial(fillableBarBackgroundColor);
            FillableBarFilledMaterial = SolidColorMaterials.SimpleSolidColorMaterial(fillableBarFilledColor);
        }

        public List<Thing> ProduceRandomItems()
        {
            return potentialResults.MakeRandomThings();
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (string error in base.ConfigErrors(parentDef))
            {
                yield return error;
            }
            if (inputThings.Count != 1)
            {
                yield return $"Required list \"{nameof(inputThings)}\" must have exactly 1 entry";
            }
            else
            {
                if (inputThings[0].thingDef == null)
                {
                    yield return $"Entry in \"{nameof(inputThings)}\" must have ThingDef";
                }
            }
            if (potentialResults.NullOrEmpty())
            {
                yield return $"List \"{nameof(potentialResults)}\" is null or empty";
            }
            if (conversionDurationTicks <= 0)
            {
                yield return $"Field \"{nameof(conversionDurationTicks)}\" must be larger than 0";
            }
        }
    }
}
