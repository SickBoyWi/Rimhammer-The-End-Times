using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace TheEndTimes
{
    public class JobDriver_TakePickledFoodFromPicklingCrock : JobDriver
    {
        private const TargetIndex BuildingInd = TargetIndex.A;
        private const TargetIndex StuffToHaulInd = TargetIndex.B;
        private const TargetIndex StorageCellInd = TargetIndex.C;
        private const int Duration = 200;

        protected Building_PicklingCrock BuildingToUse =>
            (Building_PicklingCrock)job.GetTarget(BuildingInd).Thing;

        public override bool TryMakePreToilReservations(bool yeaa) => pawn.Reserve(BuildingToUse, job, 1, -1, null);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(BuildingInd);
            this.FailOnBurningImmobile(BuildingInd);
            yield return Toils_Goto.GotoThing(BuildingInd, PathEndMode.Touch);
            yield return Toils_General.Wait(Duration).FailOnDestroyedNullOrForbidden(BuildingInd)
                .FailOnCannotTouch(BuildingInd, PathEndMode.Touch).FailOn(() => !BuildingToUse.Ready)
                .WithProgressBarToilDelay(BuildingInd, false, -0.5f);
            yield return new Toil
            {
                initAction = delegate
                {
                    Thing thing = BuildingToUse.TakeOutStuff();
                    GenPlace.TryPlaceThing(thing, pawn.Position, Map, ThingPlaceMode.Near, null);
                    StoragePriority currentPriority = StoreUtility.CurrentStoragePriorityOf(thing);
                    if (StoreUtility.TryFindBestBetterStoreCellFor(thing,
                        pawn, Map, currentPriority, pawn.Faction, out var c, true))
                    {
                        job.SetTarget(TargetIndex.C, c);
                        job.SetTarget(TargetIndex.B, thing);
                        job.count = thing.stackCount;
                    }
                    else
                    {
                        EndJobWith(JobCondition.Incompletable);
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return Toils_Reserve.Reserve(StuffToHaulInd, 1, -1, null);
            yield return Toils_Reserve.Reserve(StorageCellInd, 1, -1, null);
            yield return Toils_Goto.GotoThing(StuffToHaulInd, PathEndMode.ClosestTouch);
            yield return Toils_Haul.StartCarryThing(StuffToHaulInd, false, false, false);
            Toil carryToCell = Toils_Haul.CarryHauledThingToCell(StorageCellInd);
            yield return carryToCell;
            yield return Toils_Haul.PlaceHauledThingInCell(StorageCellInd, carryToCell, true);
            yield break;
        }
    }
}