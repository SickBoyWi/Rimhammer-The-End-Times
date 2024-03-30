using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TheEndTimes
{
    public static class JabberslytheLairCellFinder
    {
        private struct LocationCandidate
        {
            public IntVec3 cell;

            public float score;

            public LocationCandidate(IntVec3 cell, float score)
            {
                this.cell = cell;
                this.score = score;
            }
        }

        private static List<JabberslytheLairCellFinder.LocationCandidate> locationCandidates = new List<JabberslytheLairCellFinder.LocationCandidate>();
        private static Dictionary<Region, float> regionsDistanceToUnroofed = new Dictionary<Region, float>();
        private static ByteGrid closedAreaSize;
        private static ByteGrid distToCenter;
        private const float MinRequiredScore = 7.5f;
        private const float MinMountainousnessScore = 0.17f;
        private const int MountainousnessScoreRadialPatternIdx = 700;
        private const int MountainousnessScoreRadialPatternSkip = 10;
        private const float MountainousnessScorePerRock = 1f;
        private const float MountainousnessScorePerThickRoof = 0.5f;
        private const float MinCellTempToSpawnHive = -30f;
        private const float MaxDistanceToCenter = 50f;
        private static HashSet<Region> tempUnroofedRegions = new HashSet<Region>();
        private static List<IntVec3> tmpCenterLoc = new List<IntVec3>();
        private static List<KeyValuePair<IntVec3, float>> tmpDistanceResult = new List<KeyValuePair<IntVec3, float>>();

        public static bool TryFindCell(out IntVec3 cell, Map map)
        {
            JabberslytheLairCellFinder.CalculateLocationCandidates(map);
            JabberslytheLairCellFinder.LocationCandidate locationCandidate;
            if (!JabberslytheLairCellFinder.locationCandidates.TryRandomElementByWeight((JabberslytheLairCellFinder.LocationCandidate x) => x.score, out locationCandidate))
            {
                cell = IntVec3.Invalid;
                return false;
            }

            cell = locationCandidate.cell;

            return true;
        }

        private static float GetScoreAt(IntVec3 cell, Map map)
        {
            if ((float)JabberslytheLairCellFinder.distToCenter[cell] > MaxDistanceToCenter)
            {
                return 0f;
            }
            if (!cell.Walkable(map))
            {
                return 0f;
            }
            if (cell.Fogged(map))
            {
                return 0f;
            }
            if (JabberslytheLairCellFinder.CellHasBlockingThings(cell, map))
            {
                return 0f;
            }
            if (!cell.Roofed(map) || !cell.GetRoof(map).isThickRoof)
            {
                return 0f;
            }
            Region region = cell.GetRegion(map, RegionType.Set_Passable);
            if (region == null)
            {
                return 0f;
            }
            if (JabberslytheLairCellFinder.closedAreaSize[cell] < 2)
            {
                return 0f;
            }
            float temperature = cell.GetTemperature(map);
            if (temperature < MinCellTempToSpawnHive)
            {
                return 0f;
            }
            float mountainousnessScoreAt = JabberslytheLairCellFinder.GetMountainousnessScoreAt(cell, map);
            if (mountainousnessScoreAt < 0.17f)
            {
                return 0f;
            }
            int num = JabberslytheLairCellFinder.StraightLineDistToUnroofed(cell, map);
            float num2;
            if (!JabberslytheLairCellFinder.regionsDistanceToUnroofed.TryGetValue(region, out num2))
            {
                num2 = (float)num * 1.15f;
            }
            else
            {
                num2 = Mathf.Min(num2, (float)num * 4f);
            }
            num2 = Mathf.Pow(num2, 1.55f);
            float num3 = Mathf.InverseLerp(0f, 12f, (float)num);
            float num4 = Mathf.Lerp(1f, 0.18f, map.glowGrid.GroundGlowAt(cell));
            float num5 = 1f - Mathf.Clamp(JabberslytheLairCellFinder.DistToBlocker(cell, map) / 11f, 0f, 0.6f);
            float num6 = Mathf.InverseLerp(-17f, -7f, temperature);
            float num7 = num2 * num3 * num5 * mountainousnessScoreAt * num4 * num6;
            num7 = Mathf.Pow(num7, 1.2f);
            if (num7 < 7.5f)
            {
                return 0f;
            }
            return num7;
        }

        private static void CalculateLocationCandidates(Map map)
        {
            JabberslytheLairCellFinder.locationCandidates.Clear();
            JabberslytheLairCellFinder.CalculateTraversalDistancesToUnroofed(map);
            JabberslytheLairCellFinder.CalculateClosedAreaSizeGrid(map);
            JabberslytheLairCellFinder.CalculateDistanceToCenterGrid(map);
            for (int i = 0; i < map.Size.z; i++)
            {
                for (int j = 0; j < map.Size.x; j++)
                {
                    IntVec3 cell = new IntVec3(j, 0, i);
                    float scoreAt = JabberslytheLairCellFinder.GetScoreAt(cell, map);
                    if (scoreAt > 0f)
                    {
                        JabberslytheLairCellFinder.locationCandidates.Add(new JabberslytheLairCellFinder.LocationCandidate(cell, scoreAt));
                    }
                }
            }
        }

        private static bool CellHasBlockingThings(IntVec3 cell, Map map)
        {
            List<Thing> thingList = cell.GetThingList(map);
            for (int i = 0; i < thingList.Count; i++)
            {
                if (thingList[i] is Pawn || thingList[i] is Hive || thingList[i] is TunnelHiveSpawner)
                {
                    return true;
                }
                bool flag = thingList[i].def.category == ThingCategory.Building && thingList[i].def.passability == Traversability.Impassable;
                if (flag && GenSpawn.SpawningWipes(ThingDefOf.Hive, thingList[i].def))
                {
                    return true;
                }
            }
            return false;
        }

        private static int StraightLineDistToUnroofed(IntVec3 cell, Map map)
        {
            int num = 2147483647;
            int i = 0;
            while (i < 4)
            {
                Rot4 rot = new Rot4(i);
                IntVec3 facingCell = rot.FacingCell;
                int num2 = 0;
                int num3;
                while (true)
                {
                    IntVec3 intVec = cell + facingCell * num2;
                    if (!intVec.InBounds(map))
                    {
                        goto Block_1;
                    }
                    num3 = num2;
                    if (JabberslytheLairCellFinder.NoRoofAroundAndWalkable(intVec, map))
                    {
                        break;
                    }
                    num2++;
                }
            IL_6F:
                if (num3 < num)
                {
                    num = num3;
                }
                i++;
                continue;
            Block_1:
                num3 = 2147483647;
                goto IL_6F;
            }
            if (num == 2147483647)
            {
                return map.Size.x;
            }
            return num;
        }

        private static float DistToBlocker(IntVec3 cell, Map map)
        {
            int num = -2147483648;
            int num2 = -2147483648;
            for (int i = 0; i < 4; i++)
            {
                Rot4 rot = new Rot4(i);
                IntVec3 facingCell = rot.FacingCell;
                int num3 = 0;
                int num4;
                while (true)
                {
                    IntVec3 c = cell + facingCell * num3;
                    num4 = num3;
                    if (!c.InBounds(map) || !c.Walkable(map))
                    {
                        break;
                    }
                    num3++;
                }
                if (num4 > num)
                {
                    num2 = num;
                    num = num4;
                }
                else if (num4 > num2)
                {
                    num2 = num4;
                }
            }
            return (float)Mathf.Min(num, num2);
        }

        private static bool NoRoofAroundAndWalkable(IntVec3 cell, Map map)
        {
            if (!cell.Walkable(map))
            {
                return false;
            }
            if (cell.Roofed(map))
            {
                return false;
            }
            for (int i = 0; i < 4; i++)
            {
                Rot4 rot = new Rot4(i);
                IntVec3 c = rot.FacingCell + cell;
                if (c.InBounds(map) && c.Roofed(map))
                {
                    return false;
                }
            }
            return true;
        }

        private static float GetMountainousnessScoreAt(IntVec3 cell, Map map)
        {
            float num = 0f;
            int num2 = 0;
            for (int i = 0; i < 700; i += 10)
            {
                IntVec3 c = cell + GenRadial.RadialPattern[i];
                if (c.InBounds(map))
                {
                    Building edifice = c.GetEdifice(map);
                    if (edifice != null && edifice.def.category == ThingCategory.Building && edifice.def.building.isNaturalRock)
                    {
                        num += 1f;
                    }
                    else if (c.Roofed(map) && c.GetRoof(map).isThickRoof)
                    {
                        num += 0.5f;
                    }
                    num2++;
                }
            }
            return num / (float)num2;
        }

        private static void CalculateDistanceToCenterGrid(Map map)
        {
            if (JabberslytheLairCellFinder.distToCenter == null)
            {
                JabberslytheLairCellFinder.distToCenter = new ByteGrid(map);
            }
            else if (!JabberslytheLairCellFinder.distToCenter.MapSizeMatches(map))
            {
                JabberslytheLairCellFinder.distToCenter.ClearAndResizeTo(map);
            }
            JabberslytheLairCellFinder.distToCenter.Clear(255);
            JabberslytheLairCellFinder.tmpCenterLoc.Add(map.Center);
            Dijkstra<IntVec3>.Run(JabberslytheLairCellFinder.tmpCenterLoc, (IntVec3 x) => DijkstraUtility.AdjacentCellsNeighborsGetter(x, map), 
                delegate (IntVec3 a, IntVec3 b)
                {
                    if (a.x == b.x || a.z == b.z)
                    {
                        return 1f;
                    }
                    return 1.41421354f;
                }, JabberslytheLairCellFinder.tmpDistanceResult, null);
            for (int j = 0; j < JabberslytheLairCellFinder.tmpDistanceResult.Count; j++)
            {
                JabberslytheLairCellFinder.distToCenter[JabberslytheLairCellFinder.tmpDistanceResult[j].Key] = (byte)Mathf.Min(JabberslytheLairCellFinder.tmpDistanceResult[j].Value, 254.999f);
            }
        }

        private static void CalculateTraversalDistancesToUnroofed(Map map)
        {
            JabberslytheLairCellFinder.tempUnroofedRegions.Clear();
            for (int i = 0; i < map.Size.z; i++)
            {
                for (int j = 0; j < map.Size.x; j++)
                {
                    IntVec3 intVec = new IntVec3(j, 0, i);
                    Region region = intVec.GetRegion(map, RegionType.Set_Passable);
                    if (region != null && JabberslytheLairCellFinder.NoRoofAroundAndWalkable(intVec, map))
                    {
                        JabberslytheLairCellFinder.tempUnroofedRegions.Add(region);
                    }
                }
            }
            Dijkstra<Region>.Run(JabberslytheLairCellFinder.tempUnroofedRegions, (Region x) => x.Neighbors, (Region a, Region b) => Mathf.Sqrt((float)a.extentsClose.CenterCell.DistanceToSquared(b.extentsClose.CenterCell)), JabberslytheLairCellFinder.regionsDistanceToUnroofed, null);
            JabberslytheLairCellFinder.tempUnroofedRegions.Clear();
        }

        private static void CalculateClosedAreaSizeGrid(Map map)
        {
            if (JabberslytheLairCellFinder.closedAreaSize == null)
            {
                JabberslytheLairCellFinder.closedAreaSize = new ByteGrid(map);
            }
            else
            {
                JabberslytheLairCellFinder.closedAreaSize.ClearAndResizeTo(map);
            }
            for (int i = 0; i < map.Size.z; i++)
            {
                for (int j = 0; j < map.Size.x; j++)
                {
                    IntVec3 intVec = new IntVec3(j, 0, i);
                    if (JabberslytheLairCellFinder.closedAreaSize[j, i] == 0 && !intVec.Impassable(map))
                    {
                        int area = 0;
                        map.floodFiller.FloodFill(intVec, (IntVec3 c) => !c.Impassable(map), delegate (IntVec3 c)
                        {
                            area++;
                        }, 2147483647, false, null);
                        area = Mathf.Min(area, 255);
                        map.floodFiller.FloodFill(intVec, (IntVec3 c) => !c.Impassable(map), delegate (IntVec3 c)
                        {
                            JabberslytheLairCellFinder.closedAreaSize[c] = (byte)area;
                        }, 2147483647, false, null);
                    }
                }
            }
        }
    }
}
