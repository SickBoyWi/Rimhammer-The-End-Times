﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace TheEndTimes
{
    public class ChaosPortalSmall : Building, IAttackTarget, IAttackTargetSearcher, ILoadReferenceable
    {
        public bool active = false;
        public int nextPawnSpawnTick = -1;
        private List<Pawn> spawnedPawns = new List<Pawn>();
        public bool canSpawnPawns = true;
        public const int PawnSpawnRadius = 2;
        public const float MaxSpawnedPawnsPoints = 1500f;
        public const float InitialPawnsPoints = 1000f;
        private static readonly FloatRange PawnSpawnIntervalDays = new FloatRange(0.04f, .1f);
        public static List<PawnKindDef> spawnablePawnKinds = new List<PawnKindDef>();
        public static readonly string MemoAttackedByEnemy = "PortalAttacked";
        public static readonly string MemoDeSpawned = "PortalDeSpawned";
        public static readonly string MemoBurnedBadly = "PortalBurnedBadly";
        public static readonly string MemoDestroyedNonRoofCollapse = "PortalDestroyedNonRoofCollapse";

        Thing IAttackTarget.Thing
        {
            get
            {
                return this;
            }
        }

        public float TargetPriorityFactor
        {
            get
            {
                return 0.4f;
            }
        }

        public Verb CurrentEffectiveVerb
        {
            get
            {
                return null;
            }
        }

        public LocalTargetInfo LastAttackedTarget
        {
            get
            {
                return null;
            }
        }

        public int LastAttackTargetTick
        {
            get
            {
                return 0;
            }
        }

        Thing IAttackTargetSearcher.Thing
        {
            get
            {
                return this;
            }
        }

        public LocalTargetInfo TargetCurrentlyAimingAt
        {
            get
            {
                return null;
            }
        }

        private Lord Lord
        {
            get
            {
                Predicate<Pawn> hasDefendPortalLord = delegate (Pawn x)
                {
                    Lord lord = x.GetLord();
                    return lord != null && lord.LordJob is LordJob_DefendAndExpandHive;
                };
                Pawn foundPawn = this.spawnedPawns.Find(hasDefendPortalLord);
                if (base.Spawned)
                {
                    if (foundPawn == null)
                    {
                        RegionTraverser.BreadthFirstTraverse(this.GetRegion(RegionType.Set_Passable), (Region from, Region to) => true, delegate (Region r)
                        {
                            List<Thing> list = r.ListerThings.ThingsOfDef(ThingDef.Named("RH_TET_ChaosPortal_Small"));
                            for (int i = 0; i < list.Count; i++)
                            {
                                if (list[i] != this)
                                {
                                    if (list[i].Faction == this.Faction)
                                    {
                                        foundPawn = ((ChaosPortalSmall)list[i]).spawnedPawns.Find(hasDefendPortalLord);
                                        if (foundPawn != null)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                            return false;
                        }, 20, RegionType.Set_Passable);
                    }
                    if (foundPawn != null)
                    {
                        return foundPawn.GetLord();
                    }
                }
                return null;
            }
        }

        public bool ThreatDisabled(IAttackTargetSearcher disabledFor)
        {
            return false;
        }

        private float SpawnedPawnsPoints
        {
            get
            {
                this.FilterOutUnspawnedPawns();
                float num = 0f;
                for (int i = 0; i < this.spawnedPawns.Count; i++)
                {
                    num += this.spawnedPawns[i].kindDef.combatPower;
                }
                return num;
            }
        }

        public static void ResetStaticData()
        {
            ChaosPortalSmall.spawnablePawnKinds.Clear();
            ChaosPortalSmall.spawnablePawnKinds.Add(PawnKindDef.Named("RH_TET_ChaosSpawn"));
            ChaosPortalSmall.spawnablePawnKinds.Add(PawnKindDef.Named("RH_TET_ChaosSpawnAncient"));
            ChaosPortalSmall.spawnablePawnKinds.Add(PawnKindDef.Named("RH_TET_Jabberslythe"));
            ChaosPortalSmall.spawnablePawnKinds.Add(PawnKindDef.Named("RH_TET_Razorgor"));
            ChaosPortalSmall.spawnablePawnKinds.Add(PawnKindDef.Named("RH_TET_Tuskgor"));
            ChaosPortalSmall.spawnablePawnKinds.Add(PawnKindDef.Named("RH_TET_ChaosWarhound"));
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (base.Faction == null)
            {
                Faction chaos = null;

                foreach (Faction f in Find.FactionManager.AllFactions)
                {
                    if (f.Name.Equals("RH_TET_ChaosMonsterFaction"))
                    {
                        chaos = f;
                        break;
                    }
                }

                this.SetFaction(chaos, null);
            }
            if (!respawningAfterLoad && this.active)
            {
                this.SpawnInitialPawns();
            }
        }

        private void SpawnInitialPawns()
        {
            this.SpawnPawnsUntilPoints(InitialPawnsPoints);
            this.CalculateNextPawnSpawnTick();
        }

        public void SpawnPawnsUntilPoints(float points)
        {
            int num = 0;
            while (this.SpawnedPawnsPoints < points)
            {
                num++;
                if (num > 1000)
                {
                    break;
                }
                Pawn pawn;
                if (!this.TrySpawnPawn(out pawn))
                {
                    break;
                }
            }
            this.CalculateNextPawnSpawnTick();
        }

        public override void Tick()
        {
            base.Tick();
            if (base.Spawned)
            {
                this.FilterOutUnspawnedPawns();

                if (!this.active && !base.Position.Fogged(base.Map))
                {
                    this.Activate();
                }
                
                if (this.active && Find.TickManager.TicksGame >= this.nextPawnSpawnTick)
                {
                    if (this.SpawnedPawnsPoints < MaxSpawnedPawnsPoints)
                    {
                        Pawn pawn;
                        bool flag = this.TrySpawnPawn(out pawn);
                        
                        if (flag && pawn.caller != null)
                        {
                            pawn.caller.DoCall();
                        }
                    }
                    this.CalculateNextPawnSpawnTick();
                }
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            Map map = base.Map;
            base.DeSpawn(mode);
            List<Lord> lords = map.lordManager.lords;
            for (int i = 0; i < lords.Count; i++)
            {
                lords[i].ReceiveMemo(ChaosPortalSmall.MemoDeSpawned);
            }
            ChaosPortalUtility.Notify_PortalDespawned(this, map);
        }

        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            if (dinfo.Def.ExternalViolenceFor(this) && dinfo.Instigator != null && dinfo.Instigator.Faction != null)
            {
                Lord lord = this.Lord;
                if (lord != null)
                {
                    lord.ReceiveMemo(ChaosPortalSmall.MemoAttackedByEnemy);
                }
            }
            if (dinfo.Def == DamageDefOf.Flame && (float)this.HitPoints < (float)base.MaxHitPoints * 0.3f)
            {
                Lord lord2 = this.Lord;
                if (lord2 != null)
                {
                    lord2.ReceiveMemo(ChaosPortalSmall.MemoBurnedBadly);
                }
            }
            base.PostApplyDamage(dinfo, totalDamageDealt);
        }

        public override void Kill(DamageInfo? dinfo = null, Hediff exactCulprit = null)
        {
            if (base.Spawned && (!dinfo.HasValue || dinfo.Value.Category != DamageInfo.SourceCategory.Collapse))
            {
                List<Lord> lords = base.Map.lordManager.lords;
                for (int i = 0; i < lords.Count; i++)
                {
                    lords[i].ReceiveMemo(ChaosPortalSmall.MemoDestroyedNonRoofCollapse);
                }
            }
            base.Kill(dinfo, exactCulprit);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.active, "active", false, false);
            Scribe_Values.Look<int>(ref this.nextPawnSpawnTick, "nextPawnSpawnTick", 0, false);
            Scribe_Collections.Look<Pawn>(ref this.spawnedPawns, "spawnedPawns", LookMode.Reference, new object[0]);
            Scribe_Values.Look<bool>(ref this.canSpawnPawns, "canSpawnPawns", true, false);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                this.spawnedPawns.RemoveAll((Pawn x) => x == null);
            }
        }

        private void Activate()
        {
            ChaosPortalSmall.ResetStaticData();
            this.active = true;
            this.SpawnInitialPawns();
            this.CalculateNextPawnSpawnTick();
        }

        private void CalculateNextPawnSpawnTick()
        {
            float num = GenMath.LerpDouble(0f, 5f, 1f, 0.5f, (float)this.spawnedPawns.Count);
            this.nextPawnSpawnTick = Find.TickManager.TicksGame + (int)(ChaosPortalSmall.PawnSpawnIntervalDays.RandomInRange * 60000f / (num * Find.Storyteller.difficulty.enemyReproductionRateFactor));
        }

        private void FilterOutUnspawnedPawns()
        {
            for (int i = this.spawnedPawns.Count - 1; i >= 0; i--)
            {
                if (!this.spawnedPawns[i].Spawned)
                {
                    this.spawnedPawns.RemoveAt(i);
                }
            }
        }

        private bool TrySpawnPawn(out Pawn pawn)
        {
            if (!this.canSpawnPawns)
            {
                pawn = null;
                return false;
            }
            float curPoints = this.SpawnedPawnsPoints;
            IEnumerable<PawnKindDef> source = from x in ChaosPortalSmall.spawnablePawnKinds
                                              where curPoints + x.combatPower <= 2000f
                                              select x;
            PawnKindDef kindDef;
            if (!source.TryRandomElement(out kindDef))
            {
                pawn = null;
                return false;
            }
            pawn = PawnGenerator.GeneratePawn(kindDef, base.Faction);
            this.spawnedPawns.Add(pawn);
            this.Map.attackTargetsCache.TargetsHostileToFaction(Find.FactionManager.OfPlayer).Add(pawn);
            GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(base.Position, base.Map, 2, null), base.Map, WipeMode.Vanish);
            pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent, null, false, false, null, true);
            Lord lord = this.Lord;
            if (lord == null)
            {
                lord = this.CreateNewLord();
            }
            lord.AddPawn(pawn);
            SoundDef.Named("RH_TET_ChaosPortalSpawn").PlayOneShot(this);
            return true;
        }

        [DebuggerHidden]
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Spawn pawn",
                    icon = TexCommand.ReleaseAnimals,
                    action = delegate
                    {
                        Pawn pawn;
                        this.TrySpawnPawn(out pawn);
                    }
                };
            }
        }

        public override bool PreventPlayerSellingThingsNearby(out string reason)
        {
            if (this.spawnedPawns.Count > 0)
            {
                if (this.spawnedPawns.Any((Pawn p) => !p.Downed))
                {
                    reason = this.def.label;
                    return true;
                }
            }
            reason = null;
            return false;
        }

        private Lord CreateNewLord()
        {
            SpawnedPawnParams parms = new SpawnedPawnParams();
            parms.aggressive = true;
            return LordMaker.MakeNewLord(base.Faction, new LordJob_DefendAndExpandHive(parms), base.Map, null);
        }
    }
}
