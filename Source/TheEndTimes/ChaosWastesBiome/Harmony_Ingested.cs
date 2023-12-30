using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace TheEndTimes
{
    [StaticConstructorOnStartup]
    public class Harmony_Ingested
    {
        static Harmony_Ingested()
        {
            Harmony harmony = new Harmony("rimhammer.theendtimes.ingested");
            
            harmony.Patch(AccessTools.Method(typeof(Thing), "Ingested"), null,
                new HarmonyMethod(typeof(Harmony_Ingested), nameof(Ingested_PostFix)), null);
        }

        // Consider whether the meal had chaos meat in it, if so, add chaos taint he diff. 
        public static void Ingested_PostFix(Thing __instance, ref float __result, Pawn ingester)
        {
            // Don't muck about with the result, leave nutrition as is. 
            
            bool increasePawnChaosTaintLevel = false;

            // Raw meat.
            increasePawnChaosTaintLevel = IsChaosMeat(__instance.def);

            // Meals - check all ingredients in meal.
            if (!increasePawnChaosTaintLevel)
            { 
                CompIngredients compIngredients = __instance.TryGetComp<CompIngredients>();

                if (compIngredients != null)
                {
                    for (int i = 0; i < compIngredients.ingredients.Count; i++)
                    {
                        if (ingester.RaceProps.Humanlike && IsChaosMeat(compIngredients.ingredients[i]))
                        {
                            increasePawnChaosTaintLevel = true;
                            break;
                        }                    
                    }
                }
            }

            if (increasePawnChaosTaintLevel)
            {
                ChaosTaintWorkerUtility.DoPawnChaosTaintDamage(ingester, ingester.Map, true);
            }
        }

        private static bool IsChaosMeat(ThingDef ingredient)
        {
            bool returnVal = false;

            if (ingredient != null && ingredient.ingestible != null)
            { 
                // Check ingested item to see if it was or included chaos meat. If so, add chaos taint to ingester. 
                if (ingredient.ingestible.specialThoughtAsIngredient != null && ingredient.ingestible.specialThoughtAsIngredient.defName != null)
                {
                    if (ingredient.ingestible.specialThoughtAsIngredient.defName == "AteChaosMeatAsIngredient")
                    {
                        returnVal = true;
                    }
                }

                if (!returnVal && ingredient.ingestible.specialThoughtDirect != null && ingredient.ingestible.specialThoughtDirect.defName != null)
                {
                    if (ingredient.ingestible.specialThoughtDirect.defName == "AteChaosMeatDirect")
                    {
                        returnVal = true;
                    }
                }
            }

            return returnVal;
        }
    }
}
