using System;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;

public class HoldableHooks {
	public static void Load() {
		On.Celeste.TheoCrystal.Added += TheoAddedHook;
		On.Celeste.Glider.Added += JellyAddedHook;
		On.Celeste.Holdable.Release += ReleaseHook;
		IL.Celeste.Glider.Update += JellyUpdateHook;
	}

    public static void Unload() {
		On.Celeste.TheoCrystal.Added -= TheoAddedHook;
		On.Celeste.Glider.Added -= JellyAddedHook;
		On.Celeste.Holdable.Release -= ReleaseHook;
		IL.Celeste.Glider.Update -= JellyUpdateHook;
	}
    private static void TheoAddedHook(On.Celeste.TheoCrystal.orig_Added orig, Celeste.TheoCrystal self, Scene scene)
    {
		var gravity = new GravityEntity();
		gravity.entityView = gravity => {
			var theo = (TheoCrystal)gravity.Entity;
			theo.Speed = theo.Speed.RotateInv(gravity.gravity);
			theo.LiftSpeed = theo.LiftSpeed.RotateInv(gravity.gravity);
		};
		gravity.worldView = gravity => {
			var theo = (TheoCrystal)gravity.Entity;
			theo.Speed = theo.Speed.Rotate(gravity.gravity);
			theo.LiftSpeed = theo.LiftSpeed.Rotate(gravity.gravity);			
		};
		gravity.setGravity = gravity => {
			var theo = (TheoCrystal)gravity.Entity;
		};
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
		gravity.entityView = gravity => {
			var jelly = (Glider)gravity.Entity;
			jelly.Speed = jelly.Speed.RotateInv(gravity.gravity);
			jelly.LiftSpeed = jelly.LiftSpeed.RotateInv(gravity.gravity);
		};
		gravity.worldView = gravity => {
			var jelly = (Glider)gravity.Entity;
			jelly.Speed = jelly.Speed.Rotate(gravity.gravity);
			jelly.LiftSpeed = jelly.LiftSpeed.Rotate(gravity.gravity);			
		};
		gravity.setGravity = gravity => {
			var jelly = (Glider)gravity.Entity;
		};
		self.Add(gravity);
		orig(self, scene);

    }
    private static void JellyUpdateHook(ILContext il)
    {
		var cursor = new ILCursor(il);
		cursor.GotoNext(MoveType.Before, i => i.MatchStloc0());
		cursor.EmitLdarg0();
		cursor.EmitDelegate(FixJellyRotation);
    }
	private static float FixJellyRotation(float angle, Glider jelly) {
		var gravity = jelly.Components.Get<GravityComponent>();		
		if(gravity != null) {
			return angle + gravity.gravity.Angle();
		}
		return angle;
	}

}
