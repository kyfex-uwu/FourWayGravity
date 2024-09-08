using System;
using Celeste;
using Monocle;

[Tracked]
public class GravityEntity : Component {
	public Action<GravityComponent> entityView;
	public Action<GravityComponent> worldView;
	public Action<GravityComponent> setGravity;

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
		};
		gravity.worldView = (gravity) => {
			var player = (Player)gravity.Entity;
			player.Speed = player.Speed.Rotate(gravity.gravity);		
			player.DashDir = player.DashDir.Rotate(gravity.gravity);
			player.currentLiftSpeed = player.currentLiftSpeed.Rotate(gravity.gravity);
			player.lastLiftSpeed = player.lastLiftSpeed.Rotate(gravity.gravity);
		};
		gravity.setGravity = gravity => {
			var player = (Player)gravity.Entity;
			if(player.CollideCheck<Solid>()) {
				player.Ducking = true;
			}
			if(player.Holding != null) {
				GravityComponent.Set(player.Holding.Entity, gravity.gravity);
			}
		};
		return gravity;
	}
}
