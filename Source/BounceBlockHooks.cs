using System;
using Celeste;
using MonoMod.Cil;

public class BounceBlockHooks {
	public static void Load() {
		On.Celeste.BounceBlock.WindUpPlayerCheck += WindUpPlayerCheck;
	}


    public static void Unload() {
		
		On.Celeste.BounceBlock.WindUpPlayerCheck -= WindUpPlayerCheck;
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
