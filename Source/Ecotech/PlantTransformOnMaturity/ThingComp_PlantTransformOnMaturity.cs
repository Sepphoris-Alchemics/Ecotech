using RimWorld;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace Ecotech
{
    public class ThingComp_PlantTransformOnMaturity : ThingComp
    {
        ThingCompProperties_PlantTransformOnMaturity Props => base.props as ThingCompProperties_PlantTransformOnMaturity;

        Rot4 transformedRotation = Rot4.North;
        public static bool ShowTransformGhost = true;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref transformedRotation, nameof(transformedRotation));
        }

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

            if(plant.LifeStage == PlantLifeStage.Mature && CanTransform)
            {
                Transform();
            }
        }

        private AcceptanceReport CanTransform
        {
            get
            {
                if(Props.TransformedThingDef.IsPlant)
                {
                    // buildings check the occupiedRect, plants do not. As such we must iterate the occupied cells of the transformed thing one at a time
                    foreach(IntVec3 occupiedCell in GenAdj.OccupiedRect(parent.Position, transformedRotation, Props.TransformedThingDef.Size))
                    {
                        AcceptanceReport canPlantAtCell = PlantUtility.CanEverPlantAt(Props.TransformedThingDef, occupiedCell, parent.Map, out _, true);
                        if(!canPlantAtCell)
                        {
                            return canPlantAtCell;
                        }
                    }
                    return AcceptanceReport.WasAccepted;
                }
                else
                {
                    return GenConstruct.CanPlaceBlueprintAt(Props.TransformedThingDef, parent.Position, transformedRotation, parent.Map, false, parent);
                }
            }
        }

        Gizmo_PlantTransform _gizmo;
        Gizmo_PlantTransform TransformGizmo
        {
            get
            {
                if(_gizmo == null)
                {
                    _gizmo = new Gizmo_PlantTransform(this);
                }
                return _gizmo;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach(Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            yield return TransformGizmo;
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder(base.CompInspectStringExtra());

            stringBuilder.AppendLineIfNotEmpty();
            AcceptanceReport canTransform = CanTransform;
            if(canTransform)
            {
                stringBuilder.Append("Ecotech_InspectString_PlantTransformOnMaturity_CanTransform".Translate());
            }
            else
            {
                stringBuilder.Append("Ecotech_InspectString_PlantTransformOnMaturity_CannotTransform".Translate(canTransform.Reason.Named("REASON")));
            }

            return stringBuilder.ToString();
        }

        public void Rotate(RotationDirection direction)
        {
            transformedRotation.Rotate(direction);
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            base.PostDrawExtraSelectionOverlays();
            if(!ShowTransformGhost)
            {
                return;
            }
            UnityEngine.Color ghostColor = CanTransform ? UnityEngine.Color.green : UnityEngine.Color.red;
            IntVec3 position;
            Rot4 rotation;
            if(Props.TransformedThingDef.rotatable)
            {
                position = parent.Position;
                rotation = transformedRotation;
            }
            else
            {
                position = parent.Position;
                GenAdj.AdjustForRotation(ref position, ref Props.TransformedThingDef.size, transformedRotation);
                rotation = Rot4.North;
            }
            GhostDrawer.DrawGhostThing(position, rotation, Props.TransformedThingDef, Props.TransformedThingDef.graphic, ghostColor, AltitudeLayer.Blueprint);
        }

        private void Transform()
        {
            Map map = parent.Map;
            Faction faction = parent.Faction;
            parent.Destroy(DestroyMode.WillReplace);
            Thing thingToSpawn = Props.MakeThing();
            if(thingToSpawn.def.CanHaveFaction)
            {
                thingToSpawn.SetFaction(faction);
            }
            GenSpawn.Spawn(thingToSpawn, parent.Position, map, transformedRotation, WipeMode.FullRefund);
        }
    }
}
