using System;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;

public class SolidHooks {
	public static void Load() {
		IL.Celeste.Solid.GetPlayerClimbing += GetPlayerClimbing;
		IL.Celeste.Solid.GetPlayerOnTop += GetPlayerOnTop;
	}
    public static void Unload() {
		
	}
	private static Vector2 CorrectOffset(Vector2 v, Solid solid) {
		Player player = solid.Scene.Tracker.GetEntity<Player>();
		if(player != null && player.collider is TransformCollider collider) {
			return collider.gravity.gravity.Dir();
		}
		return v;
	}
    private static void GetPlayerOnTop(ILContext il)
    {
		var cursor = new ILCursor(il);
		try {
			cursor.GotoNext(
				MoveType.After,
				i => i.MatchCall<Vector2>("get_UnitY")
			);
			cursor.EmitLdarg0();
			cursor.EmitDelegate(CorrectOffset);
		} catch {
			Logger.Log(LogLevel.Info, "GHGV", "GetPlayerOnTop hook failed");
		}
    }
	private static Vector2 CorrectOffsetClimb(Vector2 v, Player player) {
		if(player.Collider is TransformCollider collider) {
			return v.Rotate(collider.gravity.gravity);
		}
		return v;
	}
    private static void GetPlayerClimbing(ILContext il)
    {
		var cursor = new ILCursor(il);
		try {
			for(int i = 0; i < 2; i++) {
				cursor.GotoNext(
					MoveType.After,
					i => i.MatchCall<Vector2>("get_UnitX")
				);
				cursor.EmitLdloc1();
				cursor.EmitDelegate(CorrectOffsetClimb);
			}
		} catch {
			
		}
    }

}
