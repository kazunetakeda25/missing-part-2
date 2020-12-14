/* Copyright 2013 Daikon Forge */
using UnityEngine;
using UnityEditor;

using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

[CustomEditor( typeof( dfPropertyBinding ) )]
public class PropertyBindingEditor : Editor
{

	public override void OnInspectorGUI()
	{

		var binder = target as dfPropertyBinding;

		dfEditorUtil.LabelWidth = 100f;

		using( dfEditorUtil.BeginGroup( "Data Source" ) )
		{

			if( binder.DataSource == null )
				binder.DataSource = new dfComponentMemberInfo();

			var dataSource = binder.DataSource;

			var sourceComponent = dfEditorUtil.ComponentField( "Component", dataSource.Component );
			if( sourceComponent != dataSource.Component )
			{
				dfEditorUtil.MarkUndo( binder, "Assign DataSource Component" );
				dataSource.Component = sourceComponent;
			}

			if( sourceComponent == null )
			{

				//var defaultComponent = binder.GetComponent<dfControl>();
				//if( defaultComponent != null )
				//{
				//    dataSource.Component = defaultComponent;
				//}

				EditorGUILayout.HelpBox( "Missing component", MessageType.Error );

				return;

			}

			var sourceComponentMembers =
				getMemberList( sourceComponent )
				.Select( m => m.Name )
				.ToArray();

			var memberIndex = findIndex( sourceComponentMembers, dataSource.MemberName );
			var selectedIndex = EditorGUILayout.Popup( "Property", memberIndex, sourceComponentMembers );
			if( selectedIndex >= 0 && selectedIndex < sourceComponentMembers.Length )
			{
				var memberName = sourceComponentMembers[ selectedIndex ];
				if( memberName != dataSource.MemberName )
				{
					dfEditorUtil.MarkUndo( binder, "Assign DataSource Member" );
					dataSource.MemberName = memberName;
				}
			}

			EditorGUILayout.Separator();

		}

		if( !binder.DataSource.IsValid )
		{
			EditorGUILayout.HelpBox( "Data source configuration is invalid", MessageType.Error );
			return;
		}

		var sourcePropertyType = binder.DataSource.GetMemberType();
		if( sourcePropertyType == null )
			return;

		using( dfEditorUtil.BeginGroup( "Data Target" ) )
		{

			if( binder.DataSource == null )
			{

				var gameObject = ( (Component)target ).gameObject;
				var defaultComponent = gameObject.GetComponent<dfControl>();

				binder.DataSource = new dfComponentMemberInfo()
				{
					Component = defaultComponent
				};

			}

			var dataTarget = binder.DataTarget;
			if( dataTarget.Component == null )
			{
				dataTarget.Component = binder.gameObject.GetComponents( typeof( MonoBehaviour ) ).FirstOrDefault();
			}

			var targetComponent = dfEditorUtil.ComponentField( "Component", dataTarget.Component );
			if( targetComponent != dataTarget.Component )
			{
				dfEditorUtil.MarkUndo( binder, "Assign DataSource Component" );
				dataTarget.Component = targetComponent;
			}

			if( targetComponent == null )
			{
				EditorGUILayout.HelpBox( "Missing component", MessageType.Error );
				return;
			}

			var targetComponentMembers =
				getMemberList( targetComponent )
				.Where( member => isCompatibleType( member, sourcePropertyType ) )
				.Select( m => m.Name )
				.ToArray();

			var memberIndex = findIndex( targetComponentMembers, dataTarget.MemberName );
			var selectedIndex = EditorGUILayout.Popup( "Property", memberIndex, targetComponentMembers );
			if( selectedIndex >= 0 && selectedIndex < targetComponentMembers.Length )
			{
				var memberName = targetComponentMembers[ selectedIndex ];
				if( memberName != dataTarget.MemberName )
				{
					dfEditorUtil.MarkUndo( binder, "Assign DataSource Member" );
					dataTarget.MemberName = memberName;
				}
			}

		}

		using( dfEditorUtil.BeginGroup( "Synchronization" ) )
		{

			var twoWay = EditorGUILayout.Toggle( "Two way", binder.TwoWay );
			if( twoWay != binder.TwoWay )
			{
				dfEditorUtil.MarkUndo( binder, "Change TwoWay property" );
				binder.TwoWay = twoWay;
			}

		}

	}

	#region Private utility methods 

	private bool isCompatibleType( MemberInfo member, Type type )
	{

		if( member.IsDefined( typeof( HideInInspector ), true ) )
			return false;

		if( member is FieldInfo )
		{

			var fieldInfo = (FieldInfo)member;

			if( type.IsAssignableFrom( fieldInfo.FieldType ) )
				return true;

			if( isNumericConversion( fieldInfo.FieldType, type ) )
			{
				return true;
			}

		}
		else if( member is PropertyInfo )
		{
			
			var propertyInfo = (PropertyInfo)member;
			
			if( type.IsAssignableFrom( propertyInfo.PropertyType ) )
				return true;

			if( isNumericConversion( propertyInfo.PropertyType, type ) )
			{
				return true;
			}

		}

		return false;

	}

	private bool isNumericConversion( Type lhs, Type rhs )
	{

		if( !lhs.IsValueType || !rhs.IsValueType )
			return false;

		var numericTypes = new Type[] 
		{
			typeof( int ), typeof( uint ), typeof( float ), typeof( double )
		};

		return numericTypes.Contains( lhs ) && numericTypes.Contains( rhs );

	}

	private int findIndex( string[] list, string value )
	{

		for( int i = 0; i < list.Length; i++ )
		{
			if( list[ i ] == value )
				return i;
		}

		return 0;

	}

	private MemberInfo[] getMemberList( Component component )
	{

		var baseMembers = component
			.GetType()
			.GetMembers( BindingFlags.Public | BindingFlags.Instance )
			.Where( m =>
				(
					m.MemberType == MemberTypes.Field ||
					m.MemberType == MemberTypes.Property
				) &&
				m.DeclaringType != typeof( MonoBehaviour ) &&
				m.DeclaringType != typeof( Behaviour ) &&
				m.DeclaringType != typeof( Component ) &&
				m.DeclaringType != typeof( UnityEngine.Object )
			)
			.OrderBy( m => m.Name )
			.ToArray();

		return baseMembers;

	}

	private bool isValidFieldType( MemberInfo member, Type requiredType )
	{
		
		if( member is FieldInfo )
			return isValidFieldType( ( (FieldInfo)member ).FieldType, requiredType );

		if( member is PropertyInfo )
			return isValidFieldType( ( (PropertyInfo)member ).PropertyType, requiredType );

		return false;

	}

	private bool isValidFieldType( Type type, Type requiredType )
	{

		if( requiredType.Equals( type ) )
			return true;

		if( requiredType.IsAssignableFrom( type ) )
			return true;

		if( typeof( IEnumerable ).IsAssignableFrom( type ) )
		{
			var genericType = type.GetGenericArguments();
			if( genericType.Length == 1 )
				return isValidFieldType( genericType[ 0 ], requiredType );
		}

		if( type != typeof( int ) && type != typeof( double ) && type != typeof( float ) )
		{
			return false;
		}

		if( requiredType != typeof( int ) && requiredType != typeof( double ) && requiredType != typeof( float ) )
		{
			return false;
		}

		return true; 

	}

	#endregion

}
