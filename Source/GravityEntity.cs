using System;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

[Tracked]
public class GravityEntity : Component {
	public Action<GravityComponent> entityView;
	public Action<GravityComponent> worldView;
	public Func<GravityComponent, bool> setGravity;

    public GravityEntity() : base(false, false)
    {
    }
	public static GravityEntity ForPlayer() {
		var gravity = new GravityEntity();
		gravity.entityView = (gravity) => {
			var player = (Player)gravity.Entity;
			player.Speed = player.Speed.RotateInv(gravity.gravity);		
			player.DashDir = player.DashDir.RotateInv(gravity.gravity);
			player.currentLiftSpeed = player.currentLiftSpeed.RotateInv(gravity.gravity);
			player.lastLiftSpeed = player.lastLiftSpeed.RotateInv(gravity.gravity);
			var move = new Vector2(Input.MoveX, Input.MoveY).RotateInv(gravity.gravity);
			Input.MoveX.Value = (int)move.X;
			Input.MoveY.Value = (int)move.Y;
			Input.GliderMoveY.Value = (int)move.Y;
			if(Input.Aim.Value != Vector2.Zero) {
				Input.Aim.Value = Input.GetAimVector(player.Facing).RotateInv(gravity.gravity);
			} else {
				Input.Aim.Value = Input.GetAimVector(player.Facing);
			}
			Input.Feather.Value = Input.Feather.Value.RotateInv(gravity.gravity);
		};
		gravity.worldView = (gravity) => {
			var player = (Player)gravity.Entity;
			player.Speed = player.Speed.Rotate(gravity.gravity);		
			player.DashDir = player.DashDir.Rotate(gravity.gravity);
			player.currentLiftSpeed = player.currentLiftSpeed.Rotate(gravity.gravity);
			player.lastLiftSpeed = player.lastLiftSpeed.Rotate(gravity.gravity);
			var move = new Vector2(Input.MoveX, Input.MoveY).Rotate(gravity.gravity);
			Input.MoveX.Value = (int)move.X;
			Input.MoveY.Value = (int)move.Y;
			Input.GliderMoveY.Value = (int)move.Y;
			if(Input.Aim.Value != Vector2.Zero) {
				Input.Aim.Value = Input.GetAimVector(player.Facing).Rotate(gravity.gravity);
			} else {
				Input.Aim.Value = Input.GetAimVector(player.Facing);
			}
			Input.Feather.Value = Input.Feather.Value.Rotate(gravity.gravity);
		};
		gravity.setGravity = gravity => {
			var player = (Player)gravity.Entity;
			if(player.CollideCheck<Solid>()) {
				player.Ducking = true;
			}
			if(player.Holding != null) {
				GravityComponent.Set(player.Holding.Entity, gravity.gravity);
			}
			return true;
		};
		return gravity;
	}
}
