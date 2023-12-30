using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TheEndTimes
{
    public class PlaceWorker_OnTopOfWalls : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef,
                                                            IntVec3 loc,
                                                               Rot4 rot,
                                                                Map map,
                                                              Thing thingToIgnore = null,
                                                              Thing thing = null)
        {

            Building building = loc.GetEdifice(map);

            if (building == null || building.def == null || building.def.graphicData == null)
                return "Must be placed on walls.";
            else if ((building.def.graphicData.linkFlags & (LinkFlags.Wall | LinkFlags.Rock)) == 0)
                return "Must be placed on walls.";

            if (rot.FacingCell != null)
            {

                IntVec3 facingLoc = loc;

                switch (rot.AsInt)
                {
                    case 0: // south
                        --facingLoc.z;
                        break;
                    case 1: // west
                        --facingLoc.x;
                        break;
                    case 2: // north
                        ++facingLoc.z;
                        break;
                    case 3: // east
                        ++facingLoc.x;
                        break;
                    default:
                        throw new System.Exception($"RH_TET: PlaceWorker_BuildingWall " +
                            "found an invalid rotation for placed thing at {loc}.");
                }
                
                Building facingLocBuilding = facingLoc.GetEdifice(map);
                if (facingLocBuilding != null && facingLocBuilding.def != null && facingLocBuilding.def.graphicData != null && IsWallRockDoorOrSolid(facingLocBuilding))
                    return (AcceptanceReport)("Must have open space in front.");
            }

            return AcceptanceReport.WasAccepted;
        }

        public override void DrawGhost(
          ThingDef def,
          IntVec3 center,
          Rot4 rot,
          Color ghostCol,
          Thing _thing = null)
        {
            base.DrawGhost(def, center, rot, ghostCol, (Thing)null);
            bool flag = false;
            Map currentMap = Find.CurrentMap;
            if (!GenGrid.InBounds(center + IntVec3Utility.RotatedBy(IntVec3.South, rot), currentMap))
                flag = true;
            Room roomGroup = GridsUtility.GetRoom(center + IntVec3Utility.RotatedBy(new IntVec3(0, 0, 1), rot), currentMap);
            if (roomGroup != null)
            {
                if (roomGroup != StaticWallSconceStuff.lastRoom)
                {
                    StaticWallSconceStuff.lastRoom = roomGroup;
                    StaticWallSconceStuff.preLitCells.Clear();
                    using (List<IntVec3>.Enumerator enumerator1 = new List<IntVec3>(roomGroup.Cells).GetEnumerator())
                    {
                        while (enumerator1.MoveNext())
                        {
                            IntVec3 current = enumerator1.Current;
                            List<Thing> thingList = new List<Thing>();
                            using (List<Thing>.Enumerator enumerator2 = GridsUtility.GetThingList(current, currentMap).GetEnumerator())
                            {
                                while (enumerator2.MoveNext())
                                {
                                    if (((string)((Def)enumerator2.Current.def).defName).Contains("SconceGlower"))
                                    {
                                        StaticWallSconceStuff.preLitCells.Add(current);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
                flag = true;
            if (!flag)
            {
                StaticWallSconceStuff.ClearCells();
                using (List<IntVec3>.Enumerator enumerator = StaticWallSconceStuff.preLitCells.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                        StaticWallSconceStuff.GetLightCells(enumerator.Current, currentMap);
                }
                StaticWallSconceStuff.GetLightCells(center + IntVec3Utility.RotatedBy(IntVec3.South, rot), currentMap);
                if (StaticWallSconceStuff.totalCells.Count > 0)
                    GenDraw.DrawFieldEdges(StaticWallSconceStuff.totalCells);
            }
        }

        private bool IsWallRockDoorOrSolid(Building b)
        {
            if (b == null)
                return false;
            if (b.def.defName.Contains("Wall"))
                return true;
            else if (b.def.building.isNaturalRock)
                return true;
            else if (b.def.fillPercent > .75f)
                return true;
            if (b.def.defName.Contains("Door") || b.def.defName.Contains("Gate"))
                return true;
            return false;
        }

        public PlaceWorker_OnTopOfWalls()
            : base()
        {
        }
    }
}
