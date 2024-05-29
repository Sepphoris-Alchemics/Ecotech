using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Terrasecurity
{
    public class ThingCompProperties_SlottedThingTransformer : CompProperties_ThingContainer
    {
        public List<ThingDef> validFuelThings;
        public int transformerSlots = 1;
        public int timeToInsertTicks = 100;
        public int timeToRemoveTicks = 100;
        Color baseFuelColor = Color.blue;
        Color insufficientFuelColor = Color.red;
        Color sufficientFuelColor = Color.green;
        // the ticks to wait for until a transformation cycle starts (does NOT include cycle duration!)
        int transformationCycleIntervalTicks = 2000;
        public int TransformationCycleIntervalTicks => transformationCycleIntervalTicks;
        // the ticks to lock input/output for while doing a transformation
        int transformationCycleTicks = 300;
        public int TransformationCycleTicks => transformationCycleTicks;
        public string contentsTranslationKey = "Contents";

        Texture2D _baseFuelTexture;
        public Texture2D BaseFuelTexture
        {
            get
            {
                if (_baseFuelTexture == null)
                    _baseFuelTexture = SolidColorMaterials.NewSolidColorTexture(baseFuelColor);
                return _baseFuelTexture;
            }
        }
        Texture2D _insufficientFuelTextureissingFuelTexture;
        public Texture2D InsufficientFuelTexture
        {
            get
            {
                if (_insufficientFuelTextureissingFuelTexture == null)
                    _insufficientFuelTextureissingFuelTexture = SolidColorMaterials.NewSolidColorTexture(insufficientFuelColor);
                return _insufficientFuelTextureissingFuelTexture;
            }
        }
        Texture2D _sufficientFuelTexture;
        public Texture2D SufficientFuelTexture
        {
            get
            {
                if (_sufficientFuelTexture == null)
                    _sufficientFuelTexture = SolidColorMaterials.NewSolidColorTexture(sufficientFuelColor);
                return _sufficientFuelTexture;
            }
        }

        public ThingCompProperties_SlottedThingTransformer()
        {
            compClass = typeof(ThingComp_SlottedThingTransformer);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (string error in base.ConfigErrors(parentDef))
            {
                yield return error;
            }

            if (validFuelThings.NullOrEmpty())
            {
                yield return $"List \"{nameof(validFuelThings)}\" is null or empty";
            }
            else
            {
                foreach (ThingDef fuelDef in validFuelThings)
                {
                    if (!fuelDef.HasModExtension<ThingDefExtension_TransformerRecipe>())
                    {
                        yield return $"Fuel def {fuelDef.defName} has no {nameof(ThingDefExtension_TransformerRecipe)}";
                    }
                }
            }
        }
    }
}
