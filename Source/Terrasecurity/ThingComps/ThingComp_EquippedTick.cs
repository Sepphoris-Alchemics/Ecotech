using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;



namespace Terrasecurity
{
    public abstract class ThingComp_EquippedTick : ThingComp
    {
        public Pawn wieldingPawn;

        abstract public void EquippedTick();

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            wieldingPawn = pawn;
            Map map = pawn.Map;
            if(map != null)
            {
                map.GetComponent<MapComponent_EquippedThingTicker>().AddTickingComp(this);
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            wieldingPawn = null;
            Map map = pawn.Map;
            if (map != null)
            {
                map.GetComponent<MapComponent_EquippedThingTicker>().RemoveTickingComp(this);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref wieldingPawn, nameof(wieldingPawn));
        }
    }

}
