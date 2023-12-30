using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace TheEndTimes
{
    public class JobDriver_PutCuredMeatIntoCuringCabinet : JobDriver
    {
        private const TargetIndex BuildingInd = TargetIndex.A;
        private const TargetIndex StuffInd = TargetIndex.B;
        private const int Duration = 200;

        protected Building_CuringCabinet buildingCurrent => (Building_CuringCabinet)this.job.GetTarget(BuildingInd).Thing;
        protected Thing StuffCurrent => this.job.GetTarget(StuffInd).Thing;

        public override bool TryMakePreToilReservations(bool yeaa) =>
            this.pawn.Reserve(this.buildingCurrent, this.job, 1, -1, null) && this.pawn.Reserve(this.StuffCurrent, this.job, 1, -1, null);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(BuildingInd);
            this.FailOnBurningImmobile(BuildingInd);
            base.AddEndCondition(() => (buildingCurrent.SpaceLeftForStuff > 0) ? JobCondition.Ongoing : JobCondition.Succeeded);
            yield return Toils_General.DoAtomic(delegate
            {
                job.count = buildingCurrent.SpaceLeftForStuff;
            });
            Toil reserveWort = Toils_Reserve.Reserve(StuffInd, 1, -1, null);
            yield return reserveWort;
            yield return Toils_Goto.GotoThing(StuffInd, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(StuffInd).FailOnSomeonePhysicallyInteracting(StuffInd);
            yield return Toils_Haul.StartCarryThing(StuffInd, false, true, false).FailOnDestroyedNullOrForbidden(StuffInd);
            yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveWort, StuffInd, TargetIndex.None, true, null);
            yield return Toils_Goto.GotoThing(BuildingInd, PathEndMode.Touch);
            yield return Toils_General.Wait(Duration).FailOnDestroyedNullOrForbidden(StuffInd).FailOnDestroyedNullOrForbidden(BuildingInd).FailOnCannotTouch(BuildingInd, PathEndMode.Touch).WithProgressBarToilDelay(BuildingInd, false, -0.5f);
            yield return new Toil
            {
                initAction = delegate
                {
                    buildingCurrent.AddStuff(StuffCurrent);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield break;
        }

    }
}
