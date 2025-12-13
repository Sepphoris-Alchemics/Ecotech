using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Ecotech
{
    [StaticConstructorOnStartup]
    public static class Common
    {
        public static JobDef fillThingConverterJobDef = DefDatabase<JobDef>.GetNamed("ET_FillThingConverter");
        public static JobDef emptyThingConverterJobDef = DefDatabase<JobDef>.GetNamed("ET_EmptyThingConverter");
        public static JobDef insertIntoSlottedTransformerJobDef = DefDatabase<JobDef>.GetNamed("ET_InsertIntoSlottedThingTransformer");
        public static JobDef infuseSapJobDef = DefDatabase<JobDef>.GetNamed("ET_InfuseSapJob");
        public static Texture2D installableInSlottedThingTransformerGizmoTexture = ContentFinder<Texture2D>.Get("UI/Gizmo/InstallableInSlottedThingTransformerGizmo");
        public static DesignationDef installInSlottedThingTransformerDesignation = DefDatabase<DesignationDef>.GetNamed("ET_InstallInSlottedThingTransformer");

        static List<TransformerRecipe> _allTransformerRecipes;
        public static List<TransformerRecipe> AllTransformerRecipes
        {
            get
            {
                if(_allTransformerRecipes == null)
                {
                    _allTransformerRecipes = new List<TransformerRecipe>();
                    foreach(ThingDef thingDef in DefDatabase<ThingDef>.AllDefsListForReading)
                    {
                        ThingDefExtension_TransformerRecipe extension = thingDef.GetModExtension<ThingDefExtension_TransformerRecipe>();
                        if(extension == null)
                        {
                            continue;
                        }
                        _allTransformerRecipes.AddRange(extension.Recipes);
                    }
                }
                return _allTransformerRecipes;
            }
        }
        static Common()
        {
            var jd = DefDatabase<JobDef>.GetNamedSilentFail("ET_InfuseSapJob");
            Log.Message("[Ecotech] InfuseSapJob loaded? " + (jd != null));
        }
    }
}
