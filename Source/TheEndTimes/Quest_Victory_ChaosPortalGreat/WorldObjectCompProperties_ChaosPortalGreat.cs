using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TheEndTimes
{
    public class WorldObjectCompProperties_ChaosPortalGreat : WorldObjectCompProperties
    {
        public WorldObjectCompProperties_ChaosPortalGreat()
        {
            this.compClass = typeof(ChaosPortalGreatComp);
        }

        [DebuggerHidden]
        public override IEnumerable<string> ConfigErrors(WorldObjectDef parentDef)
        {
            foreach (string e in base.ConfigErrors(parentDef))
            {
                yield return e;
            }
            if (!typeof(MapParent).IsAssignableFrom(parentDef.worldObjectClass))
            {
                yield return parentDef.defName + " has WorldObjectCompProperties_ChaosPortalGreat but it's not MapParent.";
            }
        }
    }
}
