using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using Verse;
using Color = UnityEngine.Color;

namespace Terrasecurity
{
    public class ThingCompProperties_EquippedLifespan : CompProperties
    {
        public int lifespanTicks = 100;
        public int showExpirationAlertOnRemainingTicks = -1;
        public EffecterDef expireEffect;
        public ThingDef replacementToSpawn;
        Color lifespanBarColor = Color.blue;

        Texture2D _lifespanBarTexture;
        public Texture2D LifespanBarTexture
        {
            get
            {
                if (_lifespanBarTexture == null)
                {
                    _lifespanBarTexture = SolidColorMaterials.NewSolidColorTexture(lifespanBarColor);
                }
                return _lifespanBarTexture;
            }
        }
    }
}
