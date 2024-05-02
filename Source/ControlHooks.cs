
using System;
using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;

public class ControlHooks {
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
		Views.WorldView(player);
		orig(self, player);
		Views.Pop(player);
    }
    private static bool Player_IsRiding_Solid(On.Celeste.Player.orig_IsRiding_Solid orig, Celeste.Player self, Celeste.Solid solid)
    {
		Views.PlayerView(self);
		var result = orig(self, solid);
		Views.Pop(self);
		return result;
    }
    private static bool Actor_IsRiding_Solid(On.Celeste.Actor.orig_IsRiding_Solid orig, Celeste.Actor self, Celeste.Solid solid)
    {
		if(self is Player player) {
			Views.PlayerView(player);
			var result = orig(self, solid);
			Views.Pop(player);
			return result;
		}
		return orig(self, solid);
    }
    private static Celeste.Player GetPlayerClimbing(On.Celeste.Solid.orig_GetPlayerClimbing orig, Celeste.Solid self)
    {
		var player = self.Scene.Tracker.GetEntity<Player>();
		Views.PlayerView(player);
		var result = orig(self);
		Views.Pop(player);
		return result;
    }
}
