using System;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
public static class Extensions {
	public static Vector2 Rotate(this Vector2 v, Gravity gravity) {
		return gravity switch {
			Gravity.Left => new Vector2(-v.Y, v.X),
			Gravity.Up => new Vector2(-v.X, -v.Y),
			Gravity.Right => new Vector2(v.Y, -v.X),
			_ => v
		};
	}
	public static Vector2 RotateInv(this Vector2 v, Gravity gravity) {
		return gravity switch {
			Gravity.Left => new Vector2(v.Y, -v.X),
			Gravity.Up => new Vector2(-v.X, -v.Y),
			Gravity.Right => new Vector2(-v.Y, v.X),
			_ => v
		};
	}
	public static Vector2 RotateAround(this Vector2 v, Vector2 origin, Gravity gravity) {
		var offset = v - origin;
		return origin + offset.Rotate(gravity);
	}
	public static Hitbox Rotate(this Hitbox hitbox, Gravity gravity) {
		var a = hitbox.TopLeft.Rotate(gravity);
		var b = hitbox.BottomRight.Rotate(gravity);
		var max = Vector2.Max(a, b);
		var min = Vector2.Min(a, b);
		return new Hitbox(max.X - min.X, max.Y - min.Y, min.X, min.Y);
	}
	public static Vector2 Dir(this Gravity g) {
		return g switch {
			Gravity.Left => -Vector2.UnitX,
			Gravity.Up => -Vector2.UnitY,
			Gravity.Right => Vector2.UnitX,
			_ => Vector2.UnitY
		};
	}
	public static Gravity Opposite(this Gravity g) {
		return g switch {
			Gravity.Left => Gravity.Right,
			Gravity.Up => Gravity.Down,
			Gravity.Right => Gravity.Left,
			_ => Gravity.Up
		};
	}
	public static Gravity Inv(this Gravity g) {
		return g switch {
			Gravity.Left => Gravity.Right,
			Gravity.Up => Gravity.Up,
			Gravity.Right => Gravity.Left,
			_ => Gravity.Down
		};
	}
	public static float Angle(this Gravity g) {
		return g switch {
			Gravity.Left => (float)Math.PI / 2f,
			Gravity.Up => (float)Math.PI,
			Gravity.Right => -(float)Math.PI / 2f,
			_ => 0f
		};
	}
	public static bool Horizontal(this Gravity g) {
		return g switch {
			Gravity.Left => true,
			Gravity.Right => true,
			_ => false
		};
	}
}
public static class Views {
	public static void EntityView(Entity entity) {
		if(entity.Collider is TransformCollider collider) {
			collider.gravity.viewStack.Push(collider.gravity.currentView);
			if(collider.gravity.currentView != View.Entity)
				collider.gravity.EntityView();
		}
	}
	public static void WorldView(Entity entity) {
		if(entity.Collider is TransformCollider collider) {
			collider.gravity.viewStack.Push(collider.gravity.currentView);
			if(collider.gravity.currentView != View.World)
				collider.gravity.WorldView();
		}
	}
	public static void Pop(Entity entity) {
		if(entity.Collider is TransformCollider collider) {
			collider.gravity.Pop();
		}
	}
}
