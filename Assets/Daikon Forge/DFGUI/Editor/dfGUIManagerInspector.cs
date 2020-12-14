/* Copyright 2013 Daikon Forge */
using UnityEngine;
using UnityEditor;

using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

[CustomEditor( typeof( dfGUIManager ) )]
public class dfGUIManagerInspector : Editor
{

	private static float lastScale = 1f;
	private static int lastWidth = 800;
	private static int lastHeight = 600;

	private dfGUIManager lastSelected = null;

	public void OnEnable()
	{
		lastSelected = null;
	}

	public override void OnInspectorGUI()
	{

		var view = target as dfGUIManager;

		if( lastSelected != view )
		{
			lastSelected = view;
			lastWidth = view.FixedWidth;
			lastHeight = view.FixedHeight;
			lastScale = view.UIScale;
		}

		if( view.enabled )
		{

			var screenSize = view.GetScreenSize();
			var screenSizeFormat = string.Format( "{0}x{1}", (int)screenSize.x, (int)screenSize.y );

			var totalControls =
				view.GetComponentsInChildren<dfControl>()
				.Length;

			var statusFormat = @"
Screen size: {4}
Total draw calls: {0}
Total triangles: {1}
Controls rendered: {2}
Total controls: {3}
";

			var status = string.Format(
				statusFormat.Trim(),
				view.TotalDrawCalls,
				view.TotalTriangles,
				view.ControlsRendered,
				totalControls,
				screenSizeFormat
				);

			EditorGUILayout.HelpBox( status, MessageType.Info );

		}

		dfEditorUtil.LabelWidth = 130f;

		using( dfEditorUtil.BeginGroup( "Rendering and Behavior" ) )
		{

			if( view.RenderCamera == null )
				GUI.color = Color.red;

			var camera = EditorGUILayout.ObjectField( "Render Camera", view.RenderCamera, typeof( Camera ), true ) as Camera;
			if( camera != view.RenderCamera )
			{
				dfEditorUtil.MarkUndo( view, "Assign Render Camera" );
				view.RenderCamera = camera;
			}

			if( camera == null )
				return;

			var renderModes = new string[] { "Orthographic", "Perspective" };
			var currentMode = camera.orthographic ? 0 : 1;
			var selectedMode = EditorGUILayout.Popup( "Render Mode", currentMode, renderModes );
			if( currentMode != selectedMode )
			{

				dfEditorUtil.MarkUndo( view, "Change Render Mode" );

				if( selectedMode == 0 )
				{

					camera.orthographic = true;
					camera.nearClipPlane = -2;
					camera.farClipPlane = 2;
					camera.transform.position = view.transform.position;

					view.transform.localScale = Vector3.one;

				}
				else
				{

					camera.orthographic = false;
					camera.nearClipPlane = 0.01f;
					camera.hideFlags = (HideFlags)0x00;

					// http://stackoverflow.com/q/2866350/154165
					var fov = camera.fieldOfView * Mathf.Deg2Rad;
					var corners = view.GetCorners();
					var width = Vector3.Distance( corners[ 3 ], corners[ 0 ] );
					var distance = width / ( 2f * Mathf.Tan( fov / 2f ) );
					var back = view.transform.TransformDirection( Vector3.back ) * distance;

					camera.transform.position = view.transform.position + back;
					camera.farClipPlane = distance * 2f;

				}

			}

			var pixelPerfect = EditorGUILayout.Toggle( "Pixel Perfect", view.PixelPerfectMode );
			if( pixelPerfect != view.PixelPerfectMode )
			{
				dfEditorUtil.MarkUndo( view, "Change Pixel Perfect Mode" );
				view.PixelPerfectMode = pixelPerfect;
				view.Render();
			}

			var eatMouseEvents = EditorGUILayout.Toggle( "Consume Mouse", view.ConsumeMouseEvents );
			if( eatMouseEvents != view.ConsumeMouseEvents )
			{
				dfEditorUtil.MarkUndo( view, "Change 'Consume Mouse Events' property" );
				view.ConsumeMouseEvents = eatMouseEvents;
			}

		}

		using( dfEditorUtil.BeginGroup( "Defaults and Materials" ) )
		{

			SelectTextureAtlas( "Default Atlas", view, "DefaultAtlas", false, true, 125 );
			SelectFontDefinition( "Default Font", view.DefaultAtlas, view, "DefaultFont", true, 125 );

			var merge = EditorGUILayout.Toggle( "Merge Materials", view.MergeMaterials );
			if( merge != view.MergeMaterials )
			{
				dfEditorUtil.MarkUndo( view, "Change Material Merge Property" );
				view.MergeMaterials = merge;
				view.Render();
			}

			var generateNormals = EditorGUILayout.Toggle( "Generate Normals", view.GenerateNormals );
			if( generateNormals != view.GenerateNormals )
			{
				dfRenderData.FlushObjectPool();
				dfEditorUtil.MarkUndo( view, "Changed Generate Normals property" );
				view.GenerateNormals = generateNormals;
				view.Render();
			}

		}

		using( dfEditorUtil.BeginGroup( "Target Resolution" ) )
		{

			#region Force user to apply changes to scale 

			lastScale = EditorGUILayout.FloatField( "UI Scale", lastScale );
			GUI.enabled = !Mathf.Approximately( lastScale, view.UIScale );

			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Space( dfEditorUtil.LabelWidth + 5 );
				if( GUILayout.Button( "Apply" ) )
				{
					dfEditorUtil.MarkUndo( view, "Change UI Scale" );
					view.UIScale = lastScale;
					view.Render();
				}
			}
			EditorGUILayout.EndHorizontal();

			GUI.enabled = true;

			#endregion

			if( Application.isPlaying )
			{

				var offset = EditInt2( "Offset", "X", "Y", view.UIOffset );
				if( offset != view.UIOffset )
				{
					dfEditorUtil.MarkUndo( view, "Change UI Offset" );
					view.UIOffset = offset;
					view.Render();
				}

			}

			#region Force user to apply changes to width or height

			lastWidth = EditorGUILayout.IntField( "Screen Width", lastWidth );
			lastHeight = EditorGUILayout.IntField( "Screen Height", lastHeight );

			GUI.enabled = lastWidth != view.FixedWidth || lastHeight != view.FixedHeight;

			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Space( dfEditorUtil.LabelWidth + 5 );
				if( GUILayout.Button( "Apply" ) )
				{
					dfEditorUtil.MarkUndo( view, "Change Resolution" );
					view.FixedWidth = lastWidth;
					view.FixedHeight = lastHeight;
					view.Render();
				}
			}
			EditorGUILayout.EndHorizontal();

			GUI.enabled = true;

			#endregion

#if !UNITY_ANDROID
			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Space( dfEditorUtil.LabelWidth + 5 );
				if( GUILayout.Button( "Use Build Settings" ) )
				{

					dfEditorUtil.MarkUndo( view, "Change Resolution" );

					var newWidth = PlayerSettings.defaultScreenWidth;
					var newHeight = PlayerSettings.defaultScreenHeight;

#if UNITY_WEBPLAYER
					newWidth = PlayerSettings.defaultWebScreenWidth;
					newHeight = PlayerSettings.defaultWebScreenHeight;
#endif

					view.FixedWidth = newWidth;
					view.FixedHeight = newHeight;

					view.RenderCamera.aspect = view.FixedWidth / view.FixedHeight;
					view.Render();

					lastWidth = view.FixedWidth;
					lastHeight = view.FixedHeight;

				}
			}
			EditorGUILayout.EndHorizontal();
#endif

		}

		using( dfEditorUtil.BeginGroup( "Design-Time Properties" ) )
		{

			var showMeshConfig = EditorPrefs.GetBool( "dfGUIManager.ShowMesh", false );
			var showMesh = EditorGUILayout.Toggle( "Show Wireframe", showMeshConfig );
			if( showMesh != showMeshConfig )
			{

				EditorPrefs.SetBool( "dfGUIManager.ShowMesh", showMesh );

				var meshRenderer = view.GetComponent<MeshRenderer>();
				if( meshRenderer != null )
				{
					EditorUtility.SetSelectedWireframeHidden( meshRenderer, !showMesh );
				}

				SceneView.RepaintAll();

			}

			var showRulersConfig = EditorPrefs.GetBool( "dfGUIManager.ShowRulers", true );
			var showRulers = EditorGUILayout.Toggle( "Show Rulers", showRulersConfig );
			if( showRulers != showRulersConfig )
			{
				EditorPrefs.SetBool( "dfGUIManager.ShowRulers", showRulers );
				SceneView.RepaintAll();
			}

			var snapToGridConfig = EditorPrefs.GetBool( "dfGUIManager.SnapToGrid", false );
			var snapToGrid = EditorGUILayout.Toggle( "Snap To Grid", snapToGridConfig );
			if( snapToGrid != snapToGridConfig )
			{
				EditorPrefs.SetBool( "dfGUIManager.SnapToGrid", snapToGrid );
				SceneView.RepaintAll();
			}

			var showGridConfig = EditorPrefs.GetBool( "dfGUIManager.ShowGrid", false );
			var showGrid = EditorGUILayout.Toggle( "Show Grid", showGridConfig );
			if( showGrid != showGridConfig )
			{
				EditorPrefs.SetBool( "dfGUIManager.ShowGrid", showGrid );
				SceneView.RepaintAll();
			}

			var gridSizeConfig = EditorPrefs.GetInt( "dfGUIManager.GridSize", 25 );
			var gridSize = Mathf.Max( EditorGUILayout.IntField( "Grid Size", gridSizeConfig ), 5 );
			if( gridSize != gridSizeConfig )
			{
				EditorPrefs.SetInt( "dfGUIManager.GridSize", gridSize );
				SceneView.RepaintAll();
			}

			var showSafeAreaConfig = EditorPrefs.GetBool( "ShowSafeArea", false );
			var showSafeArea = EditorGUILayout.Toggle( "Show Safe Area", showSafeAreaConfig );
			if( showSafeArea != showSafeAreaConfig )
			{
				EditorPrefs.SetBool( "ShowSafeArea", showSafeArea );
				SceneView.RepaintAll();
			}

			if( showSafeArea )
			{
				var marginConfig = EditorPrefs.GetFloat( "SafeAreaMargin", 10f );
				var safeAreaMargin = EditorGUILayout.Slider( "Safe %", marginConfig, 0f, 50f );
				if( marginConfig != safeAreaMargin )
				{
					EditorPrefs.SetFloat( "SafeAreaMargin", safeAreaMargin );
					SceneView.RepaintAll();
				}
			}

		}

		//dfEditorUtil.DrawHorzLine();
		EditorGUILayout.Separator();

		EditorGUILayout.BeginHorizontal();
		{

			if( GUILayout.Button( "Help" ) )
			{
				var url = "http://www.daikonforge.com/dfgui/tutorials/";
				Application.OpenURL( url );
				Debug.Log( "Opened tutorial page at " + url );
			}

			if( GUILayout.Button( "Force Refresh" ) )
			{
				dfGUIManager.RefreshAll( true );
				Debug.Log( "User interface manually refreshed" );
			}

		}
		EditorGUILayout.EndHorizontal();

	}

	public void OnSceneGUI()
	{

		if( Selection.objects.Length > 1 )
		{
			return;
		}

		var view = target as dfGUIManager;

		var evt = Event.current;
		var id = GUIUtility.GetControlID( GetType().Name.GetHashCode(), FocusType.Passive );
		var eventType = evt.GetTypeForControl( id );

		if( eventType == EventType.MouseDown )
		{

			var modifierKeyPressed = evt.alt || evt.control || evt.shift;
			if( evt.button != 0 || modifierKeyPressed )
			{

				if( evt.button == 1 && !modifierKeyPressed )
				{

					// Ensure that the mouse point is actually contained within the Manager
					var ray = HandleUtility.GUIPointToWorldRay( evt.mousePosition );
					RaycastHit hitInfo;
					if( view.GetComponent<Collider>().Raycast( ray, out hitInfo, 1000 ) )
					{

						displayContextMenu();
						evt.Use();

						return;

					}

				}

				GUIUtility.hotControl = GUIUtility.keyboardControl = 0;

			}

		}

	}

	private void displayContextMenu()
	{

		var menu = new GenericMenu();

		var items = new List<ContextMenuItem>();
		FillContextMenu( items );

		var actionFunc = new System.Action<int>( ( command ) =>
		{
			var handler = items[ command ].Handler;
			handler();
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

	protected Vector2 EditInt2( string groupLabel, string label1, string label2, Vector2 value )
	{

		var retVal = Vector2.zero;

		var savedLabelWidth = dfEditorUtil.LabelWidth;

		GUILayout.BeginHorizontal();
		{

			EditorGUILayout.LabelField( groupLabel, "", GUILayout.Width( dfEditorUtil.LabelWidth - 12 ) );

			GUILayout.BeginVertical();
			{

				dfEditorUtil.LabelWidth = 60f;

				var x = EditorGUILayout.IntField( label1, Mathf.RoundToInt( value.x ) );
				var y = EditorGUILayout.IntField( label2, Mathf.RoundToInt( value.y ) );

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

	protected void FillContextMenu( List<ContextMenuItem> menu )
	{

		// Adds a menu item for each dfControl class in the assembly that has 
		// an AddComponentMenu attribute defined.
		addContextMenuChildControls( menu );
		menu.Add( new ContextMenuItem() { MenuText = "-" } );

		// Add an option to allow the user to select any Prefab that 
		// has a dfControl component as the main component
		addContextSelectPrefab( menu );

	}

	private void addContextSelectPrefab( List<ContextMenuItem> menu )
	{

		var view = target as dfGUIManager;

		// Need to determine final control position immediately, as 
		// this information is more difficult to obtain inside of an
		// anonymous delegate
		var mousePos = Event.current.mousePosition;
		var controlPosition = raycast( mousePos );

		System.Action selectPrefab = () =>
		{
			dfPrefabSelectionDialog.Show(
				"Select a prefab Control",
				typeof( dfControl ),
				( prefab ) =>
				{

					if( prefab == null )
						return;

					dfEditorUtil.MarkUndo( view, "Add child control - " + prefab.name );

					var newGameObject = PrefabUtility.InstantiatePrefab( prefab ) as GameObject;
					var childControl = newGameObject.GetComponent<dfControl>();
					childControl.transform.parent = view.transform;
					childControl.transform.position = controlPosition;

					childControl.PerformLayout();

					Selection.activeGameObject = newGameObject;

				},
				null,
				null
			);
		};

		menu.Add( new ContextMenuItem() { MenuText = "Add Prefab...", Handler = selectPrefab } );

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

		// Look for user-defined types to add to the list
		var userAssembly = getUserAssembly();
		if( userAssembly != assembly )
		{

			var userTypes = userAssembly.GetTypes();

			var assemblyTypes =
				userTypes
				.Where( t =>
					typeof( dfControl ).IsAssignableFrom( t ) &&
					t.IsDefined( typeof( AddComponentMenu ), true )
				).ToList();

			controlTypes.AddRange( assemblyTypes  );

		}

		var options = new List<ContextMenuItem>();

		for( int i = 0; i < controlTypes.Count; i++ )
		{
			var type = controlTypes[ i ];
			var componentMenuAttribute = type.GetCustomAttributes( typeof( AddComponentMenu ), true ).First() as AddComponentMenu;
			var optionText = componentMenuAttribute.componentMenu.Replace( "Daikon Forge/User Interface/", "" );
			options.Add( buildAddChildMenuItem( optionText, type ) );
		}

		options.Sort( ( lhs, rhs ) => { return lhs.MenuText.CompareTo( rhs.MenuText ); } );

		menu.AddRange( options );

	}

	/// <summary>
	/// Returns the Assembly containing user-defined types
	/// </summary>
	/// <returns></returns>
	private Assembly getUserAssembly()
	{

		var editorAssembly = typeof( Editor ).Assembly.GetName();

		var scriptTypes = Resources.FindObjectsOfTypeAll( typeof( MonoScript ) ) as MonoScript[];
		for( int i = 0; i < scriptTypes.Length; i++ )
		{

			// Fix for Unity error that results in a crash when it calls 
			// MonoScript.GetClass() on certain shaders (and other files?)
			if( scriptTypes[ i ].GetType() != typeof( MonoScript ) )
			{
				continue;
			}

			var path = AssetDatabase.GetAssetPath( scriptTypes[ i ] );
			if( string.IsNullOrEmpty( path ) || path.Contains( "editor", true ) || !path.EndsWith( ".cs", StringComparison.InvariantCultureIgnoreCase ) )
				continue;

			var scriptClass = scriptTypes[ i ].GetClass();
			if( scriptClass == null )
				continue;

			var scriptAssembly = scriptClass.Assembly;
			var referencedAssemblies = scriptAssembly.GetReferencedAssemblies();
			if( !referencedAssemblies.Contains( editorAssembly ) )
			{
				return scriptAssembly;
			}

		}

		return null;

	}

	private ContextMenuItem buildAddChildMenuItem( string optionText, Type type )
	{

		var view = target as dfGUIManager;

		// Need to determine final control position immediately, as 
		// this information is more difficult to obtain inside of an
		// anonymous delegate
		var mousePos = Event.current.mousePosition;
		var controlPosition = raycast( mousePos );

		return new ContextMenuItem()
		{
			MenuText = "Add control/" + optionText,
			Handler = () =>
			{

				var childName = type.Name;
				if( childName.StartsWith( "df" ) )
					childName = childName.Substring( 2 );

				childName = ObjectNames.NicifyVariableName( childName );

				dfEditorUtil.MarkUndo( view, "Add Control - " + childName );

				var child = view.AddControl( type );
				child.name = childName;
				child.transform.position = controlPosition;

				Selection.activeGameObject = child.gameObject;

				child.Invalidate();
				view.Render();

			}
		};

	}

	private int getMaxZOrder()
	{

		var manager = target as dfGUIManager;
		var transform = manager.transform;

		var maxValue = 0;

		for( int i = 0; i < transform.childCount; i++ )
		{
			var control = transform.GetChild( i ).GetComponent<dfControl>();
			if( control != null )
			{
				maxValue = Mathf.Max( maxValue, control.ZOrder );
			}
		}

		return maxValue;

	}

	private Vector3 raycast( Vector2 mousePos )
	{

		var view = target as dfGUIManager;

		var plane = new Plane( view.transform.rotation * Vector3.back, view.transform.position );
		var ray = HandleUtility.GUIPointToWorldRay( mousePos );

		var distance = 0f;
		plane.Raycast( ray, out distance );

		return ray.origin + ray.direction * distance;

	}

	private static void setValue( dfGUIManager control, string propertyName, object value )
	{
		var property = control.GetType().GetProperty( propertyName );
		if( property == null )
			throw new ArgumentException( "Property '" + propertyName + "' does not exist on " + control.GetType().Name );
		property.SetValue( control, value, null );
	}

	private static object getValue( dfGUIManager control, string propertyName )
	{
		var property = control.GetType().GetProperty( propertyName );
		if( property == null )
			throw new ArgumentException( "Property '" + propertyName + "' does not exist on " + control.GetType().Name );
		return property.GetValue( control, null );
	}

	protected internal static void SelectTextureAtlas( string label, dfGUIManager view, string propertyName, bool readOnly, bool colorizeIfMissing, int labelWidth = 95 )
	{

		var savedColor = GUI.color;
		var showDialog = false;

		try
		{

			var atlas = getValue( view, propertyName ) as dfAtlas;

			GUI.enabled = !readOnly;

			if( atlas == null && colorizeIfMissing )
				GUI.color = Color.red;

			dfPrefabSelectionDialog.SelectionCallback selectionCallback = delegate( GameObject item )
			{
				var newAtlas = ( item == null ) ? null : item.GetComponent<dfAtlas>();
				dfEditorUtil.MarkUndo( view, "Change Atlas" );
				setValue( view, propertyName, newAtlas );
			};

			var value = (dfAtlas)getValue( view, propertyName );

			EditorGUILayout.BeginHorizontal();
			{

				EditorGUILayout.LabelField( label, "", GUILayout.Width( labelWidth ) );

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
					showDialog = true;
				}

			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space( 2 );

			if( showDialog )
			{
				dfEditorUtil.DelayedInvoke( (System.Action)( () =>
				{
					var dialog = dfPrefabSelectionDialog.Show( "Select Texture Atlas", typeof( dfAtlas ), selectionCallback, dfTextureAtlasInspector.DrawAtlasPreview, null );
					dialog.previewSize = 200;
				} ) );
			}

		}
		finally
		{
			GUI.enabled = true;
			GUI.color = savedColor;
		}

	}

	protected internal static void SelectFontDefinition( string label, dfAtlas atlas, dfGUIManager view, string propertyName, bool colorizeIfMissing, int labelWidth = 95 )
	{

		var savedColor = GUI.color;
		var showDialog = false;

		try
		{

			GUI.enabled = ( atlas != null );

			var value = (dfFont)getValue( view, propertyName );

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
				var font = ( item == null ) ? null : item.GetComponent<dfFont>();
				dfEditorUtil.MarkUndo( view, "Change Font" );
				setValue( view, propertyName, font );
			};

			EditorGUILayout.BeginHorizontal();
			{

				EditorGUILayout.LabelField( label, "", GUILayout.Width( labelWidth ) );

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
							var draggedFont = draggedObject != null ? draggedObject.GetComponent<dfFont>() : null;
							DragAndDrop.visualMode = ( draggedFont != null ) ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.None;
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
					showDialog = true;
				}

			}
			EditorGUILayout.EndHorizontal();

			if( value != null && !dfAtlas.Equals( atlas, value.Atlas ) )
			{
				GUI.color = Color.white;
				EditorGUILayout.HelpBox( "The selected font does not use the same Atlas as the control", MessageType.Error );
			}

			GUILayout.Space( 2 );

			if( showDialog )
			{
				dfEditorUtil.DelayedInvoke( (System.Action)( () =>
				{
					dfPrefabSelectionDialog.Show( "Select Font", typeof( dfFont ), selectionCallback, dfFontDefinitionInspector.DrawFontPreview, filterCallback );
				} ) );
			}

		}
		finally
		{
			GUI.enabled = true;
			GUI.color = savedColor;
		}

	}

	protected class ContextMenuItem
	{
		public string MenuText;
		public System.Action Handler;
	}

	public bool HasFrameBounds()
	{
		return true;
	}

	public Bounds OnGetFrameBounds()
	{

		var view = target as dfGUIManager;
		var corners = view.GetCorners();

		var size = ( corners[ 2 ] - corners[ 0 ] ) * 0.5f;
		var center = corners[ 0 ] + size;

		return new Bounds( center, size * 0.85f );

	}

}

