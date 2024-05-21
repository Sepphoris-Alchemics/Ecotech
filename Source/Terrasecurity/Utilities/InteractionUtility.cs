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
                return "NotABuilding";
            }
            if (pawn.Map.designationManager.DesignationOn(building, DesignationDefOf.Deconstruct) != null)
            {
                return "DesignatedForDeconstruct";
            }
            if (building.IsForbidden(pawn))
            {
                return "IsForbidden";
            }
            if (!pawn.CanReserve(building))
            {
                return "CannotReserve";
            }
            if (!pawn.CanReach(building, PathEndMode.ClosestTouch, Danger.Deadly))
            {
                return "CannotReach";
            }
            return true;
        }
    }
}