﻿using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace TheEndTimes
{
    public class WorkGiver_PutCuredMeatIntoCuringCabinet : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ThingDef.Named("RH_TET_CuringCabinet"));
        public override PathEndMode PathEndMode => PathEndMode.Touch;
        private const String INPUT_STUFF = "RH_TET_CuredMeat";

        private static string TemperatureTrans;
        private static string NoWortTrans;

        public static void Reset()
        {
            TemperatureTrans = "BadTemperature".Translate().ToLower();
            NoWortTrans = "RH_TET_NoCuredMeat".Translate();
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Building_CuringCabinet BuildingCurrent) ||
                BuildingCurrent.Ready || BuildingCurrent.SpaceLeftForStuff <= 0)
                return false;

            var ambientTemperature = BuildingCurrent.AmbientTemperature;
            var compProperties =
                BuildingCurrent.def.GetCompProperties<CompProperties_TemperatureRuinable>();
            if (ambientTemperature < compProperties.minSafeTemperature + 2f ||
                ambientTemperature > compProperties.maxSafeTemperature - 2f)
            {
                JobFailReason.Is(TemperatureTrans);
                return false;
            }
            if (t.IsForbidden(pawn)) return false;
            LocalTargetInfo target = t;
            if (!pawn.CanReserve(target, 1, -1, null, forced)) return false;
            if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
                return false;

            if (FindStuff(pawn, BuildingCurrent) != null) return !t.IsBurning();
            JobFailReason.Is(NoWortTrans);
            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var thingCurrent = (Building_CuringCabinet)t;
            var t2 = FindStuff(pawn, thingCurrent);
            return new Job(DefDatabase<JobDef>.GetNamed("RH_TET_Dwarfs_PutCuredMeatIntoCuringCabinet"), t, t2);
        }

        private Thing FindStuff(Pawn pawn, Building_CuringCabinet buildingCurrent)
        {
            bool Predicate(Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, -1, null, false);
            var position = pawn.Position;
            var map = pawn.Map;
            var thingReq = ThingRequest.ForDef(ThingDef.Named(INPUT_STUFF));
            const PathEndMode peMode = PathEndMode.ClosestTouch;
            var traverseParams = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false);
            Predicate<Thing> validator = Predicate;
            return GenClosest.ClosestThingReachable(position, map, thingReq, peMode, traverseParams, 9999f, validator,
                null, 0, -1, false, RegionType.Set_Passable, false);
        }
    }
}