using RimWorld;
using Verse;
using Verse.AI;

namespace Terrasecurity
{
    internal static class InteractionUtility
    {
        public static AcceptanceReport CanBeInteractedWithBy(this Thing thing, Pawn pawn)
        {
            if (!(thing is Building building))
            {
                return "Terrasecurity_FailureReason_NotABuilding".Translate();
            }
            if (pawn.Map.designationManager.DesignationOn(building, DesignationDefOf.Deconstruct) != null)
            {
                return "Terrasecurity_FailureReason_DesignatedForDeconstruct".Translate();
            }
            if (building.IsForbidden(pawn))
            {
                return "Terrasecurity_FailureReason_IsForbidden".Translate();
            }
            if (!pawn.CanReserve(building))
            {
                return "Terrasecurity_FailureReason_CannotReserve".Translate();
            }
            if (!pawn.CanReach(building, PathEndMode.ClosestTouch, Danger.Deadly))
            {
                return "Terrasecurity_FailureReason_CannotReach".Translate();
            }
            return true;
        }
    }
}