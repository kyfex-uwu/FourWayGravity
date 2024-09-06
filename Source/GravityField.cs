
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

[CustomEntity("GravityHelperGV/GravityField")]
public class GravityField : Entity {
	Gravity gravity;
	public GravityField(EntityData data, Vector2 offset) {
		Position = data.Position + offset;
		Collider = new Hitbox(data.Width, data.Height);
		gravity = data.Attr("gravity", "down") switch {
			"left" => Gravity.Left,
			"down" => Gravity.Down,
			"right" => Gravity.Right,
			"up" => Gravity.Up,
			_ => Gravity.Down
		};
	}
	public override void Update() {
		foreach(var gravity in Scene.Tracker.GetComponents<GravityEntity>()) {
			if(gravity.Entity.CollideCheck(this)) {
				GravityComponent.Set(gravity.Entity, this.gravity);
			}
		}
	}
	public override void Render() {
		var color = gravity switch {
			Gravity.Left => Color.Green,
			Gravity.Right => Color.Yellow,
			Gravity.Up => Color.Red,
			_ => Color.Blue
		};
		color.A /= 2;
		Draw.Rect(Position.X, Position.Y, Width, Height, color);
	}
}
