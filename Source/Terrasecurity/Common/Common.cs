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
    [StaticConstructorOnStartup]
    public static class Common
    {
        public static JobDef fillThingConverterJobDef = DefDatabase<JobDef>.GetNamed("TS_FillThingConverter");
        public static JobDef emptyThingConverterJobDef = DefDatabase<JobDef>.GetNamed("TS_EmptyThingConverter");
        public static JobDef insertIntoSlottedTransformerJobDef = DefDatabase<JobDef>.GetNamed("TS_InsertIntoSlottedThingTransformer");
        public static Texture2D installableInSlottedThingTransformerGizmoTexture = ContentFinder<Texture2D>.Get("UI/Gizmo/InstallableInSlottedThingTransformerGizmo");

        static List<TransformerRecipe> _allTransformerRecipes;
        public static List<TransformerRecipe> AllTransformerRecipes
        {
            get
            {
                if (_allTransformerRecipes == null)
                {
                    _allTransformerRecipes = new List<TransformerRecipe>();
                    foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefsListForReading)
                    {
                        ThingDefExtension_TransformerRecipe extension = thingDef.GetModExtension<ThingDefExtension_TransformerRecipe>();
                        if (extension == null)
                        {
                            continue;
                        }
                        _allTransformerRecipes.AddRange(extension.Recipes);
                    }
                }
                return _allTransformerRecipes;
            }
        }
    }
}
