using System;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;

public class SolidHooks
{
    public static void Load()
    {
        IL.Celeste.Solid.GetPlayerClimbing += GetPlayerClimbing;
        IL.Celeste.Solid.GetPlayerOnTop += GetPlayerOnTop;
        On.Celeste.DashSwitch.Update += HeavySquish;
    }
    public static void Unload()
    {
        IL.Celeste.Solid.GetPlayerClimbing -= GetPlayerClimbing;
        IL.Celeste.Solid.GetPlayerOnTop -= GetPlayerOnTop;
        On.Celeste.DashSwitch.Update -= HeavySquish;
    }

    private static void HeavySquish(On.Celeste.DashSwitch.orig_Update orig, DashSwitch self)
    {
        var needsMoveH = self.side != DashSwitch.Sides.Down && self.speedY != 0;
        var gravity = self.Scene.Tracker.GetEntity<Player>()?.Components.Get<GravityComponent>()?.gravity ?? Gravity.Down; 
        if(!self.pressed && (gravity != Gravity.Down || needsMoveH)) {
        	Player playerOnTop = self.GetPlayerOnTop();
            var onTop = gravity.Dir() == self.pressDirection;
    		if (onTop && playerOnTop != null)
    		{
    			if (playerOnTop.Holding != null)
    			{
    				self.OnDashed(playerOnTop, self.pressDirection);
    			}
    			else
    			{
    				if (self.speedY < 0f)
    				{
    					self.speedY = 0f;
    				}
    				self.speedY = Calc.Approach(self.speedY, 70f, 200f * Engine.DeltaTime);
                    var target = self.pressedTarget - self.pressDirection * 6f;
    				self.MoveTowardsY(target.Y, self.speedY * Engine.DeltaTime);
                    self.MoveTowardsX(target.X, self.speedY * Engine.DeltaTime);
    				if (!self.playerWasOn)
    				{
    					Audio.Play("event:/game/05_mirror_temple/button_depress", self.Position);
    				}
    			}
    		}
    		else if (playerOnTop == null)
    		{
    			if (self.speedY > 0f)
    			{
    				self.speedY = 0f;
    			}
    			self.speedY = Calc.Approach(self.speedY, -150f, 200f * Engine.DeltaTime);
                var target = self.pressedTarget - self.pressDirection * 8f;
				self.MoveTowardsY(target.Y, -self.speedY * Engine.DeltaTime);
                self.MoveTowardsX(target.X, -self.speedY * Engine.DeltaTime);
    			if (self.playerWasOn)
    			{
    				Audio.Play("event:/game/05_mirror_temple/button_return", self.Position);
    			}
    		}
    		self.playerWasOn = playerOnTop != null && onTop;
        } else {
            orig(self);
        }
    }

    private static Vector2 CorrectOffset(Vector2 v, Solid solid)
    {
        Player player = solid.Scene.Tracker.GetEntity<Player>();
        if (player != null && player.Collider is TransformCollider collider)
        {
            return collider.gravity.gravity.Dir();
        }
        return v;
    }
    private static void GetPlayerOnTop(ILContext il)
    {
        var cursor = new ILCursor(il);
        try
        {
            cursor.GotoNext(
                MoveType.After,
                i => i.MatchCall<Vector2>("get_UnitY")
            );
            cursor.EmitLdarg0();
            cursor.EmitDelegate(CorrectOffset);
        }
        catch (Exception e)
        {
            Logger.Warn("4WG", $"GetPlayerOnTop hook failed {e}");
        }
    }
    private static Vector2 CorrectOffsetClimb(Vector2 v, Player player)
    {
        if (player.Collider is TransformCollider collider)
        {
            return v.Rotate(collider.gravity.gravity);
        }
        return v;
    }
    private static void GetPlayerClimbing(ILContext il)
    {
        var cursor = new ILCursor(il);
        try
        {
            for (int i = 0; i < 2; i++)
            {
                cursor.GotoNext(
                    MoveType.After,
                    i => i.MatchCall<Vector2>("get_UnitX")
                );
                cursor.EmitLdloc1();
                cursor.EmitDelegate(CorrectOffsetClimb);
            }
        }
        catch (Exception e)
        {
            Logger.Warn("4WG", $"GetPlayerClimbing hook failed: {e}");
        }
    }
}
