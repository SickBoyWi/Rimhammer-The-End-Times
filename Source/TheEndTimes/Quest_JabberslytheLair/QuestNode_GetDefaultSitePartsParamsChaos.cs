using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;

namespace TheEndTimes
{
    public class QuestNode_GetDefaultSitePartsParamsChaos : QuestNode
    {
        public SlateRef<int> tile;
        public SlateRef<Faction> faction;
        public SlateRef<IEnumerable<SitePartDef>> sitePartDefs;
        [NoTranslate]
        public SlateRef<string> storeSitePartsParamsAs;

        protected override bool TestRunInt(Slate slate)
        {
            this.SetVars(slate);
            return true;
        }

        protected override void RunInt()
        {
            this.SetVars(RimWorld.QuestGen.QuestGen.slate);
        }

        private void SetVars(Slate slate)
        {
            List<SitePartDefWithParams> sitePartDefsWithParams;
            SiteMakerHelper.GenerateDefaultParams(slate.Get<float>("points", 0.0f, false), 
                this.tile.GetValue(slate),
                Find.FactionManager.FirstFactionOfDef(TheEndTimesDefOf.RH_TET_ChaosMonsterFaction), 
                this.sitePartDefs.GetValue(slate), 
                out sitePartDefsWithParams);

            for (int index = 0; index < sitePartDefsWithParams.Count; ++index)
            {
                if (sitePartDefsWithParams[index].def == SitePartDefOf.PreciousLump)
                    sitePartDefsWithParams[index].parms.preciousLumpResources = slate.Get<ThingDef>("targetMineable", (ThingDef)null, false);
            }

            slate.Set<List<SitePartDefWithParams>>(this.storeSitePartsParamsAs.GetValue(slate), sitePartDefsWithParams, false);
        }
    }
}
