using System.Collections.Generic;
using Verse;

namespace Ecotech
{
    public class ThingDefExtension_PlantTerrainWhitelist : DefModExtension
    {
        List<TerrainDef> terrainWhitelist;

        public List<TerrainDef> TerrainWhitelist => terrainWhitelist;
    }
}
