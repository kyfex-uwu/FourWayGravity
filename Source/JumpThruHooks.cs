using System;
using Celeste;
using Microsoft.Xna.Framework;

public class JumpThruHooks {
	public static void Load() {
		On.Celeste.JumpThru.ctor += AddComponent;
	}
    public static void Unload() {
		On.Celeste.JumpThru.ctor -= AddComponent;
	}
    private static void AddComponent(On.Celeste.JumpThru.orig_ctor orig, Celeste.JumpThru self, Vector2 position, int width, bool safe)
    {
		orig(self, position, width, safe);
		var component = new JumpThruComponent(() => self.LiftSpeed, Direction.Up);
		self.Add(component);
    }

}
