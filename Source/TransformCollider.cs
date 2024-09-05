
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

public class TransformCollider : Collider {
	public Hitbox source;
	public Hitbox hitbox;
	public Vector2 offset;
	public Vector2 move;
	public Vector2 moveFrom;
	public Vector2 checkFrom;
	public Vector2 checkAt;
	public GravityComponent gravity;
    public override float Width { 
		get {
			if(gravity.Track) {
				return source.Width;
			} else {
				return hitbox.Width;
			}
		} 
		set {} 
	}
    public override float Height { 
		get {
			if(gravity.Track) {
				return source.Height;
			} else {
				return hitbox.Height;
			}
		} 
		set {} 
	}
	public override float Top {
		get {
			if(gravity.Track) {
				return source.Top;
			} else {
				return hitbox.Top;
			}
		}
		set {} 
	}
	public override float Bottom {
		get {
			if(gravity.Track) {
				return source.Bottom;
			} else {
				return hitbox.Bottom;
			}
		}
		set {} 
	}
	public override float Left {
		get {
			if(gravity.Track) {
				return source.Left;
			} else {
				return hitbox.Left;
			}
		}
		set {} 
	}
	public override float Right {
		get {
			if(gravity.Track) {
				return source.Right;
			} else {
				return hitbox.Right;
			}
		}
		set {} 
	}
	public TransformCollider(Hitbox hitbox, Hitbox source, GravityComponent gravity) {
		this.hitbox = hitbox;
		this.gravity = gravity;
		this.source = source;
		offset = hitbox.Position;
	}
	public override void Added(Entity entity) {
		base.Added(entity);
		hitbox.Entity = Entity;
	}
	public void Update() {
		hitbox.Position = offset;
		if(!gravity.Track) return;
		var dif = Entity.Position - gravity.origin;
		hitbox.Position += -dif + dif.Rotate(gravity.gravity);
	}
    public override Collider Clone()
    {
		return new TransformCollider((Hitbox)hitbox.Clone(), source, gravity);
    }

    public override bool Collide(Circle circle) {
		Update();
		return hitbox.Collide(circle);
	}
	public override bool Collide(ColliderList list) {
		Update();
		return hitbox.Collide(list);	
	}

    public override bool Collide(Vector2 point)
    {
		Update();
		return hitbox.Collide(point);
    }

    public override bool Collide(Rectangle rect)
    {
		Update();
		return hitbox.Collide(rect);
    }

    public override bool Collide(Vector2 from, Vector2 to)
    {
		Update();
		return hitbox.Collide(from, to);
    }

    public override bool Collide(Hitbox box)
    {
		Update();
		if(box.Entity != null && box.Entity.Get<JumpThruComponent>() != null) {
			return false;
		}
		if(box.Entity != null && box.Entity is JumpThruSolid jumpthru) {
			// If a check, then must have been outside on the right side
			var player = (Player)Entity;
			var hit = box.Collide(hitbox);
			if(checkAt != checkFrom) {
				var position = player.Position;
				player.Position = checkFrom;
				Update();
				var outside = !box.Collide(hitbox);
				var side = jumpthru.direction switch {
					Direction.Up => player.Bottom <= box.Top,
					Direction.Right => player.Left >= box.Right,
					Direction.Down => player.Top >= box.Bottom,
					_ => player.Right <= box.Left
				};
				return hit && outside && side;
			} else if(move != Vector2.Zero) { // If it is a player movement, must be going in the right direction, and start outside
				var speed = move;
				if(gravity.currentView == View.Player) {
					speed = move.Rotate(gravity.gravity);
				}
				var speedCheck = jumpthru.direction switch {
					Direction.Up => speed.Y >= 0,
					Direction.Right => speed.X <= 0,
					Direction.Down => speed.Y <= 0,
					_ => speed.X >= 0
				};
				var position = player.Position;
				player.Position = moveFrom;
				Update();
				var outside = box.Collide(hitbox); 
				player.Position = position;
				return hit && outside && speedCheck;
			} else if(jumpthru.canPush) { // If it is a platform movement, platform must be going in the right direction and player started outside			
				var position = jumpthru.Position;
				jumpthru.Position = jumpthru.moveFrom;
				var outside = !box.Collide(hitbox);
				jumpthru.Position = position;
				return hit && outside;
			}
			return hit;
		}
		return hitbox.Collide(box);
    }

    public override bool Collide(Grid grid)
    {
		Update();
		return hitbox.Collide(grid);
    }
    public override void Render(Camera camera, Color color)
    {
		Draw.HollowRect(hitbox.AbsoluteX, hitbox.AbsoluteY, hitbox.Width, hitbox.Height, Color.Red);
    }
}
