using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;



namespace Ecotech
{
    public abstract class ThingComp_EquippedTick : ThingComp
    {
        public Pawn wieldingPawn;

        abstract public void EquippedTick();

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            Current.Game.GetComponent<GameComponent_EquippedThingTicker>().AddTickingComp(this);
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            wieldingPawn = pawn;

            Current.Game.GetComponent<GameComponent_EquippedThingTicker>().AddTickingComp(this);
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            wieldingPawn = null;

            Current.Game.GetComponent<GameComponent_EquippedThingTicker>().RemoveTickingComp(this);
        }

        public override void PostExposeData()
        {
            Scribe.EnterNode(this.GetType().Name);
            base.PostExposeData();
            Scribe_References.Look(ref wieldingPawn, nameof(wieldingPawn));
            Scribe.ExitNode();
        }
    }

}
