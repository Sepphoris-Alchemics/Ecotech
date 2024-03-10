using System;
using RimWorld;
using Verse;

namespace Terrasecurity
{
	public class TurretRegenRefuel : Building_TurretGun //was TurretChargeFuel
		//I believe this class stops the turret from regenerating health if it hits 0, which is something I actually want. So that section will have to be removed. Additionally, if an XML option can be added so a refueling item is optional, that would also be appreciated if time permits (but not necessary). Message me if you need clarification.
	{
		private CompRefuelable compref
		{
			get
			{
				return base.GetComp<CompRefuelable>();
			}
		}

		private Comp_PoweredShot comppoweredshot
		{
			get
			{
				return base.GetComp<Comp_PoweredShot>();
			}
		}

		private float fuelcapacity
		{
			get
			{
				return base.GetComp<CompRefuelable>().Props.fuelCapacity;
			}
		}

		private float consumefuelpershot
		{
			get
			{
				return this.AttackVerb.verbProps.consumeFuelPerShot;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref this.consumedfuel, "consumedfuel", false, false);
			Scribe_Values.Look<bool>(ref this.chargingflag, "chargingflag", false, false);
			Scribe_Values.Look<bool>(ref this.chargingrest, "chargingrest", false, false);
			Scribe_Values.Look<bool>(ref this.tickwaitrestored, "tickwaitrestored", false, false);
			Scribe_Values.Look<int>(ref this.waitforchargetick, "waitforchargetick", 0, false);
		}

		protected void TryCharging()
		{
			this.FuelConsumeChecker();
			bool flag = this.consumedfuel;
			if (flag)
			{
				this.chargingflag = false;
				this.WaitForCharge();
			}
			else
			{
				this.chargingflag = true;
				this.tickwaitrestored = false;
			}
		}

		protected void WaitForCharge()
		{
			bool flag = !this.tickwaitrestored;
			if (flag)
			{
				this.waitforchargetick = this.comppoweredshot.Props.Waitforchargetick;
				this.tickwaitrestored = true;
			}
			bool flag2 = this.waitforchargetick > 0;
			if (flag2)
			{
				this.waitforchargetick--;
			}
			bool flag3 = this.waitforchargetick <= 0;
			if (flag3)
			{
				this.consumedfuel = false;
				this.tickwaitrestored = false;
			}
		}

		protected void FuelConsumeChecker()
		{
			bool flag = !this.fuelinitialized;
			if (flag)
			{
				this.prevfuel = this.compref.Fuel;
				this.fuelinitialized = true;
			}
			this.presfuel = this.compref.Fuel;
			bool flag2 = this.presfuel - this.consumefuelpershot < 0f && !this.chargingrest;
			if (flag2)
			{
				this.chargingrest = true;
			}
			bool flag3 = this.presfuel < this.prevfuel;
			if (flag3)
			{
				this.consumedfuel = true;
				this.waitforchargetick = this.comppoweredshot.Props.Waitforchargetick;
				this.tickwaitrestored = true;
				this.prevfuel = this.presfuel;
			}
			else
			{
				bool flag4 = this.presfuel >= this.fuelcapacity && this.chargingrest;
				if (flag4)
				{
					this.chargingrest = false;
				}
				this.prevfuel = this.presfuel;
			}
		}

		public override void Tick()
		{
			base.Tick();
			bool powerOn = this.powerComp.PowerOn;
			if (powerOn)
			{
				this.TryCharging();
				bool flag = this.chargingflag && !this.chargingrest;
				if (flag)
				{
					this.AutoRefuel();
				}
			}
			else
			{
				this.waitforchargetick = this.comppoweredshot.Props.Waitforchargetick;
				this.tickwaitrestored = true;
			}
			bool flag2 = this.chargingrest;
			if (flag2)
			{
				base.TryStartShootSomething(false);
			}
			this.DisplayMessage();
		}

		protected void AutoRefuel()
		{
			bool flag = !this.ticktorefuelrestored;
			if (flag)
			{
				this.ticktorefuel = this.comppoweredshot.Props.TicksPerFuelGained;
				this.ticktorefuelrestored = true;
			}
			bool flag2 = this.compref.Fuel < this.fuelcapacity;
			if (flag2)
			{
				bool flag3 = this.ticktorefuel > 0;
				if (flag3)
				{
					this.ticktorefuel--;
				}
				bool flag4 = this.ticktorefuel == 0;
				if (flag4)
				{
					this.compref.Refuel(this.consumefuelpershot / this.compref.Props.FuelMultiplierCurrentDifficulty);
					this.ticktorefuelrestored = false;
				}
			}
		}

		protected void DisplayMessage()
		{
			bool flag = this.chargingrest;
			if (flag)
			{
				this.comppoweredshot.displaycharging = true;
			}
			else
			{
				this.comppoweredshot.displaycharging = false;
			}
			bool flag2 = this.consumedfuel && this.compref.Fuel < this.fuelcapacity;
			if (flag2)
			{
				this.comppoweredshot.displaytickrest = true;
			}
			else
			{
				this.comppoweredshot.displaytickrest = false;
			}
			bool flag3 = this.chargingflag && !this.chargingrest && this.compref.Fuel < this.fuelcapacity && this.powerComp.PowerOn;
			if (flag3)
			{
				this.comppoweredshot.displayrecharging = true;
			}
			else
			{
				this.comppoweredshot.displayrecharging = false;
			}
		}

		public bool consumedfuel;

		public bool chargingflag; //rename anything with "charge" or "recharging" to "regen"

		public bool chargingrest = false;

		public bool tickwaitrestored = false;

		private bool ticktorefuelrestored = false; //fuel can either be kept or renamed if you think it's safer

		private bool fuelinitialized = false;

		private float prevfuel;

		private float presfuel;

		public int waitforchargetick;

		private int ticktorefuel;
	}
}
