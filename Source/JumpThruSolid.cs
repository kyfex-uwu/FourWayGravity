using System;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

[Tracked]
[CustomEntity("GravityHelperGV/JumpThruSolid")]
public class JumpThruSolid : Solid
{
	public Direction direction;
	public bool speedCheck;
	public bool canPush;
	public Vector2 moveFrom;
	public JumpThruSolid(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, true) {
		direction = Direction.Right;
	}
	public JumpThruSolid(Direction direction, Vector2 position, float width, float height) : base(position, width, height, true) {
		this.direction = direction;
	}
	public override void MoveHExact(int move) {
		moveFrom = Position;
		canPush = direction switch {
			Direction.Left => move < 0,
			Direction.Right => move > 0,
			_ => false
		};
		base.MoveHExact(move);
		canPush = false;
	}
	public override void MoveVExact(int move) {
		moveFrom = Position;
		canPush = direction switch {
			Direction.Up => move < 0,
			Direction.Down => move > 0,
			_ => false
		};
		base.MoveVExact(move);
		canPush = false;
	}
	public override void DebugRender(Camera camera) {
		if(speedCheck)
			Draw.Rect(Position.X, Position.Y, Width, Height / 2, Color.Green);
		base.DebugRender(camera);
	}
}
public class JumpThruComponent : Component
{
	public JumpThruSolid solid;
	Direction direction;
	Func<Vector2> LiftSpeed;
    public JumpThruComponent(Func<Vector2> liftspeed, Direction direction) : base(true, true)
    {
		LiftSpeed = liftspeed;
		this.direction = direction;
    }
	public override void Added(Entity entity) {
		base.Added(entity);
		solid = new JumpThruSolid(direction, entity.Position, entity.Width, entity.Height);
		if(entity is Platform platform) {
			solid.Safe = platform.Safe;
		}
		if(entity.Scene != null) {
			entity.Scene.Add(solid);
		}
	}
	public override void Removed(Entity entity) {
		entity.Scene?.Remove(solid);
	}
	public override void EntityRemoved(Scene scene) {
		scene.Remove(solid);
	}
	public override void EntityAdded(Scene scene) {
		scene.Add(solid);
	}
	public override void Update() {
		solid.MoveTo(Entity.Position);
		solid.LiftSpeed = LiftSpeed(); 
	}
}
public enum Direction {
	Left, Right, Up, Down
}
