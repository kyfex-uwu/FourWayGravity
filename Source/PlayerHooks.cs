using System;
using System.Linq;
using System.Reflection;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

public class PlayerHooks {
	static ILHook hook_orig_Update;
	static ILHook hook_orig_UpdateSprite;
	static Hook hook_Ducking_get;
	delegate bool orig_Ducking_get(Player self);
	static Hook hook_LiftSpeed_set;
	delegate void orig_LiftSpeed_set(Player self, Vector2 value);
	public static void Load() {
		hook_orig_Update = new ILHook(typeof(Player).GetMethod("orig_Update"), ColliderFixHook);
		hook_orig_UpdateSprite = new ILHook(
			typeof(Player)
				.GetMethod("orig_UpdateSprite", BindingFlags.NonPublic | BindingFlags.Instance),
			 PointCheckHook
		);
		hook_Ducking_get = new Hook(typeof(Player).GetProperty("Ducking").GetGetMethod(), Ducking_get_fix);
		hook_LiftSpeed_set = new Hook(typeof(Player).GetProperty("LiftSpeed").GetSetMethod(), LiftSpeed_set_fix);
		On.Celeste.Player.TransitionTo += TransitionFix;
		On.Celeste.Player.Render += RotateSprite;
		IL.Celeste.Player.SlipCheck += PointCheckHook;
		IL.Celeste.Player.ClimbCheck += PointCheckHook;
		IL.Celeste.Player.OnCollideH += DashCollideHook;
		IL.Celeste.Player.OnCollideV += DashCollideHook;
	}

    public static void Unload() {
		hook_orig_Update?.Dispose();
		hook_orig_UpdateSprite?.Dispose();
		hook_Ducking_get?.Dispose();
		hook_LiftSpeed_set?.Dispose();
		On.Celeste.Player.TransitionTo -= TransitionFix;
		On.Celeste.Player.Render -= RotateSprite;
		IL.Celeste.Player.SlipCheck -= PointCheckHook;
		IL.Celeste.Player.ClimbCheck -= PointCheckHook;
		IL.Celeste.Player.OnCollideH -= DashCollideHook;
		IL.Celeste.Player.OnCollideV -= DashCollideHook;
	}
	public static Vector2 PointCheckCorrection(Vector2 point, Player player) {
		if(player.Collider is TransformCollider collider) {
			var origin = collider.gravity.origin;
			var corrected = (point - origin).Rotate(collider.gravity.gravity) + origin;
			corrected += collider.gravity.gravity switch {
				Gravity.Up => -new Vector2(1f),
				Gravity.Right => -Vector2.UnitY,
				_ => Vector2.Zero
			}; // Idk why this is necessary tbh probably good to investigate later
			GravityComponent.points.Add(corrected);
			return corrected;
		}
		GravityComponent.points.Add(point);
		return point;
	}
	private static void PointCheckHook(ILContext il) {
		var cursor = new ILCursor(il);
		var method = typeof(Level)
			.GetMethod("CollideCheck", new Type[] { typeof(Vector2) })
			.MakeGenericMethod(new Type[] { typeof(Solid) });
		Logger.Log(LogLevel.Info, "GHGV", $"{method}");
		while(cursor.TryGotoNext(
			MoveType.Before,
			i => i.MatchCallvirt(method)
		)) {
			Logger.Log(LogLevel.Info, "GHGV", "Patched");
			cursor.EmitLdarg0();
			cursor.EmitDelegate(PointCheckCorrection);
			cursor.TryGotoNext(
				MoveType.Before,
				i => i.MatchCallvirt(method)
			);
		}
	}
	private static void ColliderFixHook(ILContext il)
    {
		var cursor = new ILCursor(il);		
		var bounds = cursor.TryGotoNext(
			i => i.MatchCallvirt<Level>("EnforceBounds")
		);
		if(bounds) {
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
		if(player.Collider is TransformCollider collider) {
			if(collider.source == player.hurtbox)
				player.Collider = tmp;
		}
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
    private static void LiftSpeed_set_fix(orig_LiftSpeed_set orig, Player self, Vector2 value)
    {
		if(self.Collider is TransformCollider collider) {
			orig(self, value.RotateInv(collider.gravity.gravity));
		} else {
			orig(self, value);
		}
    }

    private static void RotateSprite(On.Celeste.Player.orig_Render orig, Player self)
    {
		if(self.Collider is TransformCollider collider) {
			self.Sprite.Rotation = collider.gravity.gravity.Angle();
		}
		orig(self);
    }
	private static Vector2 FixDashDirection(Vector2 direction, Player player) {
		if(player.Collider is TransformCollider collider) {
			return direction.Rotate(collider.gravity.gravity);
		}
		return direction;
	}
    private static void DashCollideHook(ILContext il)
    {
		var cursor = new ILCursor(il);
		var method = typeof(DashCollision).GetMethod("Invoke");
		while(cursor.TryGotoNext(
			MoveType.Before,
			i => i.MatchCallvirt(method)
		)) {
			Logger.Log(LogLevel.Info, "GHGV", "Dash patch");
			cursor.EmitLdarg0();
			cursor.EmitDelegate(FixDashDirection);
			cursor.EmitLdarg0();
			cursor.EmitDelegate(Views.WorldView);
			cursor.TryGotoNext(
				MoveType.After,
				i => i.MatchCallvirt(method)
			);
			cursor.EmitLdarg0();
			cursor.EmitDelegate(Views.Pop);
		}
    }
}
