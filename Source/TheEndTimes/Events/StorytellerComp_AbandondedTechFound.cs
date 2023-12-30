using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace TheEndTimes
{
    public class StorytellerComp_AbandondedTechFound : StorytellerComp
    {
        private static readonly SimpleCurve AbandondedTechFoundMTBDaysCurve = new SimpleCurve
        {
            {
                new CurvePoint(0f, 20f),
                true
            },
            {
                new CurvePoint(1f, 40f),
                true
            },
            {
                new CurvePoint(2f, 80f),
                true
            },
            {
                new CurvePoint(2.75f, 135f),
                true
            }
        };

        private float AbandondedTechFoundMTBDays
        {
            get
            {
                float x = (float)Find.TickManager.TicksGame / 3600000f;
                return StorytellerComp_AbandondedTechFound.AbandondedTechFoundMTBDaysCurve.Evaluate(x);
            }
        }

        [DebuggerHidden]
        public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
        {
            if (Rand.MTBEventOccurs(this.AbandondedTechFoundMTBDays, 60000f, 1000f))
            {
                IncidentDef def = IncidentDef.Named("RH_TET_IncidentWorker_AbandondedTechnologyFind");
                IncidentParms parms = this.GenerateParms(def.category, target);
                if (def.Worker.CanFireNow(parms))
                {
                    yield return new FiringIncident(def, this, parms);
                }
            }
        }
    }
}
