using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Terrasecurity
{
    public class ThingComp_CyclicThingSpawner : ThingComp
    {
        ThingCompProperties_CyclicThingSpawner Props => base.props as ThingCompProperties_CyclicThingSpawner;

        int nextSpawnTick = -1;

        // not sure which things will take this comp and how they tick, so just support them all.
        public override void CompTick()
        {
            base.CompTick();
            TickCycle();
        }
        public override void CompTickRare()
        {
            base.CompTickRare();
            TickCycle();
        }
        public override void CompTickLong()
        {
            base.CompTickLong();
            TickCycle();
        }

        private void TickCycle()
        {
            if(nextSpawnTick == -1)
            {
                nextSpawnTick = GenTicks.TicksGame + Props.cycleDurationRangeTicks.RandomInRange;
            }
            if(GenTicks.TicksGame > nextSpawnTick)
            {
                SpawnThings();
                nextSpawnTick = -1;
            }
        }

        private void SpawnThings()
        {
            List<Thing> things = Props.ProduceRandomItems();
            foreach (Thing thing in things)
            {
                GenPlace.TryPlaceThing(thing, parent.Position, parent.Map, ThingPlaceMode.Near);
            }
            if (Props.showNotificationWhenSpawningThings)
            {
                string messageText = "Terrasecurity_Message_SpawnedThingFromThingSpawner".Translate(parent.Label.Named("SOURCE"));
                Messages.Message(messageText, parent, MessageTypeDefOf.NeutralEvent);
            }
        }

        string cachedIntervalText;
        public override string CompInspectStringExtra()
        {
            StringBuilder text = new StringBuilder(base.CompInspectStringExtra());

            if(cachedIntervalText == null)
            {
                string intervalText = ((int)Props.cycleDurationRangeTicks.Average).ToStringTicksToPeriodVague();
                cachedIntervalText = "Terrasecurity_InspectString_ThingSpawner_AverageSpawnInterval".Translate(intervalText.Named("INTERVAL"));
            }
            if(text.Length > 0)
            {
                text.AppendLine();
            }
            text.Append(cachedIntervalText);

            return text.ToString();
        }

        public override void PostExposeData()
        {
            Scribe.EnterNode(this.GetType().Name);
            base.PostExposeData();
            Scribe_Values.Look(ref nextSpawnTick, nameof(nextSpawnTick), -1);
            Scribe.ExitNode();
        }
    }
}
