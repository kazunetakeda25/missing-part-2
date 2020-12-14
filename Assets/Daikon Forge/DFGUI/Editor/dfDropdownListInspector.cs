/* Copyright 2013 Daikon Forge */
using UnityEngine;
using UnityEditor;

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

[CanEditMultipleObjects]
[CustomEditor( typeof( dfDropdown ) )]
public class dfDropdownListInspector : dfControlInspector
{
	
	private static Dictionary<int, bool> foldouts = new Dictionary<int, bool>();

	protected override bool OnCustomInspector()
	{

		var control = target as dfDropdown;
		if( control == null )
			return false;

		dfEditorUtil.DrawSeparator();

		if( !isFoldoutExpanded( foldouts, "DropdownList Properties", true ) )
			return false;

		dfEditorUtil.LabelWidth = 110f;

		using( dfEditorUtil.BeginGroup( "Dropdown List" ) )
		{

			var trigger = EditorGUILayout.ObjectField( "Trigger", control.TriggerButton, typeof( dfControl ), true ) as dfControl;
			if( trigger != control.TriggerButton )
			{
				dfEditorUtil.MarkUndo( control, "Change trigger button" );
				control.TriggerButton = trigger;
			}

			var triggerOnMouse = EditorGUILayout.Toggle( "Click Open", control.OpenOnMouseDown );
			if( triggerOnMouse != control.OpenOnMouseDown )
			{
				dfEditorUtil.MarkUndo( control, "Change trigger action" );
				control.OpenOnMouseDown = triggerOnMouse;
			}

			var index = EditorGUILayout.IntSlider( "Selected Index", control.SelectedIndex, -1, control.Items.Length - 1 );
			if( index != control.SelectedIndex )
			{
				dfEditorUtil.MarkUndo( control, "Change Selected Index" );
				control.SelectedIndex = index;
			}

		}

		using( dfEditorUtil.BeginGroup( "Images" ) )
		{

			SelectTextureAtlas( "Atlas", control, "Atlas", false, true );

			SelectSprite( "Normal", control.Atlas, control, "BackgroundSprite" );
			SelectSprite( "Focus", control.Atlas, control, "FocusSprite", false );
			SelectSprite( "Hover", control.Atlas, control, "HoverSprite", false );
			SelectSprite( "Disabled", control.Atlas, control, "DisabledSprite", false );

		}

		using( dfEditorUtil.BeginGroup( "Text Appearance" ) )
		{

			SelectFontDefinition( "Font", control.Atlas, control, "Font", true, true );

			if( control.Font == null )
				return false;

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

			var padding = dfEditorUtil.EditPadding( "Padding", control.TextFieldPadding );
			if( padding != control.TextFieldPadding )
			{
				dfEditorUtil.MarkUndo( control, "Change control Padding" );
				control.TextFieldPadding = padding;
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

			}

		}

		using( dfEditorUtil.BeginGroup( "List Appearance" ) )
		{
			SelectSprite( "List Background", control.Atlas, control, "ListBackground" );
			SelectSprite( "Item Highlight", control.Atlas, control, "ItemHighlight", false );
			SelectSprite( "Item Hover", control.Atlas, control, "ItemHover", false );
		}

		using( dfEditorUtil.BeginGroup( "List Options" ) )
		{

			dfPrefabSelectionDialog.FilterCallback filterScrollbar = ( item ) =>
			{
				var scrollbar = item.GetComponent<dfScrollbar>();
				var scrollAtlas = scrollbar.Atlas;
				return dfAtlas.Equals( control.Atlas, scrollAtlas );
			};

			SelectPrefab<dfScrollbar>( "Scroll Bar", control, "ListScrollbar", null, filterScrollbar );

			var itemHeight = EditorGUILayout.IntField( "Item Height", control.ItemHeight );
			if( itemHeight != control.ItemHeight )
			{
				dfEditorUtil.MarkUndo( control, "Adjust List Item Height" );
				control.ItemHeight = itemHeight;
			}

			var listPosition = (dfDropdown.PopupListPosition)EditorGUILayout.EnumPopup( "List Position", control.ListPosition );
			if( listPosition != control.ListPosition )
			{
				dfEditorUtil.MarkUndo( control, "Change list position" );
				control.ListPosition = listPosition;
			}

			var maxWidth = EditorGUILayout.IntField( "Max List Width", control.MaxListWidth );
			if( maxWidth != control.MaxListWidth )
			{
				dfEditorUtil.MarkUndo( control, "Change max list width" );
				control.MaxListWidth = maxWidth;
			}

			var maxHeight = EditorGUILayout.IntField( "Max List Height", control.MaxListHeight );
			if( maxHeight != control.MaxListHeight )
			{
				dfEditorUtil.MarkUndo( control, "Change max list height" );
				control.MaxListHeight = maxHeight;
			}

			var listOffset = dfEditorUtil.EditInt2( "Offset", "X", "Y", control.ListOffset );
			if( listOffset != control.ListOffset )
			{
				dfEditorUtil.MarkUndo( control, "Change list offset" );
				control.ListOffset = listOffset;
			}

			var listPadding = dfEditorUtil.EditPadding( "Padding", control.ListPadding );
			if( !listPadding.Equals( control.ListPadding ) )
			{
				dfEditorUtil.MarkUndo( control, "Modify padding" );
				control.ListPadding = listPadding;
			}

		}

		using( dfEditorUtil.BeginGroup( "List Data" ) )
		{
			EditOptions( control );
		}

		return true;

	}

	private static void EditOptions( dfDropdown control )
	{

		GUILayout.BeginHorizontal();
		{

			EditorGUILayout.LabelField( "Options", "", GUILayout.Width( 100 ) );

			EditorGUI.BeginChangeCheck();
			var optionsString = string.Join( "\n", control.Items );
			var optionsEdit = EditorGUILayout.TextArea( optionsString, GUILayout.Height( 100f ) );
			if( EditorGUI.EndChangeCheck() )
			{
				dfEditorUtil.MarkUndo( control, "Change options" );
				var options = optionsEdit.Trim().Split( new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries );
				control.Items = options;
			}

		}
		GUILayout.EndHorizontal();

	}

}
