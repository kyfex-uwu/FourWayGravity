using System;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;

[Tracked]
public class GravityComponent : Component
{
	public Gravity gravity = Gravity.Left;
	public Vector2 origin;
	public bool Track => currentView == View.Entity;
	static Hook hook_ColliderSet;
	public Stack<View> viewStack = new();
	public View currentView = View.World;
	
    public GravityComponent(bool active, bool visible, Gravity gravity) : base(active, visible)
    {
		this.gravity = gravity;
    }
	public static void Set(Entity entity, Gravity gravity) {
		if(entity.Components.Get<GravityEntity>() == null) {
			return;
		}
		GravityComponent comp;
		TransformCollider col;
		if(entity.Collider is TransformCollider collider) {
			comp = collider.gravity;
			col = collider;
		} else {
			comp = new GravityComponent(true, true, Gravity.Down);
			entity.Add(comp);
			col = (TransformCollider)entity.Collider;
		}
		if(gravity == comp.gravity)
			return;
		Views.WorldView(entity);
		entity.Position += 4f * gravity.Dir() - 4f * comp.gravity.Dir();
		comp.gravity = gravity;
		foreach(var graphics in entity.Components.GetAll<GraphicsComponent>()) {
			graphics.Rotation = gravity.Angle();
		}
		entity.Components.Get<GravityEntity>().setGravity(comp);
		Views.Pop(entity);
		entity.Collider = entity.Collider;
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
		if(Entity is Player player) {
			var move = new Vector2(Input.MoveX, Input.MoveY).RotateInv(gravity);
			Input.MoveX.Value = (int)move.X;
			Input.MoveY.Value = (int)move.Y;
			if(Input.Aim.Value != Vector2.Zero) {
				Input.Aim.Value = Input.GetAimVector(player.Facing).RotateInv(gravity);
			} else {
				Input.Aim.Value = Input.GetAimVector(player.Facing);
			}
			Input.Feather.Value = Input.Feather.Value.RotateInv(gravity);
		}
		Views.EntityView(Entity);	
    }
	private void PostUpdate(Entity entity) {
		Views.Pop(entity);
	}
	public override void Removed(Entity entity) {
		
	}
	public void EntityView() {
		origin = Entity.Position;
		var gravityEntity = Entity.Components.Get<GravityEntity>();
		gravityEntity.entityView(this);
		currentView = View.Entity;
	}
	public void WorldView() {
		Apply(Entity);
		var gravityEntity = Entity.Components.Get<GravityEntity>();
		gravityEntity.worldView(this);
		currentView = View.World;
	}
	public void Pop() {
		if(viewStack.Count == 0)
			return;
		var view = viewStack.Pop();
		if(view == currentView)
			return;
		if(view == View.Entity) {
			EntityView();			
		} else {
			WorldView();
		}
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
			if(self is TransformCollider trans) {
				trans.Update();
				return collider.Collide(trans.hitbox);
			}
			return self switch {
				Hitbox hitbox => collider.Collide(hitbox),
				Grid grid => collider.Collide(grid),
				Circle circle => collider.Collide(circle),
				ColliderList list => collider.Collide(list),
				_ => false
			};
		}
    }
}
public enum Gravity {
	Up, Down, Left, Right
}
public enum View {
	Entity, World
}
