using HarmonyLib;
using Mono.Unix.Native;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Terrasecurity
{
    public class ThingComp_EquippedLifespan : ThingComp_EquippedTick
    {
        int ageTicks = 0;
        bool hasShownAlert = false;

        int TicksRemaining => Props.lifespanTicks - ageTicks;
        public string TicksRemainingReadable => TicksRemaining.ToStringTicksToPeriodVerbose();
        public float LifespanRemainingRatio => (float)TicksRemaining / Props.lifespanTicks;


        public ThingCompProperties_EquippedLifespan Props => base.props as ThingCompProperties_EquippedLifespan;

        /// <summary>
        /// Called while item is not equipped
        /// </summary>
        public override void CompTick()
        {
            base.CompTick();
            if (parent.Destroyed)
            {
                return;
            }
            ProgressLifespan();
        }

        /// <summary>
        /// Called while item is equipped
        /// </summary>
        public override void EquippedTick()
        {
            if (parent.Destroyed)
            {
                return;
            }
            ProgressLifespan();
            ShowAlertOnLowLifespan();
        }

        private void ProgressLifespan()
        {
            ageTicks++;
            if (ageTicks > Props.lifespanTicks)
            {
                Expire();
            }
        }

        private void Expire()
        {
            if(parent.MapHeld != null)
            {
                if (Props.expireEffect != null)
                {
                    Props.expireEffect.Spawn(parent.Position, parent.MapHeld, 1f).Cleanup();
                }
                if (Props.replacementToSpawn != null)
                {
                    GenSpawn.Spawn(Props.replacementToSpawn, parent.PositionHeld, parent.MapHeld, WipeMode.Vanish);
                }
                if (wieldingPawn != null)
                {
                    wieldingPawn.jobs.StopAll();
                }
            }
            parent.Destroy(DestroyMode.KillFinalize);
        }

        private void ShowAlertOnLowLifespan()
        {
            if (Props.showExpirationAlertOnRemainingTicks == -1)
            {
                return;
            }
            if (hasShownAlert)
            {
                return;
            }
            if(parent.Faction != Faction.OfPlayer)
            {
                return;
            }
            if(TicksRemaining > Props.showExpirationAlertOnRemainingTicks)
            {
                return;
            }
            hasShownAlert = true;
            Messages.Message("Terrasecurity_Message_LifespanAboutToExpire".Translate(parent.LabelShort.Named("THING")), parent, MessageTypeDefOf.NeutralEvent);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            yield return TimeSpanReadoutGizmo;
        }

        Gizmo _timeSpanReadoutGizmo;
        public Gizmo TimeSpanReadoutGizmo
        {
            get
            {
                if(_timeSpanReadoutGizmo == null)
                {
                    _timeSpanReadoutGizmo = new Gizmo_LifespanReadout(this);
                }
                return _timeSpanReadoutGizmo;
            }
        }

        public void DrawLifespanBar(Rect rect)
        {
            Widgets.FillableBar(rect, LifespanRemainingRatio, Props.LifespanBarTexture);

            TextAnchor previousAnchor = Text.Anchor;
            GameFont previousFont = Text.Font;
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Small;

            Rect barLabelRect = rect.ContractedBy(Gizmo_LifespanReadout.Padding);
            Widgets.Label(barLabelRect, TicksRemainingReadable);

            Text.Anchor = previousAnchor;
            Text.Font = previousFont;
        }

        public override void PostExposeData()
        {
            Scribe.EnterNode(this.GetType().Name);
            base.PostExposeData();
            Scribe_Values.Look(ref ageTicks, nameof(ageTicks));
            Scribe_Values.Look(ref hasShownAlert, nameof(hasShownAlert));
            Scribe.ExitNode();
        }
    }
}
