using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using UnityEngine;
using Verse;

namespace TheEndTimes
{
    [StaticConstructorOnStartup]
    public static class OnStartup
    {
        private static int movedDefs;

        static OnStartup()
        {
            MoveRecipesToSmithy();
        }

        private static void MoveRecipesToSmithy()
        {
            movedDefs = 0;
            foreach (ThingDef td in DefDatabase<ThingDef>.AllDefs.Where(t =>
                (t?.recipeMaker?.recipeUsers?.Contains(ThingDef.Named("FueledSmithy")) ?? false) ||
                (t?.recipeMaker?.recipeUsers?.Contains(ThingDef.Named("TableMachining")) ?? false)))
            {
                td.recipeMaker.recipeUsers.Add(ThingDef.Named("RH_TET_TableSmithy"));
                movedDefs++;
            }
            foreach (RecipeDef rd in DefDatabase<RecipeDef>.AllDefs.Where(d =>
                (d.recipeUsers?.Contains(ThingDef.Named("TableMachining")) ?? false) ||
                (d.recipeUsers?.Contains(ThingDef.Named("FueledSmithy")) ?? false)))
            {
                rd.recipeUsers.Add(ThingDef.Named("RH_TET_TableSmithy"));
                movedDefs++;
            }
            //Log.Message("The following were moved from Machining Table to Smithy:" + movedDefs);
        }
    }
}