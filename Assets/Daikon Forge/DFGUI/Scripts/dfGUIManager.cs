/* Copyright 2013 Daikon Forge */
using UnityEngine;

using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// The dfGUIManager class is responsible for compiling all control rendering 
/// data into a Mesh and rendering that Mesh to the screen. This class is the 
/// primary workhorse of the DF-GUI library and in conjunction with <see cref="dfControl"/>
/// forms the core of this library's functionality.
/// </summary>
[Serializable]
[ExecuteInEditMode]
[RequireComponent( typeof( MeshRenderer ) )]
[RequireComponent( typeof( MeshFilter ) )]
[RequireComponent( typeof( BoxCollider ) )]
[RequireComponent( typeof( dfInputManager ) )]
[AddComponentMenu( "Daikon Forge/User Interface/GUI Manager" )]
public class dfGUIManager : MonoBehaviour
{

	#region Callback and event definitions

	[dfEventCategory( "Modal Dialog" )]
	public delegate void ModalPoppedCallback( dfControl control );

	[dfEventCategory( "Global Callbacks" )]
	public delegate void RenderCallback( dfGUIManager manager );

	/// <summary>
	/// Called before a dfGUIManager instance begins rendering the user interface
	/// </summary>
	public static event RenderCallback BeforeRender;

	/// <summary>
	/// Called after a dfGUIManager instance has finished rendering the user interface
	/// </summary>
	public static event RenderCallback AfterRender;

	#endregion

	#region Serialized protected members

	[SerializeField]
	protected float uiScale = 1f;

	[SerializeField]
	protected dfInputManager inputManager;

	[SerializeField]
	protected int fixedWidth = -1;

	[SerializeField]
	protected int fixedHeight = 600;

	[SerializeField]
	protected dfAtlas atlas;

	[SerializeField]
	protected dfFont defaultFont;

	[SerializeField]
	protected bool mergeMaterials = false;

	[SerializeField]
	protected bool pixelPerfectMode = true;

	[SerializeField]
	protected Camera renderCamera = null;

	[SerializeField]
	protected bool generateNormals = false;

	[SerializeField]
	protected bool consumeMouseEvents = true;

	[SerializeField]
	protected bool overrideCamera = false;

	[SerializeField]
	protected int renderQueueBase = 3000;

	#endregion

	#region Static fields

	/// <summary>
	/// Global reference to the control that currently has input focus
	/// </summary>
	private static dfControl activeControl = null;

	/// <summary>
	/// Used to maintain a stack of "modal" controls
	/// </summary>
	private static Stack<ModalControlReference> modalControlStack = new Stack<ModalControlReference>();

	#endregion

	#region Private non-serialized fields

	private dfGUICamera guiCamera;
	private Mesh[] renderMesh;
	private MeshFilter renderFilter;
	private MeshRenderer meshRenderer;
	private int activeRenderMesh = 0;
	private int cachedChildCount = 0;
	private bool isDirty;
	private Vector2 cachedScreenSize;
	private Vector3[] corners = new Vector3[ 4 ];

	private dfList<Rect> occluders = new dfList<Rect>( 256 );

	private Stack<ClipRegion> clipStack = new Stack<ClipRegion>();
	private static dfRenderData masterBuffer = new dfRenderData( 4096 );
	private dfList<dfRenderData> drawCallBuffers = new dfList<dfRenderData>();
	private List<int> submeshes = new List<int>();
	private int drawCallCount = 0;
	private Vector2 uiOffset = Vector2.zero;

	#endregion

	#region Public properties

	/// <summary>Returns the total number of draw calls required to render this <see cref="dfGUIManager"/> instance during the last frame </summary>
	public int TotalDrawCalls { get; private set; }

	/// <summary>Returns the total number of triangles this <see cref="dfGUIManager"/> instance rendered during the last frame </summary>
	public int TotalTriangles { get; private set; }

	/// <summary>Returns the total number of controls this <see cref="dfGUIManager"/> instance rendered during the last frame </summary>
	public int ControlsRendered { get; private set; }

	/// <summary>Returns the total number of frames this <see cref="dfGUIManager"/> instance has rendered </summary>
	public int FramesRendered { get; private set; }

	/// <summary>
	/// Returns a reference to the <see cref="dfControl"/> instance that currently has input focus
	/// </summary>
	public static dfControl ActiveControl { get { return activeControl; } }

	/// <summary>
	/// Gets or sets the multiplier by which the entire UI will be scaled
	/// </summary>
	public float UIScale
	{
		get { return this.uiScale; }
		set
		{
			if( !Mathf.Approximately( value, this.uiScale ) )
			{
				this.uiScale = value;
				onResolutionChanged();
			}
		}
	}

	/// <summary>
	/// Gets or sets the amount and direction of "offset" for the entire
	/// user interface. Useful for "panning" or implementing "shake".
	/// </summary>
	public Vector2 UIOffset
	{
		get { return this.uiOffset; }
		set
		{
			if( !Vector2.Equals( this.uiOffset, value ) )
			{
				this.uiOffset = value;
				Invalidate();
			}
		}
	}

	/// <summary>
	/// Returns the <see cref="UnityEngine.Camera"/> that is used to render 
	/// the <see cref="dfGUIManager"/> and all of its controls
	/// </summary>
	public Camera RenderCamera
	{
		get { return renderCamera; }
		set
		{
			if( !object.ReferenceEquals( renderCamera, value ) )
			{

				this.renderCamera = value;
				Invalidate();

				if( value != null && value.gameObject.GetComponent<dfGUICamera>() == null )
				{
					value.gameObject.AddComponent<dfGUICamera>();
				}

				if( this.inputManager != null )
				{
					this.inputManager.RenderCamera = value;
				}

			}
		}
	}

	/// <summary>
	/// Gets/Sets a value indicating whether the GUIManager should attempt
	/// to consolidate drawcalls that use the same Material instance. This 
	/// can reduce the number of draw calls in some cases, but depending on 
	/// scene complexity may also affect control render order.
	/// </summary>
	public bool MergeMaterials
	{
		get { return this.mergeMaterials; }
		set
		{
			if( value != this.mergeMaterials )
			{
				this.mergeMaterials = value;
				invalidateAllControls();
			}
		}
	}

	/// <summary>
	/// Gets or sets whether normals and tangents will be generated for the 
	/// rendered output, which is needed by some shaders. Defaults to FALSE.
	/// </summary>
	public bool GenerateNormals
	{
		get { return this.generateNormals; }
		set
		{
			if( value != this.generateNormals )
			{

				this.generateNormals = value;

				if( this.renderMesh != null )
				{
					renderMesh[ 0 ].Clear();
					renderMesh[ 1 ].Clear();
				}

				dfRenderData.FlushObjectPool();
				invalidateAllControls();

			}
		}
	}

	/// <summary>
	/// Gets/Sets a value indicating whether controls should be resized at
	/// runtime to retain the same pixel dimensions as design time. If this
	/// value is set to TRUE, controls will always remain at the same pixel
	/// resolution regardless of the resolution of the game. If set to FALSE,
	/// controls will be scaled to fit the target resolution.
	/// </summary>
	public bool PixelPerfectMode
	{
		get { return this.pixelPerfectMode; }
		set
		{
			if( value != this.pixelPerfectMode )
			{
				this.pixelPerfectMode = value;
				onResolutionChanged();
				Invalidate();
			}
		}
	}

	/// <summary>
	/// The default <see cref="dfAtlas">Texture Atlas</see> containing the images used
	/// to render controls in this <see cref="dfGUIManager"/>. New controls added to the
	/// scene will use this Atlas by default.
	/// </summary>
	public dfAtlas DefaultAtlas
	{
		get { return atlas; }
		set
		{
			if( !dfAtlas.Equals( value, atlas ) )
			{
				this.atlas = value;
				invalidateAllControls();
			}
		}
	}

	/// <summary>
	/// The default <see cref="dfFont">Bitmapped Font</see> that will be assigned
	/// to new controls added to the scene
	/// </summary>
	public dfFont DefaultFont
	{
		get { return defaultFont; }
		set
		{
			if( value != this.defaultFont )
			{
				this.defaultFont = value;
				invalidateAllControls();
			}
		}
	}

	/// <summary>
	/// Returns the width of the target screen size
	/// </summary>
	public int FixedWidth
	{
		get { return this.fixedWidth; }
		set
		{
			if( value != this.fixedWidth )
			{
				this.fixedWidth = value;
				onResolutionChanged();
			}
		}
	}

	/// <summary>
	/// Gets/Sets the height of the target screen size.
	/// </summary>
	public int FixedHeight
	{
		get { return this.fixedHeight; }
		set
		{
			if( value != this.fixedHeight )
			{
				var previousValue = this.fixedHeight;
				this.fixedHeight = value;

				onResolutionChanged( previousValue, value );

			}
		}
	}

	/// <summary>
	/// Gets or sets whether mouse events generated on an active control
	/// will be "consumed" (unavailable for other in-game processing)
	/// </summary>
	public bool ConsumeMouseEvents
	{
		get { return this.consumeMouseEvents; }
		set { this.consumeMouseEvents = value; }
	}

	/// <summary>
	/// Gets or sets whether user scripts will override the RenderCamera's
	/// settins. If set to TRUE, then user scripts will be responsible for
	/// all camera settings on the UI camera
	/// </summary>
	public bool OverrideCamera
	{
		get { return this.overrideCamera; }
		set { this.overrideCamera = value; }
	}

	#endregion

	#region Unity events

	public void OnGUI()
	{

		if( overrideCamera || !consumeMouseEvents || !Application.isPlaying || occluders == null )
			return;

		// This code prevents "click through" by iterating 
		// through the list of rendered control positions
		// and determining if the mouse or touch currently
		// overlaps. If it does overlap, then a GUI.Box()
		// occluder is rendered and the current event is 
		// consumed.

		var mousePosition = Input.mousePosition;
		mousePosition.y = Screen.height - mousePosition.y;

		if( modalControlStack.Count > 0 )
		{
			// If there is a modal control/window being displayed, block 
			// mouse/touch input for the entire screen.
			GUI.Box( new Rect( 0, 0, Screen.width, Screen.height ), GUIContent.none, GUIStyle.none );
		}

		for( int i = 0; i < occluders.Count; i++ )
		{
			if( occluders[ i ].Contains( mousePosition ) )
			{
				GUI.Box( occluders[ i ], GUIContent.none, GUIStyle.none );
				break;
			}
		}

		if( Event.current.isMouse && Input.touchCount > 0 )
		{
			var touchCount = Input.touchCount;
			for( int i = 0; i < touchCount; i++ )
			{
				var touch = Input.GetTouch( i );
				if( touch.phase == TouchPhase.Began )
				{

					var touchPosition = touch.position;
					touchPosition.y = Screen.height - touchPosition.y;

					if( occluders.Any( x => x.Contains( touchPosition ) ) )
					{
						Event.current.Use();
						break;
					}

				}
			}
		}

	}

#if UNITY_EDITOR

	public void OnDrawGizmos()
	{

		if( meshRenderer != null )
		{
			var debugShowMesh = UnityEditor.EditorPrefs.GetBool( "dfGUIManager.ShowMesh", false );
			UnityEditor.EditorUtility.SetSelectedWireframeHidden( meshRenderer, !debugShowMesh );
		}

		GetComponent<Collider>().hideFlags = HideFlags.HideInInspector;

		// Calculate the screen size in "pixels"
		var screenSize = GetScreenSize() * PixelsToUnits();

		// Rendering a clear cube allows the user to click on the control
		// in the Unity Editor Scene Manager
		Gizmos.color = Color.clear;
		Gizmos.DrawCube( transform.position, screenSize );

		// Render the outline of the view
		Gizmos.color = new UnityEngine.Color( 0, 1, 0, 0.3f );
		var corners = GetCorners();
		for( int i = 0; i < corners.Length; i++ )
		{
			Gizmos.DrawLine( corners[ i ], corners[ ( i + 1 ) % corners.Length ] );
		}

		#region Show "Safe Area"

		var showSafeArea = UnityEditor.EditorPrefs.GetBool( "ShowSafeArea", false );
		if( showSafeArea )
		{

			var safeAreaMargin = UnityEditor.EditorPrefs.GetFloat( "SafeAreaMargin", 10f ) * 0.01f;

			// Scale corners
			var scale = 1f - safeAreaMargin;
			var center = corners[ 0 ] + ( corners[ 2 ] - corners[ 0 ] ) * 0.5f;
			for( int i = 0; i < corners.Length; i++ )
			{
				corners[ i ] = Vector3.Lerp( center, corners[ i ], scale );
			}

			Gizmos.color = new UnityEngine.Color( 0, 1, 0, 0.5f );
			for( int i = 0; i < corners.Length; i++ )
			{
				Gizmos.DrawLine( corners[ i ], corners[ ( i + 1 ) % corners.Length ] );
			}

		}

		#endregion

		// If viewing the Game tab with gizmos on, there will be no 
		// currentDrawingSceneView, so skip the rest of this function
		if( UnityEditor.SceneView.currentDrawingSceneView == null )
			return;

		if( UnityEditor.EditorPrefs.GetBool( "dfGUIManager.ShowRulers", true ) )
		{
			drawRulers();
		}

		if( UnityEditor.EditorPrefs.GetBool( "dfGUIManager.ShowGrid", false ) )
		{
			drawGrid();
		}

	}

	public void OnDrawGizmosSelected()
	{

		if( !UnityEditor.Selection.activeGameObject.transform.IsChildOf( this.transform ) )
		{
			return;
		}

		// Render the outline of the view
		Gizmos.color = UnityEngine.Color.green;
		var corners = GetCorners();
		for( int i = 0; i < corners.Length; i++ )
		{
			Gizmos.DrawLine( corners[ i ], corners[ ( i + 1 ) % corners.Length ] );
		}

	}

	private void drawGrid()
	{

		if( !UnityEditor.SceneView.currentDrawingSceneView.orthographic )
			return;

		var corners = GetCorners();

		const int SHOW_GRID_THRESHOLD = 512;

		// If the on-screen view is too zoomed out, don't show the rulers
		var screenUL = UnityEditor.HandleUtility.WorldToGUIPoint( corners[ 0 ] );
		var screenLR = UnityEditor.HandleUtility.WorldToGUIPoint( corners[ 2 ] );
		var screenSize = Vector3.Distance( screenUL, screenLR );
		if( screenSize < SHOW_GRID_THRESHOLD )
			return;

		Gizmos.color = new UnityEngine.Color( 0, 1, 0, 0.075f );

		var gridSize = UnityEditor.EditorPrefs.GetInt( "dfGUIManager.GridSize", 20 );
		if( gridSize < 5 )
			return;

		for( int x = 0; x < FixedWidth; x += gridSize )
		{

			var pos1 = Vector3.Lerp( corners[ 0 ], corners[ 1 ], (float)x / (float)FixedWidth );
			var pos2 = Vector3.Lerp( corners[ 3 ], corners[ 2 ], (float)x / (float)FixedWidth );

			Gizmos.DrawLine( pos1, pos2 );

		}

		for( int y = 0; y < FixedHeight; y += gridSize )
		{

			var pos1 = Vector3.Lerp( corners[ 0 ], corners[ 3 ], (float)y / (float)FixedHeight );
			var pos2 = Vector3.Lerp( corners[ 1 ], corners[ 2 ], (float)y / (float)FixedHeight );

			Gizmos.DrawLine( pos1, pos2 );

		}

	}

	private void drawRulers()
	{

		if( !UnityEditor.SceneView.currentDrawingSceneView.orthographic )
			return;

		var corners = GetCorners();

		const int SHOW_RULERS_THRESHOLD = 128;
		const int SHOW_SMALL_THRESHOLD = 200;
		const int INCREASE_LINESIZE_THRESHOLD = 768;
		const int SHOW_TICKS_THRESHOLD = 1280;

		// If the on-screen view is too zoomed out, don't show the rulers
		var screenUL = UnityEditor.HandleUtility.WorldToGUIPoint( corners[ 0 ] );
		var screenLL = UnityEditor.HandleUtility.WorldToGUIPoint( corners[ 3 ] );
		var screenSize = Vector3.Distance( screenUL, screenLL );
		if( screenSize < SHOW_RULERS_THRESHOLD )
			return;

		Gizmos.color = new UnityEngine.Color( 0, 1, 0, 0.5f );

		var lerpMult = Mathf.Lerp( 3, 1, INCREASE_LINESIZE_THRESHOLD / screenSize );
		var lineSize = Mathf.Lerp( 20f, 5f, SHOW_SMALL_THRESHOLD / screenSize ) / Vector3.Distance( screenUL, screenLL ) * lerpMult;

		var up = Vector3.up * lineSize;
		var left = Vector3.left * lineSize;

		for( int x = 0; x < FixedWidth; x += 2 )
		{

			var pos1 = Vector3.Lerp( corners[ 0 ], corners[ 1 ], (float)x / (float)FixedWidth );
			var pos2 = Vector3.Lerp( corners[ 3 ], corners[ 2 ], (float)x / (float)FixedWidth );

			if( x % 50 == 0 )
			{
				Gizmos.DrawLine( pos1, pos1 + up * 3 );
				Gizmos.DrawLine( pos2, pos2 - up * 3 );
			}
			else if( screenSize >= SHOW_SMALL_THRESHOLD && x % 10 == 0 )
			{
				Gizmos.DrawLine( pos1, pos1 + up );
				Gizmos.DrawLine( pos2, pos2 - up );
			}
			else if( screenSize >= SHOW_TICKS_THRESHOLD )
			{
				Gizmos.DrawLine( pos1, pos1 + up * 0.5f );
				Gizmos.DrawLine( pos2, pos2 - up * 0.5f );
			}

		}

		for( int y = 0; y < FixedHeight; y += 2 )
		{

			var pos1 = Vector3.Lerp( corners[ 0 ], corners[ 3 ], (float)y / (float)FixedHeight );
			var pos2 = Vector3.Lerp( corners[ 1 ], corners[ 2 ], (float)y / (float)FixedHeight );

			if( y % 50 == 0 )
			{
				Gizmos.DrawLine( pos1, pos1 + left * 3 );
				Gizmos.DrawLine( pos2, pos2 - left * 3 );
			}
			else if( screenSize >= SHOW_SMALL_THRESHOLD && y % 10 == 0 )
			{
				Gizmos.DrawLine( pos1, pos1 + left );
				Gizmos.DrawLine( pos2, pos2 - left );
			}
			else if( screenSize >= SHOW_TICKS_THRESHOLD )
			{
				Gizmos.DrawLine( pos1, pos1 + left * 0.5f );
				Gizmos.DrawLine( pos2, pos2 - left * 0.5f );
			}

		}

		// Draw lines at ends of rulers, looks kind of unfinished otherwise
		Gizmos.DrawLine( corners[ 1 ], corners[ 1 ] + up * 3 );
		Gizmos.DrawLine( corners[ 2 ], corners[ 2 ] - left * 3 );
		Gizmos.DrawLine( corners[ 2 ], corners[ 2 ] - up * 3 );
		Gizmos.DrawLine( corners[ 3 ], corners[ 3 ] + left * 3 );

		// If this view is the selected object or more than one object is
		// selected, do not proceed with rendering the extents of the 
		// selected object
		if( UnityEditor.Selection.activeGameObject == null || UnityEditor.Selection.activeObject == this || UnityEditor.Selection.gameObjects.Length > 1 )
			return;

		// If the selected object is not a dfControl instance, abort
		var control = UnityEditor.Selection.activeGameObject.GetComponent<dfControl>();
		if( control == null )
			return;

		Gizmos.color = Color.white;

		var controlCorners = control.GetCorners();

		// Draw extents on top ruler
		var closest1 = closestPoint( corners[ 0 ], corners[ 1 ], controlCorners[ 0 ] );
		var closest2 = closestPoint( corners[ 0 ], corners[ 1 ], controlCorners[ 1 ] );
		Gizmos.DrawLine( closest1, closest1 + up * 2 );
		Gizmos.DrawLine( closest2, closest2 + up * 2 );

		// Draw extents on bottom ruler
		closest1 = closestPoint( corners[ 2 ], corners[ 3 ], controlCorners[ 0 ] );
		closest2 = closestPoint( corners[ 2 ], corners[ 3 ], controlCorners[ 1 ] );
		Gizmos.DrawLine( closest1, closest1 - up * 2 );
		Gizmos.DrawLine( closest2, closest2 - up * 2 );

		// Draw extents on left ruler
		closest1 = closestPoint( corners[ 0 ], corners[ 3 ], controlCorners[ 0 ] );
		closest2 = closestPoint( corners[ 0 ], corners[ 3 ], controlCorners[ 2 ] );
		Gizmos.DrawLine( closest1, closest1 + left * 2 );
		Gizmos.DrawLine( closest2, closest2 + left * 2 );

		// Draw extents on right ruler
		closest1 = closestPoint( corners[ 1 ], corners[ 2 ], controlCorners[ 0 ] );
		closest2 = closestPoint( corners[ 1 ], corners[ 2 ], controlCorners[ 2 ] );
		Gizmos.DrawLine( closest1, closest1 - left * 2 );
		Gizmos.DrawLine( closest2, closest2 - left * 2 );

	}

	private static Vector3 closestPoint( Vector3 start, Vector3 end, Vector3 test, bool clamp = true )
	{

		// http://www.gamedev.net/community/forums/topic.asp?topic_id=198199&whichpage=1&#1250842

		Vector3 c = test - start;				// Vector from a to Point
		Vector3 v = ( end - start ).normalized;	// Unit Vector from a to b
		float d = ( end - start ).magnitude;	// Length of the line segment
		float t = Vector3.Dot( v, c );			// Intersection point Distance from a

		// Check to see if the point is on the line
		// if not then return the endpoint
		if( clamp )
		{
			if( t < 0 ) return start;
			if( t > d ) return end;
		}

		// get the distance to move from point a
		v *= t;

		// move from point a to the nearest point on the segment
		return start + v;

	}

#endif

	public virtual void OnDestroy()
	{

		if( meshRenderer != null )
		{

			renderFilter.sharedMesh = null;

			DestroyImmediate( renderMesh[ 0 ] );
			DestroyImmediate( renderMesh[ 1 ] );

			renderMesh = null;

		}

	}

	/// <summary>
	/// Awake is called by the Unity engine when the script instance is being loaded.
	/// </summary>
	public virtual void Awake()
	{

		// Clean up any render data that might have been allocated on a previous level
		dfRenderData.FlushObjectPool();

	}

	/// <summary>
	/// This function is called by the Unity engine when the object becomes enabled and current.
	/// </summary>
	public virtual void OnEnable()
	{

		FramesRendered = 0;
		TotalDrawCalls = 0;
		TotalTriangles = 0;

		if( meshRenderer != null )
		{
			meshRenderer.enabled = true;
		}

		if( Application.isPlaying )
		{
			onResolutionChanged();
		}

	}

	/// <summary>
	/// This function is called by the Unity engine when the cotnrol becomes 
	/// disabled or inactive.
	/// </summary>
	public virtual void OnDisable()
	{

		if( meshRenderer != null )
		{
			meshRenderer.enabled = false;
		}

		//var controls = GetComponentsInChildren<dfControl>();
		//Array.Sort( controls, renderSortFunc );
		//for( int i = 0; i < controls.Length; i++ )
		//{
		//    controls[ i ].updateControlHierarchy( true );
		//    controls[ i ].PerformLayout();
		//}

	}

	/// <summary>
	/// Start is called by the Unity engine before any of the <see cref="Update"/> 
	/// methods is called for the first time
	/// </summary>
	public virtual void Start()
	{

		var sceneCameras = FindObjectsOfType( typeof( Camera ) ) as Camera[];
		for( int i = 0; i < sceneCameras.Length; i++ )
		{

			// Get rid of Unity's extremely annoying tendency to throw errors
			// about being unable to call SendMouseEventXXX because the event
			// signatures don't match. Whose idea was that, anyways? Sheesh.
			sceneCameras[ i ].eventMask &= ~( 1 << gameObject.layer );

		}

		inputManager = GetComponent<dfInputManager>() ?? gameObject.AddComponent<dfInputManager>();
		inputManager.RenderCamera = this.RenderCamera;

		FramesRendered = 0;

		invalidateAllControls();
		updateRenderOrder();

		if( meshRenderer != null )
		{
			meshRenderer.enabled = true;
		}

	}

	/// <summary>
	/// Called by the Unity engine every frame if the control component is enabled
	/// </summary>
	public virtual void Update()
	{

		if( this.renderCamera == null || !enabled )
		{
			if( meshRenderer != null )
			{
				meshRenderer.enabled = false;
			}
			return;
		}

		if( this.renderMesh == null || this.renderMesh.Length == 0 )
		{

			initialize();

			// Gets rid of annoying flash when recompiling in the Editor 
			// but we don't actually want to refresh this early otherwise
			if( Application.isEditor && !Application.isPlaying )
			{
				Render();
			}

		}

		if( cachedChildCount != transform.childCount )
		{
			cachedChildCount = transform.childCount;
			Invalidate();
		}

		// If the screen size has changed since we last checked we need to let all
		// controls know about the new screen size so that they can reposition or 
		// resize themselves, etc.
		var currentScreenSize = GetScreenSize();
		if( ( currentScreenSize - cachedScreenSize ).sqrMagnitude > float.Epsilon )
		{
			onResolutionChanged( cachedScreenSize, currentScreenSize );
			cachedScreenSize = currentScreenSize;
		}

	}

	/// <summary>
	/// Called by the Unity engine every frame (after <see cref="Update"/>) if
	/// the control component is enabled
	/// </summary>
	public virtual void LateUpdate()
	{

		if( this.renderMesh == null || this.renderMesh.Length == 0 )
		{
			initialize();
		}

		if( !Application.isPlaying )
		{

#if UNITY_EDITOR
			// Needed for proper raycasting in Editor viewport, which doesn't 
			// update with the same frequency as the runtime application
			updateRenderOrder();
#endif

			var collider = this.GetComponent<Collider>() as BoxCollider;
			if( collider != null )
			{
				var size = this.GetScreenSize() * PixelsToUnits();
				collider.center = Vector3.zero;
				collider.size = size;
			}

		}

		if( isDirty )
		{
			isDirty = false;
			//@Profiler.BeginSample( "Rendering user interface" );
			Render();
			//@Profiler.EndSample();
		}

	}

	#endregion

	#region Public methods

	/// <summary>
	/// Returns the top-most control under the given screen position.
	/// NOTE: the <paramref name="screenPosition"/> parameter should be
	/// in "screen coordinates", such as the value from Input.mousePosition
	/// </summary>
	/// <param name="screenPosition">The screen position to check</param>
	/// <returns></returns>
	public dfControl HitTest( Vector2 screenPosition )
	{

		var ray = renderCamera.ScreenPointToRay( screenPosition );
		var maxDistance = renderCamera.farClipPlane - renderCamera.nearClipPlane;

		var hits = Physics.RaycastAll( ray, maxDistance, renderCamera.cullingMask );
		Array.Sort( hits, dfInputManager.raycastHitSorter );

		return inputManager.clipCast( hits );

	}

	/// <summary>
	/// Returns the GUI coordinates of a point in 3D space
	/// </summary>
	/// <param name="worldPoint"></param>
	/// <returns></returns>
	public Vector2 WorldPointToGUI( Vector3 worldPoint )
	{

		// Calculate the effective screen size
		var screenSize = GetScreenSize();

		// Obtain a reference to the main camera
		var mainCamera = Camera.main;

		// Convert world point to resolution-independant screen point
		var screenPoint = Camera.main.WorldToScreenPoint( worldPoint );
		screenPoint.x = screenSize.x * ( screenPoint.x / mainCamera.pixelWidth );
		screenPoint.y = screenSize.y * ( screenPoint.y / mainCamera.pixelHeight );

		// Return screen point as GUI coordinate point 
		return ScreenToGui( screenPoint );

	}

	/// <summary>
	/// Returns a value indicating the size in 3D Units that corresponds to a single 
	/// on-screen pixel, based on the current value of the FixedHeight property.
	/// </summary>
	public float PixelsToUnits()
	{
		var fixedPixelSize = 2f / (float)FixedHeight;
		return fixedPixelSize * this.UIScale;
	}

	/// <summary>
	/// Returns the set of clipping planes used to clip child controls.
	/// Planes are specified in the following order: Left, Right, Top, Bottom
	/// </summary>
	/// <returns>Returns an array of <see cref="Plane"/> that enclose the object in world coordinates</returns>
	public virtual Plane[] GetClippingPlanes()
	{

		var corners = GetCorners();

		var right = transform.TransformDirection( Vector3.right );
		var left = transform.TransformDirection( Vector3.left );
		var up = transform.TransformDirection( Vector3.up );
		var down = transform.TransformDirection( Vector3.down );

		return new Plane[]
		{
			new Plane( right, corners[ 0 ] ),
			new Plane( left, corners[ 1 ] ),
			new Plane( up, corners[ 2 ] ),
			new Plane( down, corners[ 0 ] )
		};

	}

	/// <summary>
	/// Returns an array of Vector3 values corresponding to the global 
	/// positions of this object's bounding box. The corners are specified
	/// in the following order: Top Left, Top Right, Bottom Right, Bottom Left
	/// </summary>
	public Vector3[] GetCorners()
	{

		var p2u = PixelsToUnits();
		var size = GetScreenSize() * p2u;
		var width = size.x;
		var height = size.y;

		var upperLeft = new Vector3( -width * 0.5f, height * 0.5f );
		var upperRight = upperLeft + new Vector3( width, 0 );
		var bottomLeft = upperLeft + new Vector3( 0, -height );
		var bottomRight = upperRight + new Vector3( 0, -height );

		var matrix = transform.localToWorldMatrix;

		corners[ 0 ] = matrix.MultiplyPoint( upperLeft );
		corners[ 1 ] = matrix.MultiplyPoint( upperRight );
		corners[ 2 ] = matrix.MultiplyPoint( bottomRight );
		corners[ 3 ] = matrix.MultiplyPoint( bottomLeft );

		return corners;

	}
	public void SetWidth(int width )
	{
		fixedWidth = width;
	}
	public void SetHeight(int height)
	{
		fixedHeight = height;
	}

	/// <summary>
	/// Returns a <see cref="Vector2"/> value representing the width and 
	/// height of the screen. When the application is running, this value 
	/// will be the correct size of the screen. When called in the Editor
	/// this function will return the "design" size of the screen, which 
	/// is derived from the value of the <see cref="FixedHeight"/> property.
	/// </summary>
	/// <returns></returns>
	public Vector2 GetScreenSize()
	{

		var camera = RenderCamera;

		// If the application is running and the UI is set to "pixel perfect"
		// mode, return the actual screen size. Cannot return the actual 
		// screen size while the application is not running, because Unity
		// always returns a value of 640x480 for some reason.
		var returnActualScreenSize =
			Application.isPlaying && 
			camera != null;

		if( returnActualScreenSize )
		{
			var uiScale = PixelPerfectMode ? 1 : ( camera.pixelHeight / (float)fixedHeight ) * this.uiScale;
			return ( new Vector2( camera.pixelWidth, camera.pixelHeight ) / uiScale ).CeilToInt();
		}

		return new Vector2( FixedWidth, FixedHeight );

	}

	/// <summary>
	/// Adds a new control of the specified type to the scene
	/// </summary>
	/// <typeparam name="T">The Type of control to create</typeparam>
	/// <returns>A reference to the new <see cref="dfControl"/>instance</returns>
	public T AddControl<T>() where T : dfControl
	{
		return (T)AddControl( typeof( T ) );
	}

	/// <summary>
	/// Adds a new control of the specified type to the scene
	/// </summary>
	/// <param name="type">The Type of control to create - Must derive from <see cref="dfControl"/></param>
	/// <returns>A reference to the new <see cref="dfControl"/>instance</returns>
	public dfControl AddControl( Type type )
	{

		if( !typeof( dfControl ).IsAssignableFrom( type ) )
			throw new InvalidCastException();

		var go = new GameObject( type.Name, type );
		go.transform.parent = this.transform;
		go.layer = this.gameObject.layer;

		var child = go.GetComponent( type ) as dfControl;
		child.ZOrder = getMaxZOrder() + 1;

		return child;

	}

	private int getMaxZOrder()
	{

		var max = -1;
		using( var controls = getTopLevelControls() )
		{
			for( int i = 0; i < controls.Count; i++ )
			{
				max = Mathf.Max( max, controls[ i ].ZOrder );
			}
		}

		return max;

	}

	/// <summary>
	/// Returns the render data for a specific draw call
	/// </summary>
	/// <param name="drawCallNumber">The index of the draw call to retrieve render information for</param>
	public dfRenderData GetDrawCallBuffer( int drawCallNumber )
	{
		return drawCallBuffers[ drawCallNumber ];
	}

	/// <summary>
	/// Returns a reference to the currently-current modal control, if any. 
	/// </summary>
	/// <returns>A reference to the currently-current modal control if one exists, NULL otherwise</returns>
	public static dfControl GetModalControl()
	{
		return ( modalControlStack.Count > 0 ) ? modalControlStack.Peek().control : null;
	}

	/// <summary>
	/// Converts "screen space" coordinates (y-up with origin at the bottom left corner of
	/// the screen) to gui coordinates (y-down with the origin at the top left corner of
	/// the screen)
	/// </summary>
	/// <param name="position">The screen-space coordinate to convert</param>
	public Vector2 ScreenToGui( Vector2 position )
	{
		position.y = GetScreenSize().y - position.y;
		return position;
	}

	/// <summary>
	/// Push a control onto the modal control stack. When a control is modal, only that control
	/// and all of its descendants will receive user input events.
	/// </summary>
	/// <param name="control">The control to make modal</param>
	/// <param name="callback">A function that will be called when the control is popped off of the modal stack. Can be null.</param>
	public static void PushModal( dfControl control, ModalPoppedCallback callback = null )
	{

		if( control == null )
			throw new NullReferenceException( "Cannot call PushModal() with a null reference" );

		modalControlStack.Push( new ModalControlReference()
		{
			control = control,
			callback = callback
		} );

	}

	/// <summary>
	/// Pop the current modal control from the modal control stack.
	/// </summary>
	public static void PopModal()
	{

		if( modalControlStack.Count == 0 )
			throw new InvalidOperationException( "Modal stack is empty" );

		var entry = modalControlStack.Pop();
		if( entry.callback != null )
		{
			entry.callback( entry.control );
		}

	}

	/// <summary>
	/// Sets input focus to the indicated control
	/// </summary>
	/// <param name="control">The control that should receive user input</param>
	public static void SetFocus( dfControl control )
	{

		if( activeControl == control )
			return;

		var prevFocus = activeControl;
		activeControl = control;

		var args = new dfFocusEventArgs( control, prevFocus );

		var prevFocusChain = dfList<dfControl>.Obtain();
		if( prevFocus != null )
		{
			var loop = prevFocus;
			while( loop != null )
			{
				prevFocusChain.Add( loop );
				loop = loop.Parent;
			}
		}

		var newFocusChain = dfList<dfControl>.Obtain();
		if( control != null )
		{
			var loop = control;
			while( loop != null )
			{
				newFocusChain.Add( loop );
				loop = loop.Parent;
			}
		}

		if( prevFocus != null )
		{

			prevFocusChain.ForEach( c =>
				{
					if( !newFocusChain.Contains( c ) )
					{
						c.OnLeaveFocus( args );
					}
				} );

			prevFocus.OnLostFocus( args );

		}

		if( control != null )
		{

			newFocusChain.ForEach( c =>
			{
				if( !prevFocusChain.Contains( c ) )
				{
					c.OnEnterFocus( args );
				}
			} );

			control.OnGotFocus( args );

		}

		newFocusChain.Release();
		prevFocusChain.Release();

	}

	/// <summary>
	/// Returns TRUE if the control currently has input focus, FALSE otherwise.
	/// </summary>
	/// <param name="control">The <see cref="dfControl"/> instance to test for input focus</param>
	public static bool HasFocus( dfControl control )
	{

		if( control == null )
			return false;

		return ( activeControl == control );

	}

	/// <summary>
	/// Returns TRUE if the control or any of its descendants currently has input focus, FALSE otherwise.
	/// </summary>
	/// <param name="control">The <see cref="dfControl"/> instance to test for input focus</param>
	public static bool ContainsFocus( dfControl control )
	{

		if( activeControl == control )
			return true;

		if( activeControl == null || control == null )
			return false;

		return activeControl.transform.IsChildOf( control.transform );

	}

	/// <summary>
	/// Brings the control to the front so that it will display over any other control 
	/// within the same container.
	/// </summary>
	/// <param name="control">The control instance to bring to front</param>
	public void BringToFront( dfControl control )
	{

		if( control.Parent != null )
			control = control.GetRootContainer();

		using( var allControls = getTopLevelControls() )
		{

			var maxIndex = 0;

			for( int i = 0; i < allControls.Count; i++ )
			{
				var test = allControls[ i ];
				if( test != control )
				{
					test.ZOrder = maxIndex++;
				}
			}

			control.ZOrder = maxIndex;

			Invalidate();

		}

	}

	/// <summary>
	/// Brings the control to the front so that it will display behind any other control 
	/// within the same container.
	/// </summary>
	/// <param name="control">The control instance to send to back</param>
	public void SendToBack( dfControl control )
	{

		if( control.Parent != null )
			control = control.GetRootContainer();

		using( var allControls = getTopLevelControls() )
		{

			var maxIndex = 1;

			for( int i = 0; i < allControls.Count; i++ )
			{
				var test = allControls[ i ];
				if( test != control )
				{
					test.ZOrder = maxIndex++;
				}
			}

			control.ZOrder = 0;

			Invalidate();

		}

	}

	/// <summary>
	/// Invalidates the user interface and requests a refresh, which will be performed
	/// on the next frame.
	/// </summary>
	public void Invalidate()
	{

		if( isDirty == true )
			return;

		// Setting isDirty to TRUE signals the GUIManager to redraw
		// the user interface on the next LateUpdate pass
		isDirty = true;

		// Make sure all render settings are correctly configured
		updateRenderSettings();

	}

	/// <summary>
	/// Refresh all <see cref="dfGUIManager instances"/> and ensure that all <see cref="dfControl"/>
	/// instances are forced to refresh as well.
	/// </summary>
	/// <param name="force">Set to TRUE to force each <see cref="dfGUIManager"/> instance to refresh immediately</param>
	public static void RefreshAll( bool force = false )
	{

		var views = FindObjectsOfType( typeof( dfGUIManager ) ) as dfGUIManager[];
		for( int i = 0; i < views.Length; i++ )
		{

			views[ i ].invalidateAllControls();

			// Only force a Refresh() while in the editor, 'cause 
			// Unity sucks at design-time refresh and we're trying 
			// to use it as a visual designer. Otherwise, it's better
			// to wait until the next Update() call while running.
			if( force || !Application.isPlaying )
			{
				views[ i ].Render();
			}

		}

#if UNITY_EDITOR
		if( force && UnityEditor.SceneView.currentDrawingSceneView != null )
		{
			UnityEditor.SceneView.currentDrawingSceneView.Repaint();
		}
#endif

	}

	/// <summary>
	/// Rebuild the user interface mesh and update the renderer so that the UI will
	/// be presented to the user on the next frame. <b>NOTE</b> : This function is
	/// automatically called internally and should not be called by user code.
	/// </summary>
	public void Render()
	{

		FramesRendered += 1;

		if( BeforeRender != null )
		{
			BeforeRender( this );
		}

		try
		{

			// Make sure all render settings are correctly configured
			//@Profiler.BeginSample( "Update render settings" );
			updateRenderSettings();
			//@Profiler.EndSample();

			// We'll be keeping track of how many controls were actually rendered,
			// as opposed to just how many exist in the scene.
			ControlsRendered = 0;
			occluders.Clear();

			// Other stats to be tracked for informational purposes
			TotalDrawCalls = 0;
			TotalTriangles = 0;

			if( RenderCamera == null || !enabled )
			{
				if( meshRenderer != null )
				{
					meshRenderer.enabled = false;
				}
				return;
			}

			if( meshRenderer != null && !meshRenderer.enabled )
			{
				meshRenderer.enabled = true;
			}

			if( renderMesh == null || renderMesh.Length == 0 )
			{
				Debug.LogError( "GUI Manager not initialized before Render() called" );
				return;
			}

			resetDrawCalls();

			// Define the main draw call buffer, which will be assigned as needed
			// by the renderControl() method
			var buffer = (dfRenderData)null;

			// Initialize the clipping region stack
			clipStack.Clear();
			clipStack.Push( ClipRegion.Obtain() );

			// This checksum is used to determine whether cached render and 
			// clipping data is still valid, and represents a unique checksum
			// of the rendering path for each control.
			uint checksum = dfChecksumUtil.START_VALUE;

			#region Render all current controls

			using( var controls = getTopLevelControls() )
			{

				updateRenderOrder( controls );

				for( int i = 0; i < controls.Count; i++ )
				{
					var control = controls[ i ];
					renderControl( ref buffer, control, checksum, 1f );
				}

			}

			#endregion

			// Remove any empty draw call buffers. There might be empty 
			// draw call buffers due to controls that were clipped.
			drawCallBuffers.RemoveAll( x => x.Vertices.Count == 0 );
			drawCallCount = drawCallBuffers.Count;

			// At this point, the drawCallCount variable contains the 
			// number of draw calls needed to render the user interface.
			this.TotalDrawCalls = drawCallCount;
			if( drawCallBuffers.Count == 0 )
			{
				if( renderFilter.sharedMesh != null )
				{
					renderFilter.sharedMesh.Clear();
				}
				return;
			}

			// Gather all Material instances needed for every draw call
			meshRenderer.sharedMaterials = gatherMaterials();

			// Consolidate all draw call buffers into one master buffer 
			// that will be used to build the Mesh
			var masterBuffer = compileMasterBuffer();
			this.TotalTriangles = masterBuffer.Triangles.Count / 3;

			// Build the master mesh
			var mesh = renderFilter.sharedMesh = getRenderMesh();
			mesh.Clear();
			mesh.vertices = masterBuffer.Vertices.Items;
			mesh.uv = masterBuffer.UV.Items;
			mesh.colors32 = masterBuffer.Colors.Items;

			// Only set the mesh normals and tangents if the GUIManager has 
			// been asked to generate that information
			if( generateNormals )
			{
				// Set the mesh normals (for lighting effects, etc)
				// TODO: Determine why normal buffer length may not be exact match for vertice buffer length on first frame
				if( masterBuffer.Normals.Items.Length == masterBuffer.Vertices.Items.Length )
				{
					mesh.normals = masterBuffer.Normals.Items;
					mesh.tangents = masterBuffer.Tangents.Items;
				}
			}

			#region Set sub-meshes

			mesh.subMeshCount = submeshes.Count;
			for( int i = 0; i < submeshes.Count; i++ )
			{

				// Calculate the start and length of the submesh array
				var startIndex = submeshes[ i ];
				var length = masterBuffer.Triangles.Count - startIndex;
				if( i < submeshes.Count - 1 )
				{
					length = submeshes[ i + 1 ] - startIndex;
				}

				// Allocating the array locally appears to reduce (not eliminate)
				// per-frame allocations tenfold compared to .ToArray(), at least
				// when measured with the profiler in the Editor.
				var submeshTriangles = new int[ length ];
				masterBuffer.Triangles.CopyTo( startIndex, submeshTriangles, 0, length );

				// Set the submesh's triangle index array
				mesh.SetTriangles( submeshTriangles, i );

			}

			#endregion

			// The clip stack is reset after every frame as it's only needed during rendering
			if( clipStack.Count != 1 ) Debug.LogError( "Clip stack not properly maintained" );
			clipStack.Pop().Release();
			clipStack.Clear();

		}
		catch( dfAbortRenderingException )
		{
			// Do nothing... This exception is thrown by any component that requires
			// the rendering pipeline to be aborted. For instance, the dfDynamicFont
			// class will throw this exception when the dynamic font atlas was 
			// rebuilt, which causes any control rendered with that dynamic font
			// to be invalid and require re-rendering.
			isDirty = true;
		}
		finally
		{
			if( AfterRender != null )
			{
				AfterRender( this );
			}
		}

	}

	#endregion

	#region Private utility methods

	private dfList<dfControl> getTopLevelControls()
	{

		var childCount = transform.childCount;

		var controls = dfList<dfControl>.Obtain( childCount );

		for( int i = 0; i < childCount; i++ )
		{
			var control = transform.GetChild( i ).GetComponent<dfControl>();
			if( control != null )
				controls.Add( control );
		}

		controls.Sort();

		return controls;

	}

	private void updateRenderSettings()
	{

		// If the user is still setting up the GUIManager, exit if mandatory
		// components are not yet created
		var camera = RenderCamera;
		if( camera == null )
			return;

		if( !overrideCamera )
		{
			updateRenderCamera( camera );
		}

		#region Enforce uniform scaling

		if( transform.hasChanged )
		{

			// Need to ensure that any scaling done is uniform. If the user 
			// attempts to change this manually (or even accidentally) it 
			// could screw up many things.
			//
			// Note that scaling the GUI Manager is not something that should
			// be done unless you have a very specific and unusual use case,
			// and is not necessary or even desirable otherwise.

			var scale = transform.localScale;
			var constrainScale =
				scale.x < float.Epsilon ||
				!Mathf.Approximately( scale.x, scale.y ) ||
				!Mathf.Approximately( scale.x, scale.z );

			if( constrainScale )
			{
				scale.y = scale.z = scale.x = Mathf.Max( scale.x, 0.001f );
				transform.localScale = scale;
			}

		}

		#endregion

		if( !overrideCamera )
		{

			// Since everything is positioned and sized according to a "design-time" pixel
			// size, we can scale the entire UI to fit actual pixel sizes by modifying
			// the camera's OrthographicSize or FOV property.
			if( Application.isPlaying && PixelPerfectMode )
			{

				var uiScale = camera.pixelHeight / (float)fixedHeight;

				camera.orthographicSize = uiScale;
				camera.fieldOfView = 60 * uiScale;

			}
			else
			{
				camera.orthographicSize = 1f;
				camera.fieldOfView = 60f;
			}

		}

		// TODO: Is setting Camera.transparencySortMode still needed?
		camera.transparencySortMode = TransparencySortMode.Orthographic;

		// cachedScreenSize is used to detect when the screen size changes, such 
		// as when the user resizes the application window
		if( cachedScreenSize.sqrMagnitude <= float.Epsilon )
		{
			cachedScreenSize = new Vector2( FixedWidth, FixedHeight );
		}

		// Resetting the hasChanged flag allows us to know when the transforms
		// have changed. This is very important because it allows us to avoid 
		// some expensive operations unless they are necessary.
		transform.hasChanged = false;

	}

	private void updateRenderCamera( Camera camera )
	{

		// If rendering to a RenderTexture, set the appropriate flags
		if( Application.isPlaying && camera.targetTexture != null )
		{
			camera.clearFlags = CameraClearFlags.SolidColor;
			camera.backgroundColor = Color.clear;
		}
		else
		{
			camera.clearFlags = CameraClearFlags.Depth;
		}

		// Make sure that the orthographic camera is set up to properly 
		// render the user interface. This should be correct by default,
		// but can get out of whack if the user switches between Perspective
		// and Orthographic views. This also helps the user during initial
		// setup of the user interface hierarchy.
		var cameraPosition = Application.isPlaying ? -(Vector3)uiOffset * PixelsToUnits() : Vector3.zero;
		if( camera.orthographic )
		{
			camera.nearClipPlane = Mathf.Min( camera.nearClipPlane, -1f );
			camera.farClipPlane = Mathf.Max( camera.farClipPlane, 1f );
		}
		else
		{

			// http://stackoverflow.com/q/2866350/154165
			var fov = camera.fieldOfView * Mathf.Deg2Rad;
			var corners = this.GetCorners();
			var width = Vector3.Distance( corners[ 3 ], corners[ 0 ] );
			var distance = width / ( 2f * Mathf.Tan( fov / 2f ) );
			var back = transform.TransformDirection( Vector3.back ) * distance;

			camera.farClipPlane = Mathf.Max( distance * 2f, camera.farClipPlane );
			cameraPosition += back;

		}

		// Calculate a half-pixel offset for the camera, if needed
		if( Application.isPlaying && needHalfPixelOffset() )
		{

			var height = camera.pixelHeight;
			var pixelSize = ( 2f / height ) * ( (float)height / (float)FixedHeight );

			// NOTE: The direction of the offset below is significant and should
			// not be changed. It doesn't match some of the other examples I've 
			// seen, but works well with the particulars of the DFGUI library.
			var offset = new Vector3(
				pixelSize * 0.5f,
				pixelSize * -0.5f,
				0
			);

			cameraPosition += offset;

		}

		if( !overrideCamera )
		{

			// Adjust camera position if needed
			if( Vector3.SqrMagnitude( camera.transform.localPosition - cameraPosition ) > float.Epsilon )
			{
				camera.transform.localPosition = cameraPosition;
			}

			camera.transform.hasChanged = false;

		}

	}

	private dfRenderData compileMasterBuffer()
	{

		submeshes.Clear();
		masterBuffer.Clear();

		for( int i = 0; i < drawCallCount; i++ )
		{

			submeshes.Add( masterBuffer.Triangles.Count );

			var buffer = drawCallBuffers[ i ];

			if( generateNormals && buffer.Normals.Count == 0 )
			{
				generateNormalsAndTangents( buffer );
			}

			masterBuffer.Merge( buffer, false );

		}

		// Translate the "world" coordinates in the buffer back into local 
		// coordinates relative to this GUIManager. This allows the GUIManager to be 
		// positioned anywhere in the scene without being distracting
		masterBuffer.ApplyTransform( transform.worldToLocalMatrix );

		return masterBuffer;

	}

	private void generateNormalsAndTangents( dfRenderData buffer )
	{

		var normal = buffer.Transform.MultiplyVector( Vector3.back ).normalized;

		var tangent = (Vector4)buffer.Transform.MultiplyVector( Vector3.right ).normalized;
		tangent.w = -1f;

		for( int i = 0; i < buffer.Vertices.Count; i++ )
		{
			buffer.Normals.Add( normal );
			buffer.Tangents.Add( tangent );
		}

	}

	private bool? applyHalfPixelOffset = null;
	private bool needHalfPixelOffset()
	{

		if( applyHalfPixelOffset.HasValue )
			return applyHalfPixelOffset.Value;

		var platform = Application.platform;
		var needsHPO =
			pixelPerfectMode &&
			(
				platform == RuntimePlatform.WindowsPlayer ||
				//platform == RuntimePlatform.WindowsWebPlayer ||
				platform == RuntimePlatform.WindowsEditor
			) && ( SystemInfo.graphicsShaderLevel < 40 );

		applyHalfPixelOffset = Application.isEditor || needsHPO;

		return needsHPO;

	}

	private Material[] gatherMaterials()
	{

		// TODO: Need to implement a material buffer cache (size-specific)

		// Incrementing counter to ensure that submeshes render
		// in the correct order on all platforms and environments
		int materialRenderOrder = renderQueueBase;

		// Reset the material cache to keep allocations to a minimum
		MaterialCache.Reset();

		// Count the number of non-null materials 
		var materialCount = drawCallBuffers.Matching( buff => buff != null && buff.Material != null );
		var index = 0;

		var renderMaterials = new Material[ materialCount ];
		for( int i = 0; i < drawCallBuffers.Count; i++ )
		{

			// Skip null Material instances (typically happens only during
			// initial control creation in the Unity Editor)
			if( drawCallBuffers[ i ].Material == null )
				continue;

			// HACK: This resolves an extremely odd issue with some
			// scenes where Unity is rendering submeshes out of order.
			// See: http://forum.unity3d.com/threads/192467-Unity-4-2-submesh-draw-order
			var drawCallMaterial = MaterialCache.Lookup( drawCallBuffers[ i ].Material );
			drawCallMaterial.renderQueue = materialRenderOrder++;

			// Copy the material to the final buffer
			renderMaterials[ index++ ] = drawCallMaterial;

		}

		return renderMaterials;

	}

	private void resetDrawCalls()
	{

		drawCallCount = 0;

		for( int i = 0; i < drawCallBuffers.Count; i++ )
		{
			// Release the draw call buffer back to the RenderData pool
			drawCallBuffers[ i ].Release();
		}

		drawCallBuffers.Clear();

	}

	private dfRenderData getDrawCallBuffer( Material material )
	{

		var buffer = (dfRenderData)null;

		if( MergeMaterials && material != null )
		{
			buffer = findDrawCallBufferByMaterial( material );
			if( buffer != null )
			{
				return buffer;
			}
		}

		buffer = dfRenderData.Obtain();
		buffer.Material = material;

		drawCallBuffers.Add( buffer );
		drawCallCount++;

		return buffer;

	}

	private dfRenderData findDrawCallBufferByMaterial( Material material )
	{

		for( int i = 0; i < drawCallCount; i++ )
		{
			if( drawCallBuffers[ i ].Material == material )
			{
				return drawCallBuffers[ i ];
			}
		}

		return null;

	}

	private Mesh getRenderMesh()
	{
		activeRenderMesh = ( activeRenderMesh == 1 ) ? 0 : 1;
		return renderMesh[ activeRenderMesh ];
	}

	private void renderControl( ref dfRenderData buffer, dfControl control, uint checksum, float opacity )
	{

		// Don't render controls that have the IsVisible flag disabled
		if( !control.GetIsVisibleRaw() )
			return;

		// Don't render controls that are invisible. Keeping a running 
		// accumulator for opacity allows us to know a control's final
		// calculated opacity
		var effectiveOpacity = opacity * control.Opacity;
		if( opacity <= 0.005f )
			return;

		// Grab the current clip region information off the stack
		var clipInfo = clipStack.Peek();

		// Update the checksum to include the current control
		checksum = dfChecksumUtil.Calculate( checksum, control.Version );

		// Retrieve the control's bounds, which will be used in intersection testing
		// and triangle clipping.
		var bounds = control.GetBounds();
		var controlRendered = false;

		if( !( control is IDFMultiRender ) )
		{

			// Ask the control to render itself and return a buffer of the 
			// information needed to render it as a Mesh
			var controlData = control.Render();
			if( controlData == null )
				return;

			if( processRenderData( ref buffer, controlData, bounds, checksum, clipInfo ) )
			{
				controlRendered = true;
			}

		}
		else
		{

			// Ask the control to render itself and return as many dfRenderData buffers
			// as needed to render all elements of the control as a Mesh
			var childBuffers = ( (IDFMultiRender)control ).RenderMultiple();

			if( childBuffers != null )
			{

				for( int i = 0; i < childBuffers.Count; i++ )
				{

					var childBuffer = childBuffers[ i ];

					if( processRenderData( ref buffer, childBuffer, bounds, checksum, clipInfo ) )
					{
						controlRendered = true;
					}

				}

			}

		}

		// Keep track of the number of controls rendered and where they 
		// appear on screen
		if( controlRendered )
		{
			ControlsRendered += 1;
			occluders.Add( getControlOccluder( control ) );
		}

		// If the control has the "Clip child controls" option set, push
		// its clip region information onto the stack so that all controls
		// lower in the hierarchy are clipped against that region.
		if( control.ClipChildren )
		{
			clipInfo = ClipRegion.Obtain( clipInfo, control );
			clipStack.Push( clipInfo );
		}

		// Render all child controls
		for( int i = 0; i < control.Controls.Count; i++ )
		{
			var child = control.Controls[ i ];
			renderControl( ref buffer, child, checksum, effectiveOpacity );
		}

		// No longer need the current control's clip region information
		if( control.ClipChildren )
		{
			clipStack.Pop().Release();
		}

	}

	private Rect getControlOccluder( dfControl control )
	{

		var screenRect = control.GetScreenRect();

		var hotZoneSize = new Vector2(
			screenRect.width * control.HotZoneScale.x,
			screenRect.height * control.HotZoneScale.y
		);

		var difference = new Vector2(
			hotZoneSize.x - screenRect.width,
			hotZoneSize.y - screenRect.height
		) * 0.5f;

		return new Rect(
			screenRect.x - difference.x,
			screenRect.y - difference.y,
			hotZoneSize.x,
			hotZoneSize.y
		);

	}

	private bool processRenderData( ref dfRenderData buffer, dfRenderData controlData, Bounds bounds, uint checksum, ClipRegion clipInfo )
	{

		// A new draw call is needed every time the current Material 
		// changes. If the control returned a buffer that is not empty
		// and uses a different Material, need to grab a new draw call
		// buffer from the object pool
		bool needNewDrawcall =
			buffer == null || 
			(
				controlData.IsValid() &&
				(
					!Shader.Equals( buffer.Shader, controlData.Shader ) ||
					(
						controlData.Material != null &&
						!controlData.Material.Equals( buffer.Material )
					)
				)
			);

		if( needNewDrawcall && controlData.IsValid() )
		{
			buffer = getDrawCallBuffer( controlData.Material );
		}

		// Ensure that the control's render data is properly clipped to the 
		// current clipping region
		if( controlData != null && controlData.IsValid() )
		{
			if( clipInfo.PerformClipping( buffer, bounds, checksum, controlData ) )
			{
				return true;
			}
		}

		return false;

	}

	private void initialize()
	{

		if( renderCamera == null )
		{
			Debug.LogError( "No camera is assigned to the GUIManager" );
			return;
		}

		meshRenderer = GetComponent<MeshRenderer>();
		meshRenderer.hideFlags = HideFlags.HideInInspector;

		renderFilter = GetComponent<MeshFilter>();
		renderFilter.hideFlags = HideFlags.HideInInspector;

		renderMesh = new Mesh[ 2 ]
		{
			new Mesh() { hideFlags = HideFlags.DontSave },
			new Mesh() { hideFlags = HideFlags.DontSave }
		};
		renderMesh[ 0 ].MarkDynamic();
		renderMesh[ 1 ].MarkDynamic();

		//HACK: Upgrade old versions of dfGUIManager which didn't persist the fixedWidth value
		if( fixedWidth < 0 )
		{

			// Select a "reasonable guess" value for FixedWidth
			fixedWidth = Mathf.RoundToInt( fixedHeight * 1.33333f );

			// Each control in the scene now has to have its layout information
			// rebuilt, otherwise the controls will potentially save incorrect
			// anchor margin information
			GetComponentsInChildren<dfControl>()
				.ToList()
				.ForEach( x => x.ResetLayout() );

		}

	}

	private dfGUICamera findCameraComponent()
	{

		if( guiCamera != null )
			return guiCamera;

		if( renderCamera == null )
			return null;

		guiCamera = renderCamera.GetComponent<dfGUICamera>();
		if( guiCamera == null )
		{
			guiCamera = renderCamera.gameObject.AddComponent<dfGUICamera>();
			guiCamera.transform.position = this.transform.position;
		}

		return guiCamera;

	}

	private void onResolutionChanged()
	{
		var newHeight = Application.isPlaying ? (int)renderCamera.pixelHeight : this.FixedHeight;
		onResolutionChanged( this.FixedHeight, newHeight );
	}

	private void onResolutionChanged( int oldSize, int currentSize )
	{

		var aspect = RenderCamera.aspect;

		var oldWidth = oldSize * aspect;
		var newWidth = currentSize * aspect;

		var oldResolution = new Vector2( oldWidth, oldSize );
		var newResolution = new Vector2( newWidth, currentSize );

		onResolutionChanged( oldResolution, newResolution );

	}

	private void onResolutionChanged( Vector2 oldSize, Vector2 currentSize )
	{

		cachedScreenSize = currentSize;
		applyHalfPixelOffset = null;

		var aspect = RenderCamera.aspect;

		var oldWidth = oldSize.y * aspect;
		var newWidth = currentSize.y * aspect;

		var oldResolution = new Vector2( oldWidth, oldSize.y );
		var newResolution = new Vector2( newWidth, currentSize.y );

		var controls = GetComponentsInChildren<dfControl>();
		Array.Sort( controls, renderSortFunc );

		// Notify all controls that the effective or actual screen resolution has changed
		for( int i = 0; i < controls.Length; i++ )
		{

			if( pixelPerfectMode && controls[ i ].Parent == null )
			{
				// Aligning on pixel boundaries first could mean less "drift" when 
				// the screen resolution changes
				controls[ i ].MakePixelPerfect();
			}

			controls[ i ].OnResolutionChanged( oldResolution, newResolution );

		}

		// Now that all of the controls are aware of the resolution change,
		// they need to update their layouts.
		for( int i = 0; i < controls.Length; i++ )
		{
			controls[ i ].PerformLayout();
		}

		// EXPERIMENT: If in pixel-perfect mode, make sure all controls
		// are pixel perfect after resolution change
		for( int i = 0; i < controls.Length && pixelPerfectMode; i++ )
		{

			if( controls[i].Parent == null )
			{
				controls[ i ].MakePixelPerfect();
			}

		}

	}

	private void invalidateAllControls()
	{

		var controls = GetComponentsInChildren<dfControl>();
		for( int i = 0; i < controls.Length; i++ )
		{
			controls[ i ].Invalidate();
		}

		updateRenderOrder();

	}

	/// <summary>
	/// Sorts dfControl instances by their RenderOrder property
	/// </summary>
	private int renderSortFunc( dfControl lhs, dfControl rhs )
	{
		return lhs.RenderOrder.CompareTo( rhs.RenderOrder );
	}

	/// <summary>
	/// Updates the render order of all controls that are rendered by this <see cref="dfGUIManager"/>
	/// </summary>
	private void updateRenderOrder( dfList<dfControl> list = null )
	{

		var allControls = list != null ? list : getTopLevelControls();

		allControls.Sort();

		var renderOrder = 0;
		for( int i = 0; i < allControls.Count; i++ )
		{
			var control = allControls[ i ];
			if( control.Parent == null )
			{
				control.setRenderOrder( ref renderOrder );
			}
		}

	}

	#endregion

	#region Private nested types

	/// <summary>
	/// Encapsulates the information about a dfControl's clipping region,
	/// and provides methods to clip a dfRenderData buffer against that
	/// clipping region
	/// </summary>
	private class ClipRegion
	{

		#region Private static fields

		private static Queue<ClipRegion> pool = new Queue<ClipRegion>();

		#endregion

		#region Private instance fields

		private dfList<Plane> planes;

		#endregion

		#region Constructors and object pooling

		public static ClipRegion Obtain()
		{
			return ( pool.Count > 0 ) ? pool.Dequeue() : new ClipRegion();
		}

		public static ClipRegion Obtain( ClipRegion parent, dfControl control )
		{

			var clip = ( pool.Count > 0 ) ? pool.Dequeue() : new ClipRegion();

			clip.planes.AddRange( control.GetClippingPlanes() );

			if( parent != null )
			{
				clip.planes.AddRange( parent.planes );
			}

			return clip;

		}

		public void Release()
		{
			planes.Clear();
			pool.Enqueue( this );
		}

		private ClipRegion()
		{
			planes = new dfList<Plane>();
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Perform triangle clipping on the dfControl's RenderData and append the results
		/// to the destination RenderData
		/// </summary>
		/// <param name="dest">The buffer which will receive the final results</param>
		/// <param name="control">A <see cref="Bounds"/> instance fully enclosing the control</param>
		/// <param name="controlData">The <see cref="RenderData"/> structure generated by the dfControl</param>
		/// <returns>Returns TRUE if the dfControl was rendered, FALSE if it was not (lies entirely outside the clipping region)</returns>
		public bool PerformClipping( dfRenderData dest, Bounds bounds, uint checksum, dfRenderData controlData )
		{

			// If the RenderData's Checksum matches the dfControl's current checksum 
			// then in the case of a dfControl which was previously determined to be 
			// either entirely inside of all clipping planes or entirely outside
			// of any clipping plane we can skip clipping and intersection testing
			if( controlData.Checksum == checksum )
			{

				if( controlData.Intersection == dfIntersectionType.Inside )
				{
					// Merge the control's rendering information without any clipping
					//@Profiler.BeginSample( "Merge cached buffer - Fully inside" );
					dest.Merge( controlData );
					//@Profiler.EndSample();
					return true;
				}
				else if( controlData.Intersection == dfIntersectionType.None )
				{
					// Control lies entirely outside of the clipping region,
					// no need to include any of its rendering information
					//@Profiler.BeginSample( "Discard cached buffer - No intersection" );
					//@Profiler.EndSample();
					return false;
				}

			}

			var wasRendered = false;

			//@Profiler.BeginSample( "Clipping buffer data" );

			dfIntersectionType intersectionTest;
			using( var clipPlanes = TestIntersection( bounds, out intersectionTest ) )
			{

				if( intersectionTest == dfIntersectionType.Inside )
				{
					//@Profiler.BeginSample( "Merging buffer - Fully inside" );
					dest.Merge( controlData );
					//@Profiler.EndSample();
					wasRendered = true;
				}
				else if( intersectionTest == dfIntersectionType.Intersecting )
				{
					//@Profiler.BeginSample( "Clipping intersecting buffer" );
					clipToPlanes( clipPlanes, controlData, dest, checksum );
					//@Profiler.EndSample();
					wasRendered = true;
				}

				controlData.Checksum = checksum;
				controlData.Intersection = intersectionTest;

			}

			//@Profiler.EndSample();
			return wasRendered;

		}

		public dfList<Plane> TestIntersection( Bounds bounds, out dfIntersectionType type )
		{

			if( planes == null || planes.Count == 0 )
			{
				type = dfIntersectionType.Inside;
				return null;
			}

			var clipPlanes = dfList<Plane>.Obtain( planes.Count );

			var center = bounds.center;
			var extents = bounds.extents;

			var intersecting = false;

			for( int i = 0; i < planes.Count; i++ )
			{

				var plane = planes[ i ];
				var planeNormal = plane.normal;
				var planeDist = plane.distance;

				// Compute the projection interval radius of b onto L(t) = b.c + t * p.n
				float r =
					extents.x * Mathf.Abs( planeNormal.x ) +
					extents.y * Mathf.Abs( planeNormal.y ) +
					extents.z * Mathf.Abs( planeNormal.z );

				// Compute distance of box center from plane
				float distance = Vector3.Dot( planeNormal, center ) + planeDist;

				// Intersection occurs when distance s falls within [-r,+r] interval
				if( Mathf.Abs( distance ) <= r )
				{
					intersecting = true;
					clipPlanes.Add( plane );
				}
				else
				{

					// If the control lies behind *any* of the planes, there
					// is no point in continuing with the rest of the test
					if( distance < -r )
					{
						type = dfIntersectionType.None;
						clipPlanes.Release();
						return null;
					}

				}

			}

			if( intersecting )
			{
				type = dfIntersectionType.Intersecting;
				return clipPlanes;
			}

			type = dfIntersectionType.Inside;
			clipPlanes.Release();

			return null;

		}

		public void clipToPlanes( dfList<Plane> planes, dfRenderData data, dfRenderData dest, uint controlChecksum )
		{

			if( data == null || data.Vertices.Count == 0 )
				return;

			if( planes == null || planes.Count == 0 )
			{
				dest.Merge( data );
				return;
			}

			dfClippingUtil.Clip( planes, data, dest );

		}

		#region Private utility methods

		private static int sortClipPlanes( Plane lhs, Plane rhs )
		{
			return lhs.distance.CompareTo( rhs.distance );
		}

		#endregion

		#endregion

	}

	/// <summary>
	/// Encapsulates a reference to a dfControl that has been flagged 
	/// as modal with the callback that will be invoked when it is no
	/// longer modal.
	/// </summary>
	private struct ModalControlReference
	{
		public dfControl control;
		public ModalPoppedCallback callback;
	}

	/// <summary>
	/// Used to cache instances of Material instances that are generated during
	/// rendering, to mitigate the effects of having to copy materials in 
	/// the WebPlayer due to a Unity bug
	/// </summary>
	private class MaterialCache
	{

		#region Static variables

		private static Dictionary<Material, Cache> cache = new Dictionary<Material, Cache>();

		#endregion

		#region Static methods

		public static Material Lookup( Material BaseMaterial )
		{

			if( BaseMaterial == null )
			{
				Debug.LogError( "Cache lookup on null material" );
				return null;
			}

			Cache item = null;
			if( cache.TryGetValue( BaseMaterial, out item ) )
			{
				return item.Obtain();
			}

			item = cache[ BaseMaterial ] = new Cache( BaseMaterial );

			return item.Obtain();

		}

		public static void Reset()
		{
			Cache.ResetAll();
		}

		#endregion

		#region Nested classes

		private class Cache
		{

			#region Static variables

			/// <summary>
			/// Duplicate list of all Cache instances created,
			/// so that we don't have to use Dictionary.Values to iterate 
			/// the list, which allocates an enumerator object
			/// </summary>
			private static List<Cache> cacheInstances = new List<Cache>();

			#endregion

			#region Private variables

			private Material baseMaterial;
			private List<Material> instances = new List<Material>( 10 );
			private int currentIndex = 0x00;

			#endregion

			#region Constructors

			private Cache()
			{
				// Do not allow the use of the parameterless constructor,
				// even via reflection
				throw new NotImplementedException();
			}

			public Cache( Material BaseMaterial )
			{

				this.baseMaterial = BaseMaterial;
				this.instances.Add( BaseMaterial );

				cacheInstances.Add( this );

			}

			#endregion

			#region Static methods

			/// <summary>
			/// Reset all cache entries 
			/// </summary>
			public static void ResetAll()
			{
				for( int i = 0; i < cacheInstances.Count; i++ )
				{
					cacheInstances[ i ].Reset();
				}
			}

			#endregion

			#region Public methods

			/// <summary>
			/// Lookup a copy of the base Material for this cache line. 
			/// Will return an existing copy if one exists, or will create
			/// a new copy if needed.
			/// </summary>
			public Material Obtain()
			{

				if( currentIndex < instances.Count )
				{
					return instances[ currentIndex++ ];
				}

				currentIndex += 1;

				var newCopy = new Material( baseMaterial )
				{
					hideFlags = HideFlags.DontSave,
					name = string.Format( "{0} (Copy {1})", baseMaterial.name, currentIndex )
				};

				instances.Add( newCopy );

				return newCopy;

			}

			/// <summary>
			/// Reset the current index in preparation for another render pass
			/// </summary>
			public void Reset()
			{
				currentIndex = 0;
			}

			#endregion

		}

		#endregion

	}

	#endregion

}
