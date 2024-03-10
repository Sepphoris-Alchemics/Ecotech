using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verse;

namespace Terrasecurity
{
    public class MapComponent_EquippedThingTicker : MapComponent
    {
        List<ThingComp_EquippedTick> currentlyTrackedComps = new List<ThingComp_EquippedTick>();
        List<ThingWithComps> scribedThings = new List<ThingWithComps>();

        public MapComponent_EquippedThingTicker(Map map) : base(map) { }

        public void AddTickingComp(ThingComp_EquippedTick thingComp)
        {
            currentlyTrackedComps.Add(thingComp);
        }

        public void RemoveTickingComp(ThingComp_EquippedTick thingComp)
        {
            currentlyTrackedComps.Remove(thingComp);
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            // reverse for loop iteration to prevent CollectionModifiedException
            for (int i = currentlyTrackedComps.Count - 1; i >= 0; i--)
            {
                ThingComp_EquippedTick thingComp = currentlyTrackedComps[i];
                try
                {
                    thingComp.EquippedTick();
                }
                catch(Exception e)
                {
                    Log.Error($"Exception ticking {thingComp.GetType().Name} for {thingComp.parent}: {e.Message}");
                }
            }
        }

        /// <summary>
        /// ThingComps cannot be scribed, so the parent ThingWithComps is retrieved instead and scribed. Upon loading the parents comps are iterated and all valid comps restored.
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            if(Scribe.mode == LoadSaveMode.Saving)
            {
                scribedThings = currentlyTrackedComps
                    .Select(c => c.parent)
                    .Distinct()
                    .ToList();
            }

            Scribe_Collections.Look(ref scribedThings, nameof(scribedThings), LookMode.Reference);

            if(Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                currentlyTrackedComps = scribedThings
                    .SelectMany(t => t.AllComps)
                    .Where(t => t is ThingComp_EquippedTick)
                    .Cast<ThingComp_EquippedTick>()
                    .ToList();
            }
        }
    }
}
