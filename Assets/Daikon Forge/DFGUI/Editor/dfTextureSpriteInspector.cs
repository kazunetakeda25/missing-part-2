/* Copyright 2013 Daikon Forge */
using UnityEngine;
using UnityEditor;

using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

[CanEditMultipleObjects]
[CustomEditor( typeof( dfTextureSprite ) )]
public class dfTextureSpriteInspector : dfControlInspector
{

	private static Dictionary<int, bool> foldouts = new Dictionary<int, bool>();

	protected override bool OnCustomInspector()
	{

		var control = target as dfTextureSprite;
		if( control == null )
			return false;

		dfEditorUtil.DrawSeparator();

		if( !isFoldoutExpanded( foldouts, "Texture Properties", true ) )
			return false;

		dfEditorUtil.LabelWidth = 100f;

		using( dfEditorUtil.BeginGroup( "Appearance" ) )
		{

			var texture = EditorGUILayout.ObjectField( "Texture", control.Texture, typeof( Texture2D ), false ) as Texture2D;
			if( texture != control.Texture )
			{
				dfEditorUtil.MarkUndo( control, "Assign texture" );
				control.Texture = texture;
			}

			var material = EditorGUILayout.ObjectField( "Material", control.Material, typeof( Material ), false ) as Material;
			if( material != control.Material )
			{
				dfEditorUtil.MarkUndo( control, "Assign material" );
				control.Material = material;
			}

		}

		using( dfEditorUtil.BeginGroup( "Flip" ) )
		{

			var flipHorz = EditorGUILayout.Toggle( "Flip Horz", ( control.Flip & dfSpriteFlip.FlipHorizontal ) == dfSpriteFlip.FlipHorizontal );
			var flipVert = EditorGUILayout.Toggle( "Flip Vert", ( control.Flip & dfSpriteFlip.FlipVertical ) == dfSpriteFlip.FlipVertical );
			var flip = dfSpriteFlip.None;
			if( flipHorz ) flip |= dfSpriteFlip.FlipHorizontal;
			if( flipVert ) flip |= dfSpriteFlip.FlipVertical;
			if( flip != control.Flip )
			{
				dfEditorUtil.MarkUndo( control, "Change Sprite Flip" );
				control.Flip = flip;
			}

		}

		using( dfEditorUtil.BeginGroup( "Fill" ) )
		{

			var fillType = (dfFillDirection)EditorGUILayout.EnumPopup( "Fill Direction", control.FillDirection );
			if( fillType != control.FillDirection )
			{
				dfEditorUtil.MarkUndo( control, "Change Sprite Fill Direction" );
				control.FillDirection = fillType;
			}

			var fillAmount = EditorGUILayout.Slider( "Fill Amount", control.FillAmount, 0, 1 );
			if( !Mathf.Approximately( fillAmount, control.FillAmount ) )
			{
				dfEditorUtil.MarkUndo( control, "Change Sprite Fill Amount" );
				control.FillAmount = fillAmount;
			}

			var invert = EditorGUILayout.Toggle( "Invert Fill", control.InvertFill );
			if( invert != control.InvertFill )
			{
				dfEditorUtil.MarkUndo( control, "Change Sprite Invert Fill" );
				control.InvertFill = invert;
			}

		}

		return true;

	}

}

