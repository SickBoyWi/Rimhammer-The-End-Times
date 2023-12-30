using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TheEndTimes
{
    public class IncidentWorker_AbandondedTechnologyFind : IncidentWorker
    {
        private static readonly Pair<int, float>[] CountChance = new Pair<int, float>[]
        {
            new Pair<int, float>(1, 1f),
            new Pair<int, float>(2, 0.95f),
            new Pair<int, float>(3, 0.7f),
            new Pair<int, float>(4, 0.4f),
            new Pair<int, float>(5, 0.1f)
        };

        private int RandomCountToDrop
        {
            get
            {
                float timePassedFactor = Mathf.Clamp(GenMath.LerpDouble(0.0f, 1.2f, 1f, 0.1f, (float)Find.TickManager.TicksGame / 3600000f), 0.1f, 1f);
                return ((IEnumerable<Pair<int, float>>)IncidentWorker_AbandondedTechnologyFind.CountChance).RandomElementByWeight<Pair<int, float>>((Func<Pair<int, float>, float>)(x =>
                {
                    if (x.First == 1)
                        return x.Second;
                    return x.Second * timePassedFactor;
                })).First;
            }
        }

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }
            Map map = (Map)parms.target;
            IntVec3 intVec;
            bool retVal = this.TryFindTechChunkDropCell(map.Center, map, 999999, out intVec);
            return retVal;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            IntVec3 intVec;
            if (!this.TryFindTechChunkDropCell(map.Center, map, 999999, out intVec))
            {
                return false;
            }
            this.SpawnTechChunks(intVec, map, this.RandomCountToDrop);
            Messages.Message("RH_TET_MessageTechChunkFound".Translate(), new TargetInfo(intVec, map, false), MessageTypeDefOf.NeutralEvent, true);
            return true;
        }

        private void SpawnTechChunks(IntVec3 firstChunkPos, Map map, int count)
        {
            this.SpawnChunk(firstChunkPos, map);
            for (int i = 0; i < count - 1; i++)
            {
                IntVec3 pos;
                if (this.TryFindTechChunkDropCell(firstChunkPos, map, 999999, out pos))
                {
                    this.SpawnChunk(pos, map);
                }
            }
        }

        private void SpawnChunk(IntVec3 pos, Map map)
        {
            Thing thing = ThingMaker.MakeThing(ThingDef.Named("RH_TET_TechChunk"), null);
            GenPlace.TryPlaceThing(thing, pos, map, ThingPlaceMode.Near, null, null);
        }

        private bool TryFindTechChunkDropCell(IntVec3 nearLoc, Map map, int maxDist, out IntVec3 pos)
        {
            ThingDef techChunkIncoming = ThingDef.Named("RH_TET_TechChunkIncoming");
            return TryFindAbandonedTechCell(nearLoc, map, 20, out pos);
        }

        private bool TryFindAbandonedTechCell(IntVec3 nearLoc, Map map, int maxDist, out IntVec3 pos)
        {
            return TryFindAbandondedTechCell(ThingDefOf.ShipChunkIncoming, map, out pos, 3, nearLoc, -1, true, false, false, false, true, false, (Predicate<IntVec3>)null);
        }

        public static bool TryFindAbandondedTechCell(
          ThingDef skyfaller,
          Map map,
          out IntVec3 cell,
          int minDistToEdge = 10,
          IntVec3 nearLoc = default(IntVec3),
          int nearLocMaxDist = -1,
          bool allowRoofedCells = true,
          bool allowCellsWithItems = false,
          bool allowCellsWithBuildings = false,
          bool colonyReachable = false,
          bool avoidColonistsIfExplosive = true,
          bool alwaysAvoidColonists = false,
          Predicate<IntVec3> extraValidator = null)
        {
            bool avoidColonists = ((!avoidColonistsIfExplosive ? 0 : (skyfaller.skyfaller.CausesExplosion ? 1 : 0)) | (alwaysAvoidColonists ? 1 : 0)) != 0;
            Predicate<IntVec3> validator = (Predicate<IntVec3>)(x =>
            {
                foreach (IntVec3 c in GenAdj.OccupiedRect(x, Rot4.North, skyfaller.size))
                {
                    if (!c.InBounds(map) || c.Fogged(map) || !c.Standable(map) || c.Roofed(map) && c.GetRoof(map).isThickRoof || !allowRoofedCells && c.Roofed(map) || (!allowCellsWithItems && c.GetFirstItem(map) != null || !allowCellsWithBuildings && c.GetFirstBuilding(map) != null) || c.GetFirstSkyfaller(map) != null)
                        return false;
                }
                return (!avoidColonists || !SkyfallerUtility.CanPossiblyFallOnColonist(skyfaller, x, map)) && (minDistToEdge <= 0 || x.DistanceToEdge(map) >= minDistToEdge) && ((!colonyReachable || map.reachability.CanReachColony(x)) && (extraValidator == null || extraValidator(x)));
            });
            bool retVal = false;
            if (nearLocMaxDist > 0)
            {
                retVal = CellFinder.TryFindRandomCellNear(nearLoc, map, nearLocMaxDist, validator, out cell, -1);
                return retVal;
            }
            retVal = CellFinderLoose.TryFindRandomNotEdgeCellWith(minDistToEdge, validator, map, out cell);
            return retVal;
        }
    }
}
