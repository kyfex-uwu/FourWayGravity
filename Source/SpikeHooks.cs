using Celeste;

public class SpikeHooks {
	public static void Load() {
		On.Celeste.Spikes.OnCollide += OnCollide;
	}
    public static void Unload() {
		On.Celeste.Spikes.OnCollide -= OnCollide;		
	}
    private static void OnCollide(On.Celeste.Spikes.orig_OnCollide orig, Spikes self, Player player)
    {
		if(player.Collider is TransformCollider collider) {
			var speed = player.Speed;
			player.Speed = player.Speed.Rotate(collider.gravity.gravity);
			orig(self, player);
			player.Speed = speed;
		}
    }

}
