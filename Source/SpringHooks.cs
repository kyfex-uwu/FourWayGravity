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
		GravityArrow.ApplyArrows(self, entity);
		var gravity = entity.Components.Get<GravityComponent>(); 
		if(gravity != null) {
			var hitbox = ((Hitbox)self.Collider).Rotate(gravity.gravity.Inv());
			hitbox.Position -= (self.Position - entity.Position);
			hitbox.Position += (self.Position - entity.Position).Rotate(gravity.gravity.Inv());
			Views.EntityView(entity);
			var orientation = self.Orientation;
			var prevHitbox = self.Collider;
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
				self.Orientation = Spring.Orientations.Floor;
			}
			self.Collider = hitbox;
			orig(self, entity);			
			self.Orientation = orientation;
			self.Collider = prevHitbox;
			Views.Pop(entity);
		} else {
			orig(self, entity);
		}
    }
    private static void OnCollideHoldableHook(On.Celeste.Spring.orig_OnHoldable orig, Celeste.Spring self, Holdable holdable)
    {
		var entity = holdable.Entity;
		GravityArrow.ApplyArrows(self, entity);
		var gravity = entity.Components.Get<GravityComponent>(); 
		if(gravity != null) {

	var hitbox = ((Hitbox)self.Collider).Rotate(gravity.gravity.Inv());
			hitbox.Position -= (self.Position - entity.Position);
			hitbox.Position += (self.Position - entity.Position).Rotate(gravity.gravity.Inv());
			Views.EntityView(entity);
			var orientation = self.Orientation;
			var prevHitbox = self.Collider;
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
				self.Orientation = Spring.Orientations.Floor;
			}
			self.Collider = hitbox;
			orig(self, holdable);			
			self.Orientation = orientation;
			self.Collider = prevHitbox;
			Views.Pop(entity);
		} else {
			orig(self, holdable);
		}
    }
}
