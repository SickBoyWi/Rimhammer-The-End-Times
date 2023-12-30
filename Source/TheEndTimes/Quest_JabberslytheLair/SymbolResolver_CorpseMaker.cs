using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace TheEndTimes
{
    public class SymbolResolver_CorpseMaker : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            var count = rp.hivesCount ?? 1;

            for (int i = 0; i < count; i++)
            {
                PawnKindDef pawnKindDef = (Rand.Value > 0.3f) ? PawnKindDef.Named("Tribal_ChiefMelee") : PawnKindDef.Named("Tribal_Berserker");
                Faction faction = rp.faction;
                PawnGenerationRequest pawnGenRequest = new PawnGenerationRequest(pawnKindDef, faction, PawnGenerationContext.NonPlayer, 
                    BaseGen.globalSettings?.map?.Tile ?? Find.CurrentMap.Tile, 
                    false, false, false, 
                    false, true, 0.01F,
                    false, true, false,
                    false, true);

                Pawn pawn = PawnGenerator.GeneratePawn(pawnGenRequest);
                IntVec3 spawnLoc;
                
                var map = BaseGen.globalSettings?.map ?? Find.CurrentMap;
                CellFinderLoose.TryGetRandomCellWith((x => x.IsValid && rp.rect.Contains(x) && x.GetEdifice(map) == null && x.GetFirstItem(map) == null), map, 250, out spawnLoc);
                GenSpawn.Spawn(pawn, spawnLoc, map);
                pawn.Kill(null);
                if (pawn?.Corpse is Corpse c && c.TryGetComp<CompRottable>() is CompRottable comp)
                {
                    c.Age += GenDate.TicksPerSeason * Rand.Range(12, 90);
                    comp.RotProgress += 9999999;
                }
            }
        }

        // TODO: Make utility class. This is a useful function.
        private bool IsWallOrRock(Building b)
        {
            return b != null && (b.def == ThingDefOf.Wall || b.def.building.isNaturalRock);
        }
    }
}