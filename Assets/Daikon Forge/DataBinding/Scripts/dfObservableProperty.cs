/* Copyright 2013 Daikon Forge */

using UnityEngine;

using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Provides functionality for querying or setting the value of an 
/// object's field or property via Reflection. 
/// </summary>
public class dfObservableProperty : IObservableValue
{

	#region Delegates 

	private delegate object ValueGetter();
	private delegate void ValueSetter( object value );

	#endregion

	#region Private instance fields

	private object lastValue;
	private bool hasChanged = false;

	private object target;
	private FieldInfo fieldInfo;
	private PropertyInfo propertyInfo;
	private MethodInfo propertyGetter;

	#endregion

	#region Constructors 

	internal dfObservableProperty( object target, string memberName )
	{
			
		var member = target.GetType().GetMember( memberName, BindingFlags.Public | BindingFlags.Instance ).FirstOrDefault();
		if( member == null )
			throw new ArgumentException( "Invalid property or field name: " + memberName, "memberName" );

		initMember( target, member );

	}

	internal dfObservableProperty( object target, FieldInfo field )
	{
		initField( target, field );
	}

	internal dfObservableProperty( object target, PropertyInfo property )
	{
		initProperty( target, property );
	}

	internal dfObservableProperty( object target, MemberInfo member )
	{
		initMember( target, member );
	}

	#endregion

	#region Public properties 

	/// <summary>
	/// Retrieves the current value of the observed property
	/// </summary>
	public object Value
	{
		get { return getter(); }
		set 
		{ 
			lastValue = value; 
			setter( value ); 
			hasChanged = false; 
		}
	}

	/// <summary>
	/// Returns TRUE if the observed property's value has changed
	/// since the last time this property was queried.
	/// </summary>
	public bool HasChanged
	{
		get
		{

			if( hasChanged )
				return true;

			var currentValue = getter();

			if( object.ReferenceEquals( currentValue, lastValue ) )
				hasChanged = false;
			else if( currentValue == null || lastValue == null )
				hasChanged = true;
			else
				hasChanged = !currentValue.Equals( lastValue );

			return hasChanged;

		}
	}

	/// <summary>
	/// Clears the HasChanged flag
	/// </summary>
	public void ClearChangedFlag()
	{
		hasChanged = false;
		lastValue = getter();
	}

	#endregion

	#region Private utility methods

	private void initMember( object target, MemberInfo member )
	{
		if( member is FieldInfo )
			initField( target, (FieldInfo)member );
		else
			initProperty( target, (PropertyInfo)member );
	}

	private void initField( object target, FieldInfo field )
	{

		this.target = target;
		this.fieldInfo = field;

		Value = getter();

	}

	private void initProperty( object target, PropertyInfo property )
	{

		this.target = target;
		this.propertyInfo = property;

		Value = getter();

	}

	#endregion

	#region Read/write values from a Property

	private object getter()
	{
		if( propertyInfo != null )
			return getPropertyValue();
		else
			return getFieldValue();
	}

	private void setter( object value )
	{
		if( propertyInfo != null )
			setPropertyValue( value );
		else
			setFieldValue( value );
	}

	private object getPropertyValue()
	{

		if( propertyGetter == null )
		{
			propertyGetter = propertyInfo.GetGetMethod();
			if( propertyGetter == null )
			{
				throw new InvalidOperationException( "Cannot read property: " + propertyInfo );
			}
		}

		return propertyGetter.Invoke( target, null );

	}

	private void setPropertyValue( object value )
	{

		var propertySet = propertyInfo.GetSetMethod();
		if( !propertyInfo.CanWrite || propertySet == null )
			return;

		var propertyType = propertyInfo.PropertyType;

		if( value == null || propertyType.IsAssignableFrom( value.GetType() ) )
		{
			propertyInfo.SetValue( target, value, null );
		}
		else
		{
			var convertedValue = Convert.ChangeType( value, propertyType );
			propertyInfo.SetValue( target, convertedValue, null );
		}

	}

	#endregion

	#region Read/write values from a Field

	private void setFieldValue( object value )
	{

		if( fieldInfo.IsLiteral )
			return;

		var fieldType = this.fieldInfo.FieldType;

		if( value == null || fieldType.IsAssignableFrom( value.GetType() ) )
		{
			fieldInfo.SetValue( target, value );
		}
		else
		{
			var convertedValue = Convert.ChangeType( value, fieldType );
			fieldInfo.SetValue( target, convertedValue );
		}

	}

	private void setFieldValueNOP( object value )
	{
		// Field is a constant, perform no action
	}

	private object getFieldValue()
	{
		return this.fieldInfo.GetValue( this.target );
	}

	#endregion

}
