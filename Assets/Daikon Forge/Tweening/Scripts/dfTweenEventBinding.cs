/* Copyright 2013 Daikon Forge */
using UnityEngine;

using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Provides Editor support for binding the events of a Component to 
/// the StartTween, StopTween, and Reset methods of a Tween component
/// without having to have a seperate EventBinding for each method
/// </summary>
[Serializable]
[AddComponentMenu( "Daikon Forge/Tweens/Tween Event Binding" )]
public class dfTweenEventBinding : MonoBehaviour
{

	#region Public fields 

	/// <summary>
	/// The Tween being controlled
	/// </summary>
	public Component Tween;

	/// <summary>
	/// The component whose events will be used
	/// </summary>
	public Component EventSource;

	/// <summary>
	/// The name of the event fired by <see cref="EventSource"/> that will 
	/// cause the StartTween method to be called
	/// </summary>
	public string StartEvent;

	/// <summary>
	/// The name of the event fired by <see cref="EventSource"/> that will 
	/// cause the StopTween method to be called
	/// </summary>
	public string StopEvent;

	/// <summary>
	/// The name of the event fired by <see cref="EventSource"/> that will 
	/// cause the Reset method to be called
	/// </summary>
	public string ResetEvent;

	#endregion

	#region Private variables 

	private bool isBound = false;

	private FieldInfo startEventField;
	private FieldInfo stopEventField;
	private FieldInfo resetEventField;

	private Delegate startEventHandler;
	private Delegate stopEventHandler;
	private Delegate resetEventHandler;

	#endregion

	#region Unity events

	void OnEnable()
	{
		if( isValid() )
		{
			Bind();
		}
	}

	void Start()
	{
		if( isValid() )
		{
			Bind();
		}
	}

	void OnDisable()
	{
		Unbind();
	}

	#endregion

	#region Public methods 

	/// <summary>
	/// Binds the source events to the corresponding tween methods
	/// </summary>
	public void Bind()
	{

		if( isBound && !isValid() )
			return;

		isBound = true;

		if( !string.IsNullOrEmpty( StartEvent ) )
		{
			bindEvent( StartEvent, "Play", out startEventField, out startEventHandler );
		}

		if( !string.IsNullOrEmpty( StopEvent ) )
		{
			bindEvent( StopEvent, "Stop", out stopEventField, out stopEventHandler );
		}

		if( !string.IsNullOrEmpty( ResetEvent ) )
		{
			bindEvent( ResetEvent, "Reset", out resetEventField, out resetEventHandler );
		}

	}

	/// <summary>
	/// Unbinds all source component events
	/// </summary>
	public void Unbind()
	{

		if( !isBound )
			return;

		isBound = false;

		if( startEventField != null )
		{
			unbindEvent( startEventField, startEventHandler );
			startEventField = null;
			startEventHandler = null;
		}

		if( stopEventField != null )
		{
			unbindEvent( stopEventField, stopEventHandler );
			stopEventField = null;
			stopEventHandler = null;
		}

		if( resetEventField != null )
		{
			unbindEvent( resetEventField, resetEventHandler );
			resetEventField = null;
			resetEventHandler = null;
		}

	}

	#endregion

	#region Private utility methods

	private bool isValid()
	{

		if( Tween == null || !( Tween is dfTweenComponentBase ) )
			return false;

		if( EventSource == null )
			return false;

		var noEvents =
			string.IsNullOrEmpty( StartEvent ) &&
			string.IsNullOrEmpty( StopEvent ) &&
			string.IsNullOrEmpty( ResetEvent );

		if( noEvents )
			return false;

		var sourceType = EventSource.GetType();

		if( !string.IsNullOrEmpty( StartEvent ) && getField( sourceType, StartEvent ) == null )
			return false;

		if( !string.IsNullOrEmpty( StopEvent ) && getField( sourceType, StopEvent ) == null )
			return false;

		if( !string.IsNullOrEmpty( ResetEvent ) && getField( sourceType, ResetEvent ) == null )
			return false;

		return true;

	}

	private void unbindEvent( FieldInfo eventField, Delegate eventDelegate )
	{
		var currentDelegate = (Delegate)eventField.GetValue( EventSource );
		var newDelegate = Delegate.Remove( currentDelegate, eventDelegate );
		eventField.SetValue( EventSource, newDelegate );
	}

	private void bindEvent( string eventName, string handlerName, out FieldInfo eventField, out Delegate eventHandler )
	{

		eventField = null;
		eventHandler = null;

		var method = Tween.GetType().GetMethod( handlerName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
		if( method == null )
		{
			throw new MissingMemberException( "Method not found: " + handlerName );
		}

		eventField = getField( EventSource.GetType(), eventName );
		if( eventField == null )
		{
			throw new MissingMemberException( "Event not found: " + eventName );
		}

		try
		{

			var invokeMethod = eventField.FieldType.GetMethod( "Invoke" );
			var invokeParams = invokeMethod.GetParameters();
			var handlerParams = method.GetParameters();

			if( invokeParams.Length == handlerParams.Length )
			{
				eventHandler = Delegate.CreateDelegate( eventField.FieldType, Tween, method, true );
			}
			else if( invokeParams.Length > 0 && handlerParams.Length == 0 )
			{
#if !UNITY_IPHONE
				eventHandler = createDynamicWrapper( Tween, eventField.FieldType, invokeParams, method );
#else	
				var message = string.Format( 
					"Dynamic code generation is not supported on IOS, the {0}.{1} method signature must exactly match the event signature for {2}.{3}", 
					EventSource.GetType().Name,
					handlerName, 
					Tween.GetType().Name, 
					eventName
					);

				throw new InvalidOperationException( message );
#endif
			}
			else
			{
				throw new InvalidCastException( "Event signature mismatch: " + eventHandler );
			}

		}
		catch( Exception err )
		{
			Debug.LogError( "Event binding failed - Failed to create event handler: " + err.ToString() );
			return;
		}

		var combinedDelegate = Delegate.Combine( eventHandler, (Delegate)eventField.GetValue( EventSource ) );
		eventField.SetValue( EventSource, combinedDelegate );

	}

	private FieldInfo getField( Type type, string fieldName )
	{

		return
			type.GetAllFields()
			.Where( f => f.Name == fieldName )
			.FirstOrDefault();

	}

	/// <summary>
	/// Creates a Delegate wrapper that allows a parameterless method to be used as 
	/// an event handler for an event that defines parameters. This enables the use
	/// of "notification" event handlers - Methods which either cannot make use of
	/// or don't care about event parameters. 
	/// </summary>
	private Delegate createDynamicWrapper( object target, Type delegateType, ParameterInfo[] eventParams, MethodInfo eventHandler )
	{

#if UNITY_IPHONE
		throw new InvalidOperationException( "Dynamic code generation is not supported on iOS devices" );
#else
		var paramTypes =
			new Type[] { target.GetType() }
			.Concat( eventParams.Select( p => p.ParameterType ) )
			.ToArray();

		var handler = new DynamicMethod(
			"DynamicEventWrapper_" + eventHandler.Name,
			typeof( void ),
			paramTypes
			);

		var il = handler.GetILGenerator();

		il.Emit( OpCodes.Ldarg_0 );
		il.EmitCall( OpCodes.Callvirt, eventHandler, Type.EmptyTypes );
		il.Emit( OpCodes.Ret );

		return handler.CreateDelegate( delegateType, target );
#endif

	}

	#endregion

}
