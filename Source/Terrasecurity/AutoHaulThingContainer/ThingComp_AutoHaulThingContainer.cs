using HarmonyLib;
using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Terrasecurity
{
    public abstract class ThingComp_AutoHaulThingContainer : CompThingContainer
    {
        private bool canFill = true;
        private bool canEmpty = true;

        private bool shouldFill = true;
        private bool shouldEmpty = false;

        public override void PostExposeData()
        {
            Scribe.EnterNode(this.GetType().Name);
            base.PostExposeData();
            Scribe_Values.Look(ref canFill, nameof(canFill), true);
            Scribe_Values.Look(ref canEmpty, nameof(canEmpty), true);
            Scribe_Values.Look(ref shouldFill, nameof(shouldFill), true);
            Scribe_Values.Look(ref shouldEmpty, nameof(shouldEmpty), false);
            Scribe.ExitNode();
        }

        protected abstract ThingRequest ThingRequest { get; }
        protected virtual bool HaulThingValidator(Thing thing) => true;
        public virtual int HaulCountFor(Thing thing) => Math.Min(thing.stackCount, Props.stackLimit - TotalStackCount);

        public void SetCanFill(bool canFill)
        {
            this.canFill = canFill;
        }
        public void SetCanEmpty(bool canEmpty)
        {
            this.canEmpty = canEmpty;
        }
        public void SetShouldFill(bool shouldFill)
        {
            this.shouldFill = shouldFill;
        }
        public void SetShouldEmpty(bool shouldEmpty)
        {
            this.shouldEmpty = shouldEmpty;
        }

        public virtual AcceptanceReport ShouldFill(Pawn pawn)
        {
            if (!canFill)
            {
                return "CurrentlyNotFillable";
            }
            if (Full)
            {
                return "AlreadyFull";
            }
            if (ThingRequest.IsUndefined)
            {
                return "NoThingRequested";
            }
            AcceptanceReport baseReport = parent.CanBeInteractedWithBy(pawn);
            if (!baseReport)
            {
                return baseReport;
            }
            Thing fillThing = FindHaulThingFor(pawn);
            if(fillThing == null)
            {
                return "NoFillThing";
            }
            if (ShouldEmpty(pawn, false))
            {
                return "ShouldBeEmptied";
            }
            return true;
        }

        public virtual AcceptanceReport ShouldEmpty(Pawn pawn, bool recheckBaseInteractibility = true)
        {
            if (!canEmpty)
            {
                return "CurrentlyNotEmptiable";
            }
            if (Empty)
            {
                return "AlreadyEmpty";
            }
            if (recheckBaseInteractibility)
            {
                AcceptanceReport baseReport = parent.CanBeInteractedWithBy(pawn);
                if (!baseReport)
                {
                    return baseReport;
                }
            }
            if (shouldEmpty)
            {
                return true;
            }
            bool isThingRequestValid = ThingRequest.IsUndefined;
            // if filled with something, check whether or not the current request accepts it
            isThingRequestValid |= !Empty && ThingRequest.Accepts(ContainedThing);
            if (!isThingRequestValid)
            {
                return true;
            }
            return "NotScheduledToEmpty";
        }

        public virtual Thing FindHaulThingFor(Pawn pawn)
        {
            TraverseParms traverseParms = TraverseParms.For(pawn);
            Thing foundThing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest, PathEndMode.ClosestTouch, traverseParms, validator: HaulThingValidator);
            return foundThing;
        }
    }
}
