using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[ExecuteInEditMode]
[Serializable]
[RequireComponent( typeof( dfPanel ) )]
[AddComponentMenu( "Daikon Forge/Examples/Coverflow/Scroller" )]
public class dfCoverflow : MonoBehaviour
{

	#region Public events 

	/// <summary>
	/// Raised whenever the SelectedIndex property's value is changed
	/// </summary>
	public event PropertyChangedEventHandler<int> SelectedIndexChanged;

	#endregion

	#region Serialized fields

	[SerializeField]
	public int selectedIndex = 0;

	[SerializeField]
	public int itemSize = 200;

	[SerializeField]
	public float time = 0.33f;

	[SerializeField]
	public int spacing = 5;

	[SerializeField]
	protected AnimationCurve rotationCurve = AnimationCurve.Linear( 0, 0, 1, 1 );

	[SerializeField]
	protected AnimationCurve opacityCurve = AnimationCurve.Linear( 0, 0, 1, 1 );

	#endregion

	#region Private runtime variables 

	private dfPanel container;
	private dfList<dfControl> controls;
	private dfAnimatedFloat currentX;
	private Vector2 touchStartPosition;

	private int lastSelected = -1;
	private bool isMouseDown = false;

	#endregion

	#region Unity events 

	public void OnEnable()
	{

		container = GetComponent<dfPanel>();
		container.Pivot = dfPivotPoint.MiddleCenter;
		container.ControlAdded += container_ControlCollectionChanged;
		container.ControlRemoved += container_ControlCollectionChanged;

		controls = new dfList<dfControl>( container.Controls );

		if( rotationCurve.keys.Length == 0 )
		{
			rotationCurve.AddKey( 0, 0 );
			rotationCurve.AddKey( 1, 1 );
		}

	}

	public void OnDisable()
	{

		if( container != null )
		{
			container.ControlAdded -= container_ControlCollectionChanged;
			container.ControlRemoved -= container_ControlCollectionChanged;
		}

	}

	public void Update()
	{

		if( controls == null || controls.Count == 0 )
		{
			selectedIndex = 0;
			return;
		}

		if( isMouseDown )
		{

			var selectedControl = findClosestItemToCenter();
			if( selectedControl != null )
			{
				lastSelected = selectedIndex = controls.IndexOf( selectedControl );
			}

		}

		selectedIndex = Mathf.Max( 0, selectedIndex );
		selectedIndex = Mathf.Min( controls.Count - 1, selectedIndex );

		if( Application.isPlaying )
			updateSlides();
		else
			layoutSlidesForEditor();

	}

	#endregion

	#region Control event handlers 

	void OnMouseEnter( dfControl control, dfMouseEventArgs args )
	{
		touchStartPosition = args.Position;
	}

	void OnMouseDown( dfControl control, dfMouseEventArgs args )
	{
		touchStartPosition = args.Position;
		isMouseDown = true;
	}

	void OnDragStart( dfControl control, dfDragEventArgs args )
	{
		if( args.Used )
		{
			isMouseDown = false;
		}
	}

	void OnMouseUp( dfControl control, dfMouseEventArgs args )
	{

		if( isMouseDown )
		{

			isMouseDown = false;

			var selectedControl = findClosestItemToCenter();
			if( selectedControl != null )
			{
				lastSelected = -1;
				selectedIndex = controls.IndexOf( selectedControl );
			}

		}

	}

	void OnMouseMove( dfControl control, dfMouseEventArgs args )
	{

		if( args is dfTouchEventArgs || isMouseDown )
		{

			if( !args.Used && ( args.Position - touchStartPosition ).magnitude > 5 )
			{
				currentX += args.MoveDelta.x;
				args.Use();
			}

		}

	}

	void OnResolutionChanged( dfControl control, Vector2 previousResolution, Vector2 currentResolution )
	{
		// Resetting lastSelected should cause the scroller to re-center 
		// the current selection
		lastSelected = -1;
	}

	void container_ControlCollectionChanged( dfControl container, dfControl child )
	{
		controls = new dfList<dfControl>( container.Controls );
	}

	void OnKeyDown( dfControl sender, dfKeyEventArgs args )
	{
		if( !args.Used )
		{
			if( args.KeyCode == KeyCode.RightArrow )
				selectedIndex += 1;
			else if( args.KeyCode == KeyCode.LeftArrow )
				selectedIndex -= 1;
			else if( args.KeyCode == KeyCode.Home )
				selectedIndex = 0;
			else if( args.KeyCode == KeyCode.End )
				selectedIndex = controls.Count - 1;
		}
	}

	void OnMouseWheel( dfControl sender, dfMouseEventArgs args )
	{

		if( args.Used )
			return;

		args.Use();
		container.Focus();

		selectedIndex -= (int)Mathf.Sign( args.WheelDelta );

	}

	void OnClick( dfControl sender, dfMouseEventArgs args )
	{

		// No need to do anything unless an actual item was clicked
		if( args.Source == container )
			return;

		// Do not process click event if the user was scrolling by 
		// dragging the mouse. Not needed for mobile.
		if( Vector2.Distance( args.Position, touchStartPosition ) > 20 )
			return;

		// Find highest-level control containing the click
		var source = args.Source;
		while( source != null && !controls.Contains( source ) )
		{
			source = source.Parent;
		}

		// If an item in the scroller was clicked, select it
		if( source != null )
		{

			// Select the clicked item
			lastSelected = -1;
			selectedIndex = controls.IndexOf( source );

			// Do not need to try to select "most centered" item
			isMouseDown = false;

		}

	}

	#endregion

	#region Private utility methods 

	private dfControl findClosestItemToCenter()
	{

		float closestDistance = float.MaxValue;
		dfControl closestControl = null;

		for( int i = 0; i < controls.Count; i++ )
		{
			var control = controls[ i ];
			var distance = ( control.transform.position - container.transform.position ).sqrMagnitude;
			if( distance <= closestDistance )
			{
				closestDistance = distance;
				closestControl = control;
			}
		}

		return closestControl;

	}

	private void layoutSlidesForEditor()
	{

		var controls = container.Controls;
		var left = 0;
		var top = ( container.Height - itemSize ) * 0.5f;

		for( int i = 0; i < controls.Count; i++ )
		{
			controls[ i ].RelativePosition = new Vector3( left, top );
			left += itemSize + Mathf.Max( 0, spacing );
		}

	}

	private void updateSlides()
	{

		if( currentX == null || selectedIndex != lastSelected )
		{

			var start = ( currentX != null ) ? currentX.Value : 0;

			currentX = new dfAnimatedFloat( start, calculateTargetPosition(), time )
			{
				EasingType = dfEasingType.SineEaseOut
			};

			lastSelected = selectedIndex;

		}
		
		var top = ( container.Height - itemSize ) * 0.5f;
		var itemPosition = new Vector3( currentX, top );

		var count = controls.Count;
		for( int i = 0; i < count; i++ )
		{

			var control = controls[ i ];

			control.Size = new Vector2( itemSize, itemSize );
			control.RelativePosition = itemPosition;
			control.Pivot = dfPivotPoint.MiddleCenter;

			if( Application.isPlaying )
			{

				var rot = Quaternion.Euler( 0, calcHorzRotation( itemPosition.x ), 0 );
				control.transform.localRotation = rot;

				var scale = calcScale( itemPosition.x );
				control.transform.localScale = Vector3.one * scale;

				control.Opacity = calcItemOpacity( itemPosition.x );

			}
			else
			{
				control.transform.localScale = Vector3.one;
				control.transform.localRotation = Quaternion.identity;
			}

			itemPosition.x += itemSize + spacing;

		}

		if( Application.isPlaying )
		{

			var index = 0;
			for( int i = 0; i < selectedIndex; i++ )
			{
				controls[ i ].ZOrder = index++;
			}
			for( int i = count - 1; i >= selectedIndex; i-- )
			{
				controls[ i ].ZOrder = index++;
			}

		}

	}

	private float calcScale( float offset )
	{

		var center = ( container.Width - itemSize ) * 0.5f;
		var distance = Mathf.Abs( center - offset );
		var totalSize = getTotalSize();

		return Mathf.Max( 1f - ( (float)distance / (float)totalSize ), 0.85f );

	}

	private float calcItemOpacity( float offset )
	{

		var center = ( container.Width - itemSize ) * 0.5f;
		var distance = Mathf.Abs( center - offset );
		var totalSize = getTotalSize();

		var lerp = ( (float)distance / (float)totalSize );
		return 1f - opacityCurve.Evaluate( lerp );

	}

	private float calcHorzRotation( float offset )
	{

		var center = ( container.Width - itemSize ) * 0.5f;
		var distance = Mathf.Abs( center - offset );
		var sign = Mathf.Sign( center - offset );
		var totalSize = getTotalSize();

		var lerp = (float)distance / (float)totalSize;
		lerp = rotationCurve.Evaluate( lerp );

		return ( lerp * 90f ) * -sign;

	}

	private int getTotalSize()
	{
		var count = controls.Count;
		var totalSize = count * itemSize + Mathf.Max( count, 0 ) * spacing;
		return totalSize;
	}

	private float calculateTargetPosition()
	{

		var centerPosition = ( container.Width - itemSize ) * 0.5f;

		var targetPosition = centerPosition - selectedIndex * itemSize;
		if( selectedIndex > 0 )
			targetPosition -= selectedIndex * spacing;

		return targetPosition;

	}

	#endregion

}
