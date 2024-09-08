using System;
using Celeste;
using Microsoft.Xna.Framework;
using MonoMod.Cil;

public class MiscHooks {
	public static void Load() {
		On.Celeste.BounceBlock.WindUpPlayerCheck += WindUpPlayerCheck;
		On.Celeste.DashBlock.OnDashed += DashBlockHook;
	}
    public static void Unload() {
		
		On.Celeste.BounceBlock.WindUpPlayerCheck -= WindUpPlayerCheck;
		On.Celeste.DashBlock.OnDashed -= DashBlockHook;
	}

    private static DashCollisionResults DashBlockHook(On.Celeste.DashBlock.orig_OnDashed orig, DashBlock self, Player player, Vector2 direction)
    {
		var result = orig(self, player, direction);
		if(result == DashCollisionResults.Rebound) {
			GravityArrow.ApplyArrows(self, player);
		}
		return result;
    }

    private static Celeste.Player WindUpPlayerCheck(On.Celeste.BounceBlock.orig_WindUpPlayerCheck orig, Celeste.BounceBlock self)
    {
		var player = self.Scene.Tracker.GetEntity<Player>();
		if(player != null && player.Collider is not TransformCollider)
			return orig(self);
		player = self.GetPlayerOnTop();
		if(player == null || (player != null && player.Speed.Y < 0f)) {
			player = self.GetPlayerClimbing();
		}
		return player;
    }
}
