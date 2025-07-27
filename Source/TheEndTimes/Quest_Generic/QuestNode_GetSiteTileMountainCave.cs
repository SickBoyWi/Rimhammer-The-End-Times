using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using Verse;

namespace TheEndTimes
{
    public class QuestNode_GetSiteTileMountainCave : QuestNode
    {
        [NoTranslate]
        public SlateRef<string> storeAs;
        public SlateRef<bool> preferCloserTiles;
        public SlateRef<bool> allowCaravans;

        protected override bool TestRunInt(Slate slate)
        {
            PlanetTile tile;
            if (!this.TryFindTile(slate, out tile))
                return false;
            slate.Set<int>(this.storeAs.GetValue(slate), tile, false);
            return true;
        }

        protected override void RunInt()
        {
            Slate slate = RimWorld.QuestGen.QuestGen.slate;
            PlanetTile tile;
            if (!this.TryFindTile(RimWorld.QuestGen.QuestGen.slate, out tile))
                return;
            RimWorld.QuestGen.QuestGen.slate.Set<int>(this.storeAs.GetValue(slate), tile, false);
        }

        private bool TryFindTile(Slate slate, out PlanetTile tile)
        {
            Map map = slate.Get<Map>("map", (Map)null, false) ?? Find.RandomPlayerHomeMap;
            PlanetTile nearThisTile1 = map != null ? map.Tile : PlanetTile.Invalid;
            IntRange var;
            if (slate.TryGet<IntRange>("siteDistRange", out var, false))
                return QuestNode_GetSiteTileMountainCave.TryFindNewSiteTile(out tile, var.min, var.max, this.allowCaravans.GetValue(slate), this.preferCloserTiles.GetValue(slate), nearThisTile1);

            bool flag = this.preferCloserTiles.GetValue(slate);
            int num1 = this.allowCaravans.GetValue(slate) ? 1 : 0;
            int num2 = flag ? 1 : 0;
            PlanetTile nearThisTile2 = nearThisTile1;
            return QuestNode_GetSiteTileMountainCave.TryFindNewSiteTile(out tile, 7, 27, num1 != 0, num2 != 0, nearThisTile2);
        }        
        
        // Does what TileFinder.TryFindNewSiteTile does, except requires mountains and caves.
        public static bool TryFindNewSiteTile(out PlanetTile tile, int minDist = 8, int maxDist = 30,
            bool allowCaravans = false, bool preferCloserTiles = true, int nearThisTile = -1)
        {
            Func<int, int> findTile = delegate (int root)
            {
                int minDist2 = minDist;
                int maxDist2 = maxDist;
                Predicate<PlanetTile> validator = (PlanetTile x) =>
                    !Find.WorldObjects.AnyWorldObjectAt(x)
                    && Find.World.HasCaves(x)
                    && Find.WorldGrid[x].hilliness == Hilliness.Mountainous
                    && TileFinder.IsValidTileForNewSettlement(x, null);
                TileFinderMode tfe = TileFinderMode.Random;
                if (preferCloserTiles)
                    tfe = TileFinderMode.Near;
                PlanetTile result;
                if (TileFinder.TryFindPassableTileWithTraversalDistance(root, minDist2, maxDist2, out result, validator,
                    false, tfe, false, false))
                {
                    return result;
                }
                return -1;
            };

            PlanetTile arg;
            if (nearThisTile != -1)
            {
                arg = nearThisTile;
            }
            else if (!TileFinder.TryFindRandomPlayerTile(out arg, allowCaravans, (PlanetTile x) => findTile(x) != -1))
            {
                tile = -1;
                return false;
            }
            tile = findTile(arg);
            return tile != -1;
        }
    }
}
