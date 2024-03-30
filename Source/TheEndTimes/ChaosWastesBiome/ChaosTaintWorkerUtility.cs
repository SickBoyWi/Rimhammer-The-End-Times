using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TheEndTimes
{
    public static class ChaosTaintWorkerUtility
    {
        // Limit to three mad animals per day.
        private static readonly int ticksUntilMadAnimal = 2500 * 8;
        private static int lastMadAnimalTick = 0;

        public static void DoPawnChaosTaintDamage(Pawn pawn, Map map, bool ingested = false)
        {

            Hediff hediff = null;
            float num = 0.028758334f;

            if (ingested)
            {
                num = .05f; 
            }

            // Use the same stat value for toxicicty sensitivity. It's kind of the same thing. 
            float pawnSensitivity = pawn.GetStatValue(StatDefOf.ToxicResistance, true);

            if (pawnSensitivity == 0f)
            {
                pawnSensitivity = 1f;
            }

            num *= pawnSensitivity;
            if (num != 0f)
            {
                float num2 = Mathf.Lerp(0.85f, 1.15f, Rand.ValueSeeded(pawn.thingIDNumber ^ 74374237));
                num *= num2;

                HealthUtility.AdjustSeverity(pawn, HediffDefOf.RH_TET_ChaosTaintBuildup, num);
            }

            // Second look up may be required if it didn't exist before the severity was adjusted. 
            hediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.RH_TET_ChaosTaintBuildup, false);

            // Consider other things that can happen from chaos taint. Berserk, or turn into chaos spawn. 
            if (hediff != null)
            {
                float severity = hediff.Severity;

                float spawnChance = 0f;
                float berserkChance = 0f;

                if (severity >= 1.0f)
                {
                    // Pawn died. 
                    spawnChance = .9f;
                }
                else if (severity >= .9)
                {
                    // Extreme.
                    spawnChance = .65f;
                    berserkChance = .05f;
                }
                else if (severity >= .8)
                {
                    // Extreme.
                    spawnChance = .5f;
                    berserkChance = .05f;
                }
                else if (severity >= .75)
                {
                    // Serious.
                    spawnChance = .35f;
                    berserkChance = .1f;
                }
                else if (severity >= .6)
                {
                    // Serious.
                    spawnChance = .01f;
                    berserkChance = .33f;
                }
                else if (severity >= .5)
                {
                    // Serious.
                    //spawnChance = .05f;
                    berserkChance = .3f;
                }
                else if (severity >= .4)
                {
                    // Moderate.
                    //spawnChance = .01f;
                    berserkChance = .2f;
                }
                else if (severity >= .3)
                {
                    // Moderate.
                    berserkChance = .05f;
                }
                else if (severity >= .25)
                {
                    // Minor.
                    berserkChance = .01f;
                }
                else
                {
                    // Severity is too low to turn into chaos spawn or go berserk.
                    return;
                }

                // There's only one chance of something going wrong here. Save that chance. 
                double randomChance = TheEndTimesMod.random.NextDouble();

                // Handle berserk state first, if they go berserk, they won't become a chaos spawn as that should be more rare.
                // They may become a spawn if they are already in an aggressive mental state. 
                if (!pawn.InAggroMentalState && randomChance >= berserkChance)
                {
                    if (!pawn.NonHumanlikeOrWildMan())
                    {
                        // Wake them up if they're sleeping.
                        
                        if (pawn.jobs != null)
                            pawn.mindState.priorityWork.ClearPrioritizedWorkAndJobQueue();

                        //CompTargetEffect_Berserk berserker = new CompTargetEffect_Berserk();
                        //berserker.DoEffectOn(pawn, pawn);

                        pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, "RH_TET_Reason_ChaosTaintBerserk".Translate(), 
                            true, true, 
                            false, (Pawn)null);

                        // Send a message to the user about this having happened.
                        Messages.Message("RH_TET_PawnSuccumbedToChaos_Berserk".Translate(pawn).CapitalizeFirst(), pawn, MessageTypeDefOf.NegativeHealthEvent, false);
                    }
                    else
                    {
                        // If more ticks have passed than are required for animals to go mad.
                        if ((Find.TickManager.TicksGame - lastMadAnimalTick) > ticksUntilMadAnimal)
                        {
                            lastMadAnimalTick = Find.TickManager.TicksGame;

                            // Wake them up if they're sleeping.
                            if (!pawn.Awake())
                            {
                                RestUtility.WakeUp(pawn);
                                //pawn.mindState.priorityWork.ClearPrioritizedWorkAndJobQueue();
                            }

                            // Make animals or non humans maddened.
                            //CompTargetEffect_Manhunter manhunter = new CompTargetEffect_Manhunter();
                            //manhunter.DoEffectOn(pawn, pawn);

                            pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, "RH_TET_Reason_ChaosTaintBerserk".Translate(), true, true,
                            false, (Pawn)null);

                            // Send a message to the user about this having happened.
                            Messages.Message("RH_TET_PawnWildSuccumbedToChaos_Berserk".Translate(pawn).CapitalizeFirst(), pawn, MessageTypeDefOf.NegativeHealthEvent, false);
                        }
                    }
                }
                else if (!pawn.NonHumanlikeOrWildMan() && spawnChance > 0.0)
                {
                    // Handle chaos spawn mutation chance. 

                    // Chances of this happening to a pawn that isn't a player colonist will be reduced by half.
                    // Note: this will include animals on the map that have died. 
                    if (!pawn.IsColonistPlayerControlled)
                    {
                        spawnChance = spawnChance / 2f;
                    }

                    // See if the lucky pawn has turned into a chaos spawn.
                    if (randomChance >= spawnChance)
                    {
                        // Generate new spawn in same location.
                        IntVec3 location = pawn.PositionHeld;

                        // Destroy pawn, spawn a chaos spawn. 
                        pawn.Destroy(); // Allow pawn to vanish by default.

                        // Add mood modifier to pawns due to death/mutation of colonist.
                        ApplyColonistMutatedThought(pawn);

                        // Create new spawn, in same place as destroyed pawn, make maddened.
                        PawnKindDef pawnKind = PawnKindDef.Named("RH_TET_ChaosSpawn");
                        Faction faction = Find.FactionManager.FirstFactionOfDef(FactionDef.Named("RH_TET_ChaosMonsterFaction"));
                        Pawn chaosSpawnPawn = PawnGenerator.GeneratePawn(pawnKind, faction);

                        // Spawn the chaos spawn on the map, if the pawn is on a map.
                        if (map != null)
                        {
                            GenSpawn.Spawn(chaosSpawnPawn, location, map);
                        }

                        // Send a message to the user about this having happened.
                        Messages.Message("RH_TET_PawnSuccumbedToChaos_Spawn".Translate(pawn).CapitalizeFirst(), pawn, MessageTypeDefOf.NegativeHealthEvent, false);

                        // Madden the chaos spawn.
                        CompTargetEffect_Manhunter manhunter = new CompTargetEffect_Manhunter();
                        manhunter.DoEffectOn(chaosSpawnPawn, chaosSpawnPawn);
                    }
                }
            }
        }

        private static void ApplyColonistMutatedThought(Pawn victim)
        {
            foreach (Pawn current in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive)
            {
                if (current != victim && current.needs != null && current.needs.mood != null)
                {
                    if (current.MentalStateDef != MentalStateDefOf.SocialFighting || ((MentalState_SocialFighting)current.MentalState).otherPawn != victim)
                    {
                        if (victim.Faction == Faction.OfPlayer && victim.Faction == current.Faction && victim.HostFaction != current.Faction)
                        {
                            Thought_MemoryObservation thought_MemoryObservation = null;
                            if (ThoughtDef.Named("TH_TET_ColonistCorrupted") is ThoughtDef td)
                            {
                                Thought_Memory tempThought = ThoughtMaker.MakeThought(td, 0);
                                current.needs.mood.thoughts.memories.TryGainMemory(tempThought, null);
                            }

                            if (thought_MemoryObservation != null)
                            {
                                thought_MemoryObservation.Target = current;
                            }
                        }
                    }
                }
            }
        }

        public static void DoAllPawnsOnMapChaosTaintDamage(Map map)
        {
            if (map == null)
                return;

            IReadOnlyList<Pawn> allPawnsSpawned = map?.mapPawns?.AllPawnsSpawned;

            if (allPawnsSpawned == null)
                return;

            for (int i = 0; i < allPawnsSpawned.Count; i++)
            {
                Pawn pawn = allPawnsSpawned[i];

                if (pawn == null || pawn.Position == null || pawn.def == null || pawn.def.race == null || pawn.KindLabel == null)
                    continue;

                // Don't check for pawns that are under a roof, non flesh pawns, or those that have a kind label that starts with chaos (exclude spawns and other chaos critters)
                if (!pawn.Position.Roofed(map) && pawn.def.race.IsFlesh && !pawn.KindLabel.StartsWith("chaos"))
                {
                    DoPawnChaosTaintDamage(pawn, map, false);
                }
            }
        }
    }
}