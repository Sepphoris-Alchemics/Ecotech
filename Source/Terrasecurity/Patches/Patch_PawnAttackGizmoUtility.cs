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
    /// <summary>
    /// When multiple pawns are selected their distinct gizmos are not drawn. Instead we draw one gizmo that summarizes the state of equipped lifespan weapons
    /// </summary>
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(PawnAttackGizmoUtility), nameof(PawnAttackGizmoUtility.GetAttackGizmos))]
    public static class Patch_PawnAttackGizmoUtility
    {
        [HarmonyPostfix]
        public static IEnumerable<Gizmo> AddLifespanGizmoForPawnGroup(IEnumerable<Gizmo> __result)
        {
            foreach (Gizmo gizmo in __result)
            {
                yield return gizmo;
            }
            if(Find.Selector.SelectedPawns.Count <= 1)
            {
                yield break;
            }

            bool anyLifespanEquipment = Find.Selector.SelectedPawns
                .Any(p => p.equipment.AllEquipmentListForReading
                    .Any(e => e.GetComp<ThingComp_EquippedLifespan>() != null)
                );
            if (!anyLifespanEquipment)
            {
                yield break;
            }

            yield return MultiLifespanGizmo;
        }

        static Texture2D gizmoIcon = ContentFinder<Texture2D>.Get("UI/Gizmo/MultiLifespanGizmo");

        private static Gizmo _multiLifespanGizmo;
        private static Gizmo MultiLifespanGizmo
        {
            get
            {
                if(_multiLifespanGizmo == null)
                {
                    _multiLifespanGizmo = new Command_Action()
                    {
                        defaultLabel = "Terrasecurity_Gizmo_MultiLifespanTicksRemaining".Translate(),
                        defaultDesc = "Terrasecurity_Gizmo_MultiLifespanTicksRemaining_Desc".Translate(),
                        icon = gizmoIcon,
                        action = OpenMultiLifespanWindow
                    };
                }
                return _multiLifespanGizmo;
            }
        }

        private static void OpenMultiLifespanWindow()
        {
            new Window_MultiLifespanOverview();
        }
    }
}
