using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verse;

namespace Terrasecurity
{
    public static class HashUtility
    {
        public static int GetHash<T>(IEnumerable<T> enumerable)
        {
            return string.Join("", enumerable.Select(entry => entry.GetHashCode())).GetHashCode();
        }
    }
}
