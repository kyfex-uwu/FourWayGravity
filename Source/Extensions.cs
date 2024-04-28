using System;
using Microsoft.Xna.Framework;

public static class Extensions {
	public static Vector2 Rotate(this Vector2 v, Gravity gravity) {
		return gravity switch {
			Gravity.Left => new Vector2(-v.Y, v.X),
			Gravity.Up => new Vector2(v.X, -v.Y),
			Gravity.Right => new Vector2(v.Y, -v.X),
			_ => v
		};
	}
	public static Gravity Inv(this Gravity g) {
		return g switch {
			Gravity.Left => Gravity.Right,
			Gravity.Up => Gravity.Down,
			Gravity.Right => Gravity.Left,
			_ => Gravity.Up
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
}
