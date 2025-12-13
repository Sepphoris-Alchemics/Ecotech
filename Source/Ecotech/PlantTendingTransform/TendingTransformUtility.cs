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
    public static class SapUtility
    {
        public static bool HasSap(Map map, ThingDef sapDef)
        {
            return map.listerThings.ThingsOfDef(sapDef)
                .Any(t => t.Spawned && !t.IsForbidden(Faction.OfPlayer));
        }

        public static Thing FindClosestSap(Pawn pawn, ThingDef sapDef)
        {
            return GenClosest.ClosestThingReachable(
                pawn.Position,
                pawn.Map,
                ThingRequest.ForDef(sapDef),
                PathEndMode.Touch,
                TraverseParms.For(pawn),
                validator: t => !t.IsForbidden(pawn)
            );
        }
    }
}
