using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace TheEndTimes
{
    class RH_TET_BiomeWorker_ChaosWaste : BiomeWorker_Tundra
    {
        public override float GetScore(Tile tile, int id)
        {
            if (Settings.HaveChaosWastes)
            {
                if (tile.WaterCovered)
                {
                    return -100f;
                }
                /*
                if (tile.temperature > 5f)
                {
                    return 0f;
                }
                */

                return ConsiderLatitude(id);
            }
            else
            { 
                return -100f;
            }
        }

        private float ConsiderLatitude(int tileId)
        {
            if (Settings.HaveChaosWastes)
            { 
                Vector2 longLatOf = Find.WorldGrid.LongLatOf(tileId);

                float latitude = longLatOf.y;

                if (latitude < 0)
                {
                    latitude = latitude * -1;

                }

                if (latitude >= Settings.LatitudeRange)
                {
                    if (latitude >= (Settings.LatitudeRange - Settings.LatitudeRandomVariant))
                    {
                        return 25f;
                    }
                    else
                    {
                        return 100f;
                    }
                }
                else
                {
                    return 0f;
                }
            }
            else
            { 
                return 0;
            }
        }
    }
}
