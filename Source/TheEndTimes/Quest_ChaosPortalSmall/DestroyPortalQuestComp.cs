
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace TheEndTimes
{
    public class DestroyPortalQuestComp : WorldObjectComp, IThingHolder
    {
        // TODO May need to postfix PlayerItemAccessibilityUtility.PlayerOrQuestRewardHas to include this quest comp.


        private bool rh_tet_active;
        public Faction rh_tet_factionRequestor;
        public int rh_tet_relationshipImprovement;
        public ThingOwner r_tet_rewardThing;
        private static List<Thing> rh_tet_tempRewards = new List<Thing>();

        public bool Active
        {
            get
            {
                return this.rh_tet_active;
            }
        }

        public DestroyPortalQuestComp()
        {
            this.r_tet_rewardThing = new ThingOwner<Thing>(this);
        }

        public void StartQuest(Faction requestingFaction, int relationsImprovement, List<Thing> rewards)
        {
            this.StopQuest();
            this.rh_tet_active = true;
            this.rh_tet_factionRequestor = requestingFaction;
            this.rh_tet_relationshipImprovement = relationsImprovement;
            this.r_tet_rewardThing.ClearAndDestroyContents(DestroyMode.Vanish);
            this.r_tet_rewardThing.TryAddRangeOrTransfer(rewards, true, false);
        }

        public void StopQuest()
        {
            this.rh_tet_active = false;
            this.rh_tet_factionRequestor = null;
            this.r_tet_rewardThing.ClearAndDestroyContents(DestroyMode.Vanish);
        }

        public override void CompTick()
        {
            base.CompTick();
            if (this.rh_tet_active)
            {
                MapParent mapParent = this.parent as MapParent;
                if (mapParent != null)
                {
                    this.CheckBuildingDestroyed(mapParent);
                }
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
                    if (thing != null && thing.def.defName.Equals("RH_TET_ChaosPortal_Small"))
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

            this.GiveRewardsAndSendLetter();
            this.StopQuest();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.rh_tet_active, "rh_tet_active", false, false);
            Scribe_Values.Look<int>(ref this.rh_tet_relationshipImprovement, "rh_tet_relationshipImprovement", 0, false);
            Scribe_References.Look<Faction>(ref this.rh_tet_factionRequestor, "rh_tet_factionRequestor", false);
            Scribe_Deep.Look<ThingOwner>(ref this.r_tet_rewardThing, "r_tet_rewardThing", (object)this);
        }

        private void GiveRewardsAndSendLetter()
        {
            Map map = Find.AnyPlayerHomeMap ?? ((MapParent)this.parent).Map;
            DestroyPortalQuestComp.rh_tet_tempRewards.AddRange(this.r_tet_rewardThing);
            this.r_tet_rewardThing.Clear();
            IntVec3 intVec = DropCellFinder.TradeDropSpot(map);
            DropPodUtility.DropThingsNear(intVec, map, DestroyPortalQuestComp.rh_tet_tempRewards, 110, false, false, false);
            DestroyPortalQuestComp.rh_tet_tempRewards.Clear();
            FactionRelationKind playerRelationKind = this.rh_tet_factionRequestor.PlayerRelationKind;
            TaggedString text = new TaggedString();
            text += "RH_TET_LetterDestroyPortalQuestCompleted".Translate(this.rh_tet_factionRequestor.Name, this.rh_tet_relationshipImprovement.ToString());
            this.rh_tet_factionRequestor.TryAffectGoodwillWith(Faction.OfPlayer, this.rh_tet_relationshipImprovement, false, false, null, null);
            this.rh_tet_factionRequestor.TryAppendRelationKindChangedInfo(ref text, playerRelationKind, this.rh_tet_factionRequestor.PlayerRelationKind, null);
            Find.LetterStack.ReceiveLetter("RH_TET_LetterLabelDestroyPortalQuestCompleted".Translate(), text.ToString(), LetterDefOf.PositiveEvent, new GlobalTargetInfo(intVec, map, false), this.rh_tet_factionRequestor, null);
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return this.r_tet_rewardThing;
        }

        public override void PostDestroy()
        {
            base.PostDestroy();
            if (this.r_tet_rewardThing != null)
                this.r_tet_rewardThing.ClearAndDestroyContents(DestroyMode.Vanish);
        }

        public override string CompInspectStringExtra()
        {
            if (this.rh_tet_active)
            {
                string value = GenThing.ThingsToCommaList(this.r_tet_rewardThing, true, true, 5).CapitalizeFirst();
                return "QuestTargetDestroyInspectString".Translate(this.rh_tet_factionRequestor.Name, value, GenThing.GetMarketValue(this.r_tet_rewardThing).ToStringMoney(null)).CapitalizeFirst();
            }
            return null;
        }
    }
}