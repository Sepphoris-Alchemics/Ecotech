using UnityEngine;
using Verse;

namespace Ecotech
{
    public class Gizmo_PlantTransform : Gizmo
    {
        ThingComp_PlantTransformOnMaturity comp;

        public Gizmo_PlantTransform(ThingComp_PlantTransformOnMaturity comp)
        {
            this.comp = comp;
        }

        public override float GetWidth(float maxWidth)
        {
            return Gizmo.Height;
        }

        const float margin = 4f;
        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect fullRect = new Rect(topLeft, new Vector2(GetWidth(maxWidth), Gizmo.Height));
            Widgets.DrawWindowBackground(fullRect);
            fullRect = fullRect.ContractedBy(margin);

            Rect topRow = fullRect.TopHalf();
            Rect rotateLeftRect = topRow.LeftHalf()
                .ContractedBy(margin);
            Rect rotateRightRect = topRow.RightHalf()
                .ContractedBy(margin);
            if(Widgets.ButtonImage(rotateLeftRect, TexUI.RotLeftTex))
            {
                comp.Rotate(RotationDirection.Counterclockwise);
                return new GizmoResult(GizmoState.Interacted);
            }
            if(Widgets.ButtonImage(rotateRightRect, TexUI.RotRightTex))
            {
                comp.Rotate(RotationDirection.Clockwise);
                return new GizmoResult(GizmoState.Interacted);
            }

            Rect bottomRow = fullRect.BottomHalf()
                .ContractedBy(margin);
            string ghostLabelKey = ThingComp_PlantTransformOnMaturity.ShowTransformGhost ? "Ecotech_Gizmo_PlantTransformOnMaturity_HideGhost" : "Ecotech_Gizmo_PlantTransformOnMaturity_ShowGhost";
            if(Widgets.ButtonText(bottomRow, ghostLabelKey.Translate(), overrideTextAnchor: TextAnchor.MiddleCenter))
            {
                ThingComp_PlantTransformOnMaturity.ShowTransformGhost = !ThingComp_PlantTransformOnMaturity.ShowTransformGhost;
                return new GizmoResult(GizmoState.Interacted);
            }

            return new GizmoResult(GizmoState.Clear);
        }
    }
}
