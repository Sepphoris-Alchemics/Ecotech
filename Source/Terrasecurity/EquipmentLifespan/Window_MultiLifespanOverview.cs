using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Terrasecurity
{
    public class Window_MultiLifespanOverview : Window
    {
        public Window_MultiLifespanOverview()
        {
            draggable = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
            Find.WindowStack.Add(this);
        }

        float scrollHeight = 9999f;
        Vector2 scrollPos = Vector2.zero;
        public override void DoWindowContents(Rect inRect)
        {
            RecacheForSelectedPawns();
            UIUtility.MakeAndBeginScrollView(inRect, scrollHeight, ref scrollPos, out Listing_Standard list);

            foreach ((Pawn pawn, ThingComp_EquippedLifespan comp) in cachedPawnsWithLifespanComp)
            {
                DrawLifespanCompEntry(list, pawn, comp);
            }

            list.EndScrollView(out scrollHeight);
        }

        const float windowWidth = 300f;
        const float windowHeight = 400f;
        public override Vector2 InitialSize => new Vector2(windowWidth, windowHeight);

        const float barRectHeight = 50f;
        private void DrawLifespanCompEntry(Listing_Standard list, Pawn pawn, ThingComp_EquippedLifespan comp)
        {
            string label = $"{pawn.LabelShort} ({comp.parent.LabelCap})";
            list.Label(label);
            Rect barRect = list.GetRect(barRectHeight);
            comp.DrawLifespanBar(barRect);
        }

        int cachedPawnsListHash = 0;
        List<(Pawn pawn, ThingComp_EquippedLifespan comp)> cachedPawnsWithLifespanComp = new List<(Pawn, ThingComp_EquippedLifespan)>();
        private void RecacheForSelectedPawns()
        {
            List<Pawn> selectedPawns = Find.Selector.SelectedPawns;
            if (cachedPawnsListHash == selectedPawns.GetHashCode())
            {
                return;
            }

            //Log.Message($"Recaching lifespan window");
            cachedPawnsWithLifespanComp.Clear();
            cachedPawnsListHash = selectedPawns.GetHashCode();

            foreach (Pawn pawn in Find.Selector.SelectedPawns)
            {
                foreach (ThingWithComps equippedThing in pawn.equipment.AllEquipmentListForReading)
                {
                    ThingComp_EquippedLifespan comp = equippedThing.GetComp<ThingComp_EquippedLifespan>();
                    if (comp != null)
                    {
                        cachedPawnsWithLifespanComp.Add((pawn, comp));
                    }
                }
            }
        }
    }
}
