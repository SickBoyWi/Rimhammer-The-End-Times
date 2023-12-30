using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.AI;

namespace TheEndTimes
{
    public class GenStep_ChaosPortalSmall : GenStep
    {
        private static List<CellRect> possibleRects = new List<CellRect>();
        private const int Size = 16;
        private const int PortalSize = 8;
        private float? PawnSpawnPoints = 250f;
        private static List<CellRect> portalRects = new List<CellRect>();
        public FloatRange defaultPawnGroupPointsRange = SymbolResolver_Settlement.DefaultPawnsPoints;

        public override int SeedPart
        {
            get
            {
                return 398038181;
            }
        }

        public override void Generate(Map map, GenStepParams parms)
        {
            Faction faction = Find.FactionManager.FirstFactionOfDef(TheEndTimesDefOf.RH_TET_ChaosMonsterFaction);

            // Add portal && flooring for it.
            Thing portal = GenSpawn.Spawn(ThingMaker.MakeThing(ThingDef.Named("RH_TET_ChaosPortal_Small"), null), map.Center, map, WipeMode.Vanish);
            portal.SetFaction(faction);
            map.attackTargetsCache.TargetsHostileToFaction(Find.FactionManager.OfPlayer).Add((IAttackTarget)portal);

            IEnumerable<IntVec3> portalAreaCells = portal.CellsAdjacent8WayAndInside();
            
            int minx = 1000;
            int minz = 1000;

            foreach (IntVec3 intvec3 in portalAreaCells)
            {
                if (intvec3.x < minx)
                {
                    minx = intvec3.x;
                }
                if (intvec3.z < minz)
                {
                    minz = intvec3.z;
                }
            }
            
            CellRect portalArea = new CellRect(minx - 4, minz - 4, 13, 15);

            // Clear map center, add floors? around portal.
            TerrainDef floorDef = TerrainDef.Named("FlagstoneGranite");
            ResolveParams areaAroundPortal = default(ResolveParams);
            areaAroundPortal.rect = portalArea;
            areaAroundPortal.clearEdificeOnly = true;
            areaAroundPortal.clearRoof = true;
            areaAroundPortal.faction = faction;
            areaAroundPortal.floorDef = floorDef;
            areaAroundPortal.pathwayFloorDef = floorDef;
            areaAroundPortal.spawnBridgeIfTerrainCantSupportThing = true;
            areaAroundPortal.edgeDefenseWidth = new int?(0);
            areaAroundPortal.edgeDefenseTurretsCount = new int?(0);
            areaAroundPortal.edgeDefenseMortarsCount = new int?(0);
            areaAroundPortal.settlementPawnGroupPoints = new float?(0f);
            areaAroundPortal.chanceToSkipWallBlock = 0f;
            BaseGen.symbolStack.Push("floor", areaAroundPortal);

            BaseGen.globalSettings.map = map;

            BaseGen.Generate();
            
            // Gen base part of the location.
            CellRect var;
            if (!MapGenerator.TryGetVar<CellRect>("RectOfInterest", out var))
                var = CellRect.SingleCell(map.Center); 
            ResolveParams resolveParams = new ResolveParams();
            resolveParams.rect = this.GetOutpostRect(var, map);
            resolveParams.faction = faction;
            resolveParams.edgeDefenseWidth = new int?(2);
            resolveParams.edgeDefenseTurretsCount = new int?(0);
            resolveParams.edgeDefenseMortarsCount = new int?(0);
            if (parms.sitePart != null)
            {
                resolveParams.settlementPawnGroupPoints = PawnSpawnPoints;
                resolveParams.settlementPawnGroupSeed = new int?(OutpostSitePartUtility.GetPawnGroupMakerSeed(parms.sitePart.parms));
            }
            else
                resolveParams.settlementPawnGroupPoints = new float?(this.defaultPawnGroupPointsRange.RandomInRange);

            RimWorld.BaseGen.BaseGen.globalSettings.map = map;
            RimWorld.BaseGen.BaseGen.globalSettings.minBuildings = 1;
            RimWorld.BaseGen.BaseGen.globalSettings.minBarracks = 1;
            RimWorld.BaseGen.BaseGen.symbolStack.Push("settlement", resolveParams);
            RimWorld.BaseGen.BaseGen.Generate();
        }

        private CellRect GetOutpostRect(CellRect rectToDefend, Map map)
        {
            GenStep_ChaosPortalSmall.possibleRects.Add(new CellRect(rectToDefend.minX - 1 - Size, rectToDefend.CenterCell.z - Size, Size, Size));
            GenStep_ChaosPortalSmall.possibleRects.Add(new CellRect(rectToDefend.maxX + 1, rectToDefend.CenterCell.z - Size, Size, Size));
            GenStep_ChaosPortalSmall.possibleRects.Add(new CellRect(rectToDefend.CenterCell.x - Size, rectToDefend.minZ - 1 - Size, Size, Size));
            GenStep_ChaosPortalSmall.possibleRects.Add(new CellRect(rectToDefend.CenterCell.x - Size, rectToDefend.maxZ + 1, Size, Size));
            CellRect mapRect = new CellRect(0, 0, map.Size.x, map.Size.z);
            GenStep_ChaosPortalSmall.possibleRects.RemoveAll((CellRect x) => !x.FullyContainedWithin(mapRect));
            if (GenStep_ChaosPortalSmall.possibleRects.Any<CellRect>())
            {
                return GenStep_ChaosPortalSmall.possibleRects.RandomElement<CellRect>();
            }
            return rectToDefend;
        }

        // SAVE FOR NOW. 
        //private CellRect GetPortalRect(CellRect rectToDefend, Map map)
        //{
        //    GenStep_ChaosPortalSmall.portalRects.Add(new CellRect(rectToDefend.minX - 1 - PortalSize, rectToDefend.CenterCell.z - PortalSize / 2, PortalSize, PortalSize));
        //    GenStep_ChaosPortalSmall.portalRects.Add(new CellRect(rectToDefend.maxX + 1, rectToDefend.CenterCell.z - PortalSize / 2, PortalSize, PortalSize));
        //    GenStep_ChaosPortalSmall.portalRects.Add(new CellRect(rectToDefend.CenterCell.x - PortalSize / 2, rectToDefend.minZ - 1 - PortalSize, PortalSize, PortalSize));
        //    GenStep_ChaosPortalSmall.portalRects.Add(new CellRect(rectToDefend.CenterCell.x - PortalSize / 2, rectToDefend.maxZ + 1, PortalSize, PortalSize));
        //    CellRect mapRect = new CellRect(0, 0, map.Size.x, map.Size.z);
        //    GenStep_ChaosPortalSmall.portalRects.RemoveAll((Predicate<CellRect>)(x => !x.FullyContainedWithin(mapRect)));
        //    if (GenStep_ChaosPortalSmall.portalRects.Any<CellRect>())
        //        return GenStep_ChaosPortalSmall.portalRects.RandomElement<CellRect>();
        //    return rectToDefend;
        //}

        //    public override void Generate(Map map, GenStepParams parms)
        //    {
        //        


        //        CellRect portalArea = new CellRect(minx - 4, minz - 4, 13, 15);

        //        // Clear map center, add floors around portal.
        //        TerrainDef floorDef = TerrainDef.Named("FlagstoneGranite");
        //        ResolveParams areaAroundPortal = default(ResolveParams);
        //        areaAroundPortal.rect = portalArea;
        //        areaAroundPortal.clearEdificeOnly = true;
        //        areaAroundPortal.clearRoof = true;
        //        areaAroundPortal.faction = faction;
        //        areaAroundPortal.floorDef = floorDef;
        //        areaAroundPortal.pathwayFloorDef = floorDef;
        //        areaAroundPortal.edgeDefenseWidth = new int?(0);
        //        areaAroundPortal.edgeDefenseTurretsCount = new int?(0);
        //        areaAroundPortal.edgeDefenseMortarsCount = new int?(0);
        //        areaAroundPortal.settlementPawnGroupPoints = new float?(0f);
        //        areaAroundPortal.chanceToSkipWallBlock = 0f;
        //        BaseGen.symbolStack.Push("floor", areaAroundPortal);

        //        BaseGen.globalSettings.map = map;

        //        BaseGen.Generate();




        //        // Create Base.
        //        ResolveParams resolveParams = default(ResolveParams);

        //        resolveParams.rect = this.GetOutpostRect(rectToDefend, map);
        //        resolveParams.faction = faction;
        //        resolveParams.edgeDefenseWidth = new int?(2);
        //        resolveParams.edgeDefenseTurretsCount = new int?(0);
        //        resolveParams.edgeDefenseMortarsCount = new int?(0);
        //        if (parms.siteCoreOrPart != null)
        //        {
        //            resolveParams.settlementPawnGroupPoints = new float?(250);
        //            resolveParams.settlementPawnGroupSeed = new int?(OutpostSitePartUtility.GetPawnGroupMakerSeed(parms.siteCoreOrPart.parms));
        //        }
        //        else
        //        {
        //            resolveParams.settlementPawnGroupPoints = new float?(this.defaultPawnGroupPointsRange.RandomInRange);
        //        }
        //        BaseGen.globalSettings.map = map;
        //        BaseGen.globalSettings.minBuildings = 1;
        //        BaseGen.globalSettings.minBarracks = 1;
        //        BaseGen.globalSettings.basePart_breweriesCoverage = 0;
        //        BaseGen.globalSettings.basePart_farmsCoverage = 0;
        //        BaseGen.symbolStack.Push("settlement", resolveParams);
        //        BaseGen.Generate();
        //    }
    }
}
