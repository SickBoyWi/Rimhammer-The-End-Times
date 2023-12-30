using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.AI;

namespace TheEndTimes
{
    public class GenStep_ChaosPortalGreat : GenStep
    {
        public override void Generate(Map map, GenStepParams parms)
        {
            Faction faction = Find.FactionManager.FirstFactionOfDef(TheEndTimesDefOf.RH_TET_ChaosMonsterFaction);

            IntVec3 result = map.Center;
            this.TryFindScatterCell(map, out result);
                        
            // Add portal.
            Thing portal = GenSpawn.Spawn(ThingMaker.MakeThing(ThingDef.Named("RH_TET_ChaosPortal_Great"), null), result, map, WipeMode.Vanish);
            portal.SetFaction(faction);
            map.attackTargetsCache.TargetsHostileToFaction(Find.FactionManager.OfPlayer).Add((IAttackTarget)portal);

            IEnumerable<IntVec3> portalAreaCells = portal.CellsAdjacent8WayAndInside();

            int minx = 1000;
            int minz = 1000;

            foreach (IntVec3 intvec3 in portalAreaCells)
            {
                if (intvec3.x < minx)
                {
                    minx = intvec3.x;
                }
                if (intvec3.z < minz)
                {
                    minz = intvec3.z;
                }
            }

            //BaseGen.Generate();
        }

        protected virtual bool TryFindScatterCell(Map map, out IntVec3 result)
        {
            if (RCellFinder.TryFindRandomCellNearWith(map.Center, (IntVec3 x) => this.CanScatterAt(x, map), map, out result, 3, 2147483647))
            {
                return true;
            }
            Log.Warning("Scatterer " + this.ToString() + " could not find cell to generate at.", false);
            return false;
        }

        protected bool CanScatterAt(IntVec3 c, Map map)
        {
            if (!c.Standable(map))
            {
                return false;
            }
            if (c.Roofed(map))
            {
                return false;
            }
            if (!map.reachability.CanReachMapEdge(c, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)))
            {
                return false;
            }
            CellRect cellRect = new CellRect(c.x - 16 / 2, c.z -16 / 2, 16, 16);
            if (!cellRect.FullyContainedWithin(new CellRect(0, 0, map.Size.x, map.Size.z)))
            {
                return false;
            }
            foreach (IntVec3 current in cellRect)
            {
                TerrainDef terrainDef = map.terrainGrid.TerrainAt(current);
                if (!terrainDef.affordances.Contains(TerrainAffordanceDefOf.Heavy) && (terrainDef.driesTo == null || !terrainDef.driesTo.affordances.Contains(TerrainAffordanceDefOf.Heavy)))
                {
                    return false;
                }
            }
            return true;
        }

        public override int SeedPart
        {
            get
            {
                return 860022045;
            }
        }

        private const int Size = 10;
        private static List<CellRect> possibleRects = new List<CellRect>();
        public FloatRange defaultPawnGroupPointsRange = SymbolResolver_Settlement.DefaultPawnsPoints;
    }
}
