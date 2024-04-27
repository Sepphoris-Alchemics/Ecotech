using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verse;

namespace Terrasecurity
{
    public class ThingComp_PlantTransformOnMaturity : ThingComp
    {
        ThingCompProperties_PlantTransformOnMaturity Props => base.props as ThingCompProperties_PlantTransformOnMaturity;

        /// <summary>
        /// Can't use the CompTickLong. Destroying the parent in the method causes NRE later on, so this method is called from a patch.
        /// </summary>
        //public override void CompTickLong()
        public void TransformTickLong()
        {
            base.CompTickLong();
            if(!(parent is Plant plant))
            {
                return;
            }
            if(plant.LifeStage == PlantLifeStage.Mature)
            {
                Transform();
            }
        }

        private void Transform()
        {
            Map map = parent.Map;
            parent.Destroy(DestroyMode.WillReplace);
            Thing thingToSpawn = Props.MakeThing();
            GenSpawn.Spawn(thingToSpawn, parent.Position, map, WipeMode.FullRefund);
        }
    }
}
