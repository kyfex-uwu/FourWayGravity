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
	public bool Track => currentView == View.Player;
	static Hook hook_ColliderSet;
	public static List<Vector2> points = new ();
	public Stack<View> viewStack = new();
	public View currentView = View.World;
	
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
		Views.WorldView(player);
		player.Position += 4f * gravity.Dir() - 4f * comp.gravity.Dir();
		if(player.CollideCheck<Solid>()) {
			player.Ducking = true;
		}
		comp.gravity = gravity;
		Views.Pop(player);
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
		if(Entity is Player player) {
			var move = new Vector2(Input.MoveX, Input.MoveY).RotateInv(gravity).Round();
			Input.MoveX.Value = (int)move.X;
			Input.MoveY.Value = (int)move.Y;
			if(Input.Aim.Value != Vector2.Zero) {
				Input.Aim.Value = Input.GetAimVector(player.Facing).RotateInv(gravity);
			} else {
				Input.Aim.Value = Input.GetAimVector(player.Facing);
			}
			Views.PlayerView(player);	
		}
    }
	private void PostUpdate(Entity entity) {
		Views.Pop((Player)entity);
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
	public void PlayerView() {
		origin = Entity.Position;
		var player = (Player)Entity;
		player.Speed = player.Speed.RotateInv(gravity);		
		player.DashDir = player.DashDir.RotateInv(gravity);
		currentView = View.Player;
	}
	public void WorldView() {
		var player = (Player)Entity;
		Apply(player);
		player.Speed = player.Speed.Rotate(gravity);		
		player.DashDir = player.DashDir.Rotate(gravity);
		currentView = View.World;
	}
	public void Pop() {
		if(viewStack.Count == 0)
			return;
		var view = viewStack.Pop();
		if(view == currentView)
			return;
		if(view == View.Player) {
			PlayerView();			
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
public enum Gravity {
	Up, Down, Left, Right
}
public enum View {
	Player, World
}
