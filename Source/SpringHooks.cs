using System;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

public class SpringHooks {

	public static void Load() {
		On.Celeste.Spring.OnCollide += OnCollidePlayerHook;
		On.Celeste.Spring.OnHoldable += OnCollideHoldableHook;
	}

    public static void Unload() {
		On.Celeste.Spring.OnCollide -= OnCollidePlayerHook;
		On.Celeste.Spring.OnHoldable -= OnCollideHoldableHook;
		
	}
    private static void OnCollidePlayerHook(On.Celeste.Spring.orig_OnCollide orig, Spring self, Player entity)
    {
		var gravity = entity.Components.Get<GravityComponent>(); 
		if(gravity != null) {
			Views.EntityView(entity);
			var orientation = self.Orientation;
			var dir = orientation switch {
				Spring.Orientations.Floor => Vector2.UnitY,
				Spring.Orientations.WallLeft => Vector2.UnitX,
				Spring.Orientations.WallRight => -Vector2.UnitX,
				_ => Vector2.Zero
			};
			dir = dir.Rotate(gravity.gravity);
			if(dir == Vector2.UnitY) {
				self.Orientation = Spring.Orientations.Floor;
			} else if(dir == Vector2.UnitX) {
				self.Orientation = Spring.Orientations.WallLeft;
			} else if(dir == -Vector2.UnitX) {
				self.Orientation = Spring.Orientations.WallRight;
			} else {
				Views.EntityView(entity);
				return;
			}
			orig(self, entity);
			self.Orientation = orientation;
			Views.Pop(entity);
		} else {
			orig(self, entity);
		}
    }
    private static void OnCollideHoldableHook(On.Celeste.Spring.orig_OnHoldable orig, Celeste.Spring self, Holdable holdable)
    {
		var entity = holdable.Entity;
		var gravity = entity.Components.Get<GravityComponent>(); 
		if(gravity != null) {
			Views.EntityView(entity);
			var orientation = self.Orientation;
			var dir = orientation switch {
				Spring.Orientations.Floor => Vector2.UnitY,
				Spring.Orientations.WallLeft => Vector2.UnitX,
				Spring.Orientations.WallRight => -Vector2.UnitX,
				_ => Vector2.Zero
			};
			dir = dir.Rotate(gravity.gravity);
			if(dir == Vector2.UnitY) {
				self.Orientation = Spring.Orientations.Floor;
			} else if(dir == Vector2.UnitX) {
				self.Orientation = Spring.Orientations.WallLeft;
			} else if(dir == -Vector2.UnitX) {
				self.Orientation = Spring.Orientations.WallRight;
			} else {
				Views.EntityView(entity);
				return;
			}
			orig(self, holdable);
			self.Orientation = orientation;
			Views.Pop(entity);
		} else {
			orig(self, holdable);
		}
    }
}
