using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Text;
using Verse;

namespace TheEndTimes
{
    partial class HarmonyPatches_ChaosPortal
    {
        [HarmonyPatch(typeof(CaravanEnterMapUtility), "Enter")]
        static class Patch_CaravanEnterMapUtility_Enter
        {
            static void Postfix(Caravan caravan, Map map, CaravanEnterMode enterMode)
            {
                Log.Error("MapGen Error. RH_TET Mod is attempting to recover.");

                if (map.fogGrid.IsFogged(caravan.pawns[0].DrawPos.ToIntVec3()))
                {
                    // Something went wrong with map gen. 
                    Log.Error("MapGen Error. RH_TET Mod is attempting to recover.");
                    map.fogGrid.Unfog(caravan.pawns[0].DrawPos.ToIntVec3());
                }
            }
        }        
    }
}
