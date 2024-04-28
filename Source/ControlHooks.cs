
using System;
using System.Collections.Generic;
using Celeste;
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
		On.Celeste.Actor.IsRiding_Solid += Actor_IsRiding_Solid;
		On.Celeste.Player.IsRiding_Solid += Player_IsRiding_Solid;
		On.Celeste.Solid.GetPlayerClimbing += GetPlayerClimbing;
	}
    public static void Unload() {
		On.Celeste.Level.EnforceBounds -= EnforceBounds;
		On.Celeste.Player.IsRiding_Solid -= Player_IsRiding_Solid;
		On.Celeste.Actor.IsRiding_Solid -= Actor_IsRiding_Solid;
		On.Celeste.Solid.GetPlayerClimbing -= GetPlayerClimbing;
	}
	private static void EnforceBounds(On.Celeste.Level.orig_EnforceBounds orig, Celeste.Level self, Celeste.Player player)
    {
		var prev = Set(player, false);
		orig(self, player);
		Set(player, prev);
    }
    private static bool Player_IsRiding_Solid(On.Celeste.Player.orig_IsRiding_Solid orig, Celeste.Player self, Celeste.Solid solid)
    {
		var prev = Set(self, true);
		var result = orig(self, solid);
		Set(self, prev);
		return result;
    }
    private static bool Actor_IsRiding_Solid(On.Celeste.Actor.orig_IsRiding_Solid orig, Celeste.Actor self, Celeste.Solid solid)
    {
		var prev = Set(self, true);
		var result = orig(self, solid);
		Set(self, prev);
		return result;
    }
    private static Celeste.Player GetPlayerClimbing(On.Celeste.Solid.orig_GetPlayerClimbing orig, Celeste.Solid self)
    {
		var player = self.Scene.Tracker.GetEntity<Player>();
		var prev = Set(player, true);
		var result = orig(self);
		Set(player, prev);
		return result;
    }
}
