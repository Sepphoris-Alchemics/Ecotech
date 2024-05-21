using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verse;

namespace Terrasecurity
{
    public class ThingCompProperties_MonoThingContainer : CompProperties_ThingContainer
    {
        public ThingCompProperties_MonoThingContainer()
        {
            compClass = typeof(ThingComp_MonoThingContainer);
        }
    }
}
