using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TheEndTimes
{
    public static class RH_TET_GameVictoryUtility
    {
        private static void ShowCredits(string victoryText)
        {
            Find.WindowStack.Add((Window)new Screen_Credits(victoryText)
            {
                wonGame = true
            });
            Find.MusicManagerPlay.ForceSilenceFor(999f);
            ScreenFader.StartFade(Color.clear, 3f);
        }

        private static void InitiateVictory()
        {
            SoundDefOf.ShipTakeoff.PlayOneShotOnCamera(null);
            ScreenFader.StartFade(Color.white, 5.0f);
        }

        public static void GreatPortalDestroyedEnded()
        {
            InitiateVictory();
            ShowCredits(MakeEndCredits("RH_TET_GameOverChaosPortalGreatDestroyed".Translate()));
        }

        private static string MakeEndCredits(string victoryText)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(victoryText);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(GameVictoryUtility.InMemoryOfSection());
            return stringBuilder.ToString();
        }
    }
}
