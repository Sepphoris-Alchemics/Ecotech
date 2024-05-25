using HarmonyLib;
using Mono.Security.Protocol.Tls;
using RimWorld;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Unity.Jobs;
using UnityEngine;
using Verse;
using static HarmonyLib.Code;
using static UnityEngine.GraphicsBuffer;

namespace Terrasecurity
{
    public class ThingComp_SlottedThingTransformer : CompThingContainer
    {
        [Unsaved]
        public ThingComp_MonoThingContainer fuelStorageComp;
        public List<Thing> slottedThings;
        public int totalFuelCost;
        bool isTransforming = false;
        public bool AllowsInteractions => !isTransforming;
        int transformationStartTick = -1;
        int transformationEndTick = -1;

        public override void PostExposeData()
        {
            Scribe.EnterNode(nameof(ThingComp_SlottedThingTransformer));
            base.PostExposeData();
            Scribe_Collections.Look(ref slottedThings, nameof(slottedThings), LookMode.Reference);
            Scribe_Values.Look(ref totalFuelCost, nameof(totalFuelCost), 0);
            Scribe_Values.Look(ref isTransforming, nameof(isTransforming), false);
            Scribe_Values.Look(ref transformationStartTick, nameof(transformationStartTick), -1);
            Scribe_Values.Look(ref transformationEndTick, nameof(transformationEndTick), -1);
            Scribe.ExitNode();
            if(Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if(slottedThings == null)
                {
                    slottedThings = new List<Thing>();
                }
                slottedThings.Capacity = TransformerProps.transformerSlots;
            }
        }

        public ThingCompProperties_SlottedThingTransformer TransformerProps => base.props as ThingCompProperties_SlottedThingTransformer;
        public int CurrentFuelCount => fuelStorageComp.TotalStackCount;
        public ThingDef CurrentFuelDef => fuelStorageComp.ContainedThing?.def;
        public ThingDefExtension_TransformerRecipe FuelRecipes => CurrentFuelDef?.GetModExtension<ThingDefExtension_TransformerRecipe>();
        public bool HasFuel => CurrentFuelDef != null;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            fuelStorageComp = parent.GetComp<ThingComp_MonoThingContainer>();
            if(slottedThings == null)
            {
                slottedThings = new List<Thing>(TransformerProps.transformerSlots);
                for (int i = 0; i < slottedThings.Capacity; i++)
                {
                    slottedThings.Add(null);
                }
            }
            ProgressCycle();
        }

        public AcceptanceReport CanTake(Thing thing)
        {
            if (!base.Accepts(thing))
            {
                return "base.Accepts does not allow";
            }
            if(CurrentFuelCount == 0)
            {
                return "NoFuel";
            }
            if (!FuelRecipes.AnyRecipeAppliesTo(thing))
            {
                return "NoRecipeApplies";
            }
            return true;
        }

        public AcceptanceReport TryTake(Thing thing)
        {
            for (int i = 0; i < slottedThings.Capacity; i++)
            {
                if (slottedThings[i] == null)
                {
                    bool isThingAddedToContainer = base.innerContainer.TryAddOrTransfer(thing);
                    if (!isThingAddedToContainer)
                    {
                        return "CouldNotAddToInnerContainer";
                    }
                    slottedThings[i] = thing;
                    RecacheFuelCost();
                    return true;
                }
            }
            return "NoSlotAvailable";
        }
        public AcceptanceReport TryRemove(Thing thing)
        {
            // tricky. Some things may be stackable, so they merge the stacks together and become one thing. Using SplitOff we always get only a single stack-count
            Thing removedThing = base.ContainedThing.SplitOff(1);
            if(removedThing == null)
            {
                return "CouldNotRemoveOrSplitFromInnerContainer";
            }
            int slotIndex = slottedThings.IndexOf(thing);
            if(slotIndex < 0)
            {
                return "CouldNotDetermineSlotIndex";
            }
            slottedThings[slotIndex] = null;
            GenPlace.TryPlaceThing(removedThing, parent.Position, parent.Map, ThingPlaceMode.Near);
            RecacheFuelCost();
            return true;
        }

        private void RecacheFuelCost()
        {
            totalFuelCost = slottedThings.Sum(t => t == null ? 0 : FuelRecipes.FuelCostFor(t));
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            ProgressCycle();
        }

        private void ProgressCycle()
        {
            if (transformationStartTick == -1)
            {
                transformationStartTick = GenTicks.TicksGame + TransformerProps.TransformationCycleIntervalTicks;
            }
            if (isTransforming)
            {
                if (GenTicks.TicksGame < transformationEndTick)
                {
                    return;
                }
                EndTransformationCycle();
            }
            else
            {
                if (GenTicks.TicksGame < transformationStartTick)
                {
                    return;
                }
                StartTransformationCycle();
            }
        }

        private void StartTransformationCycle()
        {
            isTransforming = true;
            transformationEndTick = GenTicks.TicksGame + TransformerProps.TransformationCycleTicks;
            fuelStorageComp.SetCanEmpty(false);
            fuelStorageComp.SetCanFill(false);
        }

        private void EndTransformationCycle()
        {
            isTransforming = false;
            DoTransformations();
            transformationStartTick = GenTicks.TicksGame + TransformerProps.TransformationCycleIntervalTicks;
            fuelStorageComp.SetCanEmpty(true);
            fuelStorageComp.SetCanFill(true);
        }

        public void DoTransformations()
        {
            if(CurrentFuelCount <= 0)
            {
                //Log.Message($"Ended transformation, 0 fuel available");
                return;
            }
            ThingDefExtension_TransformerRecipe recipeExtension = CurrentFuelDef.GetModExtension<ThingDefExtension_TransformerRecipe>();
            for (int i = 0; i < slottedThings.Capacity; i++)
            {
                //Log.Message($"Trying to retrieve index {i} for list count {slottedThings.Count} list capacity {slottedThings.Capacity}");
                Thing slotThing = slottedThings[i];
                if(slotThing == null)
                {
                    continue;
                }
                if(recipeExtension.TryDoWork(this, slotThing, out Thing producedThing, out int consumedFuel))
                {
                    GenPlace.TryPlaceThing(producedThing, parent.Position, parent.Map, ThingPlaceMode.Near);
                    slottedThings[i] = null;
                    //Log.Message($"Slot {i} consumed fuel: {consumedFuel}");
                    fuelStorageComp.ContainedThing.stackCount -= consumedFuel;
                }
            }
            RecacheFuelCost();
        }

        public override string CompInspectStringExtra()
        {
            string text = base.CompInspectStringExtra();
            if(text != "")
            {
                text += "\n";
            }
            if(isTransforming)
            {
                string formattedDuration = (transformationEndTick - GenTicks.TicksGame).ToStringTicksToPeriodVerbose();
                text += "Terrasecurity_Gizmo_SlottedThingConverter_TransformationCycleEndsIn".Translate(formattedDuration.Named("FORMATTEDDURATION"));
            }
            else
            {
                string formattedDuration = (transformationStartTick - GenTicks.TicksGame).ToStringTicksToPeriodVerbose();
                text += "Terrasecurity_Gizmo_SlottedThingConverter_TransformationCycleStartsIn".Translate(formattedDuration.Named("FORMATTEDDURATION"));
            }
            //text += $"\nstart: {transformationStartTick}";
            //text += $"\nend: {transformationEndTick}";
            //text += $"\ntransforming ? {isTransforming}";
            return text;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            yield return TransformerGizmo;
            if (Prefs.DevMode)
            {
                yield return new Command_Action()
                {
                    defaultLabel = "Dev: Force Transform now",
                    action = DoTransformations
                };
            }
        }

        public void SwapSlots(int indexA, int indexB)
        {
            //Log.Message($"Swapping slot {indexA} and {indexB}");
            Thing thingA = slottedThings[indexA];
            Thing thingB = slottedThings[indexB];
            slottedThings[indexB] = thingA;
            slottedThings[indexA] = thingB;
        }

        Gizmo _transformerGizmo;
        private Gizmo TransformerGizmo
        {
            get
            {
                if(_transformerGizmo == null)
                {
                    _transformerGizmo = new Gizmo_SlottedThingTransformer(parent as Building);
                }
                return _transformerGizmo;
            }
        }
    }
}
