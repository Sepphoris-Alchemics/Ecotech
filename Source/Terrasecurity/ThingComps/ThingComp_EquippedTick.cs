using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;



namespace Terrasecurity
{
    public abstract class ThingComp_EquippedTick : ThingComp
    {
        abstract public void EquippedTick();

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            pawn.Map.GetComponent<MapComponent_EquippedThingTicker>().AddTickingComp(this);
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            pawn.Map.GetComponent<MapComponent_EquippedThingTicker>().RemoveTickingComp(this);
        }
    }

}
