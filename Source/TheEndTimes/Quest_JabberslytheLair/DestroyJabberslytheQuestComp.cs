using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

namespace TheEndTimes
{
    public class DestroyJabberslytheQuestComp : WorldObjectComp
    {
        private bool rh_tet_activeJabber;

        public bool Active
        {
            get
            {
                return this.rh_tet_activeJabber;
            }
        }

        public DestroyJabberslytheQuestComp()
        {
        }

        public void StartQuest(Faction requestingFaction)
        {
            this.StopQuest();
            this.rh_tet_activeJabber = true;
        }

        public void StopQuest()
        {
            this.rh_tet_activeJabber = false;
        }

        public override void CompTick()
        {
            base.CompTick();
            if (this.rh_tet_activeJabber)
            {
                MapParent mapParent = this.parent as MapParent;
                if (mapParent != null)
                {
                    this.CheckAllEnemiesDefeated(mapParent);
                }
            }
        }

        private void CheckAllEnemiesDefeated(MapParent mapParent)
        {
            if (!mapParent.HasMap)
            {
                return;
            }
            if (GenHostility.AnyHostileActiveThreatToPlayer(mapParent.Map))
            {
                return;
            }
            this.SendLetter();
            this.StopQuest();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.rh_tet_activeJabber, "rh_tet_activeJabber", false, false);
        }

        private void SendLetter()
        {
            Map map = Find.AnyPlayerHomeMap ?? ((MapParent)this.parent).Map;
            IntVec3 intVec = DropCellFinder.TradeDropSpot(map);
            string text = "RH_TET_LetterDestroyJabberslytheQuestCompleted".Translate();
            Find.LetterStack.ReceiveLetter("RH_TET_LetterLabelDestroyJabberslytheQuestCompleted".Translate(), text, LetterDefOf.PositiveEvent, new GlobalTargetInfo(intVec, map, false), null, null);
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            return;
        }

        public override void PostPostRemove()
        {
            base.PostPostRemove();
        }

        public override string CompInspectStringExtra()
        {
            List<Thing> list = new List<Thing>();
            if (this.rh_tet_activeJabber)
            {
                string value = GenThing.ThingsToCommaList(list, true, true, 5).CapitalizeFirst();
                return "RH_TET_DestroyJabberslytheQuestInspectString".Translate().CapitalizeFirst();
            }
            return null;
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return null;
        }
    }
}
