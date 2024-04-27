using System;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

public class PlayerHooks {
	static ILHook hook_orig_Update;
	static Hook hook_Ducking_get;
	delegate bool orig_Ducking_get(Player self);
	public static void Load() {
		hook_orig_Update = new ILHook(typeof(Player).GetMethod("orig_Update"), ColliderFixHook);
		hook_Ducking_get = new Hook(typeof(Player).GetProperty("Ducking").GetGetMethod(), Ducking_get_fix);
		On.Celeste.Player.TransitionTo += TransitionFix;
	}
    public static void Unload() {
		hook_orig_Update?.Dispose();
		hook_Ducking_get?.Dispose();
		On.Celeste.Player.TransitionTo -= TransitionFix;
	}
	private static void ColliderFixHook(ILContext il)
    {
		var cursor = new ILCursor(il);		
		Logger.Log(LogLevel.Info, "GHGV", "Starting hook");
		var bounds = cursor.TryGotoNext(
			i => i.MatchCallvirt<Level>("EnforceBounds")
		);
		if(bounds) {
			Logger.Log(LogLevel.Info, "GHGV", "Reached bounds check");
			var match = cursor.TryGotoPrev(
				MoveType.After,
				i => i.MatchCall(typeof(Entity).GetProperty("Collider").GetSetMethod())
			);
			if(match) {
				cursor.MoveAfterLabels();
				cursor.Emit(OpCodes.Ldarg_0);
				cursor.Emit(OpCodes.Ldloc, 14);
				cursor.EmitDelegate(CollideFix);
			}
		}
    }
	private static void CollideFix(Player player, Collider tmp) {
		player.Collider = tmp;
	}
    private static bool TransitionFix(On.Celeste.Player.orig_TransitionTo orig, Player self, Vector2 target, Vector2 direction)
    {
		var corrected = target - direction * new Vector2(8f, 12f);
		var offset = direction * self.Collider.Size;
		return orig(self, corrected + offset + direction, direction);
    }
    private static bool Ducking_get_fix(orig_Ducking_get orig, Player self)
    {
		if(self.Collider is TransformCollider transformCollider) {
			return transformCollider.source == self.duckHitbox || transformCollider.source == self.duckHurtbox;
		}
		return orig(self);
    }
}
