using System;
using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;

public class ActorHooks {
	public static void Load() {
		On.Celeste.Actor.MoveHExact += CheckOutsideH;
		On.Celeste.Actor.MoveVExact += CheckOutsideV;
		On.Monocle.Collide.Check_Entity_IEnumerable1_Vector2 += CollideCheckAt;
		On.Monocle.Collide.First_Entity_IEnumerable1_Vector2 += CollideFirstAt;
	}

    public static void Unload() {
		On.Celeste.Actor.MoveHExact -= CheckOutsideH;
		On.Celeste.Actor.MoveVExact -= CheckOutsideV;
		On.Monocle.Collide.Check_Entity_IEnumerable1_Vector2 -= CollideCheckAt;
		On.Monocle.Collide.First_Entity_IEnumerable1_Vector2 -= CollideFirstAt;
	}

    private static Monocle.Entity CollideFirstAt(On.Monocle.Collide.orig_First_Entity_IEnumerable1_Vector2 orig, Monocle.Entity a, IEnumerable<Monocle.Entity> b, Vector2 at)
    {
		if(a.Collider is TransformCollider collider) {
			collider.checkFrom = a.Position;
			collider.checkAt = at;
			var result = orig(a, b, at);
			collider.checkFrom = Vector2.Zero;
			collider.checkAt = Vector2.Zero;
			return result;
		}
		return orig(a, b, at);
    }

    private static bool CollideCheckAt(On.Monocle.Collide.orig_Check_Entity_IEnumerable1_Vector2 orig, Monocle.Entity a, IEnumerable<Monocle.Entity> b, Vector2 at)
    {
		if(a.Collider is TransformCollider collider) {
			collider.checkFrom = a.Position;
			collider.checkAt = at;
			var result = orig(a, b, at);
			collider.checkFrom = Vector2.Zero;
			collider.checkAt = Vector2.Zero;
			return result;
		}
		return orig(a, b, at);
    }
    private static bool CheckOutsideH(On.Celeste.Actor.orig_MoveHExact orig, Celeste.Actor self, int moveH, Celeste.Collision onCollide, Celeste.Solid pusher)
    {
		if(self.Collider is TransformCollider collider) {
			collider.move = Vector2.UnitX * moveH;
			collider.moveFrom = self.Position;
			var result = orig(self, moveH, onCollide, pusher);
			collider.move = Vector2.Zero;
			return result;
		}
		return orig(self, moveH, onCollide, pusher);
    }
    private static bool CheckOutsideV(On.Celeste.Actor.orig_MoveVExact orig, Celeste.Actor self, int moveV, Celeste.Collision onCollide, Celeste.Solid pusher)
    {
		if(self.Collider is TransformCollider collider) {
			collider.move = Vector2.UnitY * moveV;
			collider.moveFrom = self.Position;
			var result = orig(self, moveV, onCollide, pusher);
			collider.move = Vector2.Zero;
			return result;
		}
		return orig(self, moveV, onCollide, pusher);
    }
}
