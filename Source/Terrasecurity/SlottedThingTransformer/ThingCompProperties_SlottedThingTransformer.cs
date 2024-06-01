using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static Mono.Security.X509.X520;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UIElements.UxmlAttributeDescription;

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
        int transformationCycleIntervalTicks = -1;
        public int TransformationCycleIntervalTicks => transformationCycleIntervalTicks;

        /// <summary>
        /// Can be used to target exact days.
        /// !!If this field is used, transformationCycleIntervalTicks must not be used.Set to -1 or omit from the Def to use cycle interval ticks!!
        /// The current in-game ticks are modulo'd and added with this value, which allows targeting an exact tick in a cycle.
        /// E.g: 
        ///- Current game tick: 7,572,374
        /// - modulo set to 900,000 (ticks in a season) 
        /// The following math is executed:
        /// 7572374 + (900000 - (7572374 % 900000)) = 8100000
        /// That means the cycle starts at the calculated tick 8100000 (in 527626 ticks)
        /// </summary>
        int transformationCycleIntervalModulo = -1;
        public int TransformationCycleIntervalModulo => transformationCycleIntervalModulo;
        // the ticks to lock input/output for while doing a transformation
        int transformationCycleTicks = 300;
        public int TransformationCycleTicks => transformationCycleTicks;
        public string contentsTranslationKey = "Contents";
        int cycleInspectStringRelativeToAbsoluteThreshold = -1;
        public int CycleInspectStringRelativeToAbsoluteThreshold => cycleInspectStringRelativeToAbsoluteThreshold;

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
            if(transformationCycleIntervalTicks != -1 && transformationCycleIntervalModulo != -1)
            {
                yield return $"Cannot use {nameof(transformationCycleIntervalTicks)} and {nameof(transformationCycleIntervalModulo)} at the same time. Either of these values must be set to -1.";
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
