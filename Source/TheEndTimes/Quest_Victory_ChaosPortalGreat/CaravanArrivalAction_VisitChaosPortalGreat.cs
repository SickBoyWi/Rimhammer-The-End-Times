using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

namespace TheEndTimes
{
    public class CaravanArrivalAction_VisitChaosPortalGreat : CaravanArrivalAction
    {
        private MapParent target;

        public override string Label
        {
            get
            {
                return "RH_TET_VisitChaosPortalGreat".Translate();
            }
        }

        public override string ReportString
        {
            get
            {
                return "RH_TET_VisitingChaosPortalGreat".Translate();
            }
        }

        public CaravanArrivalAction_VisitChaosPortalGreat()
        {
        }

        public CaravanArrivalAction_VisitChaosPortalGreat(ChaosPortalGreatComp chaosPortalGreat)
        {
            this.target = (MapParent)chaosPortalGreat.parent;
        }

        public override FloatMenuAcceptanceReport StillValid(Caravan caravan, int destinationTile)
        {
            FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(caravan, destinationTile);
            if (!floatMenuAcceptanceReport)
            {
                return floatMenuAcceptanceReport;
            }
            if (this.target != null && this.target.Tile != destinationTile)
            {
                return false;
            }
            return CaravanArrivalAction_VisitChaosPortalGreat.CanVisit(caravan, this.target);
        }

        public override void Arrived(Caravan caravan)
        {
            if (!this.target.HasMap)
            {
                LongEventHandler.QueueLongEvent(delegate
                {
                    this.DoArrivalAction(caravan);
                }, "GeneratingMapForNewEncounter", false, null);
            }
            else
            {
                this.DoArrivalAction(caravan);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<MapParent>(ref this.target, "target", false);
        }

        private void DoArrivalAction(Caravan caravan)
        {
            bool flag = !this.target.HasMap;
            if (flag)
            {
                this.target.SetFaction(Faction.OfPlayer);
            }
            Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(this.target.Tile, null);
            Pawn t = caravan.PawnsListForReading[0];
            CaravanEnterMapUtility.Enter(caravan, orGenerateMap, CaravanEnterMode.Edge, CaravanDropInventoryMode.UnloadIndividually, false, null);
            if (flag)
            {
                Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
                Find.LetterStack.ReceiveLetter("RH_TET_ChaosPortalGreat_FoundLabel".Translate(), "RH_TET_ChaosPortalGreat_Found".Translate(), LetterDefOf.PositiveEvent, new GlobalTargetInfo(this.target.Map.Center, this.target.Map, false), null, null);
            }
            else
            {
                Find.LetterStack.ReceiveLetter("LetterLabelCaravanEnteredMap".Translate(this.target), "LetterCaravanEnteredMap".Translate(caravan.Label, this.target).CapitalizeFirst(), LetterDefOf.NeutralEvent, t, null, null);
            }
        }

        public static FloatMenuAcceptanceReport CanVisit(Caravan caravan, MapParent chaosPortalGreatComp)
        {
            if (chaosPortalGreatComp == null || !chaosPortalGreatComp.Spawned || chaosPortalGreatComp.GetComponent<ChaosPortalGreatComp>() == null)
            {
                return false;
            }
            if (chaosPortalGreatComp.EnterCooldownBlocksEntering())
            {
                return FloatMenuAcceptanceReport.WithFailMessage("MessageEnterCooldownBlocksEntering".Translate(chaosPortalGreatComp.EnterCooldownDaysLeft().ToString("0.#")));
            }
            return true;
        }

        public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, MapParent chaosPortalGreat)
        {
            return CaravanArrivalActionUtility.GetFloatMenuOptions<CaravanArrivalAction_VisitChaosPortalGreat>(() => CaravanArrivalAction_VisitChaosPortalGreat.CanVisit(caravan, chaosPortalGreat), () => new CaravanArrivalAction_VisitChaosPortalGreat(chaosPortalGreat.GetComponent<ChaosPortalGreatComp>()), "RH_TET_VisitChaosPortalGreat".Translate(chaosPortalGreat.Label), caravan, chaosPortalGreat.Tile, chaosPortalGreat);
        }
    }
}
