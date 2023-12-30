using RimWorld;
using Verse;

namespace TheEndTimes
{
    public class Jabberslythe : Pawn, IObservedThoughtGiver
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            // Try to keep old age issues from affecting the jabber.
            var heDiffsToBeRemoved = this?.health?.hediffSet?.hediffs.FindAll(x =>
                x.def == HediffDef.Named("BadBack") ||
                x.def == HediffDef.Named("Asthma") ||
                x.def == HediffDef.Named("HeartArteryBlockage") ||
                x.def == HediffDef.Named("Cataract") ||
                x.def == HediffDef.Named("HearingLoss"));

            if (!heDiffsToBeRemoved.NullOrEmpty())
            {
                if (heDiffsToBeRemoved != null)
                { 
                    foreach (var heDiffs in heDiffsToBeRemoved)
                    {
                        this?.health?.RemoveHediff(heDiffs);
                    }
                }
            }
        }

        public Thought_Memory GiveObservedThought(Pawn p)
        {
            Thought_MemoryObservation thought = null;

            if (this.InAggroMentalState)
            {
                if (ThoughtDef.Named("RH_TET_ObservedJabberslytheEnraged") is ThoughtDef td)
                {
                    thought = (Thought_MemoryObservation)ThoughtMaker.MakeThought(td);
                }
            }
            else if (this.Dead)
            {
                if (ThoughtDef.Named("RH_TET_ObservedJabberslytheDead") is ThoughtDef td)
                {
                    thought = (Thought_MemoryObservation)ThoughtMaker.MakeThought(td);
                }
            }
            else
            {
                if (ThoughtDef.Named("RH_TET_ObservedJabberslythe") is ThoughtDef td)
                {
                    thought = (Thought_MemoryObservation)ThoughtMaker.MakeThought(td);
                }
            }

            if (thought != null)
            {
                thought.Target = this;
                return thought;
            }

            return null;
        }

        public override void PreApplyDamage(ref DamageInfo damageInfo, out bool damageAbsorbed)
        {
            base.PreApplyDamage(ref damageInfo, out damageAbsorbed);
            if (!this.InMentalState && damageInfo.Instigator is Pawn p && p?.Faction == Faction.OfPlayerSilentFail)
            {
                this.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, p.Label, true, false,null);
            }
        }

        public HistoryEventDef GiveObservedHistoryEvent(Pawn observer)
        {
            return (HistoryEventDef)null;
        }
    }
}
