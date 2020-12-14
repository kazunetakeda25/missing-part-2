/* Copyright 2013 Daikon Forge */
using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using Object = UnityEngine.Object;

[CanEditMultipleObjects()]
[CustomEditor( typeof( dfControl ), true )]
public class dfControlInspector : Editor
{

	private static Dictionary<int, bool> commonFoldouts = new Dictionary<int, bool>();

	private int targetObjectCount = 0;

	public override void OnInspectorGUI()
	{

		targetObjectCount = 
			Selection.gameObjects
			.Select( c => c.GetComponent<dfControl>() )
			.Where( c => c != null )
			.Count();

		if( targetObjectCount > 1 )
		{
			OnInspectMultiple();
			return;
		}

		var control = target as dfControl;
		if( control == null )
			return;

		if( !control.gameObject.activeInHierarchy )
		{
			EditorGUILayout.HelpBox( "The GameObject is disabled", MessageType.Warning );
		}

		if( !control.enabled )
		{
			EditorGUILayout.HelpBox( "The control component is disabled", MessageType.Warning );
		}

		var isValidControl =
			control.gameObject.activeInHierarchy &&
			control.transform.parent != null &&
			control.GetManager() != null;

		if( !isValidControl )
		{
			EditorGUILayout.HelpBox( "This control must be a child of a GUI Manager or another control", MessageType.Error );
			return;
		}

		if( isFoldoutExpanded( commonFoldouts, "Control Properties" ) )
		{
			OnInspectCommonProperties( control );
		}

		OnCustomInspector();

		EditorGUILayout.Separator();

	}

	private void OnInspectMultiple()
	{
		showAlignmentButtons();
	}

	private void showAlignmentButtons()
	{

		dfEditorUtil.LabelWidth = 110f;

		using( dfEditorUtil.BeginGroup( "Align Edges" ) )
		{

			GUILayout.BeginHorizontal();
			{
				GUILayout.Space( 25 );
				if( GUILayout.Button( "Left" ) ) { alignEdgeLeft(); }
				if( GUILayout.Button( "Right" ) ) { alignEdgeRight(); }
				if( GUILayout.Button( "Top" ) ) { alignEdgeTop(); }
				if( GUILayout.Button( "Bottom" ) ) { alignEdgeBottom(); }
			}
			GUILayout.EndHorizontal();

		}

		using( dfEditorUtil.BeginGroup( "Align Centers" ) )
		{

			GUILayout.BeginHorizontal();
			{
				GUILayout.Space( 25 );
				if( GUILayout.Button( "Horizontally" ) ) { alignCenterHorz(); }
				if( GUILayout.Button( "Vertically" ) ) { alignCenterVert(); }
			}
			GUILayout.EndHorizontal();

		}

		using( dfEditorUtil.BeginGroup( "Distribute" ) )
		{

			GUILayout.BeginHorizontal();
			{
				GUILayout.Space( 25 );
				if( GUILayout.Button( "Horizontally" ) ) { distributeControlsHorizontally(); }
				if( GUILayout.Button( "Vertically" ) ) { distributeControlsVertically(); }
			}
			GUILayout.EndHorizontal();

		}

		using( dfEditorUtil.BeginGroup( "Make Same Size" ) )
		{

			GUILayout.BeginHorizontal();
			{
				GUILayout.Space( 25 );
				if( GUILayout.Button( "Horizontally" ) ) { makeSameSizeHorizontally(); }
				if( GUILayout.Button( "Vertically" ) ) { makeSameSizeVertically(); }
			}
			GUILayout.EndHorizontal();

		}

	}

	private void OnInspectCommonProperties( dfControl control )
	{

		dfEditorUtil.LabelWidth = 110f;

		using( dfEditorUtil.BeginGroup( "Layout" ) )
		{

			EditorGUI.BeginChangeCheck();
			var rel = dfEditorUtil.EditInt2( "Position", "Left", "Top", control.RelativePosition );
			if( EditorGUI.EndChangeCheck() && Vector2.Distance( rel, control.RelativePosition ) > float.Epsilon )
			{
				dfEditorUtil.MarkUndo( control, "Change control Position" );
				control.RelativePosition = rel;
			}

			EditorGUI.BeginChangeCheck();
			var size = dfEditorUtil.EditInt2( "Size", "Width", "Height", control.Size );
			if( EditorGUI.EndChangeCheck() && Vector2.Distance( size, control.Size ) > float.Epsilon )
			{

				dfEditorUtil.MarkUndo( control, "Change control Size" );
				control.Size = size;

				// If control's anchor includes centering, its layout should be recalculated
				control.PerformLayout();

			}

			var pivot = (dfPivotPoint)EditorGUILayout.EnumPopup( "Pivot", control.Pivot );
			if( pivot != control.Pivot )
			{
				dfEditorUtil.MarkUndo( control, "Change control Pivot" );
				control.Pivot = pivot;
			}

			var anchor = EditAnchor( control.Anchor );
			if( anchor != control.Anchor )
			{
				dfEditorUtil.MarkUndo( control, "Change control Anchor" );
				control.Anchor = anchor;
				if( anchor.IsAnyFlagSet( dfAnchorStyle.CenterHorizontal | dfAnchorStyle.CenterVertical ) )
				{
					control.PerformLayout();
				}
			}

		}

		using( dfEditorUtil.BeginGroup( "Size Limits" ) )
		{

			var minSize = dfEditorUtil.EditInt2( "Min. Size", "Width", "Height", control.MinimumSize );
			if( Vector2.Distance( minSize, control.MinimumSize ) > float.Epsilon )
			{
				dfEditorUtil.MarkUndo( control, "Change minimum size" );
				control.MinimumSize = minSize;
			}

			var maxSize = dfEditorUtil.EditInt2( "Max. Size", "Width", "Height", control.MaximumSize );
			if( Vector2.Distance( maxSize, control.MaximumSize ) > float.Epsilon )
			{
				dfEditorUtil.MarkUndo( control, "Change minimum size" );
				control.MaximumSize = maxSize;
			}

			var hotZoneScale = EditFloat2( "Hot Zone Scale", "X", "Y", control.HotZoneScale );
			if( !Vector2.Equals( hotZoneScale, control.HotZoneScale ) )
			{
				dfEditorUtil.MarkUndo( control, "Change Hot Zone Scale" );
				control.HotZoneScale = hotZoneScale;
			}

		}

		using( dfEditorUtil.BeginGroup( "Behavior" ) )
		{

			var enabled = EditorGUILayout.Toggle( "Enabled", control.IsEnabled );
			if( enabled != control.IsEnabled )
			{
				dfEditorUtil.MarkUndo( control, "Change control Enabled" );
				control.IsEnabled = enabled;
			}

			GUI.enabled = canEditIsVisibleProperty( control );
			var visible = EditorGUILayout.Toggle( "Visible", control.IsVisible );
			if( visible != control.IsVisible )
			{
				dfEditorUtil.MarkUndo( control, "Change control Visible" );
				control.IsVisible = visible;
			}
			GUI.enabled = true;

			var interactive = EditorGUILayout.Toggle( "Interactive", control.IsInteractive );
			if( interactive != control.IsInteractive )
			{
				dfEditorUtil.MarkUndo( control, "Change control Interactive property" );
				control.IsInteractive = interactive;
			}

			var canFocus = EditorGUILayout.Toggle( "Can Focus", control.CanFocus );
			if( canFocus != control.CanFocus )
			{
				dfEditorUtil.MarkUndo( control, "Change CanFocus property" );
				control.CanFocus = canFocus;
			}

			var clips = EditorGUILayout.Toggle( "Clip Children", control.ClipChildren );
			if( clips != control.ClipChildren )
			{
				dfEditorUtil.MarkUndo( control, "Change control ClipChildren property" );
				control.ClipChildren = clips;
			}

		}

		using( dfEditorUtil.BeginGroup( "Appearance" ) )
		{

			var color = EditorGUILayout.ColorField( "Color", control.Color );
			if( color != control.Color )
			{
				dfEditorUtil.MarkUndo( control, "Change control Color" );
				control.Color = color;
			}

			var disabledColor = EditorGUILayout.ColorField( "Disabled Color", control.DisabledColor );
			if( disabledColor != control.DisabledColor )
			{
				dfEditorUtil.MarkUndo( control, "Change control Disabled Color" );
				control.DisabledColor = disabledColor;
			}

			// NOTE: dfControl.Opacity is quantized to 255 levels
			var opacity = EditorGUILayout.Slider( "Opacity", control.Opacity, 0, 1 );
			if( Mathf.Abs( opacity - control.Opacity ) > ( 1f / 255f ) )
			{
				dfEditorUtil.MarkUndo( control, "Change control Opacity" );
				control.Opacity = opacity;
			}

			var controlGroup = getControlGroup( control );
			var maxIndex = controlGroup != null && controlGroup.Count > 0 
				? controlGroup.Max( x => x.ZOrder )
				: 0;

			var zorder = EditorGUILayout.IntSlider( "Z-Order", control.ZOrder, 0, maxIndex );
			if( zorder != control.ZOrder )
			{
				
				dfEditorUtil.MarkUndo( control, "Change control Z-Order" );

				if( control.Parent == null )
				{
					setZOrder( controlGroup, control, zorder );
				}
				else
				{
					control.ZOrder = zorder;
				}

			}

			var tabIndex = EditorGUILayout.IntField( "Tab Index", control.TabIndex );
			if( tabIndex != control.TabIndex )
			{
				dfEditorUtil.MarkUndo( control, "Change control Tab Index" );
				control.TabIndex = tabIndex;
			}

			var tooltip = EditorGUILayout.TextField( "Tooltip", control.Tooltip );
			if( tooltip != control.Tooltip )
			{
				dfEditorUtil.MarkUndo( control, "Change control Tooltip" );
				control.Tooltip = tooltip;
			}

		}

		EditorGUILayout.Separator();

		EditorGUILayout.BeginHorizontal();
		{

			if( GUILayout.Button( "Help" ) )
			{
				var url = "http://www.daikonforge.com/dfgui/tutorials/";
				Application.OpenURL( url );
				Debug.Log( "View online help at " + url );
			}

			if( GUILayout.Button( "Snap to Pixel" ) )
			{
				dfEditorUtil.MarkUndo( control, "Snap to pixel boundaries" );
				control.MakePixelPerfect();
			}

		}
		EditorGUILayout.EndHorizontal();

	}

	private bool canEditIsVisibleProperty( dfControl control )
	{

		if( control.Parent == null )
			return true;

		control = control.Parent;

		while( control != null )
		{
			
			var serialized = new SerializedObject( control );
			var property = serialized.FindProperty( "isVisible" ).boolValue;
			if( !property )
				return false;

			control = control.Parent;

		}

		return true;

	}

	private void setZOrder( List<dfControl> group, dfControl control, int zorder )
	{

		for( int i = 0; i < group.Count; i++ )
		{
			var other = group[ i ];
			if( other != control && other.ZOrder == zorder )
			{
				other.ZOrder = control.ZOrder;
				break;
			}
		}

		control.ZOrder = zorder;

		group.Sort();

		for( int i = 0; i < group.Count; i++ )
		{
			group[ i ].ZOrder = i;
		}

	}

	private List<dfControl> getControlGroup( dfControl control )
	{

		if( control.transform.parent != null && control.Parent != null )
		{
			return control.Parent.Controls.ToList();
		}

		var topControls = new List<dfControl>();
		var top = control.GetManager().transform;

		for( int i = 0; i < top.childCount; i++ )
		{
			var childControl = top.GetChild( i ).GetComponent<dfControl>();
			if( childControl != null )
			{
				topControls.Add( childControl );
			}
		}

		return topControls;

	}

	protected virtual bool OnCustomInspector()
	{
		// Intended to be overridden
		return false;
	}

	protected bool isFoldoutExpanded( Dictionary<int, bool> list, string label, bool defaultValue = true )
	{

		var isExpanded = defaultValue;
		var controlID = target.GetInstanceID();
		if( list.ContainsKey( controlID ) )
		{
			isExpanded = list[ controlID ];
		}
		isExpanded = EditorGUILayout.Foldout( isExpanded, label );
		list[ controlID ] = isExpanded;

		return isExpanded;

	}

	protected Vector2 EditFloat2( string groupLabel, string label1, string label2, Vector2 value )
	{

		var retVal = Vector2.zero;

		var savedLabelWidth = dfEditorUtil.LabelWidth;

		GUILayout.BeginHorizontal();
		{

			EditorGUILayout.LabelField( groupLabel, "", GUILayout.Width( dfEditorUtil.LabelWidth - 12 ) );

			GUILayout.BeginVertical();
			{

				dfEditorUtil.LabelWidth = 60f;

				var x = EditorGUILayout.FloatField( label1, value.x );
				var y = EditorGUILayout.FloatField( label2, value.y );

				retVal.x = x;
				retVal.y = y;

			}
			GUILayout.EndVertical();

			GUILayout.FlexibleSpace();

		}
		GUILayout.EndHorizontal();

		dfEditorUtil.LabelWidth = savedLabelWidth;

		return retVal;

	}

	protected internal static void SelectPrefab<T>( string label, dfControl control, string propertyName, dfPrefabSelectionDialog.PreviewCallback previewCallback = null, dfPrefabSelectionDialog.FilterCallback filter = null ) where T : dfControl
	{

		var value = getValue( control, propertyName ) as dfControl;

		dfPrefabSelectionDialog.SelectionCallback selectionCallback = delegate( GameObject item )
		{
			var newValue = ( item == null ) ? null : item.GetComponent<T>();
			dfEditorUtil.MarkUndo( control, "Change " + ObjectNames.NicifyVariableName( propertyName ) );
			setValue( control, propertyName, newValue );
		};

		EditorGUILayout.BeginHorizontal();
		{

			EditorGUILayout.LabelField( label, "", GUILayout.Width( dfEditorUtil.LabelWidth - 5 ) );

			var displayText = value == null ? "[none]" : value.name;
			GUILayout.Label( displayText, "TextField" );

			var evt = Event.current;
			if( evt != null )
			{
				Rect textRect = GUILayoutUtility.GetLastRect();
				if( evt.type == EventType.MouseDown && evt.clickCount == 2 )
				{
					if( textRect.Contains( evt.mousePosition ) )
					{
						if( GUI.enabled && value != null )
						{
							Selection.activeObject = value;
							EditorGUIUtility.PingObject( value );
						}
					}
				}
				else if( evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform )
				{
					if( textRect.Contains( evt.mousePosition ) )
					{
						var draggedObject = DragAndDrop.objectReferences.First() as GameObject;
						var draggedFont = draggedObject != null ? draggedObject.GetComponent<T>() : null;
						DragAndDrop.visualMode = ( draggedFont != null ) ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.None;
						if( evt.type == EventType.DragPerform )
						{
							selectionCallback( draggedObject );
						}
						evt.Use();
					}
				}
			}

			if( GUI.enabled && GUILayout.Button( new GUIContent( " ", "Edit" ), "IN ObjectField", GUILayout.Width( 14 ) ) )
			{
				dfEditorUtil.DelayedInvoke( (System.Action)( () =>
				{
					dfPrefabSelectionDialog.Show( "Select " + ObjectNames.NicifyVariableName( typeof( T ).Name ), typeof( T ), selectionCallback, previewCallback, filter );
				} ) );
			}

		}
		EditorGUILayout.EndHorizontal();

		GUILayout.Space( 2 );

	}

	protected internal static void SelectTextureAtlas( string label, dfControl control, string propertyName, bool readOnly, bool colorizeIfMissing )
	{

		var savedColor = GUI.color;

		try
		{

			var atlas = getValue( control, propertyName ) as dfAtlas;

			GUI.enabled = !readOnly;

			if( atlas == null && colorizeIfMissing )
				GUI.color = Color.red;

			dfPrefabSelectionDialog.SelectionCallback selectionCallback = delegate( GameObject item )
			{
				var newAtlas = ( item == null ) ? null : item.GetComponent<dfAtlas>();
				dfEditorUtil.MarkUndo( control, "Change Atlas" );
				setValue( control, propertyName, newAtlas );
			};

			var value = (dfAtlas)getValue( control, propertyName );

			EditorGUILayout.BeginHorizontal();
			{

				EditorGUILayout.LabelField( label, "", GUILayout.Width( dfEditorUtil.LabelWidth - 6 ) );

				GUILayout.Space( 2 );

				var displayText = value == null ? "[none]" : value.name;
				GUILayout.Label( displayText, "TextField" );

				var evt = Event.current;
				if( evt != null )
				{
					Rect textRect = GUILayoutUtility.GetLastRect();
					if( evt.type == EventType.MouseDown && evt.clickCount == 2 )
					{
						if( textRect.Contains( evt.mousePosition ) )
						{
							if( GUI.enabled && value != null )
							{
								Selection.activeObject = value;
								EditorGUIUtility.PingObject( value );
							}
						}
					}
					else if( evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform )
					{
						if( textRect.Contains( evt.mousePosition ) )
						{
							var draggedObject = DragAndDrop.objectReferences.First() as GameObject;
							var draggedFont = draggedObject != null ? draggedObject.GetComponent<dfAtlas>() : null;
							DragAndDrop.visualMode = ( draggedFont != null ) ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.None;
							if( evt.type == EventType.DragPerform )
							{
								selectionCallback( draggedObject );
							}
							evt.Use();
						}
					}
				}

				if( GUI.enabled && GUILayout.Button( new GUIContent( " ", "Edit Atlas" ), "IN ObjectField", GUILayout.Width( 14 ) ) )
				{
					dfEditorUtil.DelayedInvoke( (System.Action)( () =>
					{
						var dialog = dfPrefabSelectionDialog.Show( "Select Texture Atlas", typeof( dfAtlas ), selectionCallback, dfTextureAtlasInspector.DrawAtlasPreview, null );
						dialog.previewSize = 200;
					} ) );
				}

			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space( 2 );

		}
		finally
		{
			GUI.enabled = true;
			GUI.color = savedColor;
		}

	}

	protected internal static void SelectFontDefinition( string label, dfAtlas atlas, dfControl control, string propertyName, bool colorizeIfMissing, bool allowDynamicFonts = false )
	{

		var savedColor = GUI.color;

		try
		{

			var value = (dfFontBase)getValue( control, propertyName );

			if( value == null && colorizeIfMissing )
				GUI.color = Color.red;

			dfPrefabSelectionDialog.FilterCallback filterCallback = delegate( GameObject item )
			{
				if( atlas == null )
					return false;
				var font = item.GetComponent<dfFont>();
				if( font == null || font.Atlas == null )
					return false;
				if( !dfAtlas.Equals( font.Atlas, atlas ) )
					return false;
				return true;
			};

			dfPrefabSelectionDialog.SelectionCallback selectionCallback = delegate( GameObject item )
			{
				
				var font = ( item == null ) ? null : item.GetComponent<dfFontBase>();
				
				dfEditorUtil.MarkUndo( control, "Change Font" );
				setValue( control, propertyName, font );

			};

			EditorGUILayout.BeginHorizontal();
			{

				EditorGUILayout.LabelField( label, "", GUILayout.Width( dfEditorUtil.LabelWidth - 6 ) );

				GUILayout.Space( 2 );

				var displayText = value == null ? "[none]" : value.name;
				GUILayout.Label( displayText, "TextField" );

				var evt = Event.current;
				if( evt != null )
				{
					Rect textRect = GUILayoutUtility.GetLastRect();
					if( evt.type == EventType.MouseDown && evt.clickCount == 2 )
					{
						if( textRect.Contains( evt.mousePosition ) )
						{
							if( GUI.enabled && value != null )
							{
								Selection.activeObject = value;
								EditorGUIUtility.PingObject( value );
							}
						}
					}
					else if( evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform )
					{
						if( textRect.Contains( evt.mousePosition ) )
						{
							
							var draggedObject = DragAndDrop.objectReferences.First() as GameObject;
							var draggedFont =
								( draggedObject != null )
								? draggedObject.GetComponent<dfFontBase>() 
								: null;

							var block = ( draggedFont is dfDynamicFont ) &&!allowDynamicFonts;

							DragAndDrop.visualMode = ( draggedFont != null && !block )
								? DragAndDropVisualMode.Copy 
								: DragAndDropVisualMode.None;
							
							if( evt.type == EventType.DragPerform )
							{
								selectionCallback( draggedObject );
							}

							evt.Use();

						}
					}
				}

				if( GUI.enabled && GUILayout.Button( new GUIContent( " ", "Edit Font" ), "IN ObjectField", GUILayout.Width( 14 ) ) )
				{
					dfEditorUtil.DelayedInvoke( (System.Action)( () =>
					{
						dfPrefabSelectionDialog.Show( "Select Font", typeof( dfFont ), selectionCallback, dfFontDefinitionInspector.DrawFontPreview, filterCallback );
					} ) );
				}

			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space( 2 );

		}
		finally
		{
			GUI.color = savedColor;
		}

	}

	protected internal static void SelectSprite( string label, dfAtlas atlas, dfControl control, string propertyName, bool colorizeIfMissing = true )
	{

		var savedColor = GUI.color;

		try
		{

			GUI.enabled = ( atlas != null );

			dfSpriteSelectionDialog.SelectionCallback callback = delegate( string spriteName )
			{
				dfEditorUtil.MarkUndo( control, "Change Sprite" );
				setValue( control, propertyName, spriteName );
			};

			var value = (string)getValue( control, propertyName );
			if( atlas == null || atlas[ value ] == null && colorizeIfMissing )
				GUI.color = Color.red;

			EditorGUILayout.BeginHorizontal();
			{

				EditorGUILayout.LabelField( label, "", GUILayout.Width( dfEditorUtil.LabelWidth - 6 ) );

				GUILayout.Space( 2 );

				var displayText = string.IsNullOrEmpty( value ) ? "[none]" : value;
				GUILayout.Label( displayText, "TextField" );

				var evt = Event.current;
				if( evt != null && evt.type == EventType.MouseDown && evt.clickCount == 2 )
				{
					Rect rect = GUILayoutUtility.GetLastRect();
					if( rect.Contains( evt.mousePosition ) )
					{
						if( GUI.enabled && value != null )
						{
							dfTextureAtlasInspector.SelectedSprite = value;
							Selection.activeObject = atlas;
							EditorGUIUtility.PingObject( atlas );
						}
					}
				}

				if( GUI.enabled && GUILayout.Button( new GUIContent( " ", "Edit " + label ), "IN ObjectField", GUILayout.Width( 14 ) ) )
				{
					dfEditorUtil.DelayedInvoke( (System.Action)( () =>
					{
						dfSpriteSelectionDialog.Show( "Select Sprite: " + label, atlas, value, callback );
					} ) );
				}

			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space( 2 );

		}
		finally
		{
			GUI.enabled = true;
			GUI.color = savedColor;
		}

	}

	private static void setValue( dfControl control, string propertyName, object value )
	{
		var property = control.GetType().GetProperty( propertyName );
		if( property == null )
			throw new ArgumentException( "Property '" + propertyName + "' does not exist on " + control.GetType().Name );
		property.SetValue( control, value, null );
	}

	private static object getValue( dfControl control, string propertyName )
	{
		var property = control.GetType().GetProperty( propertyName );
		if( property == null )
			throw new ArgumentException( "Property '" + propertyName + "' does not exist on " + control.GetType().Name );
		return property.GetValue( control, null );
	}

	private dfAnchorStyle EditAnchor( dfAnchorStyle value )
	{

		const float OPTION_WIDTH = 100f;
		var labelWidth = 85;// dfEditorUtil.LabelWidth;

		var retVal = value;

		using( dfEditorUtil.BeginGroup( "Anchor" ) )
		{

			GUILayout.BeginHorizontal();
			{

				EditorGUILayout.LabelField( "Anchor", "", GUILayout.Width( labelWidth ) );

				GUILayout.BeginVertical();
				{

					dfEditorUtil.LabelWidth = OPTION_WIDTH;

					var left = EditorGUILayout.Toggle( "Left", retVal.IsFlagSet( dfAnchorStyle.Left ) );
					var right = EditorGUILayout.Toggle( "Right", retVal.IsFlagSet( dfAnchorStyle.Right ) );
					var top = EditorGUILayout.Toggle( "Top", retVal.IsFlagSet( dfAnchorStyle.Top ) );
					var bottom = EditorGUILayout.Toggle( "Bottom", retVal.IsFlagSet( dfAnchorStyle.Bottom ) );

					retVal = retVal.SetFlag( dfAnchorStyle.Top, top );
					retVal = retVal.SetFlag( dfAnchorStyle.Left, left );
					retVal = retVal.SetFlag( dfAnchorStyle.Right, right );
					retVal = retVal.SetFlag( dfAnchorStyle.Bottom, bottom );

					if( top || bottom )
					{
						retVal = retVal.SetFlag( dfAnchorStyle.CenterVertical, false );
					}

					if( left || right )
					{
						retVal = retVal.SetFlag( dfAnchorStyle.CenterHorizontal, false );
					}

				}
				GUILayout.EndVertical();

				GUILayout.FlexibleSpace();

			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{

				EditorGUILayout.LabelField( "Center", "", GUILayout.Width( labelWidth ) );

				GUILayout.BeginVertical();
				{

					dfEditorUtil.LabelWidth = OPTION_WIDTH;

					var horz = EditorGUILayout.Toggle( "Horizontal", retVal.IsFlagSet( dfAnchorStyle.CenterHorizontal ) );
					var vert = EditorGUILayout.Toggle( "Vertical", retVal.IsFlagSet( dfAnchorStyle.CenterVertical ) );

					retVal = retVal.SetFlag( dfAnchorStyle.CenterHorizontal, horz );
					retVal = retVal.SetFlag( dfAnchorStyle.CenterVertical, vert );

					if( horz )
					{
						retVal = retVal.SetFlag( dfAnchorStyle.Left, false );
						retVal = retVal.SetFlag( dfAnchorStyle.Right, false );
					}

					if( vert )
					{
						retVal = retVal.SetFlag( dfAnchorStyle.Top, false );
						retVal = retVal.SetFlag( dfAnchorStyle.Bottom, false );
					}

				}
				GUILayout.EndVertical();

				GUILayout.FlexibleSpace();

			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{

				EditorGUILayout.LabelField( "Mode", "", GUILayout.Width( labelWidth ) );

				GUILayout.BeginVertical();
				{

					dfEditorUtil.LabelWidth = OPTION_WIDTH;

					var proportional = EditorGUILayout.Toggle( "Proportional", retVal.IsFlagSet( dfAnchorStyle.Proportional ) );
					retVal = retVal.SetFlag( dfAnchorStyle.Proportional, proportional );

				}
				GUILayout.EndVertical();

				GUILayout.FlexibleSpace();

			}
			GUILayout.EndHorizontal();

		}

		return retVal;

	}

	/// <summary>
	/// Hook to allow custom dfControl inspectors to perform a "default" action
	/// when the user double-clicks the control in the Editor
	/// </summary>
	/// <param name="control">The control that was double-clicked</param>
	/// <param name="evt">The event information</param>
	/// <returns>Returns TRUE if the event was processed, FALSE otherwise</returns>
	protected virtual bool OnControlDoubleClick( dfControl control, Event evt )
	{

		var camera = control.GetCamera();
		var mousePos = Event.current.mousePosition;
		var ray = HandleUtility.GUIPointToWorldRay( mousePos );
		var maxDistance = 1000f;

		var hits = Physics.RaycastAll( ray, maxDistance, camera.cullingMask );
		var controls = hits
			.Select( h => h.collider.GetComponent<dfControl>() )
			.Where( c => c != null )
			.OrderBy( c => c.RenderOrder )
			.ToList();

		if( controls.Count <= 1 )
			return false;

		var controlIndex = controls.FindIndex( c => c == control );
		if( controlIndex == -1 )
			return false;

		var focus = control;
		if( controlIndex > 0 )
			focus = controls[ controlIndex - 1 ];
		else
			focus = controls[ controls.Count - 1 ];

		GUIUtility.hotControl = GUIUtility.keyboardControl = 0;
		Selection.activeGameObject = focus.gameObject;
		SceneView.lastActiveSceneView.Repaint();

		return true;

	}

	#region Implementation of resizing handles

	private Vector3[] resizeHandles = new Vector3[ 4 ];
	private Vector2 lastDragMousePos = Vector2.zero;
	private List<ResizeUndoInfo> resizeUndoData = new List<ResizeUndoInfo>();
	private int resizeHandleIndex = -1;
	private bool isDraggingEdge = false;
	private bool isMovingWithSnap = false;

	public virtual void OnSceneGUI()
	{

		var control = target as dfControl;
		if( control == null )
			return;

		var snapToGrid = EditorPrefs.GetBool( "dfGUIManager.SnapToGrid", false );
		var gridSize = EditorPrefs.GetInt( "dfGUIManager.GridSize", 25 );

		var evt = Event.current;
		var id = GUIUtility.GetControlID( GetType().Name.GetHashCode(), FocusType.Passive );
		var eventType = evt.GetTypeForControl( id );

		var pixelsToUnits = control.GetManager().PixelsToUnits();

		var corners = new Vector3[ 4 ];
		var size = control.Size * pixelsToUnits;
		corners[ 0 ] = Vector2.zero;
		corners[ 1 ].x = 0; corners[ 1 ].y = -size.y;
		corners[ 2 ].x = size.x; corners[ 2 ].y = -size.y;
		corners[ 3 ].x = size.x; corners[ 3 ].y = 0;

		var offset = control.Pivot.UpperLeftToTransform( size );
		var rot = control.transform.localToWorldMatrix;
		for( int i = 0; i < 4; i++ )
		{

			corners[ i ] = rot.MultiplyPoint( corners[ i ] - offset );

		}

		var handleColor = Handles.color;
		var gizmoColor = Gizmos.color;

		Handles.color = Gizmos.color = new Color( 0.3f, 0.75f, 1f, 0.5f );
		Handles.DrawLine( corners[ 0 ], corners[ 1 ] );
		Handles.DrawLine( corners[ 1 ], corners[ 2 ] );
		Handles.DrawLine( corners[ 2 ], corners[ 3 ] );
		Handles.DrawLine( corners[ 3 ], corners[ 0 ] );

		if( control.gameObject == Selection.activeGameObject )
		{

			var handleRotation = control.transform.rotation; 
				
			for( int i = 0; i < 4; i++ )
			{

				var handleLocation = resizeHandles[ i ] = Vector3.Lerp( corners[ i ], corners[ ( i + 1 ) % 4 ], 0.5f ); 
				var handleSize = HandleUtility.GetHandleSize( handleLocation );

				Handles.SphereCap( -1, handleLocation, handleRotation, handleSize * 0.175f );
				Handles.SphereCap( -1, handleLocation, handleRotation, handleSize * 0.175f );

				// Support resize mouse cursor
				var hsc = HandleUtility.WorldToGUIPoint( handleLocation );
				var rect = new Rect( hsc.x - 10, hsc.y - 10, 20, 20 );
				var cursor = ( i % 2 ) == 0 ? MouseCursor.ResizeHorizontal : MouseCursor.ResizeVertical;
				EditorGUIUtility.AddCursorRect( rect, cursor );

			}

		}

		Handles.color = handleColor;
		Gizmos.color = gizmoColor;

		switch( eventType )
		{

			case EventType.KeyDown:
				if( evt.keyCode == KeyCode.Escape && ( isDraggingEdge || isMovingWithSnap ) )
				{

					isDraggingEdge = false;
					isMovingWithSnap = false;
					resizeHandleIndex = -1;

					undoResize();

					GUIUtility.hotControl = GUIUtility.keyboardControl = 0;
					evt.Use();

					dfGUIManager.RefreshAll();
					SceneView.RepaintAll();

				}
				else if( evt.control )
				{
					var mult = evt.shift ? 5 : 1;
					switch( evt.keyCode )
					{
						case KeyCode.LeftArrow:
							control.Position += Vector3.left * mult;
							evt.Use();
							break;
						case KeyCode.RightArrow:
							control.Position += Vector3.right * mult;
							evt.Use();
							break;
						case KeyCode.UpArrow:
							control.Position += Vector3.up * mult;
							evt.Use();
							break;
						case KeyCode.DownArrow:
							control.Position += Vector3.down * mult;
							evt.Use();
							break;
					}
				}
				break;

			case EventType.MouseDown:
				isDraggingEdge = false;

				var modifierKeyPressed = evt.alt || evt.control || evt.shift;
				if( evt.button != 0 || modifierKeyPressed )
				{

					if( evt.button == 1 && isMouseOverControl( evt.mousePosition ) && !modifierKeyPressed )
					{
						displayContextMenu();
						evt.Use();
						return;
					}

					if( evt.button == 0 && evt.control )
					{
						doMultiSelectRaycast();
						evt.Use();
						return;
					}

					GUIUtility.hotControl = GUIUtility.keyboardControl = 0;

					break;

				}

				if( evt.clickCount == 2 )
				{
					if( OnControlDoubleClick( control, evt ) )
					{
						evt.Use();
						return;
					}
				}

				var minIndex = -1;
				var minDistance = float.MaxValue;
				var selectDistance = 10f;

				lastDragMousePos = Event.current.mousePosition;
				for( int i = 0; i < resizeHandles.Length; i++ )
				{
					var handleScreenPoint = HandleUtility.WorldToGUIPoint( resizeHandles[ i ] );
					var distanceToHandle = Vector2.Distance( lastDragMousePos, handleScreenPoint );
					if( distanceToHandle < minDistance )
					{
						minDistance = distanceToHandle;
						minIndex = i;
					}
				}

				if( minDistance < selectDistance )
				{
					GUIUtility.hotControl = GUIUtility.keyboardControl = id;
					evt.Use();
					resizeHandleIndex = minIndex;
				}
				else if( control.gameObject == Selection.activeGameObject )
				{

					resizeHandleIndex = -1;

					var startDragging =
						Selection.gameObjects.Length == 1 &&
						snapToGrid &&
						evt.button == 0 &&
						Tools.current == Tool.Move &&
						!( evt.alt || evt.control || evt.shift );

					if( startDragging )
					{
						isMovingWithSnap = true;
						saveResizeUndoInfo();
						lastDragMousePos = roundToNearest( getGUIManagerPosition( evt.mousePosition ), gridSize );
					}

				}

				break;

			case EventType.MouseDrag:
				if( resizeHandleIndex == -1 )
				{
					if( isMovingWithSnap )
					{

						var dragMousePos = roundToNearest( getGUIManagerPosition( evt.mousePosition ), gridSize );
						var dragDistance = Vector2.Distance( dragMousePos, lastDragMousePos );
						if( dragDistance > 0 )
						{

							var dragDelta = (Vector3)( dragMousePos - lastDragMousePos ) * pixelsToUnits;
							lastDragMousePos = dragMousePos;

							for( int i = 0; i < Selection.transforms.Length; i++ )
							{
								Selection.transforms[ i ].position += dragDelta;
							}

							snapSelectionToGrid();

						}

						evt.Use();

					}
					break;
				}

				if( !isDraggingEdge )
				{

					if( evt.button == 0 && Vector2.Distance( lastDragMousePos, evt.mousePosition ) > 3 )
					{
						dfEditorUtil.MarkUndo( control, "Change control size" );
						isDraggingEdge = true;
						saveResizeUndoInfo();
						snapSelectionToGrid();
					}

				}
				else
				{
					dragEdgeHandle( raycast( evt.mousePosition ) );
				}
				evt.Use();
				break;

			case EventType.MouseUp:
				if( isDraggingEdge )
				{

					resizeUndoData.Clear();

					var controls =
						Selection.gameObjects
						.Select( c => c.GetComponent<dfControl>() )
						.Where( c => c != null )
						.ToList();

					for( int i = 0; i < controls.Count; i++ )
					{

						control = controls[ i ];

						// If the control's anchor includes centering, should give 
						// it a chance to update the layout
						control.Invalidate();
						control.PerformLayout();
						control.GetRootContainer().MakePixelPerfect();

					}

					resizeHandleIndex = -1;
					evt.Use();

					SceneView.RepaintAll();
					GUIUtility.hotControl = GUIUtility.keyboardControl = 0;

				}
				else if( isMovingWithSnap )
				{
					isMovingWithSnap = false;
					snapSelectionToGrid();
				}
				isMovingWithSnap = false;
				isDraggingEdge = false;
				resetSelectionLayouts();
				break;

		}

	}

	private void resetSelectionLayouts()
	{

		var controls =
			Selection.gameObjects
			.Select( c => c.GetComponent<dfControl>() )
			.Where( c => c != null )
			.OrderByDescending( c => c.RenderOrder )
			.ToList();

		for( int i = 0; i < controls.Count; i++ )
		{

			var controlComponent = controls[ i ];
			if( controlComponent != null )
			{
				
				var anchor = controlComponent.Anchor;
				if( anchor.IsAnyFlagSet( dfAnchorStyle.CenterHorizontal | dfAnchorStyle.CenterVertical ) )
				{
					controlComponent.PerformLayout();
				}

				controlComponent.ResetLayout( false, true );
				EditorUtility.SetDirty( controlComponent );

			}

		}

	}

	private void snapSelectionToGrid()
	{

		var snapToGrid = EditorPrefs.GetBool( "dfGUIManager.SnapToGrid", false );
		if( !snapToGrid )
			return;

		var controls =
			Selection.gameObjects
			.Select( c => c.GetComponent<dfControl>() )
			.Where( c => c != null )
			.ToList();

		if( controls.Count == 0 )
			return;

		var gridSize = EditorPrefs.GetInt( "dfGUIManager.GridSize", 25 );
		var pixelsToUnits = ( (dfControl)target ).GetManager().PixelsToUnits();
		var corner = controls[ 0 ].GetManager().GetCorners()[ 0 ];

		var transforms = Selection.transforms;
		var snapUnits = pixelsToUnits * gridSize;

		for( int i = 0; i < transforms.Length; i++ )
		{
			var pos = transforms[ i ].position;
			transforms[ i ].position = corner + roundToNearest( pos - corner, snapUnits );
			var controlComponent = transforms[ i ].GetComponent<dfControl>();
			if( controlComponent != null )
			{
				controlComponent.ResetLayout( false, true );
			}
		}

	}

	private Vector2 getGUIManagerPosition( Vector2 position )
	{

		position.y = Camera.current.pixelHeight - position.y;

		var control = this.target as dfControl;
		var manager = control.GetManager();

		var ray = Camera.current.ScreenPointToRay( position );
		var corner = manager.GetCorners()[ 0 ];
		var plane = new Plane( manager.transform.TransformDirection( Vector3.forward ), corner );

		var distance = 0f;
		plane.Raycast( ray, out distance );

		var hit = ray.origin + ray.direction * distance;

		var offset = ( ( hit - corner ) / manager.PixelsToUnits() ).RoundToInt();

		return offset;

	}

	private void saveResizeUndoInfo()
	{

		this.resizeUndoData.Clear();

		var controls =
			Selection.gameObjects
			.Select( c => c.GetComponent<dfControl>() )
			.Where( c => c != null )
			.ToList();

		for( int i = 0; i < controls.Count; i++ )
		{
			resizeUndoData.Add( new ResizeUndoInfo( controls[ i ] ) );
		}

	}

	private void undoResize()
	{

		for( int i = 0; i < resizeUndoData.Count; i++ )
		{
			resizeUndoData[ i ].UndoResize();
		}

		resizeUndoData.Clear();

	}

	private void doMultiSelectRaycast()
	{

		var ray = HandleUtility.GUIPointToWorldRay( Event.current.mousePosition );

		var hits = Physics.RaycastAll( ray, 1000f, 1 << ( (dfControl)target ).gameObject.layer );
		var controlClicked = hits
			.Select( h => h.collider.GetComponent<dfControl>() )
			.Where( c => c != null )
			.OrderBy( c => c.RenderOrder )
			.FirstOrDefault();

		if( controlClicked == null )
			return;

		var selectedObjects = Selection.objects.ToList();
		if( selectedObjects.Contains( controlClicked ) )
			selectedObjects.Remove( controlClicked );
		else
			selectedObjects.Add( controlClicked );

		Selection.objects = selectedObjects.ToArray();

	}

	private bool isMouseOverControl( Vector2 mousePos )
	{

		var control = target as dfControl;

		var ray = HandleUtility.GUIPointToWorldRay( mousePos );

		RaycastHit hitInfo;
		return control.GetComponent<Collider>().Raycast( ray, out hitInfo, 100f );

	}

	private void dragEdgeHandle( Vector3 mousePos )
	{

		var controls =
			Selection.gameObjects
			.Select( c => c.GetComponent<dfControl>() )
			.Where( c => c != null )
			.ToList();

		if( controls.Count == 0 )
			return;

		markUndo( controls, "Resize control" );

		var snapToGrid = EditorPrefs.GetBool( "dfGUIManager.SnapToGrid", false );
		var gridSize = EditorPrefs.GetInt( "dfGUIManager.GridSize", 25 );

		for( int i = 0; i < controls.Count; i++ )
		{

			var control = controls[ i ];
			var pixelsToUnits = control.GetManager().PixelsToUnits();

			// Need to transform the world-space locations of both the drag handle 
			// and the mouse pos so that we can get an accurate [X,Y] difference.
			var matrix = control.transform.worldToLocalMatrix;
			var start = matrix.MultiplyPoint( resizeHandles[ resizeHandleIndex ] ) / pixelsToUnits;
			var end = matrix.MultiplyPoint( mousePos ) / pixelsToUnits;

			// The difference between the two points determines how much to resize
			var sizeDiff = ( end - start );

			// Not sure this happens in practice, but if the user has not moved the 
			// mouse then just exit.
			if( sizeDiff.magnitude <= float.Epsilon )
				return;

			// Just for convenience, turn position and size into LRTB values
			var pos = control.Position;
			var size = control.Size;
			var min = control.CalculateMinimumSize();
			var left = pos.x;
			var top = pos.y;
			var right = left + size.x;
			var bottom = top - size.y;

			switch( resizeHandleIndex )
			{
				case 0: // Left edge
					left = Mathf.Min( right - min.x, left + sizeDiff.x );
					if( snapToGrid ) left = left.RoundToNearest( gridSize );
					break;
				case 2: // Right edge
					right = Mathf.Max( left + min.x, right + sizeDiff.x );
					if( snapToGrid ) right = right.RoundToNearest( gridSize );
					break;
				case 3: // Top edge
					top = Mathf.Max( bottom + min.y, top + sizeDiff.y );
					if( snapToGrid ) top = top.RoundToNearest( gridSize );
					break;
				case 1: // Bottom edge
					bottom = Mathf.Min( top - min.y, bottom + sizeDiff.y );
					if( snapToGrid ) bottom = bottom.RoundToNearest( gridSize );
					break;
			}

			EditorUtility.SetDirty( control );

			// NOTE: It is important to set the Size property before setting the
			// Position property when setting them both, as Position may rely on
			// Size in order to determine where to place the control relative to
			// the control's Pivot value.
			control.Size = new Vector2( right - left, top - bottom ).RoundToInt();
			control.Position = new Vector3( left, top ).RoundToInt();

		}

	}

	private Vector2 roundToNearest( Vector2 pos, float gridSize )
	{
		pos.x = pos.x.RoundToNearest( gridSize );
		pos.y = pos.y.RoundToNearest( gridSize );
		return pos;
	}

	private Vector3 roundToNearest( Vector3 pos, float gridSize )
	{
		pos.x = pos.x.RoundToNearest( gridSize );
		pos.y = pos.y.RoundToNearest( gridSize );
		pos.z = pos.z.RoundToNearest( gridSize );
		return pos;
	}

	private Vector3 raycast( Vector2 mousePos )
	{

		var control = target as dfControl;

		var plane = new Plane( control.transform.rotation * Vector3.back, control.transform.position );
		var ray = HandleUtility.GUIPointToWorldRay( mousePos );

		var distance = 0f;
		plane.Raycast( ray, out distance );

		return ray.origin + ray.direction * distance;

	}

	#endregion

	#region Context menu 

	protected virtual void FillContextMenu( List<ContextMenuItem> menu )
	{

		// If this function is overridden the derived class may have added
		// items to the beginning of the menu that can be assumed to be 
		// specific to the control's class, so add a seperator automatically
		if( menu.Count > 0 )
			menu.Add( new ContextMenuItem() { MenuText = "-" } );

		if( Selection.objects.Length == 1 )
		{

			// Adds a menu item for each dfControl class in the assembly that has 
			// an AddComponentMenu attribute defined.
			addContextMenuChildControls( menu );

			// Adds a menu item for each Tween class in the assembly that has 
			// an AddComponentMenu attribute defined.
			addContextTweens( menu );

			// Adds a menu item for each IDataBindingComponent class in the assembly that has 
			// an AddComponentMenu attribute defined.
			addContextDatabinding( menu );

			// Add an option to allow the user to select any Prefab that 
			// has a dfControl component as the main component
			addContextSelectPrefab( menu );

			// Add a "Create Script" menu item to display the "Create Script" dialog
			menu.Add( new ContextMenuItem() { MenuText = "Create Script...", Handler = ( selectedControl ) =>
			{
				dfScriptWizard.CreateScript( selectedControl );
			}});

			menu.Add( new ContextMenuItem() { MenuText = "-" } );

			addContextControlOrder( menu );
			menu.Add( new ContextMenuItem() { MenuText = "-" } );

			// Add a menu option to select any control under the cursor
			addContextSelectControl( menu );

			// Add standard menu options
			addContextCenter( menu );
			addContextFill( menu );
			addContextDock( menu );

		}
		else
		{
			addContextMultiEdit( menu );
		}

	}

	private void addContextMultiEdit( List<ContextMenuItem> menu )
	{
		addContextMultiAlignEdges( menu );
		addContextMultiAlignCenters( menu );
		addContextMultiDistribute( menu );
		addContextMultiSameSize( menu );
	}

	private void addContextMultiSameSize( List<ContextMenuItem> menu )
	{

		menu.Add( new ContextMenuItem()
		{
			MenuText = "Make Same Size/Horizontally",
			Handler = ( control ) => { makeSameSizeHorizontally(); }
		} );

		menu.Add( new ContextMenuItem()
		{
			MenuText = "Make Same Size/Vertically",
			Handler = ( control ) => { makeSameSizeVertically(); }
		} );

	}

	private void addContextMultiDistribute( List<ContextMenuItem> menu )
	{

		targetObjectCount =
			Selection.gameObjects
			.Select( c => c.GetComponent<dfControl>() )
			.Where( c => c != null )
			.Count();

		if( targetObjectCount <= 2 )
			return;

		menu.Add( new ContextMenuItem()
		{
			MenuText = "Distribute/Horizontally",
			Handler = ( control ) => { distributeControlsHorizontally(); }
		} );

		menu.Add( new ContextMenuItem()
		{
			MenuText = "Distribute/Vertically",
			Handler = ( control ) => { distributeControlsVertically(); }
		} );

	}

	private void addContextMultiAlignEdges( List<ContextMenuItem> menu )
	{

		menu.Add( new ContextMenuItem()
		{
			MenuText = "Align Edges/Left",
			Handler = ( control ) => { alignEdgeLeft(); }
		} );

		menu.Add( new ContextMenuItem()
		{
			MenuText = "Align Edges/Right",
			Handler = ( control ) => { alignEdgeRight(); }
		} );

		menu.Add( new ContextMenuItem()
		{
			MenuText = "Align Edges/Top",
			Handler = ( control ) => { alignEdgeTop(); }
		} );

		menu.Add( new ContextMenuItem()
		{
			MenuText = "Align Edges/Bottom",
			Handler = ( control ) => { alignEdgeBottom(); }
		} );

	}

	private void addContextMultiAlignCenters( List<ContextMenuItem> menu )
	{

		menu.Add( new ContextMenuItem()
		{
			MenuText = "Align Centers/Horizontally",
			Handler = ( control ) => { alignCenterHorz(); }
		} );

		menu.Add( new ContextMenuItem()
		{
			MenuText = "Align Centers/Vertically",
			Handler = ( control ) => { alignCenterVert(); }
		} );

	}

	private void addContextControlOrder( List<ContextMenuItem> menu )
	{

		menu.Add( new ContextMenuItem()
		{
			MenuText = "Ordering/Bring to Front",
			Handler = ( control ) =>
			{
				dfEditorUtil.MarkUndo( control, "Bring to front" );
				control.BringToFront();
				SceneView.lastActiveSceneView.Repaint();
			}
		} );

		menu.Add( new ContextMenuItem()
		{
			MenuText = "Ordering/Send To Back",
			Handler = ( control ) =>
			{
				dfEditorUtil.MarkUndo( control, "Send to back" );
				control.SendToBack();
				SceneView.lastActiveSceneView.Repaint();
			}
		} );

	}

	private void addContextSelectControl( List<ContextMenuItem> menu )
	{

		var camera = ( (dfControl)target ).GetCamera();
		if( camera == null )
			return;

		var needSeparator = false;

		var mousePos = Event.current.mousePosition;
		var ray = HandleUtility.GUIPointToWorldRay( mousePos );
		var maxDistance = 1000f;

		var hits = Physics.RaycastAll( ray, maxDistance, camera.cullingMask );
		for( int i = 0; i < hits.Length; i++ )
		{

			var control = hits[ i ].collider.GetComponent<dfControl>();
			if( control == null || control == target )
				continue;

			needSeparator = true;

			menu.Add( new ContextMenuItem()
			{
				MenuText = "Select Control/" + control.name,
				Handler = ( obj ) =>
				{
					Selection.activeGameObject = control.gameObject;
				}
			} );

		}

		if( needSeparator )
		{
			menu.Add( new ContextMenuItem() { MenuText = "-" } );
		}

	}

	private void addContextSelectPrefab( List<ContextMenuItem> menu )
	{

		// Need to determine final control position immediately, as 
		// this information is more difficult to obtain inside of an
		// anonymous delegate
		var mousePos = Event.current.mousePosition;
		var controlPosition = raycast( mousePos );

		Action<dfControl> selectPrefab = ( control ) =>
		{
			dfPrefabSelectionDialog.Show(
				"Select a prefab Control",
				typeof( dfControl ),
				( prefab ) =>
				{

					if( prefab == null )
						return;

					dfEditorUtil.MarkUndo( control, "Add child control - " + prefab.name );
						
					var newGameObject = PrefabUtility.InstantiatePrefab( prefab ) as GameObject;
					var childControl = newGameObject.GetComponent<dfControl>();
					childControl.transform.parent = control.transform;
					childControl.transform.position = controlPosition;
						
					//control.AddControl( childControl );
						
					Selection.activeGameObject = newGameObject;

				},
				null,
				null
			);
		};

		menu.Add( new ContextMenuItem() { MenuText = "Add Prefab...", Handler = selectPrefab } );

	}

	private void addContextDatabinding( List<ContextMenuItem> menu )
	{

		var assembly = typeof( IDataBindingComponent ).Assembly;
		var types = assembly.GetTypes();

		var controlTypes = types
			.Where( t =>
				typeof( IDataBindingComponent ).IsAssignableFrom( t ) &&
				t.IsDefined( typeof( AddComponentMenu ), true )
			).ToList();

		var options = new List<ContextMenuItem>();

		for( int i = 0; i < controlTypes.Count; i++ )
		{
			var type = controlTypes[ i ];
			var componentMenuAttribute = type.GetCustomAttributes( typeof( AddComponentMenu ), true ).First() as AddComponentMenu;
			var optionText = componentMenuAttribute.componentMenu.Replace( "Daikon Forge/Data Binding/", "" );
			options.Add( new ContextMenuItem()
			{
				MenuText = "Add Binding/" + optionText,
				Handler = ( control ) =>
				{
					dfEditorUtil.MarkUndo( control, "Add Binding - " + type.Name );
					var child = control.gameObject.AddComponent( type );
					Selection.activeGameObject = child.gameObject;
				}
			} );
		}

		options.Sort( ( lhs, rhs ) => { return lhs.MenuText.CompareTo( rhs.MenuText ); } );

		menu.AddRange( options );

	}

	private void addContextTweens( List<ContextMenuItem> menu )
	{

		var assembly = Assembly.GetAssembly( target.GetType() );
		var types = assembly.GetTypes();

		var controlTypes = types
			.Where( t =>
				typeof( dfTweenPlayableBase ).IsAssignableFrom( t ) &&
				t.IsDefined( typeof( AddComponentMenu ), true )
			).ToList();

		var options = new List<ContextMenuItem>();

		for( int i = 0; i < controlTypes.Count; i++ )
		{
			var type = controlTypes[ i ];
			var componentMenuAttribute = type.GetCustomAttributes( typeof( AddComponentMenu ), true ).First() as AddComponentMenu;
			var optionText = componentMenuAttribute.componentMenu.Replace( "Daikon Forge/Tweens/", "" );
			options.Add( new ContextMenuItem()
			{
				MenuText = "Add Tween/" + optionText,
				Handler = ( control ) =>
				{
					dfEditorUtil.MarkUndo( control, "Add Tween - " + type.Name );
					var child = control.gameObject.AddComponent( type );
					Selection.activeGameObject = child.gameObject;
				}
			} );
		}

		options.Sort( ( lhs, rhs ) => { return lhs.MenuText.CompareTo( rhs.MenuText ); } );

		menu.AddRange( options );

	}

	private void addContextMenuChildControls( List<ContextMenuItem> menu )
	{

		var assembly = Assembly.GetAssembly( target.GetType() );
		var types = assembly.GetTypes();
			
		var controlTypes = types
			.Where( t => 
				typeof( dfControl ).IsAssignableFrom( t ) &&
				t.IsDefined( typeof( AddComponentMenu ), true )
			).ToList();

		var options = new List<ContextMenuItem>();

		for( int i = 0; i < controlTypes.Count; i++ )
		{
			var type = controlTypes[i];
			var componentMenuAttribute = type.GetCustomAttributes( typeof( AddComponentMenu ), true ).First() as AddComponentMenu;
			var optionText = componentMenuAttribute.componentMenu.Replace( "Daikon Forge/User Interface/", "" );
			options.Add( buildAddChildMenuItem( optionText, type ) );
		}

		options.Sort( ( lhs, rhs ) => { return lhs.MenuText.CompareTo( rhs.MenuText ); } );

		menu.AddRange( options );

	}

	private ContextMenuItem buildAddChildMenuItem( string optionText, Type type )
	{

		// Need to determine final control position immediately, as 
		// this information is more difficult to obtain inside of an
		// anonymous delegate
		var mousePos = Event.current.mousePosition;
		var controlPosition = raycast( mousePos );

		return new ContextMenuItem()
		{
			MenuText = "Add Control/" + optionText,
			Handler = ( control ) => 
			{
				
				var childName = type.Name;
				if( childName.StartsWith( "df" ) )
					childName = childName.Substring( 2 );

				childName = ObjectNames.NicifyVariableName( childName );

				dfEditorUtil.MarkUndo( control, "Add Control - " + childName );

				var child = control.AddControl( type );
				child.name = childName;
				child.transform.position = controlPosition;

				Selection.activeGameObject = child.gameObject;

			}
		};

	}

	private void addContextDock( List<ContextMenuItem> menu )
	{

		menu.Add( new ContextMenuItem()
		{
			MenuText = "Dock/Left",
			Handler = ( control ) =>
			{
				dfEditorUtil.MarkUndo( control, "Dock control" );

				var containerSize = ( control.Parent != null )
					? control.Parent.Size
					: control.GetManager().GetScreenSize();

				control.Size = new Vector2( control.Size.x, containerSize.y );
				control.RelativePosition = new Vector3( 0, 0 );
				control.Anchor = dfAnchorStyle.Left | dfAnchorStyle.Top | dfAnchorStyle.Bottom;
			}
		} );

		menu.Add( new ContextMenuItem()
		{
			MenuText = "Dock/Top",
			Handler = ( control ) =>
			{
				dfEditorUtil.MarkUndo( control, "Dock control" );

				var containerSize = ( control.Parent != null )
					? control.Parent.Size
					: control.GetManager().GetScreenSize();

				control.Size = new Vector2( containerSize.x, control.Size.y );
				control.RelativePosition = new Vector3( 0, 0 );
				control.Anchor =  dfAnchorStyle.Left | dfAnchorStyle.Right | dfAnchorStyle.Top;
			}
		} );

		menu.Add( new ContextMenuItem()
		{
			MenuText = "Dock/Right",
			Handler = ( control ) =>
			{
				dfEditorUtil.MarkUndo( control, "Dock control" );

				var containerSize = ( control.Parent != null )
					? control.Parent.Size
					: control.GetManager().GetScreenSize();

				control.Size = new Vector2( control.Size.x, containerSize.y );
				control.RelativePosition = new Vector3( containerSize.x - control.Size.x, 0 );
				control.Anchor = dfAnchorStyle.Top | dfAnchorStyle.Bottom | dfAnchorStyle.Right;
			}
		} );

		menu.Add( new ContextMenuItem()
		{
			MenuText = "Dock/Bottom",
			Handler = ( control ) =>
			{
				dfEditorUtil.MarkUndo( control, "Dock control" );

				var containerSize = ( control.Parent != null )
					? control.Parent.Size
					: control.GetManager().GetScreenSize();

				control.Size = new Vector2( containerSize.x, control.Size.y );
				control.RelativePosition = new Vector3( 0, containerSize.y - control.Size.y );
				control.Anchor = dfAnchorStyle.Left | dfAnchorStyle.Right | dfAnchorStyle.Bottom;
			}
		} );

	}

	private void addContextFill( List<ContextMenuItem> menu )
	{

		var targetControl = target as dfControl;

		var containerName = targetControl.Parent != null ? targetControl.Parent.name : "screen";
		var rootText = "Fit to " + containerName;

		menu.Add( new ContextMenuItem()
		{
			MenuText = rootText + "/Horizontally",
			Handler = ( control ) =>
			{
				dfEditorUtil.MarkUndo( control, "Fit to container" );

				var containerSize = ( control.Parent != null )
					? control.Parent.Size
					: control.GetManager().GetScreenSize();

				control.Size = new Vector2( containerSize.x, control.Size.y );
				control.RelativePosition = new Vector3( 0, control.RelativePosition.y );

			}
		} );

		menu.Add( new ContextMenuItem()
		{
			MenuText = rootText + "/Vertically",
			Handler = ( control ) =>
			{
				dfEditorUtil.MarkUndo( control, "Fit to container" );

				var containerSize = ( control.Parent != null )
					? control.Parent.Size
					: control.GetManager().GetScreenSize();

				control.Size = new Vector2( control.Size.x, containerSize.y );
				control.RelativePosition = new Vector3( control.RelativePosition.x, 0 );

			}
		} );

		menu.Add( new ContextMenuItem()
		{
			MenuText = rootText + "/Both",
			Handler = ( control ) =>
			{
				dfEditorUtil.MarkUndo( control, "Fit to container" );

				var containerSize = ( control.Parent != null )
					? control.Parent.Size
					: control.GetManager().GetScreenSize();

				control.Size = containerSize;
				control.RelativePosition = Vector3.zero;

			}
		} );

	}

	private void addContextCenter( List<ContextMenuItem> menu )
	{

		var targetControl = target as dfControl;

		var containerName = targetControl.Parent != null ? "in " + targetControl.Parent.name : " on screen";
		var rootText = "Center " + containerName;

		menu.Add( new ContextMenuItem()
		{
			MenuText = rootText + "/Horizontally",
			Handler = ( control ) =>
			{
				dfEditorUtil.MarkUndo( control, "Center horizontally" );

				var containerSize = ( control.Parent != null )
					? control.Parent.Size
					: control.GetManager().GetScreenSize();

				var posX = ( containerSize.x - control.Size.x ) * 0.5f;
				var pos = control.RelativePosition;
				control.RelativePosition = new Vector3( posX, pos.y );
			}
		} );

		menu.Add( new ContextMenuItem()
		{
			MenuText = rootText + "/Vertically",
			Handler = ( control ) =>
			{
				dfEditorUtil.MarkUndo( control, "Center vertically" );

				var containerSize = ( control.Parent != null )
					? control.Parent.Size
					: control.GetManager().GetScreenSize();

				var posY = ( containerSize.y - control.Size.y ) * 0.5f;
				var pos = control.RelativePosition;
				control.RelativePosition = new Vector3( pos.x, posY );
			}
		} );

		menu.Add( new ContextMenuItem()
		{
			MenuText = rootText + "/Both",
			Handler = ( control ) =>
			{
				dfEditorUtil.MarkUndo( control, "Center" );

				var containerSize = ( control.Parent != null )
					? control.Parent.Size
					: control.GetManager().GetScreenSize();

				var posX = ( containerSize.x - control.Size.x ) * 0.5f;
				var posY = ( containerSize.y - control.Size.y ) * 0.5f;
				control.RelativePosition = new Vector3( posX, posY );
			}
		} );

	}

	private void displayContextMenu()
	{

		var control = target as dfControl;
		if( control == null )
			return;

		var menu = new GenericMenu();

		var items = new List<ContextMenuItem>();
		FillContextMenu( items );

		var actionFunc = new Action<int>( ( command ) =>
		{
			var handler = items[ command ].Handler;
			handler( control );
		} );

		var options = items.Select( i => i.MenuText ).ToList();
		for( int i = 0; i < options.Count; i++ )
		{
			var index = i;
			if( options[ i ] == "-" )
				menu.AddSeparator( "" );
			else
				menu.AddItem( new GUIContent( options[ i ] ), false, () => { actionFunc( index ); } );
		}

		menu.ShowAsContext();

	}

	private void markUndo( List<dfControl> controls, string undoMessage )
	{

		if( controls == null || controls.Count == 0 )
			return;

#if UNITY_4_3

		// Record undo information for the dfGUIManager instance that 
		// is responsible for the controls
		var manager = controls[ 0 ].GetManager();
		Undo.RegisterFullObjectHierarchyUndo( manager );
		EditorUtility.SetDirty( manager );

		// Record undo information for each selected control
		controls.ForEach( c => dfEditorUtil.MarkUndo( c, "Resize Control" ) );

#else
		Undo.RegisterSceneUndo( undoMessage );
#endif

	}

	#region Control alignment functions 

	private void alignEdgeLeft()
	{

		var minX = float.MaxValue;

		var controls =
			Selection.gameObjects
			.Select( c => c.GetComponent<dfControl>() )
			.Where( c => c != null )
			.ToList();

		for( int i = 0; i < controls.Count; i++ )
		{

			var control = controls[ i ];
			var corners = control.GetCorners();

			minX = Mathf.Min( minX, corners[ 0 ].x );

		}

		markUndo( controls, "Align Left" );

		for( int i = 0; i < controls.Count; i++ )
		{

			var control = controls[ i ];

			EditorUtility.SetDirty( control );

			var position = control.transform.position;
			var corners = control.GetCorners();
			var offset = corners[ 0 ] - position;

			control.transform.position = new Vector3(
				minX - offset.x,
				position.y,
				position.z
			);

			control.MakePixelPerfect();
			control.ResetLayout( false, true );

		}

		dfGUIManager.RefreshAll( true );

	}

	private void alignEdgeRight()
	{

		var maxX = float.MinValue;

		var controls =
			Selection.gameObjects
			.Select( c => c.GetComponent<dfControl>() )
			.Where( c => c != null )
			.ToList();

		for( int i = 0; i < controls.Count; i++ )
		{

			var control = controls[ i ];
			var corners = control.GetCorners();

			maxX = Mathf.Max( maxX, corners[ 1 ].x );

		}

		markUndo( controls, "Align Right" );

		for( int i = 0; i < controls.Count; i++ )
		{

			var control = controls[ i ];

			EditorUtility.SetDirty( control );

			var position = control.transform.position;
			var corners = control.GetCorners();
			var offset = corners[ 1 ] - position;

			control.transform.position = new Vector3(
				maxX - offset.x,
				position.y,
				position.z
			);

			control.MakePixelPerfect();
			control.ResetLayout( false, true );

		}

		dfGUIManager.RefreshAll( true );

	}

	private void alignEdgeTop()
	{

		var maxY = float.MinValue;

		var controls =
			Selection.gameObjects
			.Select( c => c.GetComponent<dfControl>() )
			.Where( c => c != null )
			.ToList();

		for( int i = 0; i < controls.Count; i++ )
		{

			var control = controls[ i ];
			var corners = control.GetCorners();

			maxY = Mathf.Max( maxY, corners[ 1 ].y );

		}

		markUndo( controls, "Align Top" );

		for( int i = 0; i < controls.Count; i++ )
		{

			var control = controls[ i ];

			EditorUtility.SetDirty( control );

			var position = control.transform.position;
			var corners = control.GetCorners();
			var offset = corners[ 1 ] - position;

			control.transform.position = new Vector3(
				position.x,
				maxY - offset.y,
				position.z
			);

			control.MakePixelPerfect();
			control.ResetLayout( false, true );

		}

		dfGUIManager.RefreshAll( true );

	}

	private void alignEdgeBottom()
	{

		var minY = float.MaxValue;

		var controls =
			Selection.gameObjects
			.Select( c => c.GetComponent<dfControl>() )
			.Where( c => c != null )
			.ToList();

		for( int i = 0; i < controls.Count; i++ )
		{

			var control = controls[ i ];
			var corners = control.GetCorners();

			minY = Mathf.Min( minY, corners[ 2 ].y );

		}

		markUndo( controls, "Align Top" );

		for( int i = 0; i < controls.Count; i++ )
		{

			var control = controls[ i ];

			EditorUtility.SetDirty( control );

			var position = control.transform.position;
			var corners = control.GetCorners();
			var offset = corners[ 2 ] - position;

			control.transform.position = new Vector3(
				position.x,
				minY - offset.y,
				position.z
			);

			control.MakePixelPerfect();
			control.ResetLayout( false, true );

		}

		dfGUIManager.RefreshAll( true );

	}

	private void alignCenterHorz()
	{

		var centers = new List<Vector3>();

		var controls =
			Selection.gameObjects
			.Select( c => c.GetComponent<dfControl>() )
			.Where( c => c != null )
			.ToList();

		for( int i = 0; i < controls.Count; i++ )
		{
			centers.Add( controls[ i ].GetCenter() );
		}

		var averagedCenter = centers[ 0 ];
		for( int i = 1; i < centers.Count; i++ )
		{
			averagedCenter = averagedCenter + centers[ i ];
		}

		averagedCenter /= centers.Count;

		markUndo( controls, "Align Center Horizontally" );

		for( int i = 0; i < controls.Count; i++ )
		{

			var control = controls[ i ];

			EditorUtility.SetDirty( control );

			var position = control.transform.position;
			var offset = control.GetCenter() - position;

			control.transform.position = new Vector3(
				averagedCenter.x - offset.x,
				position.y,
				position.z
			);

			control.MakePixelPerfect();
			control.ResetLayout( false, true );

		}

		dfGUIManager.RefreshAll( true );

	}

	private void alignCenterVert()
	{

		var centers = new List<Vector3>();

		var controls =
			Selection.gameObjects
			.Select( c => c.GetComponent<dfControl>() )
			.Where( c => c != null )
			.ToList();

		for( int i = 0; i < controls.Count; i++ )
		{
			var control = controls[ i ];
			centers.Add( control.GetCenter() );
		}

		var averagedCenter = centers[ 0 ];
		for( int i = 1; i < centers.Count; i++ )
		{
			averagedCenter = averagedCenter + centers[ i ];
		}

		averagedCenter /= centers.Count;

		markUndo( controls, "Align Center Vertically" );

		for( int i = 0; i < controls.Count; i++ )
		{

			var control = controls[ i ];

			EditorUtility.SetDirty( control );

			var position = control.transform.position;
			var offset = control.GetCenter() - position;

			control.transform.position = new Vector3(
				position.x,
				averagedCenter.y - offset.y,
				position.z
			);

			control.MakePixelPerfect();
			control.ResetLayout( false, true );

		}

		dfGUIManager.RefreshAll( true );

	}

	private void makeSameSizeHorizontally()
	{

		var maxWidth = 0f;

		var controls =
			Selection.gameObjects
			.Select( c => c.GetComponent<dfControl>() )
			.Where( c => c != null )
			.ToList();

		for( int i = 0; i < controls.Count; i++ )
		{
			maxWidth = Mathf.Max( controls[ i ].Width, maxWidth );
		}

		markUndo( controls, "Make same size" );

		for( int i = 0; i < controls.Count; i++ )
		{

			var control = controls[ i ];

			control.Width = maxWidth;
			control.PerformLayout();
			control.MakePixelPerfect();

			EditorUtility.SetDirty( control );

		}

		dfGUIManager.RefreshAll( true );

	}

	private void makeSameSizeVertically()
	{

		var maxHeight = 0f;

		var controls =
			Selection.gameObjects
			.Select( c => c.GetComponent<dfControl>() )
			.Where( c => c != null )
			.ToList();

		for( int i = 0; i < controls.Count; i++ )
		{
			maxHeight = Mathf.Max( controls[ i ].Height, maxHeight );
		}

		markUndo( controls, "Make same size" );

		for( int i = 0; i < controls.Count; i++ )
		{

			var control = controls[ i ];

			control.Height = maxHeight;
			control.PerformLayout();
			control.MakePixelPerfect();

			EditorUtility.SetDirty( control );

		}

		dfGUIManager.RefreshAll( true );

	}

	private void distributeControlsHorizontally()
	{

		var minX = float.MaxValue;
		var maxX = float.MinValue;

		var controls =
			Selection.gameObjects
			.Select( c => c.GetComponent<dfControl>() )
			.Where( c => c != null )
			.OrderBy( c => ( (dfControl)c ).transform.position.x )
			.ToList();

		if( controls.Count <= 2 )
			return;

		for( int i = 0; i < controls.Count; i++ )
		{

			var pos = controls[ i ].transform.position;

			minX = Mathf.Min( pos.x, minX );
			maxX = Mathf.Max( pos.x, maxX );

		}

		var step = ( maxX - minX ) / ( controls.Count - 1 );

		markUndo( controls, "Distribute Horizontally" );

		for( int i = 0; i < controls.Count; i++ )
		{
		
			var control = controls[ i ];
			var position = control.transform.position;

			control.transform.position = new Vector3(
				minX + i * step,
				position.y,
				position.z
			);

			control.MakePixelPerfect();
			control.ResetLayout( false, true );

		}

		dfGUIManager.RefreshAll( true );

	}

	private void distributeControlsVertically()
	{

		var minY = float.MaxValue;
		var maxY = float.MinValue;

		var controls =
			Selection.gameObjects
			.Select( c => c.GetComponent<dfControl>() )
			.Where( c => c != null )
			.OrderByDescending( c => c.transform.position.y )
			.ToList();

		if( controls.Count <= 2 )
			return;

		for( int i = 0; i < controls.Count; i++ )
		{

			var pos = controls[ i ].transform.position;

			minY = Mathf.Min( pos.y, minY );
			maxY = Mathf.Max( pos.y, maxY );

		}

		var step = ( maxY - minY ) / ( controls.Count - 1 );

		markUndo( controls, "Distribute Horizontally" );

		for( int i = 0; i < controls.Count; i++ )
		{

			var control = controls[ i ];
			var position = control.transform.position;

			control.transform.position = new Vector3(
				position.x,
				maxY - i * step,
				position.z
			);

			control.MakePixelPerfect();
			control.ResetLayout( false, true );

		}

		dfGUIManager.RefreshAll( true );

	}

	#endregion

	#endregion

	#region Private utility classes 

	protected class ContextMenuItem
	{
		public string MenuText;
		public Action<dfControl> Handler;
	}

	protected class ResizeUndoInfo
	{

		private dfControl control;
		private Vector3 position;
		private Vector2 size;

		public ResizeUndoInfo( dfControl control )
		{
			this.control = control;
			this.position = control.Position;
			this.size = control.Size;
		}

		public void UndoResize()
		{
			this.control.Size = this.size;
			this.control.Position = this.position;
		}

	}

	#endregion

}
