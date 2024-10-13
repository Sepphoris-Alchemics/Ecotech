using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Ecotech
{
    public class Gizmo_LifespanReadout : Gizmo
    {
        const float width = 140f;
        public const float Padding = 4f;

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
            TooltipHandler.TipRegion(fullGizmoRect, "Ecotech_Gizmo_LifespanTicksRemaining_Desc".Translate(itemName));
            fullGizmoRect = fullGizmoRect.ContractedBy(Padding);

            TextAnchor previousAnchor = Text.Anchor;
            GameFont previousFont = Text.Font;
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Small;

            Rect titleLabelRect = fullGizmoRect.TopPartPixels(fullGizmoRect.height * (1 - barHeightRatio));
            Widgets.Label(titleLabelRect, "Ecotech_Gizmo_LifespanTicksRemaining".Translate(itemName));

            Text.Anchor = previousAnchor;
            Text.Font = previousFont;

            Rect barRect = fullGizmoRect.BottomPartPixels(fullGizmoRect.height * barHeightRatio);
            thingComp.DrawLifespanBar(barRect);

            return new GizmoResult(GizmoState.Clear);
        }
    }
}
