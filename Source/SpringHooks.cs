using System;
using Celeste;
using Microsoft.Xna.Framework;

public class SpringHooks {

	public static void Load() {
		On.Celeste.Spring.OnCollide += OnCollideHook;
	}

    public static void Unload() {
		On.Celeste.Spring.OnCollide -= OnCollideHook;
		
	}

    private static void OnCollideHook(On.Celeste.Spring.orig_OnCollide orig, Celeste.Spring self, Celeste.Player player)
    {
		if(player.Collider is TransformCollider transformCollider) {
			Views.ActorView(player);
			var orientation = self.Orientation;
			var dir = orientation switch {
				Spring.Orientations.Floor => Vector2.UnitY,
				Spring.Orientations.WallLeft => Vector2.UnitX,
				Spring.Orientations.WallRight => -Vector2.UnitX,
				_ => Vector2.Zero
			};
			dir = dir.Rotate(transformCollider.gravity.gravity);
			if(dir == Vector2.UnitY) {
				self.Orientation = Spring.Orientations.Floor;
			} else if(dir == Vector2.UnitX) {
				self.Orientation = Spring.Orientations.WallLeft;
			} else if(dir == -Vector2.UnitX) {
				self.Orientation = Spring.Orientations.WallRight;
			} else {
				Views.ActorView(player);
				return;
			}
			orig(self, player);
			self.Orientation = orientation;
			Views.Pop(player);
		} else {
			orig(self, player);
		}
    }
}
