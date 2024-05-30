using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using static UnityEngine.GraphicsBuffer;

namespace Terrasecurity
{
    [StaticConstructorOnStartup]
    public class Gizmo_SlottedThingTransformer : Gizmo
    {
        static Texture2D noFuelIcon = ContentFinder<Texture2D>.Get("UI/Gizmo/NoFuelInTransformerIcon");
        ThingComp_SlottedThingTransformer transformerComp;
        float allSlotsWidth;
        DesignationManager designationManager;
        const float fuelGaugeWidth = Gizmo.Height / 2;
        const float slotColumnWidth = Gizmo.Height;
        const int slotsPerColumn = 2;

        int SlotColumnCount => Mathf.CeilToInt((float)transformerComp.TransformerProps.transformerSlots / slotsPerColumn);
        public ThingComp_MonoThingContainer FuelStorageComp => transformerComp.fuelStorageComp;

        public Gizmo_SlottedThingTransformer(Building parent)
        {
            transformerComp = parent.GetComp<ThingComp_SlottedThingTransformer>();
            allSlotsWidth = SlotColumnCount * slotColumnWidth;
            designationManager = transformerComp.parent.MapHeld.designationManager;
        }

        public override float GetWidth(float maxWidth)
        {
            return fuelGaugeWidth + allSlotsWidth;
        }

        const float gizmoMargin = 6f;
        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect fullRect = new Rect(topLeft, new Vector2(GetWidth(maxWidth), Gizmo.Height));
            Widgets.DrawWindowBackground(fullRect);

            Rect fuelGaugeRect = fullRect.LeftPartPixels(fuelGaugeWidth);
            fuelGaugeRect = fuelGaugeRect.ContractedBy(gizmoMargin);
            Rect slotsRect = fullRect.RightPartPixels(fullRect.width - fuelGaugeRect.width);
            slotsRect = slotsRect.ContractedBy(gizmoMargin);
            DrawFuelGauge(fuelGaugeRect);
            DrawSlots(slotsRect);

            return new GizmoResult(GizmoState.Clear);
        }

        const float iconHeightRatio = 0.4f;
        private void DrawFuelGauge(Rect inRect)
        {
            Rect currentIconRect = inRect.BottomPart(iconHeightRatio);
            Rect fillableBarRect = inRect.TopPart(1 - iconHeightRatio).ContractedBy(4f, 0);
            //fillableBarRect = fillableBarRect.ContractedBy(12f, 0);
            float fillableBarPercent = 0;
            Texture2D currentIcon;

            if (FuelStorageComp.CurrentlyAcceptedThingDef == null)
            {
                currentIcon = noFuelIcon;
            }
            else
            {
                currentIcon = FuelStorageComp.CurrentlyAcceptedThingDef.uiIcon;
                fillableBarPercent = (float)transformerComp.CurrentFuelCount / FuelStorageComp.Props.stackLimit;

                TaggedString currentFuelText = "Terrasecurity_Gizmo_SlottedThingConverter_CurrentFuel".Translate(transformerComp.CurrentFuelCount.Named("CURRENT"), FuelStorageComp.Props.stackLimit.Named("MAX"));
                if(transformerComp.totalFuelCost > 0)
                {
                    currentFuelText += $"\n{"Terrasecurity_Gizmo_SlottedThingConverter_ConsumedFuel".Translate(transformerComp.totalFuelCost.Named("CONSUMED"))}";
                }
                TooltipHandler.TipRegion(fillableBarRect, currentFuelText);
            }

            Texture2D barTexture = transformerComp.TransformerProps.BaseFuelTexture;
            // if fuel exists and any slot is filled, change the bar color to visualize whether or not the fuel is sufficient for a full transformation
            if (transformerComp.HasFuel && !transformerComp.Empty)
            {
                if(transformerComp.totalFuelCost < transformerComp.CurrentFuelCount)
                {
                    barTexture = transformerComp.TransformerProps.SufficientFuelTexture;
                }
                else
                {
                    barTexture = transformerComp.TransformerProps.InsufficientFuelTexture;
                }
            }
            UIUtility.VerticalFillableBar(fillableBarRect, fillableBarPercent, true, barTexture, false);

            Widgets.DrawTextureFitted(currentIconRect, currentIcon, 1);
            TooltipHandler.TipRegion(currentIconRect, "Terrasecurity_Gizmo_SlottedThingConverter_PickFuel".Translate());
            if (!transformerComp.AllowsInteractions)
            {
                DrawDisabledOverlay(currentIconRect);
            }
            if (Widgets.ButtonInvisible(currentIconRect))
            {
                MakeFuelPickerFloatMenu();
            }
            Widgets.DrawHighlightIfMouseover(currentIconRect);
        }

        public void MakeFuelPickerFloatMenu()
        {
            List<FloatMenuOption> options = GetFuelPickerOptions().ToList();

            foreach (FloatMenuOption option in options)
            {
                DisableOptionDuringTransformation(option);
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }

        private IEnumerable<FloatMenuOption> GetFuelPickerOptions()
        {
            // checking reserved state prevents emptying fuel whilst a pawn is installing an item or anything that might break if fuel suddenly disappears
            bool isTransformerReserved = FuelStorageComp.parent.Map.reservationManager.IsReservedByAnyoneOf(FuelStorageComp.parent, Faction.OfPlayer);
            if (isTransformerReserved)
            {
                yield return new FloatMenuOption("Terrasecurity_Gizmo_SlottedThingConverter_FuelChangeNotPossibleDueToReservation".Translate(), null);
                yield break;
            }
            if (!FuelStorageComp.Empty)
            {
                yield return new FloatMenuOption("Terrasecurity_Gizmo_SlottedThingConverter_Eject".Translate(), RemoveFuel);
            }
            foreach (ThingDef fuelThingDef in transformerComp.TransformerProps.validFuelThings)
            {
                yield return new FloatMenuOption(fuelThingDef.LabelCap, () => FuelStorageComp.SetAcceptedThingDef(fuelThingDef), shownItemForIcon: fuelThingDef);
            }
        }

        private void RemoveFuel()
        {
            IntVec3 position = transformerComp.parent.Position;
            Map map = transformerComp.parent.Map;
            Predicate<Thing> removeAction = (Thing thing) => GenPlace.TryPlaceThing(thing, position, map, ThingPlaceMode.Near);
            for (int i = 0; i < transformerComp.slottedThings.Capacity; i++)
            {
                transformerComp.slottedThings[i] = null;
            }
            transformerComp.innerContainer.TryDropAll(position, map, ThingPlaceMode.Near);
            FuelStorageComp.innerContainer.TryDropAll(position, map, ThingPlaceMode.Near);
            FuelStorageComp.SetAcceptedThingDef(null);
        }

        const float slotPadding = 2f;
        private void DrawSlots(Rect inRect)
        {
            Rect[] columns = new Rect[SlotColumnCount];
            for (int i = 0; i < SlotColumnCount; i++)
            {
                columns[i] = new Rect(inRect.xMin + i * slotColumnWidth, inRect.yMin, slotColumnWidth, inRect.height);
                //columns[i] = columns[i].ContractedBy(slotPadding);
            }
            for (int slotIndex = 0; slotIndex < transformerComp.TransformerProps.transformerSlots; slotIndex++)
            {
                int columnIndex = Mathf.FloorToInt((float)slotIndex / slotsPerColumn);
                DrawSlotAtIndex(columns[columnIndex], slotIndex);
            }
        }

        static Color slotBackgroundColor = new ColorInt(31, 35, 39).ToColor;
        static Color slotBorderColor = new ColorInt(97, 108, 122).ToColor;
        private void DrawSlotAtIndex(Rect columnRect, int index)
        {
            float slotHeight = columnRect.height / slotsPerColumn;
            float y = columnRect.yMin + index % slotsPerColumn * slotHeight;
            Rect slotRect = new Rect(columnRect.xMin, y, columnRect.width, slotHeight);
            slotRect = slotRect.ContractedBy(slotPadding);
            Widgets.DrawBoxSolidWithOutline(slotRect, slotBackgroundColor, slotBorderColor);
            slotRect = slotRect.ContractedBy(slotPadding);
            TooltipHandler.TipRegion(slotRect, "Terrasecurity_Gizmo_SlottedThingConverter_SlotDescription".Translate((index + 1).Named("SLOTINDEX")));
            Thing slottedThing = transformerComp.slottedThings[index];

            if (slottedThing == null)
            {
                if (Widgets.ButtonInvisible(slotRect))
                {
                    OpenEmptySlotMenu();
                }
                string emptySlotLabel;
                if (transformerComp.AllowsInteractions)
                {
                    emptySlotLabel = "Terrasecurity_Gizmo_SlottedThingConverter_Empty".Translate();
                }
                else
                {
                    emptySlotLabel = "Terrasecurity_Gizmo_SlottedThingConverter_Locked".Translate();
                }
                UIUtility.Label(slotRect, emptySlotLabel, anchor: TextAnchor.MiddleCenter);
            }
            else
            {
                if (Widgets.ButtonInvisible(slotRect))
                {
                    OpenFilledSlotMenu(slottedThing, index);
                }
                Widgets.DrawTextureFitted(slotRect, slottedThing.def.uiIcon, 1);
            }
            if (!transformerComp.AllowsInteractions)
            {
                DrawDisabledOverlay(slotRect);
            }
        }

        static Color disabledColor = new Color(0, 0, 0, 0.5f);
        private void DrawDisabledOverlay(Rect inRect)
        {
            Widgets.DrawRectFast(inRect, disabledColor);
        }

        private void OpenEmptySlotMenu()
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>()
            {
                new FloatMenuOption("Terrasecurity_Gizmo_SlottedThingConverter_SelectTargetThing".Translate(), BeginInsertTargeting)
            };
            foreach (FloatMenuOption option in options)
            {
                DisableOptionDuringTransformation(option);
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }
        private void OpenFilledSlotMenu(Thing thing, int slotIndex)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>()
            {
                new FloatMenuOption("Terrasecurity_Gizmo_SlottedThingConverter_Eject".Translate(), () => transformerComp.TryRemove(thing))
            };
            for (int i = 0; i < transformerComp.TransformerProps.transformerSlots; i++)
            {
                if(slotIndex == i)
                {
                    continue;
                }
                int targetSlot = i;
                Action swapAction = () => transformerComp.SwapSlots(slotIndex, targetSlot);
                options.Add(new FloatMenuOption("Terrasecurity_Gizmo_SlottedThingConverter_SwapSlot".Translate((slotIndex+1).Named("FROM"), (targetSlot+1).Named("TO")), swapAction));
            }

            foreach (FloatMenuOption option in options)
            {
                DisableOptionDuringTransformation(option);
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }

        private void DisableOptionDuringTransformation(FloatMenuOption option)
        {
            if (transformerComp.AllowsInteractions)
            {
                return;
            }
            option.Disabled = true;
            option.Label += $" ({"Terrasecurity_Gizmo_SlottedThingConverter_DisabledDueToTransformationCycle".Translate()})";
        }

        private void BeginInsertTargeting()
        {
            TargetingParameters targetParameters = new TargetingParameters()
            {
                canTargetBuildings = false,
                canTargetPawns = false,
                canTargetItems = true,
                mapObjectTargetsMustBeAutoAttackable = false,
                validator = CanTargetThing,
            };

            Action<LocalTargetInfo> targetAction = (LocalTargetInfo target) =>
            {
                Designation designation = new Designation(target, Common.installInSlottedThingTransformerDesignation);
                designationManager.AddDesignation(designation);
                //Job job = JobMaker.MakeJob(Common.insertIntoSlottedTransformerJobDef, target.Thing, transformerComp.parent);
                //job.count = 1;
                //BeginPawnTargeting(job);
            };

            Action<LocalTargetInfo> guiAction = (_) => Widgets.MouseAttachedLabel("Terrasecurity_Gizmo_SlottedThingConverter_SelectThingForSlot".Translate());
            Find.Targeter.BeginTargeting(targetParameters, targetAction, null, null, onGuiAction: guiAction);
        }

        private bool CanTargetThing(TargetInfo target)
        {
            if(!target.HasThing)
            {
                return false;
            }
            if(designationManager.DesignationOn(target.Thing) != null)
            {
                return false;
            }
            if (!transformerComp.HasFuel)
            {
                return false;
            }
            if (!transformerComp.FuelRecipes.AnyRecipeAppliesTo(target.Thing))
            {
                return false;
            }
            return true;
        }

        //private void BeginPawnTargeting(Job jobToGive)
        //{
        //    TargetingParameters parameters = new TargetingParameters()
        //    {
        //        onlyTargetColonists = true,
        //        validator = (TargetInfo target) => target.Thing is Pawn pawn && transformerComp.parent.CanBeInteractedWithBy(pawn)
        //    };

        //    Action<LocalTargetInfo> targetAction = (LocalTargetInfo target) =>
        //    {
        //        target.Pawn.jobs.TryTakeOrderedJob(jobToGive);
        //        Find.Targeter.StopTargeting();
        //    };
        //    Action<LocalTargetInfo> guiAction = (_) => Widgets.MouseAttachedLabel("Terrasecurity_Gizmo_SlottedThingConverter_SelectPawnForJob".Translate());

        //    Find.Targeter.BeginTargeting(parameters, targetAction, null, null, onGuiAction: guiAction);
        //}
    }
}
