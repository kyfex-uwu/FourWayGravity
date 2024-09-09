using System;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod;
using Monocle;

public class BadelineChaserHooks {
	public static void Load() {
		On.Celeste.Player.UpdateChaserStates += UpdateChaserStates;
		On.Celeste.BadelineOldsite.Added += BadelineGravityEntity;
		On.Celeste.BadelineOldsite.Update += BadelineUpdate;
	}
	public static void Unload() {
		On.Celeste.Player.UpdateChaserStates -= UpdateChaserStates;
		On.Celeste.BadelineOldsite.Added -= BadelineGravityEntity;
		On.Celeste.BadelineOldsite.Update -= BadelineUpdate;
	}

    private static void BadelineUpdate(On.Celeste.BadelineOldsite.orig_Update orig, Celeste.BadelineOldsite self)
    {
		orig(self);
		if(self.following && self.player != null && !self.player.Dead) {
			var gravityStates = self.player.Components.Get<ChaserGravityStates>();
			var timeAgo = self.followBehindTime + self.followBehindIndexDelay;
			var gravity = Gravity.Down;
			if(gravityStates != null) {
				foreach (var state in gravityStates.states)
				{
					float ago = self.Scene.TimeActive - state.time;
					if (ago <= timeAgo)
					{
						gravity = state.gravity;
						break;
					}
				}
			}
			self.Sprite.Rotation = gravity.Angle();
		}
    }

    private static void BadelineGravityEntity(On.Celeste.BadelineOldsite.orig_Added orig, Celeste.BadelineOldsite self, Scene scene)
    {
		orig(self, scene);
    }

    private static void UpdateChaserStates(On.Celeste.Player.orig_UpdateChaserStates orig, Celeste.Player self)
    {
		var prev = self.ChaserStates.Count;
		orig(self);
		var states = self.Components.Get<ChaserGravityStates>();
		if(states == null) {
			states = new ChaserGravityStates();
			self.Add(states);
		}
		if(states.states.Count > 0) {
			for(int i = 0; i < prev - self.ChaserStates.Count + 1; i++)
			{
				states.states.RemoveAt(0);
			}
		}
		if(self.Collider is TransformCollider transformCollider) {
			states.states.Add(new GravityState(
				self.ChaserStates.Last().TimeStamp,
				transformCollider.gravity.gravity	
			));
			return;
		}
		states.states.Add(new GravityState(
			self.ChaserStates.Last().TimeStamp,
			Gravity.Down
		));
    }
}
public struct GravityState {
	public float time;
	public Gravity gravity;
	public GravityState(float time, Gravity gravity) {
		this.time = time;
		this.gravity = gravity;
	}
	
}
public class ChaserGravityStates : Component {
	public List<GravityState> states = new ();

    public ChaserGravityStates() : base(false, false)
    {
    }
}
