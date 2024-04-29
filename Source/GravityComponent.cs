using System;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;

public class GravityComponent : Component
{
	public Gravity gravity = Gravity.Left;
	public Vector2 origin;
	public bool track = false;
	static Hook hook_ColliderSet;
	public static List<Vector2> points = new ();
    public GravityComponent(bool active, bool visible, Gravity gravity) : base(active, visible)
    {
		this.gravity = gravity;
    }
	public static void Set(Player player, Gravity gravity) {
		GravityComponent comp;
		TransformCollider col;
		if(player.Collider is TransformCollider collider) {
			comp = collider.gravity;
			col = collider;
		} else {
			comp = new GravityComponent(true, true, Gravity.Down);
			player.Add(comp);
			col = (TransformCollider)player.Collider;
		}
		if(gravity == comp.gravity)
			return;
		var prev = comp.track;
		comp.track = false;
		comp.Apply(player);
		player.Position += 8f * gravity.Dir() - 4f * comp.gravity.Dir();

		player.Speed = player.Speed.Rotate(comp.gravity).Rotate(gravity.Complement());
		player.DashDir = player.DashDir.Rotate(comp.gravity).Rotate(gravity.Complement());
		comp.gravity = gravity;
		comp.track = prev;
		comp.origin = player.Position;
		player.Collider = player.Collider;
	}
	public override void Added(Entity entity) {
		base.Added(entity);
		origin = Entity.Position;
		entity.PreUpdate += PreUpdate;
		entity.PostUpdate += PostUpdate;
		entity.Collider = entity.Collider;
	}
	public void Apply(Entity entity) {
		var dif = entity.Position - origin;
		var dest = origin + dif.Rotate(gravity);
		entity.Position = dest;
	}
    private void PreUpdate(Entity entity)
    {
		track = true;
		origin = Entity.Position;
		if(Entity is Player player) {
			var move = new Vector2(Input.MoveX, Input.MoveY).Rotate(gravity.Complement()).Round();
			Input.MoveX.Value = (int)move.X;
			Input.MoveY.Value = (int)move.Y;
			if(Input.Aim.Value != Vector2.Zero) {
				Input.Aim.Value = Input.GetAimVector(player.Facing).Rotate(gravity.Complement());
			} else {
				Input.Aim.Value = Input.GetAimVector(player.Facing);
			}
		}
    }
	private void PostUpdate(Entity entity) {
		track = false;
		Apply(entity);
		origin = Entity.Position;
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
		foreach(var point in points) {
			Draw.Point(point, Color.Blue);
		}
		points.Clear();
	}
	public delegate void orig_ColliderSet(Entity self, Collider collider);
	public static void ColliderSet(orig_ColliderSet orig, Entity self, Collider collider) {
		var gravity = self.Components.Get<GravityComponent>();
		if(gravity != null) {
			Hitbox box;
			if(collider is Hitbox hitbox) {
				box = hitbox;
			} else if(collider is TransformCollider trans) {
				box = trans.source;
			} else {
				orig(self, collider);
				return;
			}
			var rotated = box.Rotate(gravity.gravity);
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
    public override float Width { 
		get {
			if(gravity.track) {
				return source.Width;
			} else {
				return hitbox.Width;
			}
		} 
		set {} 
	}
    public override float Height { 
		get {
			if(gravity.track) {
				return source.Height;
			} else {
				return hitbox.Height;
			}
		} 
		set {} 
	}
	public override float Top {
		get {
			if(gravity.track) {
				return source.Top;
			} else {
				return hitbox.Top;
			}
		}
		set {} 
	}
	public override float Bottom {
		get {
			if(gravity.track) {
				return source.Bottom;
			} else {
				return hitbox.Bottom;
			}
		}
		set {} 
	}
	public override float Left {
		get {
			if(gravity.track) {
				return source.Left;
			} else {
				return hitbox.Left;
			}
		}
		set {} 
	}
	public override float Right {
		get {
			if(gravity.track) {
				return source.Right;
			} else {
				return hitbox.Right;
			}
		}
		set {} 
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

