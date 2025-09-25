using RimWorld;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using Verse;
using Verse.Noise;

namespace Ecotech
{
    public class ThingComp_PlantTransformOnMaturity : ThingComp
    {
        public static bool ShowTransformGhost = true;

        Rot4 transformedRotation = Rot4.North;
        /// <summary>
        /// Plants cannot have factions. But buildings created by transforming plants may have factions.
        /// </summary>
        Faction potentialTransformedFaction;

        public ThingCompProperties_PlantTransformOnMaturity Props => base.props as ThingCompProperties_PlantTransformOnMaturity;
        IntVec3 TransformedOffset => Props.TransformedPositionOffset.RotatedBy(transformedRotation);
        IntVec3 TransformedPosition => parent.Position + TransformedOffset;
        public Faction PotentialTransformedFaction
        {
            set => potentialTransformedFaction = value;
        }
        IEnumerable<IntVec3> TransformedOccupiedCells => GenAdj.CellsOccupiedBy(TransformedPosition, transformedRotation, Props.TransformedThingDef.Size);

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref transformedRotation, nameof(transformedRotation));
            Scribe_References.Look(ref potentialTransformedFaction, nameof(potentialTransformedFaction));
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
                    foreach(IntVec3 occupiedCell in GenAdj.OccupiedRect(TransformedPosition, transformedRotation, Props.TransformedThingDef.Size))
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
                    return GenConstruct.CanPlaceBlueprintAt(Props.TransformedThingDef, TransformedPosition, transformedRotation, parent.Map, false, parent);
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
                position = TransformedPosition;
                rotation = transformedRotation;
            }
            else
            {
                position = TransformedPosition;
                GenAdj.AdjustForRotation(ref position, ref Props.TransformedThingDef.size, transformedRotation);
                rotation = Rot4.North;
            }
            GhostDrawer.DrawGhostThing(position, rotation, Props.TransformedThingDef, Props.TransformedThingDef.graphic, ghostColor, AltitudeLayer.Blueprint);
            GenDraw.DrawFieldEdges(GenAdj.CellsOccupiedBy(position, rotation, Props.TransformedThingDef.Size).ToList(), Color.white);
        }

        private void Transform()
        {
            Map map = parent.Map;
            parent.Destroy(DestroyMode.WillReplace);
            Thing thingToSpawn = Props.MakeThing();

            SetFaction(thingToSpawn);
            GenSpawn.Spawn(thingToSpawn, TransformedPosition, map, transformedRotation, WipeMode.FullRefund);
            WipeOtherPlants(thingToSpawn, map);
        }

        private void SetFaction(Thing spawnedThing)
        {
            if(spawnedThing.def.CanHaveFaction)
            {
                spawnedThing.SetFaction(potentialTransformedFaction);
            }
            // if the produced thing is also a plant that transforms, pass the faction onto that plant
            ThingComp_PlantTransformOnMaturity spawnedComp = spawnedThing.TryGetComp<ThingComp_PlantTransformOnMaturity>();
            if(spawnedComp != null)
            {
                spawnedComp.PotentialTransformedFaction = potentialTransformedFaction;
            }
        }

        private void WipeOtherPlants(Thing spawnedThing, Map map)
        {
            foreach(IntVec3 occupiedCell in spawnedThing.OccupiedRect())
            {
                List<Thing> otherThingsAtCell = occupiedCell.GetThingList(map);
                foreach(Thing otherThing in otherThingsAtCell)
                {
                    if(otherThing == spawnedThing)
                    {
                        continue;
                    }
                    if(!otherThing.def.IsPlant)
                    {
                        continue;
                    }
                    otherThing.Destroy(DestroyMode.Vanish);
                }
            }
        }
    }
}
