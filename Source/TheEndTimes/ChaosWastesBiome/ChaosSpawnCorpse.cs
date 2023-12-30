using RimWorld;
using Verse;

namespace TheEndTimes
{
    public class ChaosSpawnCorpse : Corpse, IObservedThoughtGiver
    {
        public new Thought_Memory GiveObservedThought(Pawn p)
        {
            if (this.StoringThing() == null)
            {
                Thought_MemoryObservation thought_MemoryObservation = null;
                if (ThoughtDef.Named("RH_TET_ObservedChaosSpawnDead") is ThoughtDef td)
                {
                    thought_MemoryObservation =
                        (Thought_MemoryObservation)ThoughtMaker.MakeThought(td);
                }
                
                if (thought_MemoryObservation != null)
                {
                    thought_MemoryObservation.Target = this;
                    return thought_MemoryObservation;
                }
            }

            return null;
        }
    }
}
