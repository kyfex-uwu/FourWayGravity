using System;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;

public class HoldableHooks
{
    public static void Load()
    {
        On.Celeste.TheoCrystal.Added += TheoAddedHook;
        On.Celeste.Glider.Added += JellyAddedHook;
        On.Celeste.Holdable.Release += ReleaseHook;
        On.Celeste.Holdable.Added += HoldableAdded;
        On.Celeste.Glider.Render += JellyRenderHook;
        On.Celeste.Glider.Update += JellyUpdateHook;
        IL.Celeste.TheoCrystal.OnCollideH += HoldableSwitchFix;
        IL.Celeste.TheoCrystal.OnCollideV += HoldableSwitchFix;
        IL.Celeste.Glider.OnCollideH += HoldableSwitchFix;
    }
    public static void Unload()
    {
        On.Celeste.TheoCrystal.Added -= TheoAddedHook;
        On.Celeste.Glider.Added -= JellyAddedHook;
        On.Celeste.Holdable.Release -= ReleaseHook;
        On.Celeste.Holdable.Added -= HoldableAdded;
        On.Celeste.Glider.Render -= JellyRenderHook;
        On.Celeste.Glider.Update -= JellyUpdateHook;
        IL.Celeste.TheoCrystal.OnCollideH -= HoldableSwitchFix;
        IL.Celeste.TheoCrystal.OnCollideV -= HoldableSwitchFix;
        IL.Celeste.Glider.OnCollideH -= HoldableSwitchFix;
    }

    private static void HoldableSwitchFix(ILContext il)
    {
        var cursor = new ILCursor(il);
        try {
            cursor.GotoNext(i => i.MatchCallvirt<DashCollision>("Invoke"));
            cursor.EmitLdarg0();
            cursor.EmitRotate();
        } catch(Exception e) {
            Logger.Warn("4WG", $"Dash switch fix failed {e}");
        }
    }

    private static void HoldableAdded(On.Celeste.Holdable.orig_Added orig, Holdable self, Entity entity)
    {
        if (entity.Components.Get<GravityEntity>() != null)
        {
            return;
        }
        var gravity = new GravityEntity();
        gravity.entityView = gravity =>
        {
            self.SetSpeed(self.GetSpeed().RotateInv(gravity.gravity));
            if (self.Entity is Actor actor)
            {
                actor.currentLiftSpeed = actor.currentLiftSpeed.RotateInv(gravity.gravity);
                actor.lastLiftSpeed = actor.lastLiftSpeed.RotateInv(gravity.gravity);
            }
        };
        gravity.worldView = gravity =>
        {
            self.SetSpeed(self.GetSpeed().Rotate(gravity.gravity));
            if (self.Entity is Actor actor)
            {
                actor.currentLiftSpeed = actor.currentLiftSpeed.Rotate(gravity.gravity);
                actor.lastLiftSpeed = actor.lastLiftSpeed.Rotate(gravity.gravity);
            }
        };
        gravity.setGravity = gravity =>
        {
            var holderGravity = self.Holder?.Components.Get<GravityComponent>()?.gravity;
            if (holderGravity != null && holderGravity != gravity.gravity)
            {
                return false;
            }
            return true;
        };
        entity.Add(gravity);
        orig(self, entity);
    }

    private static void TheoAddedHook(On.Celeste.TheoCrystal.orig_Added orig, Celeste.TheoCrystal self, Scene scene)
    {
        var gravity = new GravityEntity();
        gravity.entityView = gravity =>
        {
            var theo = (TheoCrystal)gravity.Entity;
            theo.Speed = theo.Speed.RotateInv(gravity.gravity);
            theo.currentLiftSpeed = theo.LiftSpeed.RotateInv(gravity.gravity);
            theo.prevLiftSpeed = theo.prevLiftSpeed.RotateInv(gravity.gravity);
            theo.lastLiftSpeed = theo.lastLiftSpeed.RotateInv(gravity.gravity);
        };
        gravity.worldView = gravity =>
        {
            var theo = (TheoCrystal)gravity.Entity;
            theo.Speed = theo.Speed.Rotate(gravity.gravity);
            theo.currentLiftSpeed = theo.LiftSpeed.Rotate(gravity.gravity);
            theo.prevLiftSpeed = theo.prevLiftSpeed.Rotate(gravity.gravity);
            theo.lastLiftSpeed = theo.lastLiftSpeed.Rotate(gravity.gravity);
        };
        gravity.setGravity = gravity =>
        {
            var theo = (TheoCrystal)gravity.Entity;
            var holderGravity = theo.Hold.Holder?.Components.Get<GravityComponent>()?.gravity;
            if (holderGravity != null && holderGravity != gravity.gravity)
            {
                return false;
            }
            return true;
        };
        self.Components.RemoveAll<GravityEntity>();
        self.Add(gravity);
        orig(self, scene);
    }
    private static void ReleaseHook(On.Celeste.Holdable.orig_Release orig, Holdable self, Vector2 force)
    {
        Views.EntityView(self.Entity);
        orig(self, force);
        Views.Pop(self.Entity);
    }

    private static void JellyAddedHook(On.Celeste.Glider.orig_Added orig, Glider self, Scene scene)
    {
        var gravity = new GravityEntity();
        gravity.entityView = gravity =>
        {
            var jelly = (Glider)gravity.Entity;
            jelly.Speed = jelly.Speed.RotateInv(gravity.gravity);
            jelly.currentLiftSpeed = jelly.LiftSpeed.RotateInv(gravity.gravity);
            jelly.prevLiftSpeed = jelly.prevLiftSpeed.RotateInv(gravity.gravity);
            jelly.lastLiftSpeed = jelly.lastLiftSpeed.RotateInv(gravity.gravity);
            var move = new Vector2(Input.MoveX, Input.MoveY).RotateInv(gravity.gravity);
            Input.GliderMoveY.Value = (int)move.Y;
            Input.GliderMoveY.Value = 0;
        };
        gravity.worldView = gravity =>
        {
            var jelly = (Glider)gravity.Entity;
            jelly.Speed = jelly.Speed.Rotate(gravity.gravity);
            jelly.currentLiftSpeed = jelly.LiftSpeed.Rotate(gravity.gravity);
            jelly.prevLiftSpeed = jelly.prevLiftSpeed.Rotate(gravity.gravity);
            jelly.lastLiftSpeed = jelly.lastLiftSpeed.Rotate(gravity.gravity);
            var move = new Vector2(Input.MoveX, Input.MoveY).Rotate(gravity.gravity);
            Input.GliderMoveY.Value = (int)move.Y;
        };
        gravity.setGravity = gravity =>
        {
            var jelly = (Glider)gravity.Entity;
            var holderGravity = jelly.Hold.Holder?.Components.Get<GravityComponent>()?.gravity;
            if (holderGravity != null && holderGravity != gravity.gravity)
            {
                return false;
            }
            return true;
        };
        self.Components.RemoveAll<GravityEntity>();
        self.Add(gravity);
        orig(self, scene);

    }
    private static void JellyRenderHook(On.Celeste.Glider.orig_Render orig, Glider self)
    {
        var gravity = self.Components.Get<GravityComponent>();
        var angle = 0f;
        if (gravity != null)
        {
            angle = gravity.gravity.Angle();
        }
        self.sprite.Rotation += angle;
        orig(self);
        self.sprite.Rotation -= angle;
    }

    private static void JellyUpdateHook(On.Celeste.Glider.orig_Update orig, Glider self)
    {
        if (self.Hold.Holder != null)
        {
            Views.EntityView(self.Hold.Holder);
        }
        orig(self);
        if (self.Hold.Holder != null)
        {
            Views.Pop(self.Hold.Holder);
        }
    }
}
