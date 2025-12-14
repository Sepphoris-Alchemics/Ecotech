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
    public class SapTransformOption
    {
        public ThingDef sap;
        public ThingDef result;
        public ThingStyleDef plantStyle;
    }
}