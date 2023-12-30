using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace TheEndTimes
{
    public static class ChaosPortalUtility
    {
        private const float PortalPreventsClaimingInRadius = 2f;

        public static int TotalSpawnedPortalsCount(Map map)
        {
            return map.listerThings.ThingsOfDef(ThingDef.Named("RH_TET_ChaosPortal_Small")).Count;
        }

        public static bool AnyPortalPreventsClaiming(Thing thing)
        {
            if (!thing.Spawned)
            {
                return false;
            }
            int num = GenRadial.NumCellsInRadius(2f);
            for (int i = 0; i < num; i++)
            {
                IntVec3 c = thing.Position + GenRadial.RadialPattern[i];
                if (c.InBounds(thing.Map) && c.GetFirstThing(thing.Map, thing.def) != null)
                {
                    return true;
                }
            }
            return false;
        }

        public static void Notify_PortalDespawned(ChaosPortalSmall portal, Map map)
        {
            int num = GenRadial.NumCellsInRadius(2f);

            Faction chaosFaction = null;
            foreach (Faction f in Find.FactionManager.AllFactions)
            {
                if (f.Name.Equals("RH_TET_ChaosMonsterFaction"))
                {
                    chaosFaction = f;
                    break;
                }
            }

            for (int i = 0; i < num; i++)
            {
                IntVec3 c = portal.Position + GenRadial.RadialPattern[i];
                if (c.InBounds(map))
                {
                    List<Thing> thingList = c.GetThingList(map);
                    for (int j = 0; j < thingList.Count; j++)
                    {
                        if (thingList[j].Faction == chaosFaction && !ChaosPortalUtility.AnyPortalPreventsClaiming(thingList[j]))
                        {
                            thingList[j].SetFaction(null, null);
                        }
                    }
                }
            }
        }

        public static void Notify_PortalDespawned(ChaosPortalGreat portal, Map map)
        {
            int num = GenRadial.NumCellsInRadius(2f);

            Faction chaosFaction = null;
            foreach (Faction f in Find.FactionManager.AllFactions)
            {
                if (f.Name.Equals("RH_TET_ChaosMonsterFaction"))
                {
                    chaosFaction = f;
                    break;
                }
            }

            for (int i = 0; i < num; i++)
            {
                IntVec3 c = portal.Position + GenRadial.RadialPattern[i];
                if (c.InBounds(map))
                {
                    List<Thing> thingList = c.GetThingList(map);
                    for (int j = 0; j < thingList.Count; j++)
                    {
                        if (thingList[j].Faction == chaosFaction && !ChaosPortalUtility.AnyPortalPreventsClaiming(thingList[j]))
                        {
                            thingList[j].SetFaction(null, null);
                        }
                    }
                }
            }
        }
    }
}
