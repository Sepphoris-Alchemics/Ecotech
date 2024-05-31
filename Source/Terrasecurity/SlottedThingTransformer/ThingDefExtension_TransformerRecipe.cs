using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verse;

namespace Terrasecurity
{
    public class ThingDefExtension_TransformerRecipe : DefModExtension
    {
        List<TransformerRecipe> recipes;
        public List<TransformerRecipe> Recipes => recipes;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string error in base.ConfigErrors())
            {
                yield return error;
            }
            if (recipes.NullOrEmpty())
            {
                yield return $"List \"{nameof(recipes)}\" is null or empty";
            }
            else
            {
                foreach (TransformerRecipe recipe in recipes)
                {
                    foreach (string error in recipe.ConfigErrors())
                    {
                        yield return error;
                    }
                }
            }
        }

        public bool AnyRecipeAppliesTo(Thing thing)
        {
            return recipes.Any(recipe => recipe.AppliesTo(thing));
        }
        public bool AnyRecipeAppliesTo(ThingDef thingDef)
        {
            return recipes.Any(recipe => recipe.AppliesTo(thingDef));
        }

        public int FuelCostFor(Thing thing)
        {
            return recipes
                .Where(recipe => recipe.AppliesTo(thing))
                .Sum(recipe => recipe.FuelCount);
        }

        public bool TryDoWork(ThingComp_SlottedThingTransformer transformerComp, Thing inputThing, out Thing producedThing, out int consumedFuel)
        {
            for (int i = 0; i < recipes.Count; i++)
            {
                TransformerRecipe recipe = recipes[i];
                AcceptanceReport report = recipe.TryDoWork(transformerComp, inputThing, out producedThing, out consumedFuel);
                if (report)
                {
                    //Log.Message($"Successfully applied recipe at index {i} and transformed {inputThing.def.defName} with {consumedFuel} fuel into {producedThing.def.defName}");
                    return true;
                }
                //else
                //{
                //    Log.Message($"Could not apply recipe at index {i} for {inputThing.def.defName}: {report.Reason}");
                //}
            }
            producedThing = null;
            consumedFuel = 0;
            return false;
        }
    }

    public class TransformerRecipe
    {
        int fuelCount;
        public int FuelCount => fuelCount;
        ThingDef inputThingDef;
        ThingDef outputThingDef;
        ThingDef outputStuff;

        public bool AppliesTo(Thing inputThing)
        {
            return AppliesTo(inputThing.def);
        }
        public bool AppliesTo(ThingDef inputThing)
        {
            return inputThing == inputThingDef;
        }

        public AcceptanceReport TryDoWork(ThingComp_SlottedThingTransformer transformerComp, Thing inputThing, out Thing producedThing, out int consumedFuel)
        {
            producedThing = null;
            consumedFuel = 0;
            if (transformerComp.CurrentFuelCount < fuelCount)
            {
                return "Terrasecurity_FailureReason_NotEnoughFuel".Translate();
            }
            if (!AppliesTo(inputThing))
            {
                return $"Not Correct Input {inputThing?.def?.defName} vs required {this.inputThingDef}";
            }
            if (!transformerComp.innerContainer.Remove(inputThing))
            {
                return "Terrasecurity_FailureReason_CouldNotRemoveThingFromContainer".Translate();
            }
            inputThing.Destroy();
            producedThing = ExecuteRecipe(out consumedFuel);
            return true;
        }

        private Thing ExecuteRecipe(out int consumedFuel)
        {
            Thing producedThing;
            producedThing = ThingMaker.MakeThing(outputThingDef, outputStuff);
            producedThing = producedThing.TryMakeMinified();

            consumedFuel = fuelCount;

            return producedThing;
        }

        public IEnumerable<string> ConfigErrors()
        {
            if(fuelCount == 0)
            {
                yield return $"{nameof(fuelCount)} must be larger than 0";
            }
            if (inputThingDef == null)
            {
                yield return $"Required field \"{nameof(inputThingDef)}\" is not set";
            }
            if (outputThingDef == null)
            {
                yield return $"Required field \"{nameof(outputThingDef)}\" is not set";
            }
            if(outputThingDef.MadeFromStuff && outputStuff == null)
            {
                yield return $"{nameof(outputThingDef)} is made of stuff, but required field {nameof(outputStuff)} is not set";
            }
        }
    }
}
