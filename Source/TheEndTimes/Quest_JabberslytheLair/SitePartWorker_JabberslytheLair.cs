using RimWorld;
using RimWorld.Planet;
using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace TheEndTimes
{
    public class SitePartWorker_JabberslytheLair : SitePartWorker
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
            SitePartParams defaultParams = base.GenerateDefaultParams(myThreatPoints, tile, faction);
            
            float threatPoints = 0.0f;
            if (defaultParams.animalKind == null)
                threatPoints = TheEndTimesDefOf.RH_TET_Jabberslythe.combatPower;
            else
                threatPoints = defaultParams.animalKind.combatPower;
            
            defaultParams.threatPoints = Mathf.Max(defaultParams.threatPoints, threatPoints);
            
            return defaultParams;
        }

        private int GetEnemiesCount(Site site, SitePartParams parms)
        {
            return 1;
        }
    }
}
