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
    public class Plant_SapInfusable : Plant
    {
        private Graphic cachedGraphic;
        private ThingDef cachedSap;

        public override Graphic Graphic
        {
            get
            {
                CompSapInfusable sapComp = this.TryGetComp<CompSapInfusable>();
                ThingDef sap = sapComp?.infusedSapDef;

                // Might remove this later and see if I can figure out how to make the plant assume a sap upon spawning if this doesn't help.
                if (sap == null)
                    return base.Graphic;

                // I think I know how cached works, but I'm not sure if this is how to handle it
                if (cachedGraphic != null && cachedSap == sap)
                    return cachedGraphic;

                var transformComp =
                    this.TryGetComp<ThingComp_PlantTransformOnMaturity>();

                var option = transformComp?.Props?.sapTransformOptions?
                    .FirstOrDefault(o => o.sap == sap && o.graphicData != null);

                if (option?.graphicData == null)
                    return base.Graphic;

                cachedSap = sap;
                cachedGraphic = option.graphicData.Graphic;
                return cachedGraphic;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            cachedGraphic = null;
            cachedSap = null;
        }
    }
}