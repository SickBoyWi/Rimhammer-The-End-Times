using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TheEndTimes
{
    public class PlaceWorker_Candle : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef,
                                                            IntVec3 loc,
                                                               Rot4 rot,
                                                                Map map,
                                                              Thing thingToIgnore = null,
                                                              Thing thing = null)
        {

            Building building = loc.GetEdifice(map);

            if (IsWallRockDoorOrSolid(building))
                return "May not be placed on walls, large buildings, or doors.";

            return AcceptanceReport.WasAccepted;
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

        public PlaceWorker_Candle()
            : base()
        {
        }
    }
}
