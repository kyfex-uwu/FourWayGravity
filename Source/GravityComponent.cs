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
		var gravityEntity = entity.Components.Get<GravityEntity>();
		Views.WorldView(entity);
		var prevPos = entity.Position;
		if(gravity == comp.gravity.Opposite()) {
			if(gravity.Horizontal()) {
				entity.Position += entity.Width * gravity.Dir();
			} else {
				entity.Position += entity.Height * gravity.Dir();
			}
		} else {
			var half = 4f;
			if(comp.gravity.Horizontal()) {
				half = entity.Height / 2;
			} else {
				half = entity.Width / 2;
			}
			half = (int)half;
			entity.Position += half * gravity.Dir() - half * comp.gravity.Dir();
		}
		var prevGravity = comp.gravity;
		comp.gravity = gravity;
		if(!entity.Components.Get<GravityEntity>().setGravity(comp)) {
			entity.Position = prevPos;
			comp.gravity = prevGravity;
			Views.Pop(entity);
			return;
		}
		foreach(var graphics in entity.Components.GetAll<GraphicsComponent>()) {
			graphics.Rotation = gravity.Angle();
		}
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
