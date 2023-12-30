using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace TheEndTimes
{
    public class GenStep_JabberslytheLair : GenStep
    {
        private const int Size = 6;
        public override int SeedPart { get; }
        private List<IntVec3> possibleSpawnCells = new List<IntVec3>();

        public override void Generate(Map map, GenStepParams parms)
        {
            IntVec3 loc;
            if (!JabberslytheLairCellFinder.TryFindCell(out loc, map))
            {
                Log.Warning("Could not find a spot for the jabberslythe lair to be. Defaulting to center of map.");
                loc = map.Center;
            }

            Faction faction = Find.FactionManager.FirstFactionOfDef(TheEndTimesDefOf.RH_TET_ChaosMonsterFaction);

            CellRect rectToDefend;
            CellRect centerRectToDefend;
            if (!MapGenerator.TryGetVar<CellRect>("RectOfInterest", out rectToDefend))
            {
                rectToDefend = CellRect.SingleCell(loc);
            }
            if (!MapGenerator.TryGetVar<CellRect>("RectOfInterest", out centerRectToDefend))
            {
                centerRectToDefend = CellRect.SingleCell(map.Center);
            }
           
            ResolveParams rp = default(ResolveParams);
            //rp.rect = this.GetOutpostRect(rectToDefend, map);
            rp.rect = rectToDefend;
            rp.faction = faction;
            BaseGen.symbolStack.Push("ensureCanReachMapEdge", rp);

            BaseGen.globalSettings.map = map;
            Lord singlePawnLord = rp.singlePawnLord ?? LordMaker.MakeNewLord(faction, new LordJob_DefendPoint(rp.rect.CenterCell), map, null);
            
            //BaseGen.symbolStack.Push("outdoorLighting", rp);

            ResolveParams resolveParams3 = rp;
            float biologicalAge = (float)TheEndTimesMod.random.Next(800, 950);
            PawnGenerationRequest pawnRequest = new PawnGenerationRequest(
                    TheEndTimesDefOf.RH_TET_Jabberslythe, faction, PawnGenerationContext.NonPlayer, 
                    -1, true, false, 
                    false, false, false, 
                    0F, false, true, 
                    false, false, false, 
                    false, false, false, 
                    false, 0.0f, 0.0f, 
                    null, 0.0f, null, 
                    null, null, null, 
                    0.0f, biologicalAge, biologicalAge);
            Pawn pawn = PawnGenerator.GeneratePawn(pawnRequest);
            
            resolveParams3.singlePawnToSpawn = pawn;
            resolveParams3.rect = new CellRect(rp.rect.CenterCell.x, rp.rect.CenterCell.z, 3, 3);
            resolveParams3.singlePawnLord = singlePawnLord;
            BaseGen.symbolStack.Push("pawn", resolveParams3);
            
            // Jabber Room
            var jabberRoomRect = rp.rect;

            // Animal Filth
            // TODO FIX:
            //ResolveParams filth = rp;
            //filth.rect = BottomOfMap(jabberRoomRect).ExpandedBy(3);
            //filth.filthDef = ThingDef.Named("Filth_AnimalFilth");
            //filth.chanceToSkipFloor = 0f;
            //filth.filthDensity = new FloatRange(4, 8);
            //filth.streetHorizontal = true;
            //BaseGen.symbolStack.Push("filthMaker", filth);
            
            // Treasure
            ResolveParams treasureHorde = rp;
            treasureHorde.rect = BottomOfMap(jabberRoomRect).ExpandedBy(5);
            treasureHorde.thingSetMakerDef = TheEndTimesDefOf.RH_TET_Treasure;
            var newParamsForItemGen = new ThingSetMakerParams();
            newParamsForItemGen.countRange = new IntRange(15, 20);
            newParamsForItemGen.maxThingMarketValue = Rand.Range(10000, 15000);
            treasureHorde.thingSetMakerParams = newParamsForItemGen;
            treasureHorde.singleThingStackCount = 250;
            BaseGen.symbolStack.Push("stockpile", treasureHorde);
            
            BaseGen.Generate();

            ResolveParams rp2 = default(ResolveParams);
            rp2.rect = centerRectToDefend;
            rp2.faction = faction;
            BaseGen.symbolStack.Push("ensureCanReachMapEdge", rp2);

            // Corpses
            ResolveParams corpses = rp2;
            corpses.rect = centerRectToDefend.ExpandedBy(15);
            corpses.hivesCount = Rand.Range(15, 30); // Hive count is how many bodies to make.
            corpses.faction = Find.FactionManager.RandomEnemyFaction();
            BaseGen.symbolStack.Push("corpseMaker", corpses);

            // More Corpses
            ResolveParams moreCorpses = rp2;
            moreCorpses.rect = centerRectToDefend.ExpandedBy(25);
            moreCorpses.hivesCount = Rand.Range(10, 15); // Hive count is how many bodies to make.
            moreCorpses.faction = Find.FactionManager.RandomEnemyFaction();
            BaseGen.symbolStack.Push("corpseMaker", moreCorpses);

            BaseGen.globalSettings.map = map;

            BaseGen.Generate();
        }

        public static CellRect TopOfMap(CellRect rect)
        {
            return new CellRect(rect.minX, rect.minZ, rect.Width, (int)(rect.Height / 2f));
        }


        public static CellRect BottomOfMap(CellRect rect)
        {
            return new CellRect(rect.minX, rect.minZ + (int)(rect.Height / 2f), rect.Width, (int)(rect.Height / 2f));
        }

    }
}
