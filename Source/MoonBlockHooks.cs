
using System;
using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

public class MoonBlockHooks {

	public static void Load() {
		On.Celeste.FloatySpaceBlock.MoveToTarget += MoveToTargetHook;
		On.Celeste.FloatySpaceBlock.Update += UpdateHook;
	}


    public static void Unload() {
		On.Celeste.FloatySpaceBlock.MoveToTarget -= MoveToTargetHook;
		On.Celeste.FloatySpaceBlock.Update -= UpdateHook;
	}
	
    private static void MoveToTargetHook(On.Celeste.FloatySpaceBlock.orig_MoveToTarget orig, Celeste.FloatySpaceBlock self)
    {
		if(self.Components.Get<MultidirSink>() == null) {
			orig(self);
			return;
		}
		var sinkDirection = self.Components.Get<MultidirSink>().direction.SafeNormalize();
		float bob = (float)(4.0 * Math.Sin(self.sineWave));
		Vector2 knockback = Calc.YoYo(Ease.QuadIn(self.dashEase)) * self.dashDirection * 8f;
		Vector2 dip = 12f * Ease.SineInOut(self.yLerp) * sinkDirection;
		if(self.sinkTimer > 0) {
			var player = self.Scene.Tracker.GetEntity<Player>();
		}
		foreach (KeyValuePair<Platform, Vector2> move in self.Moves)
		{
			Platform key = move.Key;
			Vector2 value = move.Value;
			key.MoveTo(value + dip + knockback + bob * Vector2.UnitY);
		}
    }
    private static void UpdateHook(On.Celeste.FloatySpaceBlock.orig_Update orig, FloatySpaceBlock self)
    {
		orig(self);
		if(self.sinkTimer > 0) {
			var player = self.Scene.Tracker.GetEntity<Player>();
			if(player != null && player.Collider is TransformCollider collider) {
				var sink = self.Components.Get<MultidirSink>();
				if(sink == null) {
					self.Add(sink = new MultidirSink(true, false));
				}
				sink.targetDirection = collider.gravity.gravity.Dir();
			}
		}
    }

}
public class MultidirSink : Component {
	public Vector2 targetDirection;
	public Vector2 direction;

    public MultidirSink(bool active, bool visible) : base(active, visible)
    {
    }
	public override void Update() {
		direction += (targetDirection - direction) / 20;
	} 
}
