
using System;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

[CustomEntity("FourWayGravity/GravityField")]
public class GravityField : Entity
{
    float time = 0;
    HashSet<Entity> inside = new();
    public GravityField(EntityData data, Vector2 offset)
    {
        Position = data.Position + offset;
        Collider = new Hitbox(data.Width, data.Height);
    }
    public override void Awake(Scene scene)
    {
        GravityArrow.GetArrows(this);
    }
    public override void Update()
    {
        time += Engine.DeltaTime;
        foreach (var gravity in Scene.Tracker.GetComponents<GravityEntity>())
        {
            if (!inside.Contains(gravity.Entity) && gravity.Entity.CollideCheck(this))
            {
                if(GravityArrow.ApplyArrows(this, gravity.Entity)) {
                    inside.Add(gravity.Entity);
                }
            }
        }
        foreach (var entity in inside)
        {
            if (!CollideCheck(entity))
            {
                inside.Remove(entity);
            }
        }
    }
    public override void Render()
    {
        for (int y = 0; y < Height; y++)
        {
            var wave = (float)Math.Sin(time * 3 + y / 4) * 2;
            Draw.Rect(Position.X + wave, Position.Y + y, Width, 1, Color.White * 0.2f);
        }
        for (int x = 0; x < Width; x++)
        {
            var wave = (float)Math.Sin(time * 3 + x / 4) * 2;
            Draw.Rect(Position.X + x, Position.Y + wave, 1, Height, Color.White * 0.2f);
        }
        base.Render();
    }
}
