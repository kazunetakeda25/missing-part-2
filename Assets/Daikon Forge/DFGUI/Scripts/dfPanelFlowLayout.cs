using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent( typeof( dfPanel ) )]
[ExecuteInEditMode]
[AddComponentMenu( "Daikon Forge/User Interface/Panel Addon/Flow Layout" )]
public class dfPanelFlowLayout : MonoBehaviour
{

	#region Serialized properties 

	[SerializeField]
	protected RectOffset borderPadding = new RectOffset();

	[SerializeField]
	protected Vector2 itemSpacing = new Vector2();

	[SerializeField]
	protected dfControlOrientation flowDirection = dfControlOrientation.Horizontal;

	[SerializeField]
	protected bool hideClippedControls = false;

	#endregion

	#region Public properties 

	/// <summary>
	/// Gets or sets the direction in which child controls will be arranged
	/// </summary>
	public dfControlOrientation Direction
	{
		get { return this.flowDirection; }
		set
		{
			if( value != this.flowDirection )
			{
				this.flowDirection = value;
				performLayout();
			}
		}
	}

	/// <summary>
	/// Gets or sets the amount of padding that will be applied to each control
	/// when arranging child controls
	/// </summary>
	public Vector2 ItemSpacing
	{
		get
		{
			return this.itemSpacing;
		}
		set
		{
			value = Vector2.Max( value, Vector2.zero );
			if( !Vector2.Equals( value, this.itemSpacing ) )
			{
				this.itemSpacing = value;
				performLayout();
			}
		}
	}

	/// <summary>
	/// Gets or sets the amount of padding that will be applied to the
	/// borders of the Panel
	/// </summary>
	public RectOffset BorderPadding
	{
		get
		{
			if( this.borderPadding == null ) this.borderPadding = new RectOffset();
			return this.borderPadding;
		}
		set
		{
			value = value.ConstrainPadding();
			if( !RectOffset.Equals( value, this.borderPadding ) )
			{
				this.borderPadding = value;
				performLayout();
			}
		}
	}

	/// <summary>
	/// Gets or sets whether controls which would be clipped by the 
	/// panel's border should be hidden.
	/// </summary>
	public bool HideClippedControls
	{
		get { return this.hideClippedControls; }
		set
		{
			if( value != this.hideClippedControls )
			{
				this.hideClippedControls = value;
				performLayout();
			}
		}
	}

	#endregion

	#region Private runtime variables

	private dfPanel panel;

	#endregion

	#region Unity events 

	public void OnEnable()
	{
		this.panel = GetComponent<dfPanel>();
		panel.SizeChanged += OnSizeChanged;
	}

	#endregion

	#region dfPanel events 

	public void OnControlAdded( dfControl container, dfControl child )
	{
		child.ZOrderChanged += child_ZOrderChanged;
		child.SizeChanged += child_SizeChanged;
		performLayout();
	}

	public void OnControlRemoved( dfControl container, dfControl child )
	{
		child.ZOrderChanged -= child_ZOrderChanged;
		child.SizeChanged -= child_SizeChanged;
		performLayout();
	}

	public void OnSizeChanged( dfControl control, Vector2 value )
	{
		performLayout();
	}

	void child_SizeChanged( dfControl control, Vector2 value )
	{
		performLayout();
	}

	void child_ZOrderChanged( dfControl control, int value )
	{
		performLayout();
	}

	#endregion

	#region Private utility methods

	private void performLayout()
	{

		if( panel == null )
		{
			this.panel = GetComponent<dfPanel>();
		}

		var position = new Vector3( borderPadding.left, borderPadding.top );

		var firstInLine = true;
		var maxX = panel.Width - borderPadding.right;
		var maxY = panel.Height - borderPadding.bottom;
		var maxSize = 0;

		var controls = panel.Controls;
		for( int i = 0; i < controls.Count; i++, firstInLine = false )
		{

			if( !firstInLine )
			{
				if( flowDirection == dfControlOrientation.Horizontal )
					position.x += itemSpacing.x;
				else
					position.y += itemSpacing.y;
			}

			var control = controls[ i ];

			if( flowDirection == dfControlOrientation.Horizontal )
			{
				if( !firstInLine && position.x + control.Width >= maxX )
				{
					
					position.x = borderPadding.left;
					position.y += maxSize;

					maxSize = 0;
					firstInLine = true;

				}
			}
			else
			{
				if( !firstInLine && position.y + control.Height >= maxY )
				{

					position.y = borderPadding.top;
					position.x += maxSize;

					maxSize = 0;
					firstInLine = true;

				}
			}

			control.RelativePosition = position;

			if( flowDirection == dfControlOrientation.Horizontal )
			{
				position.x += control.Width;
				maxSize = Mathf.Max( Mathf.CeilToInt( control.Height + itemSpacing.y ), maxSize );
			}
			else
			{
				position.y += control.Height;
				maxSize = Mathf.Max( Mathf.CeilToInt( control.Width + itemSpacing.x ), maxSize );
			}

			control.IsVisible = canShowControlUnclipped( control );

		}

	}

	private bool canShowControlUnclipped( dfControl control )
	{

		if( !hideClippedControls )
			return true;

		var position = control.RelativePosition;

		if( position.x + control.Width >= panel.Width - borderPadding.right )
			return false;

		if( position.y + control.Height >= panel.Height - borderPadding.bottom )
			return false;

		return true;

	}

	#endregion

}
