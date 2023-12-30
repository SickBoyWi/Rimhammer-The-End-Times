using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using RimWorld;
using RimWorld.BaseGen;
using UnityEngine;
using Verse;

namespace TheEndTimes
{
    public class SettingsController : Mod
    {
        public SettingsController(ModContentPack content) : base(content)
        {
            base.GetSettings<Settings>();
        }

        public override string SettingsCategory()
        {
            return "Rimhammer - The End Times";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoSettingsWindowContents(inRect);
        }
    }

    public class Settings : ModSettings
    {
        private const float DEFAULT_LATITUDE_RANGE = 52;
        private const float DEFAULT_LATITUDE_RANGE_RANDOMVARIANT = 6f;

        private static Vector2 scrollPosition = Vector2.zero;
        private const int GAP_SIZE = 24;

        public static bool HaveChaosWastes = true;
        public static float LatitudeRange = DEFAULT_LATITUDE_RANGE;
        public static float LatitudeRandomVariant = DEFAULT_LATITUDE_RANGE_RANDOMVARIANT;

        public static bool IncludeUranium = true;
        public static bool IncludePlasteel = true;

        public override void ExposeData()
        {
            base.ExposeData();
            
            Scribe_Values.Look<bool>(ref HaveChaosWastes, "RH_TET_Include_ChaosWastes", true, false);
            Scribe_Values.Look<float>(ref LatitudeRange, "RH_TET_ChaosWastes_Latitude", DEFAULT_LATITUDE_RANGE, false);
            Scribe_Values.Look<float>(ref LatitudeRandomVariant, "RH_TET_ChaosWastes_Latitude_RandomVariant", DEFAULT_LATITUDE_RANGE_RANDOMVARIANT, false);

            Scribe_Values.Look<bool>(ref IncludeUranium, "RH_TET_Include_Uranium", false, false);
            Scribe_Values.Look<bool>(ref IncludePlasteel, "RH_TET_Include_Plasteel", false, false);
        }

        public static void DoSettingsWindowContents(Rect rect)
        {
            Rect scroll = new Rect(5f, 45f, 430, rect.height - 40);
            Rect view = new Rect(0, 45, 400, 1200);

            Widgets.BeginScrollView(scroll, ref scrollPosition, view, true);
            Listing_Standard ls = new Listing_Standard();
            ls.Begin(view);

            ls.Label("RH_TET_ChaosWastes".Translate());
            ls.Gap(GAP_SIZE);   
            
            ls.CheckboxLabeled("RH_TET_Include_ChaosWastes".Translate(), ref HaveChaosWastes);
            ls.Gap(GAP_SIZE);

            if (HaveChaosWastes)
            {
                ls.Label("RH_TET_ChaosWastes_Latitude".Translate() + ": " + LatitudeRange);
                LatitudeRange = (int)ls.Slider(LatitudeRange, 15, 85);
                ls.Gap(GAP_SIZE);

                ls.Label("RH_TET_ChaosWastes_Latitude_RandomVariant".Translate() + ": " + LatitudeRandomVariant);
                LatitudeRandomVariant = (int)ls.Slider(LatitudeRandomVariant, 0, 14);
                ls.Gap(GAP_SIZE);
                
                if (ls.ButtonText("RH_TET_Default".Translate()))
                {
                    LatitudeRange = DEFAULT_LATITUDE_RANGE;
                    LatitudeRandomVariant = DEFAULT_LATITUDE_RANGE_RANDOMVARIANT;
                }
                ls.Gap(GAP_SIZE);
            }

            ls.Label("RH_TET_OtherSettings".Translate());
            ls.Gap(GAP_SIZE);

            ls.CheckboxLabeled("RH_TET_Include_Uranium".Translate(), ref IncludeUranium);
            ls.Label("RH_TET_Include_UraniumMessage".Translate());
            ls.Gap(GAP_SIZE);

            ls.CheckboxLabeled("RH_TET_Include_Plasteel".Translate(), ref IncludePlasteel);
            ls.Label("RH_TET_Include_UraniumMessage".Translate());
            ls.Gap(GAP_SIZE);

            ls.End();
            Widgets.EndScrollView();
        }
    }
}