using System;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;

public class GravityComponent : Component
{
	public Gravity gravity = Gravity.Right;
	public Vector2 origin;
	public bool track;
	static Hook hook_ColliderSet;
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
		if(Entity is Player player) {
			var move = new Vector2(Input.MoveX, Input.MoveY).Rotate(gravity.Inv()).Round();
			Input.MoveX.Value = (int)move.X;
			Input.MoveY.Value = (int)move.Y;
			if(Input.Aim.Value != Vector2.Zero) {
				Input.Aim.Value = Input.GetAimVector(player.Facing).Rotate(gravity.Inv());
			} else {
				Input.Aim.Value = Input.GetAimVector(player.Facing);
			}
		}
    }
	private void PostUpdate(Entity entity) {
		track = false;
		var dif = entity.Position - origin;
		var dest = origin + dif.Rotate(gravity);
		entity.Position = dest;
	}
	public override void Removed(Entity entity) {
		
	}
	public override void DebugRender(Camera camera) {
		if(Entity is Player player) {
			foreach(var e in Scene.Tracker.GetEntities<Solid>()) {
				var solid = (Solid)e;
				if(player.IsRiding(solid) && solid is not SolidTiles) {
					Draw.Rect(e.X, e.Y, e.Width, e.Height, Color.Yellow);
				}
			}
		}
	}
	public delegate void orig_ColliderSet(Entity self, Collider collider);
	public static void ColliderSet(orig_ColliderSet orig, Entity self, Collider collider) {
		var gravity = self.Components.Get<GravityComponent>();
		if(collider is Hitbox box && box != null && gravity != null) {
			var corner1 = box.TopLeft.Rotate(gravity.gravity);
			var corner2 = box.BottomRight.Rotate(gravity.gravity);
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
		On.Monocle.Collider.Collide_Collider += OnCollideCollider;
	}
    public static void RemoveHooks() {
		On.Monocle.Collider.Collide_Collider -= OnCollideCollider;
		hook_ColliderSet?.Dispose();
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
	public GravityComponent gravity;
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
		hitbox.Position += -dif + dif.Rotate(gravity.gravity);
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
public enum Gravity {
	Up, Down, Left, Right
}

