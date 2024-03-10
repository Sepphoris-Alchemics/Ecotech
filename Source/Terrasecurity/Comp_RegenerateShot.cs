using System;
using Verse;

namespace Terrasecurity
{
	public class Comp_RegenerateShot : ThingComp //was Comp_PoweredShot
	{
		public CompProperties_RegenerateShot Props
		{
			get
			{
				return (CompProperties_RegenerateShot)this.props;
			}
		}
		public override string CompInspectStringExtra()
		{
			string text = string.Concat(new string[0]);
			bool flag = this.displaycharging;
			if (flag)
			{
				text += string.Format("{0}", this.Props.ChargingResetMessage);
			}
			else
			{
				bool flag2 = this.displaytickrest;
				if (flag2)
				{
					text += string.Format("{0}", this.Props.ChargingRestMessage);
				}
				bool flag3 = this.displayrecharging;
				if (flag3)
				{
					text += string.Format("{0}", this.Props.ChargingMessage);
				}
			}
			return text;
		}

		public bool displaycharging = false;

		public bool displaytickrest = false;

		public bool displayrecharging = false;
	}
}
