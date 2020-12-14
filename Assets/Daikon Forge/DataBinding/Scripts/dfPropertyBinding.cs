/* Copyright 2013 Daikon Forge */

using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Provides the ability to bind a property on one object to the value of 
/// another property on another object.
/// </summary>
[Serializable]
[AddComponentMenu( "Daikon Forge/Data Binding/Property Binding" )]
public class dfPropertyBinding : MonoBehaviour, IDataBindingComponent
{

	#region Public fields

	/// <summary>
	/// Specifies which field or property to bind to on the source component
	/// </summary>
	public dfComponentMemberInfo DataSource;

	/// <summary>
	/// Specifies which field or property to bind to on the target component
	/// </summary>
	public dfComponentMemberInfo DataTarget;

	/// <summary>
	/// If set to TRUE, the property will be synchronized 
	/// between DataSource and DataTarget. ie: When either 
	/// property changes, the other will be set to match.
	/// If set to FALSE, only changes to DataSource will 
	/// be mirrored to DataTarget.
	/// </summary>
	public bool TwoWay = false;

	#endregion

	#region Private fields 

	private dfObservableProperty sourceProperty;
	private dfObservableProperty targetProperty;

	private bool isBound = false;

	#endregion 

	#region Unity events 

	public void OnEnable()
	{
		if( !isBound && DataSource.IsValid && DataTarget.IsValid )
		{
			Bind();
		}
	}

	public void Start()
	{
		if( !isBound && DataSource.IsValid && DataTarget.IsValid )
		{
			Bind();
		}
	}

	public void OnDisable()
	{
		Unbind();
	}

	public void Update()
	{

		if( sourceProperty == null || targetProperty == null )
			return;

		if( sourceProperty.HasChanged )
		{
			targetProperty.Value = sourceProperty.Value;
			sourceProperty.ClearChangedFlag();
		}
		else if( TwoWay && targetProperty.HasChanged )
		{
			sourceProperty.Value = targetProperty.Value;
			targetProperty.ClearChangedFlag();
		}

	}

	#endregion

	#region Public methods

	/// <summary>
	/// Bind the source and target properties 
	/// </summary>
	public void Bind()
	{

		if( isBound )
			return;

		if( !DataSource.IsValid || !DataTarget.IsValid )
		{
			Debug.LogError( string.Format( "Invalid data binding configuration - Source:{0}, Target:{1}", DataSource, DataTarget ) );
			return;
		}

		sourceProperty = DataSource.GetProperty();
		targetProperty = DataTarget.GetProperty();

		isBound = ( sourceProperty != null ) && ( targetProperty != null );

		if( isBound )
		{
			// Ensure that both properties are synced at start
			targetProperty.Value = sourceProperty.Value;
		}

	}

	/// <summary>
	/// Unbind the source and target properties 
	/// </summary>
	public void Unbind()
	{

		if( !isBound )
			return;

		sourceProperty = null;
		targetProperty = null;

		isBound = false;
	}

	#endregion

	#region System.Object overrides 

	/// <summary>
	/// Returns a formatted string summarizing this object's state
	/// </summary>
	public override string ToString()
	{
			
		string sourceType = DataSource != null && DataSource.Component != null ? DataSource.Component.GetType().Name : "[null]";
		string sourceMember = DataSource != null && !string.IsNullOrEmpty( DataSource.MemberName ) ? DataSource.MemberName : "[null]";

		string targetType = DataTarget != null && DataTarget.Component != null ? DataTarget.Component.GetType().Name : "[null]";
		string targetMember = DataTarget != null && !string.IsNullOrEmpty( DataTarget.MemberName ) ? DataTarget.MemberName : "[null]";

		return string.Format( "Bind {0}.{1} -> {2}.{3}", sourceType, sourceMember, targetType, targetMember );

	}

	#endregion

}
