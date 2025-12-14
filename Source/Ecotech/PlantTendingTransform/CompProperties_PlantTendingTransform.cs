using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;
using Verse.AI;
using UnityEngine;

namespace Ecotech
{
    public class CompProperties_SapInfusable : CompProperties
    {
        public List<ThingDef> supportedSaps;
        public CompProperties_SapInfusable()
        {
            compClass = typeof(CompSapInfusable);
        }
    }

    public class Comp_SapInfusedOverlay : ThingComp
    {
        private CompSapInfusable sapComp;
        private ThingComp_PlantTransformOnMaturity transformComp;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            sapComp = parent.GetComp<CompSapInfusable>();
            transformComp = parent.GetComp<ThingComp_PlantTransformOnMaturity>();
        }

        public override void PostDraw()
        {
            base.PostDraw();

            if (sapComp?.infusedSapDef == null)
                return;

            if (transformComp?.Props?.sapTransformOptions == null)
                return;

            Plant plant = parent as Plant;
            if (plant == null)
                return;

            SapTransformOption option = transformComp.Props.sapTransformOptions
                .FirstOrDefault(o => o.sap == sapComp.infusedSapDef);

            if (option?.graphicData == null)
                return;

            float growth =
                parent.def.plant.visualSizeRange
                    .LerpThroughRange(plant.Growth);

            Graphic graphic = option.graphicData.Graphic;

            graphic.drawSize = new Vector2(growth, growth);
            graphic.data.drawOffset.z = growth / 2.5f;

            graphic.Draw(parent.DrawPos, parent.Rotation, parent);
        }
    }
}