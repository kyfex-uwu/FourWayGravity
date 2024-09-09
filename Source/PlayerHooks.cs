using System;
using System.Linq;
using System.Reflection;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

public class PlayerHooks
{
    static ILHook hook_orig_Update;
    static ILHook hook_orig_UpdateSprite;
    static ILHook hook_Dash_Coroutine;
    static Hook hook_Ducking_get;
    static Type dashCoroutineType;
    delegate bool orig_Ducking_get(Player self);
    public static void Load()
    {
        hook_orig_Update = new ILHook(typeof(Player).GetMethod("orig_Update"), Update);
        hook_orig_UpdateSprite = new ILHook(
            typeof(Player)
                .GetMethod("orig_UpdateSprite", BindingFlags.NonPublic | BindingFlags.Instance),
             PointCheckHook
        );
        hook_Ducking_get = new Hook(typeof(Player).GetProperty("Ducking").GetGetMethod(), Ducking_get_fix);
        On.Celeste.Player.ctor += ConstructorHook;
        On.Celeste.Player.ExplodeLaunch_Vector2_bool_bool += ExplodeLaunch;
        On.Celeste.Player.BoostUpdate += BoostUpdateHook;
        On.Celeste.Player.BoostEnd += BoostEndHook;
        On.Celeste.Player.Pickup += PickupHook;
        var stateMachineTarget = typeof(Player).GetMethod("DashCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget();
        hook_Dash_Coroutine = new ILHook(
            stateMachineTarget,
            DashCoroutineHook);
        IL.Celeste.Player.SlipCheck += PointCheckHook;
        IL.Celeste.Player.ClimbCheck += PointCheckHook;
        IL.Celeste.Player.OnCollideH += DashCollideHook;
        IL.Celeste.Player.OnCollideV += DashCollideHook;
        IL.Celeste.Player.UpdateCarry += UpdateCarryHook;
    }

    public static void Unload()
    {
        hook_orig_Update?.Dispose();
        hook_orig_UpdateSprite?.Dispose();
        hook_Ducking_get?.Dispose();
        hook_Dash_Coroutine?.Dispose();
        On.Celeste.Player.ctor -= ConstructorHook;
        On.Celeste.Player.ExplodeLaunch_Vector2_bool_bool -= ExplodeLaunch;
        On.Celeste.Player.BoostUpdate -= BoostUpdateHook;
        On.Celeste.Player.BoostEnd -= BoostEndHook;
        On.Celeste.Player.Pickup -= PickupHook;
        IL.Celeste.Player.SlipCheck -= PointCheckHook;
        IL.Celeste.Player.ClimbCheck -= PointCheckHook;
        IL.Celeste.Player.OnCollideH -= DashCollideHook;
        IL.Celeste.Player.OnCollideV -= DashCollideHook;
        IL.Celeste.Player.UpdateCarry -= UpdateCarryHook;
    }
    private static void ConstructorHook(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode)
    {
        orig(self, position, spriteMode);
        self.Add(GravityEntity.ForPlayer());
    }
    public static Vector2 PointCheckCorrection(Vector2 point, Player player)
    {
        if (player.Collider is TransformCollider collider)
        {
            var origin = collider.gravity.origin;
            var corrected = (point - origin).Rotate(collider.gravity.gravity) + origin;
            corrected += collider.gravity.gravity switch
            {
                Gravity.Up => -new Vector2(1f),
                Gravity.Right => -Vector2.UnitY,
                _ => Vector2.Zero
            }; // Idk why this is necessary tbh probably good to investigate later
            return corrected;
        }
        return point;
    }
    private static void PointCheckHook(ILContext il)
    {
        var cursor = new ILCursor(il);
        var method = typeof(Level)
            .GetMethod("CollideCheck", new Type[] { typeof(Vector2) })
            .MakeGenericMethod(new Type[] { typeof(Solid) });
        while (cursor.TryGotoNext(
            MoveType.Before,
            i => i.MatchCallvirt(method)
        ))
        {
            cursor.EmitLdarg0();
            cursor.EmitDelegate(PointCheckCorrection);
            cursor.TryGotoNext(
                MoveType.Before,
                i => i.MatchCallvirt(method)
            );
        }
    }
    private static void Update(ILContext il)
    {
        var cursor = new ILCursor(il);
        try
        {
            cursor.GotoNext(i => i.MatchCallvirt<PlayerCollider>("Check"));
            cursor.GotoPrev(
                MoveType.After,
                i => i.MatchCall(typeof(Entity).GetProperty("Collider").GetSetMethod())
            );
            cursor.EmitLdarg0();
            cursor.EmitDelegate(Views.WorldView);
            cursor.GotoNext(
                MoveType.After,
                i => i.MatchCall(typeof(Entity).GetProperty("Collider").GetSetMethod())
            );
            // Pop before the return in the loop
            cursor.EmitLdarg0();
            cursor.EmitDelegate(Views.Pop);
            // Reset collider
            cursor.GotoNext(
                MoveType.After,
                i => i.MatchCall(typeof(Entity).GetProperty("Collider").GetSetMethod())
            );
            cursor.MoveAfterLabels();
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldloc, 14);
            cursor.EmitDelegate(CollideFix);
            cursor.EmitLdarg0();
            cursor.EmitDelegate(Views.Pop);

            cursor.Index = 0;
            cursor.GotoNext(MoveType.Before, i => i.MatchCallvirt<SpeedRing>(nameof(SpeedRing.Init)));
            cursor.GotoPrev(MoveType.Before, i => i.MatchCall<Entity>("get_Center"));
            cursor.EmitLdarg0();
            cursor.EmitDelegate(Views.WorldView);
            cursor.GotoNext(MoveType.After, i => i.MatchCallvirt<SpeedRing>(nameof(SpeedRing.Init)));
            cursor.EmitLdarg0();
            cursor.EmitDelegate(Views.Pop);

            cursor.Index = 0;
            cursor.GotoNext(i => i.MatchCall(typeof(Actor).GetMethod("MoveHExact")));
            cursor.EmitLdarg0();
            cursor.EmitDelegate(Views.WorldView);
            cursor.GotoNext(MoveType.After, i => i.MatchCall(typeof(Actor).GetMethod("MoveVExact")));
            cursor.EmitLdarg0();
            cursor.EmitDelegate(Views.Pop);
        }
        catch (Exception e)
        {
            Logger.Warn("GHGV", $"Update hook failed {e}");
        }
    }
    private static void CollideFix(Player player, Collider tmp)
    {
        if (player.Collider is TransformCollider collider)
        {
            if (collider.source == player.hurtbox)
                player.Collider = tmp;
        }
    }
    private static bool Ducking_get_fix(orig_Ducking_get orig, Player self)
    {
        if (self.Collider is TransformCollider transformCollider)
        {
            return transformCollider.source == self.duckHitbox || transformCollider.source == self.duckHurtbox;
        }
        return orig(self);
    }
    private static Vector2 FixDashDirection(Vector2 direction, Player player)
    {
        if (player.Collider is TransformCollider collider)
        {
            return direction.Rotate(collider.gravity.gravity);
        }
        return direction;
    }
    private static void DashCollideHook(ILContext il)
    {
        var cursor = new ILCursor(il);
        var method = typeof(DashCollision).GetMethod("Invoke");
        while (cursor.TryGotoNext(
            MoveType.Before,
            i => i.MatchCallvirt(method)
        ))
        {
            cursor.EmitLdarg0();
            cursor.EmitDelegate(FixDashDirection);
            cursor.EmitLdarg0();
            cursor.EmitDelegate(Views.WorldView);
            cursor.TryGotoNext(
                MoveType.After,
                i => i.MatchCallvirt(method)
            );
            cursor.EmitLdarg0();
            cursor.EmitDelegate(Views.Pop);
        }
    }
    private static Vector2 FixSlashDirection(Vector2 direction, Player player)
    {
        if (player.Collider is TransformCollider collider)
        {
            return direction.Rotate(collider.gravity.gravity);
        }
        return direction;
    }
    private static void DashCoroutineHook(ILContext il)
    {
        var cursor = new ILCursor(il);
        var methods = typeof(Monocle.Calc).GetMethods();
        var method = methods.Where(method => method.GetParameters().Length == 1 && method.Name == "Angle").First();
        while (cursor.TryGotoNext(MoveType.Before,
            i => i.MatchCall(method)
        ))
        {
            cursor.EmitLdloc1();
            cursor.EmitDelegate(FixDashDirection);
            cursor.GotoNext(MoveType.After, i => i.MatchCall(method));
        }
    }
    private static Vector2 ExplodeLaunch(On.Celeste.Player.orig_ExplodeLaunch_Vector2_bool_bool orig, Player self, Vector2 from, bool snapUp, bool sidesOnly)
    {
        Views.EntityView(self);
        if (self.Collider is TransformCollider collider)
        {
            from = collider.gravity.origin + (from - collider.gravity.origin).RotateInv(collider.gravity.gravity);
            if (collider.gravity.gravity == Gravity.Left || collider.gravity.gravity == Gravity.Right)
            {
                sidesOnly = false;
            }
        }
        var result = orig(self, from, snapUp, sidesOnly);
        Views.Pop(self);
        return result;
    }
    private static int BoostUpdateHook(On.Celeste.Player.orig_BoostUpdate orig, Player self)
    {
        Views.WorldView(self);
        var result = orig(self);
        Views.Pop(self);
        return result;
    }

    private static void UpdateCarryHook(ILContext il)
    {
        try
        {
            var cursor = new ILCursor(il);
            cursor.GotoNext(i => i.MatchCallvirt<Holdable>(nameof(Holdable.Carry)));
            cursor.EmitLdarg0();
            cursor.EmitDelegate(FixCarryPosition);
        }
        catch (Exception e)
        {
            Logger.Warn("GHGV", $"UpdateCarry hook failed to load {e}");
        }
    }
    private static Vector2 FixCarryPosition(Vector2 position, Player player)
    {
        if (player.Collider is TransformCollider transformCollider)
        {
            var before = player.Position;
            Views.WorldView(player);
            var playerPos = player.Position;
            var offset = position - before;
            Views.Pop(player);
            return playerPos + offset.Rotate(transformCollider.gravity.gravity);
        }
        return position;
    }
    private static bool PickupHook(On.Celeste.Player.orig_Pickup orig, Player self, Holdable pickup)
    {
        var result = orig(self, pickup);
        if (result && pickup.Entity.Components.Get<GravityEntity>() != null && self.Collider is TransformCollider transformCollider)
        {
            pickup.Entity.Position = pickup.Entity.Position.RotateAround(self.Position, transformCollider.gravity.gravity.Inv());
            GravityComponent.Set(pickup.Entity, transformCollider.gravity.gravity);
        }
        return result;
    }
    private static void BoostEndHook(On.Celeste.Player.orig_BoostEnd orig, Player self)
    {
        Views.WorldView(self);
        orig(self);
        Views.Pop(self);
    }
}
