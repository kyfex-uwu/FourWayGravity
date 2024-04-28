using System;
using System.Collections.Generic;
using System.Reflection;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

public class PlayerHairHooks {
	public static void Load() {
		IL.Celeste.PlayerHair.Render += PlayerHairRender;
		IL.Celeste.PlayerHair.AfterUpdate += PlayerHairAfterUpdate;
	}
    public static void Unload() {
		IL.Celeste.PlayerHair.Render -= PlayerHairRender;
		IL.Celeste.PlayerHair.AfterUpdate -= PlayerHairAfterUpdate;
	}
    private static void PlayerHairAfterUpdate(ILContext il)
    {
		var cursor = new ILCursor(il);
		if(cursor.TryGotoNext(
			MoveType.After,
			i => i.MatchCallvirt(typeof(List<Vector2>).GetProperty("Item").GetSetMethod())
		)) {
			cursor.EmitLdarg0();
			cursor.EmitDelegate(OffsetHair);
		}
    }
	private static void OffsetHair(PlayerHair hair) {
		Vector2 vector = hair.Sprite.HairOffset * new Vector2((float)hair.Facing, 1f);
		hair.Nodes[0] = hair.Sprite.RenderPosition;
		var offset = Vector2.UnitY * -9f * hair.Sprite.Scale.Y + vector;
		if(hair.Entity.Collider is TransformCollider collider) {
			hair.Nodes[0] += offset.Rotate(collider.gravity.gravity);
		} else {
			hair.Nodes[0] += offset;
		}
	}
    private static void PlayerHairRender(ILContext il)
    {
		var cursor = new ILCursor(il);
		for(int i = 0; i < 5; i++) {
			if(cursor.TryGotoNext(
				MoveType.Before,
				i => i.MatchCallvirt<MTexture>("Draw")
			)) {
				cursor.Next.Operand = il.Import(typeof(MTexture).GetMethod(
					"Draw",
					new Type[] {
						typeof(Vector2), typeof(Vector2), typeof(Color), typeof(Vector2), typeof(float)
					}
				));
				cursor.EmitLdarg0();
				cursor.EmitDelegate(GetAngle);
				cursor.TryGotoNext(
					MoveType.Before,
					i => i.MatchCallvirt<MTexture>("Draw")
				);
			}
		}
    }
	private static float GetAngle(PlayerHair hair) {
		if(hair.Sprite.Entity.Collider is TransformCollider collider) {
			return collider.gravity.gravity.Angle();
		}
		return 0f;
	}
}
