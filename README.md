The core idea is that the player moves normally, with a collider wrapper that translates their movement so collisions are processed as if the player is where they are supposed to be. Then at the end of the frame (But also at other times, im working on something to make this easier) the movement the player has done since the last recored point is rotated around that point.

This way all collisions directly with the collider are handled, and you dont need to touch the player movement much at all (The main change I've made was to change point checks in SlipCheck)

So then most issues could be resolved by controlling whether the player is in a world view, where the speed is adjusted to what it should be, and the player isn't tracked by the collider (So they can be pushed by solids etc) or whether its in a player view, where movement is later rotated.

I'm working on cleaning up this but the immediate plan at the moment is to ensure that the view is correct by hooking Player.Update for PlayerCollider checks and DashCollide checks.
This should handle spikes and kevins at the very least.
