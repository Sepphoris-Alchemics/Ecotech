using System;
using Verse;

namespace Terrasecurity
{
	public class CompProperties_RegenerateShot : CompProperties //wasCompProperties_PoweredShot
	{
		public CompProperties_RegenerateShot()
		{
			this.compClass = typeof(Comp_PoweredShot);
		}

		public int Waitforchargetick;

		public int TicksPerFuelGained;

		public string RegenResetMessage; //for all three of these, "Charged was renamed to "Regen"

		public string RegenRestMessage;

		public string RegeneratingMessage;
	}
}
