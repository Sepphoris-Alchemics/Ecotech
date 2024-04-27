using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verse;

namespace Terrasecurity
{
    public static class Common
    {
        public static JobDef fillThingConverterJobDef = DefDatabase<JobDef>.GetNamed("TS_FillThingConverter");
        public static JobDef emptyThingConverterJobDef = DefDatabase<JobDef>.GetNamed("TS_EmptyThingConverter");
    }
}
