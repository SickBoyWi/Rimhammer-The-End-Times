using RimWorld;
using Verse;

namespace TheEndTimes
{
    public class ChaosSpawn : Pawn, IObservedThoughtGiver
    {

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            var hediffsToRemove = this?.health?.hediffSet?.hediffs.FindAll(x =>
                x.def == RimWorld.HediffDefOf.BadBack ||
                x.def == RimWorld.HediffDefOf.Cataract ||
                x.def == RimWorld.HediffDefOf.Gunshot);
            if (!hediffsToRemove.NullOrEmpty())
            {
                if (hediffsToRemove != null)
                    foreach (var hd in hediffsToRemove)
                    {
                        this?.health?.RemoveHediff(hd);
                    }
            }
        }

        public Thought_Memory GiveObservedThought(Pawn p)
        {
            Thought_MemoryObservation thought_MemoryObservation = null;
            
            if (this.Dead)
            {
                if (ThoughtDef.Named("RH_TET_ObservedChaosSpawnDead") is ThoughtDef td)
                {
                    thought_MemoryObservation =
                        (Thought_MemoryObservation)ThoughtMaker.MakeThought(td);
                }
            }
            else
            {
                if (ThoughtDef.Named("RH_TET_ObservedChaosSpawn") is ThoughtDef td)
                {
                    thought_MemoryObservation =
                        (Thought_MemoryObservation)ThoughtMaker.MakeThought(td);
                }
            }

            if (thought_MemoryObservation != null)
            {
                thought_MemoryObservation.Target = this;
                return thought_MemoryObservation;
            }

            return null;
        }

        public HistoryEventDef GiveObservedHistoryEvent(Pawn observer)
        {
            return (HistoryEventDef)null;
        }
    }
}
