using HugsLib;
using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace TheEndTimes
{
    public class Building_WallSconce : Building
    {
        private Thing glower;
        private WallSconceGlower wallGlower;
        public Map placedMap;
        private IntVec3 pos;
        private IntVec3 glowPos;
        public string defStr;
        public string moteStr;
        public ThingDef onDef;
        private Graphic onGraphic;
        public CompRefuelable compFuel;
        private bool fueled;

        public override Graphic Graphic
        {
            get
            {
                if (!this.fueled)
                    return this.DefaultGraphic;
                this.onGraphic = GraphicDatabase.Get((Type)((GraphicData)this.onDef.graphicData).graphicClass, (string)((GraphicData)this.onDef.graphicData).texPath, ((ShaderTypeDef)((GraphicData)this.onDef.graphicData).shaderType).Shader, (Vector2)((GraphicData)this.onDef.graphicData).drawSize, ((Thing)this).DrawColor, ((Thing)this).DrawColorTwo);
                return this.onGraphic;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.placedMap = ((Thing)this).Map;
            this.pos = ((Thing)this).Position;
            this.glowPos = this.Position + IntVec3Utility.RotatedBy(IntVec3.South, this.Rotation);
            this.compFuel = (CompRefuelable)this.GetComp<CompRefuelable>();

            this.ColorSetup();
            if (this.glower == null)
                this.SpawnGlower();
            else if (this.wallGlower == null)
                this.wallGlower = (WallSconceGlower)this.glower;
            StaticWallSconceStuff.lastRoom = (Room)null;
            HugsLibController.Instance.DistributedTicker.RegisterTickability(new Action(this.CustomTick), 30, (Thing)this);
        }

        public override void DeSpawn(DestroyMode mode)
        {
            this.DespawnGlower();
            StaticWallSconceStuff.lastRoom = (Room)null;
            base.DeSpawn(mode);
        }

        public override void Destroy(DestroyMode mode)
        {
            this.DespawnGlower();
            StaticWallSconceStuff.lastRoom = (Room)null;
            base.Destroy(mode);
        }

        private void CustomTick()
        {
            this.TickStuff();
        }

        private void TickStuff()
        {
            if (!this.Spawned|| this.Destroyed)
                return;
            if (this.compFuel.HasFuel)
            {
                if (!this.fueled)
                {
                    this.wallGlower.ToggleGlower(true);
                    this.fueled = true;
                }
            }
            else if (this.fueled)
            {
                this.wallGlower.ToggleGlower(false);
                this.fueled = false;
            }
            if (!GenGrid.Impassable(this.pos, this.Map))
                this.WallDestroyed();
            else if (GenGrid.Impassable(this.glowPos, this.Map))
                this.LightBlocked();
        }

        private void SpawnGlower()
        {
            if (this.glower != null)
                return;
            this.glower = ThingMaker.MakeThing(ThingDef.Named(this.defStr), (ThingDef)null);
            GenSpawn.Spawn(this.glower, this.glowPos, this.Map, (WipeMode)0);
            this.wallGlower = (WallSconceGlower)this.glower;
            this.wallGlower.ToggleGlower(this.fueled);
        }

        private void DespawnGlower()
        {
            if (this.glower != null)
                ((Entity)this.glower).DeSpawn(DestroyMode.Vanish);
            this.glower = (Thing)null;
            this.wallGlower = (WallSconceGlower)null;
        }

        private void LightBlocked()
        {
            Messages.Message((TaggedString)(Translator.Translate("RH_TET_DestroyedBlocked")), (MessageTypeDef)MessageTypeDefOf.NegativeEvent, true);
            ((Thing)this).Destroy(DestroyMode.Vanish);
        }

        private void WallDestroyed()
        {
            Messages.Message((TaggedString)(Translator.Translate("RH_TET_DestroyedWall")), (MessageTypeDef)MessageTypeDefOf.NegativeEvent, true);
            ((Thing)this).Destroy(DestroyMode.Deconstruct);
        }

        public virtual void ColorSetup()
        {
            this.defStr = "RH_TET_SconceGlower";
            this.moteStr = "RH_TET_WallSconce_On";
            this.onDef = ThingDef.Named(this.moteStr);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Thing>(ref this.glower, "glower", false);
            Scribe_Values.Look<bool>(ref this.fueled, "fueled", false);
        }

        public Building_WallSconce()
            : base()
        {
        }
    }
}
