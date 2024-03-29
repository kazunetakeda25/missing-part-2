﻿/* Copyright 2013 Daikon Forge */
using UnityEngine;

using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Implements a standard checkbox (or toggle) control
/// </summary>
[Serializable]
[ExecuteInEditMode]
[RequireComponent( typeof( BoxCollider ) )]
[AddComponentMenu( "Daikon Forge/User Interface/Checkbox" )]
public class dfCheckbox : dfControl
{

	#region Public events

	/// <summary>
	/// Raised whenever the IsChecked property changes
	/// </summary>
	public event PropertyChangedEventHandler<bool> CheckChanged;

	#endregion

	#region Protected serialized members 

	[SerializeField]
	protected bool isChecked = false;

	[SerializeField]
	protected dfControl checkIcon = null;

	[SerializeField]
	protected dfLabel label = null;

	[SerializeField]
	protected dfControl group = null;

	#endregion

	#region Public properties 

	/// <summary>
	/// Gets or sets whether the checkbox is checked
	/// </summary>
	public bool IsChecked
	{
		get { return this.isChecked; }
		set
		{
			if( value != this.isChecked )
			{
				this.isChecked = value;
				OnCheckChanged();
			}
		}
	}

	/// <summary>
	/// Gets/Sets a reference to the dfControl used to display the checkmark icon
	/// </summary>
	public dfControl CheckIcon
	{
		get { return this.checkIcon; }
		set
		{
			if( value != this.checkIcon )
			{
				this.checkIcon = value;
				Invalidate();
			}
		}
	}

	/// <summary>
	/// The dfLabel control responsible for displaying this dfCheckbox's 
	/// Text property
	/// </summary>
	public dfLabel Label
	{
		get { return this.label; }
		set
		{
			if( value != this.label )
			{
				this.label = value;
				Invalidate();
			}
		}
	}

	/// <summary>
	/// If set, only one dfCheckbox attached to the indicated dfControl
	/// can have its IsChecked property set at a time. This is used
	/// to simulate RadioButton functionality
	/// </summary>
	public dfControl GroupContainer
	{
		get { return this.group; }
		set
		{
			if( value != group )
			{
				group = value;
				Invalidate();
			}
		}
	}

	/// <summary>
	/// The text to display on the dfCheckbox control
	/// </summary>
	public string Text
	{
		get
		{
			if( label != null ) return label.Text;
			return "[LABEL NOT SET]";
		}
		set
		{
			if( label != null )
			{
				label.Text = value;
			}
		}
	}

	/// <summary>
	/// Gets a value indicating whether the control can receive focus.
	/// </summary>
	public override bool CanFocus
	{
		get { return IsEnabled && IsVisible; }
	}

	#endregion

	#region Event handlers 

	public override void Start()
	{

		base.Start();

		if( checkIcon != null )
		{
			checkIcon.BringToFront();
			checkIcon.IsVisible = IsChecked;
		}

	}

	protected internal override void OnKeyPress( dfKeyEventArgs args )
	{

		if( args.KeyCode == KeyCode.Space )
		{
			OnClick( new dfMouseEventArgs( this, dfMouseButtons.Left, 1, new Ray(), Vector2.zero, 0 ) );
			return;
		}

		base.OnKeyPress( args );

	}

	protected internal override void OnClick( dfMouseEventArgs args )
	{

		if( group == null )
		{
			this.IsChecked = !this.IsChecked;
		}
		else
		{

			var list = transform.parent.GetComponentsInChildren<dfCheckbox>() as dfCheckbox[];
			for( int i = 0; i < list.Length; i++ )
			{
				var control = list[ i ];
				if( control != this && control.GroupContainer == this.GroupContainer && control.IsChecked )
				{
					control.IsChecked = false;
				}
			}

			this.IsChecked = true;

		}

		args.Use();

		base.OnClick( args );

	}

	protected internal void OnCheckChanged()
	{

		SignalHierarchy( "OnCheckChanged", this.isChecked );

		if( CheckChanged != null )
		{
			CheckChanged( this, this.isChecked );
		}

		if( checkIcon != null )
		{

			if( IsChecked )
			{
				checkIcon.BringToFront();
			}

			checkIcon.IsVisible = this.IsChecked;

		}

	}

	#endregion

}
