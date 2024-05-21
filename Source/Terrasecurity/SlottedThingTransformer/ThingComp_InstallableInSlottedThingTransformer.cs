using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Terrasecurity
{
    public class ThingComp_InstallableInSlottedThingTransformer : ThingComp
    {
        Gizmo _installGizmo;
        public Gizmo InstallGizmo
        {
            get
            {
                if(_installGizmo == null)
                {
                    _installGizmo = new Command_Action()
                    {
                        defaultLabel = "Terrasecurity_Gizmo_InstallableInThingTransformer_Label".Translate(),
                        defaultDesc = "Terrasecurity_Gizmo_InstallableInThingTransformer_Description".Translate(),
                        icon = Common.installableInSlottedThingTransformerGizmoTexture,
                        action = OpenInstallTargeter
                    };
                }
                return _installGizmo;
            }
        }

        public Pawn Holder
        {
            get
            {
                Pawn_EquipmentTracker pawn_EquipmentTracker = parent?.ParentHolder as Pawn_EquipmentTracker;
                if (pawn_EquipmentTracker == null)
                {
                    return null;
                }
                return pawn_EquipmentTracker.pawn;
            }
        }

        void OpenInstallTargeter()
        {
            TargetingParameters targetingParameters = new TargetingParameters()
            {
                canTargetPawns = false,
                validator = TargetAcceptsThing,
            };
            Action<LocalTargetInfo> onGuiAction = (LocalTargetInfo target) => Widgets.MouseAttachedLabel("Terrasecurity_Gizmo_InstallableInThingTransformer_Description".Translate());
            Find.Targeter.BeginTargeting(targetingParameters, GiveInstallJob, highlightAction: null, targetValidator: null, onGuiAction: onGuiAction);
        }

        private bool TargetAcceptsThing(TargetInfo target)
        {
            if (!target.HasThing)
            {
                return false;
            }
            ThingComp_SlottedThingTransformer comp = target.Thing.TryGetComp<ThingComp_SlottedThingTransformer>();
            if(comp == null)
            {
                return false;
            }
            if (!comp.CanTake(parent))
            {
                return false;
            }
            return true;
        }

        Thing DropItem(out Pawn wearer)
        {
            if(parent is Apparel apparel)
            {
                wearer = apparel.Wearer;
                wearer.apparel.Remove(apparel);
            }
            CompEquippable equipmentComp = parent.GetComp<CompEquippable>();
            if(equipmentComp != null)
            {
                wearer = Holder;
                wearer.equipment.Remove(parent);
            }
            else
            {
                throw new NotImplementedException($"Tried to use non-apparel and non-equipment thing for {nameof(ThingComp_InstallableInSlottedThingTransformer)}");
            }
            GenPlace.TryPlaceThing(parent, wearer.Position, wearer.Map, ThingPlaceMode.Near);
            return parent;
        }

        void GiveInstallJob(LocalTargetInfo target)
        {
            Thing thingToInstall = DropItem(out Pawn wearer);
            Job job = JobMaker.MakeJob(Common.insertIntoSlottedTransformerJobDef, thingToInstall, target.Thing);
            job.count = 1;
            wearer.jobs.TryTakeOrderedJob(job);
        }
    }
}
