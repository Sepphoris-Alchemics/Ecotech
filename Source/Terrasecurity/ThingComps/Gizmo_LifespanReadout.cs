using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Terrasecurity
{
    public class Gizmo_LifespanReadout : Gizmo
    {
        const float width = 140f;
        const float padding = 4f;

        ThingComp_EquippedLifespan thingComp;

        public Gizmo_LifespanReadout(ThingComp_EquippedLifespan thingComp)
        {
            this.thingComp = thingComp;
        }

        public override float GetWidth(float maxWidth) => width;

        const float barHeightRatio = 0.6f;
        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            NamedArgument itemName = thingComp.parent.LabelShortCap.Named("ITEMNAME");

            Rect fullGizmoRect = new Rect(topLeft, new Vector2(width, Gizmo.Height));
            Widgets.DrawWindowBackground(fullGizmoRect);
            TooltipHandler.TipRegion(fullGizmoRect, "Terrasecurity_Gizmo_LifespanTicksRemaining_Desc".Translate(itemName));
            fullGizmoRect = fullGizmoRect.ContractedBy(padding);

            TextAnchor previousAnchor = Text.Anchor;
            GameFont previousFont = Text.Font;
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Small;

            Rect titleLabelRect = fullGizmoRect.TopPartPixels(fullGizmoRect.height * (1 - barHeightRatio));
            Widgets.Label(titleLabelRect, "Terrasecurity_Gizmo_LifespanTicksRemaining".Translate(itemName));

            Rect barRect = fullGizmoRect.BottomPartPixels(fullGizmoRect.height * barHeightRatio);
            Widgets.FillableBar(barRect, thingComp.LifespanRemainingRatio, thingComp.Props.LifespanBarTexture);

            Rect barLabelRect = barRect.ContractedBy(padding);
            Widgets.Label(barLabelRect, thingComp.TicksRemainingReadable);

            Text.Anchor = previousAnchor;
            Text.Font = previousFont;

            return new GizmoResult(GizmoState.Clear);
        }
    }
}
