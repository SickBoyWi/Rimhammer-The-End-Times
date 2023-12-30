using System;
using System.Collections;
using System.Collections.Generic;
using Verse;

namespace TheEndTimes
{
    public static class StaticWallSconceStuff
    {
        public static int lastRad = 8;
        public static int lastPow = 10;
        public static int lastCost = 5;
        public static float displayRad = 5.2f;
        public static List<IntVec3> preLitCells = new List<IntVec3>();
        public static List<IntVec3> totalCells = new List<IntVec3>();
        public static Room lastRoom;

        public static void GetLightCells(IntVec3 pos, Map map)
        {
            bool flag = false;
            if (!GenGrid.InBounds(pos, map))
                flag = true;
            Region region = GridsUtility.GetRegion(pos, map, (RegionType)6);
            if (region == null)
                flag = true;
            if (flag)
                return;

            RegionTraverser.BreadthFirstTraverse(region, (RegionEntryPredicate)((from, r) => r.door == null), (RegionProcessor)(r =>
            {
                foreach (IntVec3 cell in r.Cells)
                {
                    if (cell.InHorDistOf(pos, StaticWallSconceStuff.displayRad) && !StaticWallSconceStuff.totalCells.Contains(cell))
                        StaticWallSconceStuff.totalCells.Add(cell);
                }
                return false;
            }), 13, (RegionType)6);
        }

        public static void ClearCells()
        {
            StaticWallSconceStuff.totalCells.Clear();
        }
    }
}
