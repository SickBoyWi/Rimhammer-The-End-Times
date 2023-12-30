using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TheEndTimes
{
    [StaticConstructorOnStartup]
    public class ChaosSpawnCorpse_ThingClassUpdater
    {
        static ChaosSpawnCorpse_ThingClassUpdater()
        {
            AdjustChaosSpawnCorpseThingClass();
        }

        private static void AdjustChaosSpawnCorpseThingClass()
        {
            ThingDef.Named("RH_TET_ChaosSpawnRace").race.corpseDef.thingClass = new TheEndTimes.ChaosSpawnCorpse().GetType();
        }
    }
}
