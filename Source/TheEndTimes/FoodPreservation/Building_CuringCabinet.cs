using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TheEndTimes
{
    [StaticConstructorOnStartup]
    public class Building_CuringCabinet : Building
    {
        private int stuffCount;
        private float progressInt;
        //private const float daysToStartRotPickling = 5f;
        private Material barFilledCachedMat;
        public const int MaxCapacity = 100;
        private const int DurationTillReady = 600000; //600000 is 10 days. So 60000 ticks per day.
        public const float MinIdealTemperature = 7f;
        private static readonly Vector2 BarSize = new Vector2(0.55f, 0.1f);
        private static readonly Color BarZeroProgressColor = new Color(0.4f, 0.27f, 0.22f);
        private static readonly Color BarFermentedColor = new Color(0.9f, 0.85f, 0.2f);
        private static readonly Material BarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f), false);
        private CompIngredients compIngredients = new CompIngredients();

        public float Progress
        {
            get
            {
                return this.progressInt;
            }
            set
            {
                if (value == this.progressInt)
                {
                    return;
                }
                this.progressInt = value;
                this.barFilledCachedMat = null;
            }
        }

        private Material BarFilledMat
        {
            get
            {
                if (this.barFilledCachedMat == null)
                {
                    this.barFilledCachedMat = SolidColorMaterials.SimpleSolidColorMaterial(Color.Lerp(Building_CuringCabinet.BarZeroProgressColor, Building_CuringCabinet.BarFermentedColor, this.Progress), false);
                }
                return this.barFilledCachedMat;
            }
        }

        public int SpaceLeftForStuff
        {
            get
            {
                if (this.Ready)
                {
                    return 0;
                }
                return MaxCapacity - this.stuffCount;
            }
        }

        private bool Empty
        {
            get
            {
                return this.stuffCount <= 0;
            }
        }

        public bool Ready
        {
            get
            {
                return !this.Empty && this.Progress >= 1f;
            }
        }

        private float CurrentTempProgressSpeedFactor
        {
            get
            {
                CompProperties_TemperatureRuinable compProperties = this.def.GetCompProperties<CompProperties_TemperatureRuinable>();
                float ambientTemperature = base.AmbientTemperature;
                if (ambientTemperature < compProperties.minSafeTemperature)
                {
                    return 0.1f;
                }
                if (ambientTemperature < 7f)
                {
                    return GenMath.LerpDouble(compProperties.minSafeTemperature, 7f, 0.1f, 1f, ambientTemperature);
                }
                return 1f;
            }
        }

        private float ProgressPerTickAtCurrentTemp
        {
            get
            {
                return 1.66666666E-06f * this.CurrentTempProgressSpeedFactor; // 10 days.
            }
        }

        private int EstimatedTicksLeft
        {
            get
            {
                return Mathf.Max(Mathf.RoundToInt((1f - this.Progress) / this.ProgressPerTickAtCurrentTemp), 0);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.stuffCount, "stuffCount", 0, false);
            Scribe_Values.Look<float>(ref this.progressInt, "progress", 0f, false);
        }

        public override void TickRare()
        {
            base.TickRare();
            if (!this.Empty)
            {
                this.Progress = Mathf.Min(this.Progress + 200f * this.ProgressPerTickAtCurrentTemp, 1f);
            }
        }

        public void AddStuff(int count)
        {
            base.GetComp<CompTemperatureRuinable>().Reset();
            if (this.Ready)
            {
                Log.Warning("Tried to add meat to a curing cabinet that is already fully dried.");
                return;
            }
            int num = Mathf.Min(count, MaxCapacity - this.stuffCount);
            if (num <= 0)
            {
                return;
            }
            this.Progress = GenMath.WeightedAverage(0f, (float)num, this.Progress, (float)this.stuffCount);
            this.stuffCount += num;
        }

        protected override void ReceiveCompSignal(string signal)
        {
            if (signal == "RuinedByTemperature")
            {
                this.Reset();
            }
        }

        private void Reset()
        {
            this.stuffCount = 0;
            this.Progress = 0f;
            this.compIngredients = new CompIngredients();
        }

        public void AddStuff(Thing stuff)
        {
            int num = Mathf.Min(stuff.stackCount, MaxCapacity - this.stuffCount);
            if (num > 0)
            {
                CompIngredients ingredients = stuff.TryGetComp<CompIngredients>();

                for (int i = 0; i < ingredients.ingredients.Count; i++)
                {
                    compIngredients.RegisterIngredient(ingredients.ingredients[i]);
                }

                this.AddStuff(num);
                stuff.SplitOff(num).Destroy(DestroyMode.Vanish);
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            if (stringBuilder.Length != 0)
            {
                stringBuilder.AppendLine();
            }
            CompTemperatureRuinable comp = base.GetComp<CompTemperatureRuinable>();
            if (!this.Empty && !comp.Ruined)
            {
                if (this.Ready)
                {
                    stringBuilder.AppendLine("RH_TET_ContainsDryingCuredFood".Translate(new object[]
                    {
                        this.stuffCount,
                        MaxCapacity
                    }));
                }
                else
                {
                    stringBuilder.AppendLine("RH_TET_ContainsDryingCuring".Translate(new object[]
                    {
                        this.stuffCount,
                        MaxCapacity
                    }));
                }
            }
            if (!this.Empty)
            {
                if (this.Ready)
                {
                    stringBuilder.AppendLine("RH_TET_Dried".Translate());
                }
                else
                {
                    stringBuilder.AppendLine("RH_TET_DryingProgress".Translate(new object[]
                    {
                        this.Progress.ToStringPercent(),
                        this.EstimatedTicksLeft.ToStringTicksToPeriod()
                    }));
                    if (this.CurrentTempProgressSpeedFactor != 1f)
                    {
                        stringBuilder.AppendLine("RH_TET_CuringCabinetOutOfIdealTemperature".Translate(new object[]
                        {
                            this.CurrentTempProgressSpeedFactor.ToStringPercent()
                        }));
                    }
                }
            }
            stringBuilder.AppendLine("Temperature".Translate() + ": " + base.AmbientTemperature.ToStringTemperature("F0"));
            stringBuilder.AppendLine(string.Concat(new string[]
            {
                "RH_TET_IdealDryingTemperature".Translate(),
                ": ",
                7f.ToStringTemperature("F0"),
                " ~ ",
                comp.Props.maxSafeTemperature.ToStringTemperature("F0")
            }));
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public Thing TakeOutStuff()
        {
            if (!this.Ready)
            {
                Log.Warning("Tried to get dried cured meat, but it's not yet dried.");
                return null;
            }
            Thing thing = ThingMaker.MakeThing(ThingDef.Named("RH_TET_DriedCuredMeat"), null);

            // Swap ingredients with those actually used.
            CompIngredients thingIngredients = thing.TryGetComp<CompIngredients>();
            thingIngredients.ingredients = compIngredients.ingredients.ListFullCopy();
            
            thing.stackCount = this.stuffCount;            

            this.Reset();
            return thing;
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            if (this.Empty)
                return;
            Vector3 vector3 = drawLoc;
            vector3.y += 0.03846154f;
            vector3.z += 0.25f;
            GenDraw.DrawFillableBar(new GenDraw.FillableBarRequest()
            {
                center = vector3,
                size = Building_CuringCabinet.BarSize,
                fillPercent = (float)this.stuffCount / 25f,
                filledMat = this.BarFilledMat,
                unfilledMat = Building_CuringCabinet.BarUnfilledMat,
                margin = 0.1f,
                rotation = Rot4.North
            });
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            if (Prefs.DevMode && !this.Empty)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Debug: Set progress to 1",
                    action = delegate
                    {
                        this.Progress = 1f;
                    }
                };
            }
            yield break;
        }
    }
}
