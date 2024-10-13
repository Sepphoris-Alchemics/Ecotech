using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verse;

namespace Ecotech
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            Harmony harmony = new Harmony(nameof(Ecotech));
            harmony.PatchAll();
            PostGameLoadConfigErrors.RunPostLoadConfigErrorChecks();
        }
    }
}
