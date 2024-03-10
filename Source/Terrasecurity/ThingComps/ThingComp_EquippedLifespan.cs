using HarmonyLib;
using Mono.Unix.Native;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verse;

namespace Terrasecurity
{
    public class ThingComp_EquippedLifespan : ThingComp_EquippedTick
    {
        int ageTicks = 0;

        int TicksRemaining => Props.lifespanTicks - ageTicks;
        public string TicksRemainingReadable => TicksRemaining.ToStringTicksToPeriodVerbose();
        public float LifespanRemainingRatio => (float)TicksRemaining / Props.lifespanTicks;


        public ThingCompProperties_EquippedLifespan Props => base.props as ThingCompProperties_EquippedLifespan;

        public override void EquippedTick()
        {
            ageTicks++;
            if(ageTicks > Props.lifespanTicks)
            {
                Expire();
            }
        }

        private void Expire()
        {
            if (Props.expireEffect != null)
            {
                Props.expireEffect.Spawn(parent.Position, parent.Map, 1f).Cleanup();
            }
            if (Props.replacementToSpawn != null)
            {
                GenSpawn.Spawn(Props.replacementToSpawn, parent.PositionHeld, parent.MapHeld, WipeMode.Vanish);
            }
            parent.Destroy(DestroyMode.KillFinalize);
        }

        //public override string CompInspectStringExtra()
        //{
        //    string text = base.CompInspectStringExtra();
        //    text += "Terrasecurity_Gizmo_LifespanTicksRemaining".Translate(TicksRemainingReadable);
        //    return text;
        //}

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

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ageTicks, nameof(ageTicks));
        }
    }
}
