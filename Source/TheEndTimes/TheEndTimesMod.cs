using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

namespace TheEndTimes
{
    public class TheEndTimesMod : Mod
    {
        public static System.Random random = new System.Random(Guid.NewGuid().GetHashCode());
        public static TechLevel MAX_TECHLEVEL = TechLevel.Medieval;

        public TheEndTimesMod(ModContentPack content) : base(content)
        {
            Harmony harmony = new Harmony("rimworld.rimhammer.theendtimes");
            
            harmony.Patch(AccessTools.Method(typeof(Verse.Pawn), nameof(Verse.Pawn.ButcherProducts)), null,
            new HarmonyMethod(typeof(TheEndTimesMod), nameof(ButcherProducts_PostFix)), null);
        }

        static void ButcherProducts_PostFix(Verse.Pawn __instance, ref IEnumerable<Thing> __result, float efficiency)
        {
            // Setting fat amount to 1/3 the meat, divided by two, reduced by efficiency number.
            int fatAmount = GenMath.RoundRandom(__instance.GetStatValue(DefDatabase<StatDef>.GetNamed("MeatAmount", true), true) * .33F * efficiency);
            if (fatAmount > 0)
            {
                List<Thing> NewList = new List<Thing>();
                foreach (Thing entry in __result)
                {
                    NewList.Add(entry);
                }
                Thing animalFatStack = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("RH_TET_AnimalFat"), null);
                animalFatStack.stackCount = fatAmount;
                NewList.Add(animalFatStack);
                
                IEnumerable<Thing> output = NewList;
                __result = output;
            }
        }
    }
}