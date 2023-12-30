using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TheEndTimes
{
    public class GameCondition_ChaosStorm : GameCondition
    {
        private const int LerpTicks = 5000;
        private const float MaxSkyLerpFactor = 0.5f;
        private const float SkyGlow = 0.85f;
        private SkyColorSet SkyColors;
        private List<SkyOverlay> overlays;
        private const int CheckInterval = 3451;
        private const float BuildupPerDay = 0.5f;
        private const float PlantKillChance = 0f;
        private const float CorpseRotProgressAdd = 3000f;

        public GameCondition_ChaosStorm()
        {
            ColorInt colorInt = new ColorInt(255, 45, 0);
            Color arg_50_0 = colorInt.ToColor;
            ColorInt colorInt2 = new ColorInt(255, 186, 0);
            this.SkyColors = new SkyColorSet(arg_50_0, colorInt2.ToColor, new Color(0.6f, 0.8f, 0.5f), 0.85f);
            this.overlays = new List<SkyOverlay>
            {
                new WeatherOverlay_Fallout()
            };
            //? base..ctor();
        }

        public override void Init()
        {
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.ForbiddingDoors, OpportunityType.Critical);
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.AllowedAreas, OpportunityType.Critical);
        }

        public override void GameConditionTick()
        {
            List<Map> affectedMaps = base.AffectedMaps;
            if (Find.TickManager.TicksGame % 3451 == 0)
            {
                for (int i = 0; i < affectedMaps.Count; i++)
                {
                    ChaosTaintWorkerUtility.DoAllPawnsOnMapChaosTaintDamage(affectedMaps[i]);
                }
            }
            for (int j = 0; j < this.overlays.Count; j++)
            {
                for (int k = 0; k < affectedMaps.Count; k++)
                {
                    this.overlays[j].TickOverlay(affectedMaps[k]);
                }
            }
        }

        // Make things rot faster in chaos storm. 
        public override void DoCellSteadyEffects(IntVec3 c, Map map)
        {
            if (!c.Roofed(map))
            {
                List<Thing> thingList = c.GetThingList(map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    Thing thing = thingList[i];
                    if (thing.def.category == ThingCategory.Item)
                    {
                        CompRottable compRottable = thing.TryGetComp<CompRottable>();
                        if (compRottable != null && compRottable.Stage < RotStage.Dessicated)
                        {
                            compRottable.RotProgress += 3000f;
                        }
                    }
                }
            }
        }

        public override void GameConditionDraw(Map map)
        {
            for (int i = 0; i < this.overlays.Count; i++)
            {
                this.overlays[i].DrawOverlay(map);
            }
        }

        public override float SkyTargetLerpFactor(Map map)
        {
            return GameConditionUtility.LerpInOutValue(this, 5000f, 0.5f);
        }

        public override SkyTarget? SkyTarget(Map map)
        {
            return new SkyTarget?(new SkyTarget(0.85f, this.SkyColors, 1f, 1f));
        }

        public override float AnimalDensityFactor(Map map)
        {
            return 0.5f;
        }

        public override float PlantDensityFactor(Map map)
        {
            return 0.5f;
        }

        // No one wants to be outside in this!
        public override bool AllowEnjoyableOutsideNow(Map map)
        {
            return false;
        }

        public override List<SkyOverlay> SkyOverlays(Map map)
        {
            return this.overlays;
        }
    }
}