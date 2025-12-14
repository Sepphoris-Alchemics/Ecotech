using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;
using Verse.AI;

namespace Ecotech
{
    public class CompProperties_SapInfusable : CompProperties
    {
        public List<ThingDef> supportedSaps;

        public CompProperties_SapInfusable()
        {
            compClass = typeof(CompSapInfusable);
        }
    }
}