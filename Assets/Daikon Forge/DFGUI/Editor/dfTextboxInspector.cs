/* Copyright 2013 Daikon Forge */
using UnityEngine;
using UnityEditor;

using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

[CanEditMultipleObjects]
[CustomEditor( typeof( dfTextbox ) )]
public class dfTextboxInspector : dfControlInspector
{

	private static Dictionary<int, bool> foldouts = new Dictionary<int, bool>();

	protected override bool OnCustomInspector()
	{

		var control = target as dfTextbox;
		if( control == null )
			return false;

		dfEditorUtil.DrawSeparator();

		if( !isFoldoutExpanded( foldouts, "Textbox Properties", true ) )
			return false;

		dfEditorUtil.LabelWidth = 110f;

		SelectTextureAtlas( "Atlas", control, "Atlas", false, true );
		if( !dfAtlas.Equals( control.Atlas, control.GUIManager.DefaultAtlas ) )
		{
			EditorGUILayout.HelpBox( "This control does not use the same Texture Atlas as the View, which will result in an additional draw call.", MessageType.Info );
		}

		using( dfEditorUtil.BeginGroup( "Data" ) )
		{

			var text = EditorGUILayout.TextField( "Text", control.Text );
			if( text != control.Text )
			{
				dfEditorUtil.MarkUndo( control, "Change Text" );
				control.Text = text;
			}

		}

		using( dfEditorUtil.BeginGroup( "General" ) )
		{

			var readOnly = EditorGUILayout.Toggle( "Read Only", control.ReadOnly );
			if( readOnly != control.ReadOnly )
			{
				dfEditorUtil.MarkUndo( control, "Change ReadOnly property" );
				control.ReadOnly = readOnly;
			}

			if( !readOnly )
			{

				var selectOnFocus = EditorGUILayout.Toggle( "Focus Select", control.SelectOnFocus );
				if( selectOnFocus != control.SelectOnFocus )
				{
					dfEditorUtil.MarkUndo( control, "Change Select On Focus property" );
					control.SelectOnFocus = selectOnFocus;
				}

			}

			var asPassword = EditorGUILayout.Toggle( "Password", control.IsPasswordField );
			if( asPassword != control.IsPasswordField )
			{
				dfEditorUtil.MarkUndo( control, "Change Textword Password value" );
				control.IsPasswordField = asPassword;
			}

			if( asPassword )
			{
				var passChar = EditorGUILayout.TextField( "Password Char", control.PasswordCharacter );
				if( passChar != control.PasswordCharacter )
				{
					dfEditorUtil.MarkUndo( control, "Change password character" );
					control.PasswordCharacter = passChar;
				}
			}

			var cursorWidth = EditorGUILayout.IntField( "Cursor Width", control.CursorWidth );
			if( cursorWidth != control.CursorWidth )
			{
				dfEditorUtil.MarkUndo( control, "Change Cursor Width" );
				control.CursorWidth = cursorWidth;
			}

			var maxLength = EditorGUILayout.IntField( "Max Length", control.MaxLength );
			if( maxLength != control.MaxLength )
			{
				dfEditorUtil.MarkUndo( control, "Change Max Length" );
				control.MaxLength = maxLength;
			}

		}

		using( dfEditorUtil.BeginGroup( "Mobile" ) )
		{

			var useKeyboard = EditorGUILayout.Toggle( "Show Keyboard", control.UseMobileKeyboard );
			if( useKeyboard != control.UseMobileKeyboard )
			{
				dfEditorUtil.MarkUndo( control, "Change 'Show Keyboard' property" );
				control.UseMobileKeyboard = useKeyboard;
			}

			if( useKeyboard )
			{

				var triggerType = (dfMobileKeyboardTrigger)EditorGUILayout.EnumPopup( "Trigger", control.MobileKeyboardTrigger );
				if( triggerType != control.MobileKeyboardTrigger )
				{
					dfEditorUtil.MarkUndo( control, "Change mobile keyboard trigger" );
					control.MobileKeyboardTrigger = triggerType;
				}

				var keyboardType = (TouchScreenKeyboardType)EditorGUILayout.EnumPopup( "Keyboard Type", control.MobileKeyboardType );
				if( keyboardType != control.MobileKeyboardType )
				{
					dfEditorUtil.MarkUndo( control, "Change mobile keyboard type" );
					control.MobileKeyboardType = keyboardType;
				}

				var useAutoCorrect = EditorGUILayout.Toggle( "Auto-Correct", control.MobileAutoCorrect );
				if( useAutoCorrect != control.MobileAutoCorrect )
				{
					dfEditorUtil.MarkUndo( control, "Toggle Auto-Correct");
					control.MobileAutoCorrect = useAutoCorrect;
				}

				var hideInputField = EditorGUILayout.Toggle( "Hide Input", control.HideMobileInputField );
				if( hideInputField != control.HideMobileInputField )
				{
					dfEditorUtil.MarkUndo( control, "Toggle 'Hide Input Field'" );
					control.HideMobileInputField = hideInputField;
				}

			}
		
		}

		using( dfEditorUtil.BeginGroup( "Text Appearance" ) )
		{

			SelectFontDefinition( "Font", control.Atlas, control, "Font", true, true );

			if( control.Font == null )
				return false;

			var align = (TextAlignment)EditorGUILayout.EnumPopup( "Text Align", control.TextAlignment );
			if( align != control.TextAlignment )
			{
				dfEditorUtil.MarkUndo( control, "Change Text Alignment" );
				control.TextAlignment = align;
			}

			var textColor = EditorGUILayout.ColorField( "Text Color", control.TextColor );
			if( textColor != control.TextColor )
			{
				dfEditorUtil.MarkUndo( control, "Change Text Color" );
				control.TextColor = textColor;
			}

			var textScale = EditorGUILayout.FloatField( "Text Scale", control.TextScale );
			if( textScale != control.TextScale )
			{
				dfEditorUtil.MarkUndo( control, "Change Text Scale" );
				control.TextScale = textScale;
			}

			var scaleMode = (dfTextScaleMode)EditorGUILayout.EnumPopup( "Auto Scale", control.TextScaleMode );
			if( scaleMode != control.TextScaleMode )
			{
				dfEditorUtil.MarkUndo( control, "Change Text Scale Mode" );
				control.TextScaleMode = scaleMode;
			}

			var padding = dfEditorUtil.EditPadding( "Padding", control.Padding );
			if( padding != control.Padding )
			{
				dfEditorUtil.MarkUndo( control, "Change Textbox Padding" );
				control.Padding = padding;
			}

			var selectionBack = EditorGUILayout.ColorField( "Select Color", control.SelectionBackgroundColor );
			if( selectionBack != control.SelectionBackgroundColor )
			{
				dfEditorUtil.MarkUndo( control, "Change Selection Background Text Color" );
				control.SelectionBackgroundColor = selectionBack;
			}

			SelectSprite( "Blank Texture", control.Atlas, control, "SelectionSprite" );
			if( string.IsNullOrEmpty( control.SelectionSprite ) )
			{
				EditorGUILayout.HelpBox( "This control needs a blank texture to use for rendering the selection background and cursor", MessageType.Info );
			}

			var shadow = EditorGUILayout.Toggle( "Shadow Effect", control.Shadow );
			if( shadow != control.Shadow )
			{
				dfEditorUtil.MarkUndo( control, "Change Shadow Effect" );
				control.Shadow = shadow;
			}

			if( shadow )
			{

				var shadowColor = EditorGUILayout.ColorField( "Shadow Color", control.ShadowColor );
				if( shadowColor != control.ShadowColor )
				{
					dfEditorUtil.MarkUndo( control, "Change Shadow Color" );
					control.ShadowColor = shadowColor;
				}

				var shadowOffset = dfEditorUtil.EditInt2( "Shadow Offset", "X", "Y", control.ShadowOffset );
				if( shadowOffset != control.ShadowOffset )
				{
					dfEditorUtil.MarkUndo( control, "Change Shadow Color" );
					control.ShadowOffset = shadowOffset;
				}

				dfEditorUtil.LabelWidth = 120f;

			}

		}

		using( dfEditorUtil.BeginGroup( "Images" ) )
		{
			
			SelectSprite( "Normal", control.Atlas, control, "BackgroundSprite" );
			SelectSprite( "Focus", control.Atlas, control, "FocusSprite", false );
			SelectSprite( "Hover", control.Atlas, control, "HoverSprite", false );
			SelectSprite( "Disabled", control.Atlas, control, "DisabledSprite", false );
		}

		return true;

	}

}
