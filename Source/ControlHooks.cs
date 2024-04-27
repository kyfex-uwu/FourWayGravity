
using System;
using System.Collections.Generic;
using Monocle;
using MonoMod.RuntimeDetour;

public class ControlHooks {
	private static bool Set(Entity entity, bool value) {
		var gravity = entity.Components.Get<GravityComponent>();
		if(gravity != null) {
			var prev = gravity.track;
			gravity.track = value;
			return prev;
		} 
		return false;	}
	public static void Load() {
		On.Celeste.Level.EnforceBounds += EnforceBounds;
		On.Celeste.Actor.IsRiding_Solid += IsRiding_Solid;
	}

    public static void Unload() {
		On.Celeste.Level.EnforceBounds -= EnforceBounds;
		On.Celeste.Actor.IsRiding_Solid -= IsRiding_Solid;
	}
	private static void EnforceBounds(On.Celeste.Level.orig_EnforceBounds orig, Celeste.Level self, Celeste.Player player)
    {
		var prev = Set(player, false);
		orig(self, player);
		Set(player, prev);
    }
    private static bool IsRiding_Solid(On.Celeste.Actor.orig_IsRiding_Solid orig, Celeste.Actor self, Celeste.Solid solid)
    {
		var prev = Set(self, true);
		var result = orig(self, solid);
		Set(self, prev);
		return result;
    }
}
