using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TheEndTimes
{
    public class WorldObjectCompProperties_DestroyBuildingQuestComp : WorldObjectCompProperties
    {
        public WorldObjectCompProperties_DestroyBuildingQuestComp()
        {
            this.compClass = typeof(DestroyPortalQuestComp);
        }
    }
}
