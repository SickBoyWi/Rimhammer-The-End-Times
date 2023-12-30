using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TheEndTimes
{
    // TODO 20221015 CLEAN UP TECHNICAL DEBT HERE.
    [StaticConstructorOnStartup]
    public static class RemovePostMedievalHarmony
    {
        private const int START_DATE = 2519;

        static RemovePostMedievalHarmony()
        {
            Harmony harmony = new Harmony("rimworld.warhammerize");

            // Set the game date to the date from the Warhammer End Times.
            //Changes the starting date of RimWorld.
            harmony.Patch(AccessTools.Property(typeof(TickManager), "StartingYear").GetGetMethod(), null,
                new HarmonyMethod(typeof(RemovePostMedievalHarmony), nameof(StartingYear_PostFix)), null);
            harmony.Patch(AccessTools.Method(typeof(GenDate), "Year"), null,
                new HarmonyMethod(typeof(RemovePostMedievalHarmony), nameof(Year_PostFix)), null);
        }

        //TickManager
        public static void StartingYear_PostFix(ref int __result)
        {
            __result = START_DATE; // The year that the end times started.
        }

        //GenDate
        public static void Year_PostFix(long absTicks, float longitude, ref int __result)
        {
            long num = absTicks + ((long)GenDate.TimeZoneAt(longitude) * 2500L);
            __result = START_DATE + Mathf.FloorToInt((float)num / 3600000f);
        }

        //GenDate
        public static void DateFullStringAt_PostFix(long absTicks, Vector2 location, ref string __result)
        {
            int num = GenDate.DayOfSeason(absTicks, location.x) + 1;
            string value = Find.ActiveLanguageWorker.OrdinalNumber(num, Gender.None);
            __result = "TET_FullDate".Translate(value, GenDate.Quadrum(absTicks, location.x).Label(), GenDate.Year(absTicks, location.x), num);
        }

        //GenDate
        public static void DateReadoutStringAt_PostFix(long absTicks, Vector2 location, ref string __result)
        {
            int num = GenDate.DayOfSeason(absTicks, location.x) + 1;
            string value = Find.ActiveLanguageWorker.OrdinalNumber(num, Gender.None);
            __result = "TET_DateReadout".Translate(value, GenDate.Quadrum(absTicks, location.x).Label(), GenDate.Year(absTicks, location.x), num);
        }
    }
}