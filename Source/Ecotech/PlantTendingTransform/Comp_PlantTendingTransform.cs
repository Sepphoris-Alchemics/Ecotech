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
    public class CompSapInfusable : ThingComp
    {
        public ThingDef infusedSapDef;
        public CompProperties_SapInfusable Props =>
            (CompProperties_SapInfusable)props;

        public bool IsInfused => infusedSapDef != null;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (parent.DestroyedOrNull())
                yield break;

            Plant plant = parent as Plant;
            if (plant == null)
                yield break;

            Command_Action command = new Command_Action
            {
                icon = ContentFinder<UnityEngine.Texture2D>.Get("UI/Gizmo/InfuseSapGizmo", true)
            };

            if (IsInfused)
            {
                command.defaultLabel = "Infused";
                command.defaultDesc =
                    "This growth has already been infused with" +
                    infusedSapDef.label.CapitalizeFirst() +
                    "\n\nIt cannot be infused again.";
                command.Disable("Already infused");
            }
            else if (plant.Growth >= 1f)
            {
                command.defaultLabel = "Infuse sap…";
                command.defaultDesc =
                    "This plant is fully grown and can no longer be infused.";
                command.Disable("Already fully grown");
            }
            else
            {
                command.defaultLabel = "Infuse sap…";
                command.defaultDesc =
                    "Influence what this plant grows into by infusing it with a specific sap.";
                command.action = OpenSapMenu;
            }

            yield return command;
        }

        private void OpenSapMenu()
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            Map map = parent.Map;

            if (Props?.supportedSaps == null)
            {
                options.Add(new FloatMenuOption("No saps available.", null));
                Find.WindowStack.Add(new FloatMenu(options));
                return;
            }

            Pawn pawn = Find.CurrentMap.mapPawns.FreeColonists
                .FirstOrDefault(p => p.CanReach(parent, PathEndMode.Touch, Danger.Some));

            if (pawn == null)
            {
                options.Add(new FloatMenuOption("No colonist can reach this plant.", null));
                Find.WindowStack.Add(new FloatMenu(options));
                return;
            }

            foreach (ThingDef sapDef in Props.supportedSaps)
            {
                AddSapOption(options, pawn, sapDef);
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private void AddSapOption(List<FloatMenuOption> options, Pawn pawn, ThingDef sapDef)
        {
            if (sapDef == null)
                return;

            Thing sap = SapUtility.FindClosestSap(pawn, sapDef);

            if (sap != null)
            {
                options.Add(new FloatMenuOption(
                    sapDef.label.CapitalizeFirst(),
                    () => StartInfusionJob(sapDef)
                ));
            }
            else
            {
                options.Add(new FloatMenuOption(
                    sapDef.label.CapitalizeFirst() + " (none available)",
                    null,
                    MenuOptionPriority.DisabledOption
                ));
            }
        }

        private void StartInfusionJob(ThingDef sapDef)
        {
            Pawn pawn = Find.CurrentMap.mapPawns.FreeColonists
                .FirstOrDefault(p => p.CanReach(parent, PathEndMode.Touch, Danger.Some));

            if (pawn == null)
                return;

            Thing sap = SapUtility.FindClosestSap(pawn, sapDef);
            if (sap == null)
                return;

            JobDef jobDef = DefDatabase<JobDef>.GetNamedSilentFail("ET_InfuseSapJob");
            if (jobDef == null)
            {
                Log.Error("[Ecotech] ET_InfuseSapJob JobDef not found in DefDatabase.");
                return;
            }

            Job job = JobMaker.MakeJob(Common.infuseSapJobDef, parent, sap);
            pawn.jobs.TryTakeOrderedJob(job);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref infusedSapDef, "infusedSapDef");
        }
    }
}