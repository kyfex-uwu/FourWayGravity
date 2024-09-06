using System;
using Celeste;
using Monocle;

public class TheoHooks {
	public static void Load() {
		On.Celeste.TheoCrystal.Added += AddedHook;
	}

    public static void Unload() {
		On.Celeste.TheoCrystal.Added -= AddedHook;
	}
    private static void AddedHook(On.Celeste.TheoCrystal.orig_Added orig, Celeste.TheoCrystal self, Scene scene)
    {
		var gravity = new GravityEntity();
		gravity.entityView = gravity => {
			var theo = (TheoCrystal)gravity.Entity;
			theo.Speed = theo.Speed.RotateInv(gravity.gravity);
			theo.LiftSpeed = theo.Speed.RotateInv(gravity.gravity);
		};
		gravity.worldView = gravity => {
			var theo = (TheoCrystal)gravity.Entity;
			theo.Speed = theo.Speed.Rotate(gravity.gravity);
			theo.LiftSpeed = theo.Speed.Rotate(gravity.gravity);			
		};
		gravity.setGravity = gravity => {
			var theo = (TheoCrystal)gravity.Entity;
			theo.sprite.Rotation = gravity.gravity.Angle();
		};
		self.Add(gravity);
		orig(self, scene);
    }

}
