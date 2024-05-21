using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verse;

namespace Terrasecurity
{
    public class PostGameLoadConfigErrors
    {
        static HashSet<ThingDef> thingsToCheckForRecipes = new HashSet<ThingDef>();

        public static void RunPostLoadConfigErrorChecks()
        {
            foreach (string error in PostStartupConfigErrors())
            {
                Log.Error(error);
            }
        }

        public static void AddDefToCheckForRecipe(ThingDef thing)
        {
            thingsToCheckForRecipes.Add(thing);
        }

        private static IEnumerable<string> PostStartupConfigErrors()
        {
            foreach (ThingDef thingDef in thingsToCheckForRecipes)
            {
                if (!Common.AllTransformerRecipes.Any(r => r.AppliesTo(thingDef)))
                {
                    yield return $"No recipe exists to transform {thingDef.defName}. At least one fuel with {nameof(ThingDefExtension_TransformerRecipe)} must exist that accepts this ThingDef as an input thing.";
                }
            }
        }
    }
}
