using RimWorld;
using RimWorld.Planet;
using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace TheEndTimes
{
    public class SitePartWorker_ChaosPortalSmall : SitePartWorker
    {
        public override string GetArrivedLetterPart(Map map, out LetterDef preferredLetterDef, out LookTargets lookTargets)
        {
            string arrivedLetterPart = base.GetArrivedLetterPart(map, out preferredLetterDef, out lookTargets);
            lookTargets = (from x in map.mapPawns.AllPawnsSpawned
                           where x.RaceProps.Humanlike && x.HostileTo(Faction.OfPlayer)
                           select x).FirstOrDefault<Pawn>();
            return arrivedLetterPart;
        }

        public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
        {
            return string.Concat(new object[]
            {
                base.GetPostProcessedThreatLabel(site, sitePart),
                " (",
                this.GetEnemiesCount(site, sitePart.parms),
                " ",
                "Enemies".Translate(),
                ")"
            });
        }

        public override SitePartParams GenerateDefaultParams(
          float myThreatPoints,
          int tile,
          Faction faction)
        {
            if (faction == null)
            {
                faction = Find.FactionManager.FirstFactionOfDef(TheEndTimesDefOf.RH_TET_ChaosMonsterFaction);
                Log.Warning("RH_TET: Faction null in Chaos Portal Site Part Worker. Correcting manually, to avoid errors.");
            }
            SitePartParams defaultParams = base.GenerateDefaultParams(myThreatPoints, tile, faction);
            defaultParams.threatPoints = Mathf.Max(defaultParams.threatPoints, faction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Settlement));
            return defaultParams;
        }

        private int GetEnemiesCount(Site site, SitePartParams parms)
        {
            return PawnGroupMakerUtility.GeneratePawnKindsExample(new PawnGroupMakerParms
            {
                tile = site.Tile,
                faction = site.Faction,
                groupKind = PawnGroupKindDefOf.Settlement,
                points = parms.threatPoints,
                inhabitants = true,
                seed = new int?(OutpostSitePartUtility.GetPawnGroupMakerSeed(parms))
            }).Count<PawnKindDef>();
        }
    }
}
