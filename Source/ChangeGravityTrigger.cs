using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.FourWayGravity;

[CustomEntity("FourWayGravity/ChangeGravityTrigger")]
public class ChangeGravityTrigger : Trigger {
    public readonly Gravity direction;
    public ChangeGravityTrigger(EntityData data, Vector2 offset):base(data, offset) {
        this.direction = data.Enum("direction", Gravity.Down);
    }

    public override void OnEnter(Player player) {
        base.OnEnter(player);
        GravityComponent.Set(player, this.direction);
    }
}