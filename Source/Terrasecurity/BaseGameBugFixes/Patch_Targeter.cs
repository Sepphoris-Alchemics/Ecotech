#if v1_4
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verse;

namespace Terrasecurity.BaseGameBugFixes
{
    /// <summary>
    /// 1.4 does not reset the onGui action for targeter when targeting ends. That means the game keeps drawing the mouse attached label even if there is no active targeter
    /// Issue is fixed in 1.5
    /// </summary>
    [HarmonyPatch(typeof(Targeter), nameof(Targeter.StopTargeting))]
    public static class Patch_Targeter
    {
        [HarmonyPostfix]
        public static void RemoveOnGuiAction(ref Action<LocalTargetInfo> ___onGuiAction)
        {
            ___onGuiAction = null;
        }
    }
}
#endif