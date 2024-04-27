using System;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

public class GravityComponent : Component
{
	public float angle = 0f;
	public Vector2 origin;
	public bool track;
	static Hook hook_ColliderSet;
	static ILHook hook_orig_Update;
    public GravityComponent(bool active, bool visible) : base(active, visible)
    {
    }
	public override void Added(Entity entity) {
		base.Added(entity);
		origin = Entity.Position;
		entity.PreUpdate += PreUpdate;
		entity.PostUpdate += PostUpdate;
	}
    private void PreUpdate(Entity entity)
    {
		track = true;
		origin = Entity.Position;
    }
	private void PostUpdate(Entity entity) {
		track = false;
		var dif = entity.Position - origin;
		var dest = origin + dif.Rotate(angle);
		entity.Position = dest;
	}
	public override void Removed(Entity entity) {
		
	}
	public override void DebugRender(Camera camera) {
	}
	public delegate void orig_ColliderSet(Entity self, Collider collider);
	public static void ColliderSet(orig_ColliderSet orig, Entity self, Collider collider) {
		var gravity = self.Components.Get<GravityComponent>();
		if(collider is Hitbox box && box != null && gravity != null) {
			var corner1 = box.TopLeft.Rotate(gravity.angle);
			var corner2 = box.BottomRight.Rotate(gravity.angle);
			var max = Vector2.Max(corner1, corner2);
			var min = Vector2.Min(corner1, corner2);
			var rotated = new Hitbox(max.X - min.X, max.Y - min.Y, min.X, min.Y);  
			var transform = new TransformCollider(rotated, box, gravity);
			orig(self, transform);
			return;
		}
		orig(self, collider);
	}
	public static void SetHooks() {
		hook_ColliderSet = new Hook(
			typeof(Entity).GetProperty("Collider").GetSetMethod(),
			typeof(GravityComponent).GetMethod("ColliderSet")
		);
		hook_orig_Update = new ILHook(typeof(Player).GetMethod("orig_Update"), ColliderCheckHook);
		On.Monocle.Collider.Collide_Collider += OnCollideCollider;
	}
    public static void RemoveHooks() {
		On.Monocle.Collider.Collide_Collider -= OnCollideCollider;
		IL.Celeste.Player.Update -= ColliderCheckHook;
		hook_ColliderSet?.Dispose();
		hook_orig_Update?.Dispose();
	}
	private static void CollideFix(Player player, Collider tmp) {
		Logger.Log(LogLevel.Info, "GHGV", "yay yippee");
		if(player.Collider is TransformCollider collider) {
			if(collider.source == player.hurtbox) {
				player.Collider = tmp;
			}
		}
	}
    private static void ColliderCheckHook(ILContext il)
    {
		var cursor = new ILCursor(il);
		Logger.Log(LogLevel.Info, "GHGV", "Starting hook");
		var bounds = cursor.TryGotoNext(
			i => i.MatchCallvirt(typeof(Level).GetMethod("EnforceBounds"))
		);
		if(bounds) {
			Logger.Log(LogLevel.Info, "GHGV", "Reached bounds check");
			var match = cursor.TryGotoPrev(
				MoveType.After,
				i => i.MatchEndfinally()
			);
			if(match) {
				Logger.Log(LogLevel.Info, "GHGV", "Destination");
				cursor.Emit(OpCodes.Ldarg_0);
				cursor.Emit(OpCodes.Ldloc, 14);
				cursor.EmitDelegate<Action<Player, Collider>>(CollideFix);
			}
		}
    }
    private static bool OnCollideCollider(On.Monocle.Collider.orig_Collide_Collider orig, Collider self, Collider collider)
    {
		try {
			return orig(self, collider);
		} catch {
			return self switch {
				Hitbox hitbox => collider.Collide(hitbox),
				Grid grid => collider.Collide(grid),
				Circle circle => collider.Collide(circle),
				ColliderList list => collider.Collide(list),
				_ => throw new NotImplementedException()
			};
		}
    }
}
public class TransformCollider : Collider {
	public Hitbox source;
	public Hitbox hitbox;
	public Vector2 offset;
	GravityComponent gravity;
    public override float Width { get => hitbox.Width; set => hitbox.Width = value; }
    public override float Height { get => hitbox.Height; set => hitbox.Height = value; }
    public override float Top { 
		get {
			Update(); return hitbox.Top;
		} 
		set => hitbox.Top = value; 
	}
    public override float Bottom { 
		get {
			Update(); return hitbox.Bottom;
		} 
		set => hitbox.Bottom = value; 
	}
    public override float Left { 
		get {
			Update(); return hitbox.Left;
		} 
		set => hitbox.Left = value; 
	}
    public override float Right { 
		get {
			Update(); return hitbox.Right;
		} 
		set => hitbox.Right = value; 
	}
	public TransformCollider(Hitbox hitbox, Hitbox source, GravityComponent gravity) {
		this.hitbox = hitbox;
		this.gravity = gravity;
		this.source = source;
		offset = hitbox.Position;
	}
	public override void Added(Entity entity) {
		base.Added(entity);
		hitbox.Entity = Entity;
	}
	public void Update() {
		hitbox.Position = offset;
		if(!gravity.track) return;
		var dif = Entity.Position - gravity.origin;
		hitbox.Position += -dif + dif.Rotate(gravity.angle);
	}
    public override Collider Clone()
    {
		return new TransformCollider((Hitbox)hitbox.Clone(), source, gravity);
    }

    public override bool Collide(Circle circle) {
		Update();
		return hitbox.Collide(circle);
	}
	public override bool Collide(ColliderList list) {
		Update();
		return hitbox.Collide(list);	
	}

    public override bool Collide(Vector2 point)
    {
		Update();
		return hitbox.Collide(point);
    }

    public override bool Collide(Rectangle rect)
    {
		Update();
		return hitbox.Collide(rect);
    }

    public override bool Collide(Vector2 from, Vector2 to)
    {
		Update();
		return hitbox.Collide(from, to);
    }

    public override bool Collide(Hitbox box)
    {
		Update();
		return hitbox.Collide(box);
    }

    public override bool Collide(Grid grid)
    {
		Update();
		return hitbox.Collide(grid);
    }
    public override void Render(Camera camera, Color color)
    {
		Draw.HollowRect(hitbox.AbsoluteX, hitbox.AbsoluteY, hitbox.Width, hitbox.Height, Color.Red);
    }
}
