using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;

public static class HookUtils {
	public static void EmitRotate(this ILCursor cursor) {
		cursor.EmitDelegate(DoRotate);
	}
	private static Vector2 DoRotate(Vector2 v, Entity entity) {
		var gravity = entity.Components.Get<GravityComponent>()?.gravity ?? Gravity.Down;
		return v.Rotate(gravity);
	}
	public static void EmitRotateInv(this ILCursor cursor) {
		cursor.EmitDelegate(DoRotateInv);
	}
	private static Vector2 DoRotateInv(Vector2 v, Entity entity) {
		var gravity = entity.Components.Get<GravityComponent>()?.gravity ?? Gravity.Down;
		return v.RotateInv(gravity);
	}
}
