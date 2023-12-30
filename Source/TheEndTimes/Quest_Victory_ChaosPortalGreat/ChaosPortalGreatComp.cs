using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace TheEndTimes
{
    [StaticConstructorOnStartup]
    public class ChaosPortalGreatComp : WorldObjectComp
    {
        private bool active = true;

        public override void CompTick()
        {
            base.CompTick();
            MapParent mapParent = this.parent as MapParent;

            if (mapParent != null)
            {
                this.CheckBuildingDestroyed(mapParent);
            }
        }

        private void CheckBuildingDestroyed(MapParent mapParent)
        {
            if (!mapParent.HasMap)
            {
                return;
            }

            CellRect centerAreaOfMap = new CellRect(mapParent.Map.Center.x - 16, mapParent.Map.Center.z - 16, 32, 32);
            Thing portal = null;

            foreach (IntVec3 intvec3 in centerAreaOfMap.Cells)
            {
                intvec3.GetFirstBuilding(mapParent.Map);
                
                foreach (Thing thing in intvec3.GetThingList(mapParent.Map))
                {
                    if (thing != null && thing.def.defName.Equals("RH_TET_ChaosPortal_Great"))
                    {
                        portal = thing;
                        break;
                    }

                }
            }

            if (portal != null && !portal.Destroyed)
            {
                return;
            }

            if (active)
            {
                active = false;
                this.CleanUpQuestMap(mapParent);
                RH_TET_GameVictoryUtility.GreatPortalDestroyedEnded();
            }
        }

        public void CleanUpQuestMap(MapParent mapParent)
        {
            TaleRecorder.RecordTale(TaleDef.Named("RH_TET_ChaosPortalGreatDestroyed"), new object[]
            {
                    mapParent.Map.mapPawns.FreeColonists.RandomElement<Pawn>()
            });
            
            // Kick the colonist pawns out of the map, and destroy the world object!
            TimedForcedExit.ForceReform(Find.CurrentMap.Parent);
            //WorldObjectCompProperties_TimedForcedExit FIND THIS COMP TO FIX
        }

        [DebuggerHidden]
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
        {
            foreach (FloatMenuOption f in CaravanArrivalAction_VisitChaosPortalGreat.GetFloatMenuOptions(caravan, (MapParent)this.parent))
            {
                yield return f;
            }
        }
    }
}
