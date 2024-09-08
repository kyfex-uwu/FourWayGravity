using System;
using System.Reflection;
using Celeste;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

public class MiscHooks {
	static ILHook MoveBlockController;
	public static void Load() {
		On.Celeste.BounceBlock.WindUpPlayerCheck += WindUpPlayerCheck;
		On.Celeste.DashBlock.OnDashed += DashBlockHook;
		MoveBlockController = new ILHook(typeof(MoveBlock).GetMethod("Controller", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), ControllerHook);
	}
    public static void Unload() {
		
		On.Celeste.BounceBlock.WindUpPlayerCheck -= WindUpPlayerCheck;
		On.Celeste.DashBlock.OnDashed -= DashBlockHook;
		MoveBlockController?.Dispose();
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
	private static void ControllerHook(ILContext il) {
		var cursor = new ILCursor(il);
		cursor.GotoNext(MoveType.After, i => i.MatchCallvirt<Solid>("HasPlayerOnTop"));
		cursor.EmitLdloc1();
		cursor.EmitDelegate(InvertMoveBlockCond);
		cursor.GotoNext(MoveType.After, i => i.MatchCallvirt<Solid>("HasPlayerClimbing"));
		cursor.EmitLdloc1();
		cursor.EmitDelegate(InvertMoveBlockCond);	
	}
	private static bool InvertMoveBlockCond(bool cond, MoveBlock block) {
		var player = block.Scene.Tracker.GetEntity<Player>();
		if(player?.Collider is TransformCollider transformCollider) {
			if (transformCollider.gravity.gravity.Horizontal()) {
				return !cond;
			}
		}
		return cond;
	}
}
