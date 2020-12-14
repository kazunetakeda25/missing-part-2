// @cond DOXY_IGNORE
/* Copyright 2013 Daikon Forge */

using UnityEngine;

using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This is a "marker" exception that indicates that DFGUI should abort
/// the current rendering pass
/// </summary>
public class dfAbortRenderingException : System.Exception
{
}

/// <summary>
/// Implements clipboard copy/paste functionality in standalone and web player
/// deployment targets. <b>NOTE:</b> Because Unity does not provide access to
/// this functionality, this class uses reflection to obtain access to private
/// members of the GUIUtility class, and may not continue to work in future 
/// version of Unity.
/// </summary>
public class dfClipboardHelper
{

	private static PropertyInfo m_systemCopyBufferProperty = null;

	private static PropertyInfo GetSystemCopyBufferProperty()
	{

		if( m_systemCopyBufferProperty == null )
		{
			Type time = typeof( GUIUtility );
			m_systemCopyBufferProperty = time.GetProperty( "systemCopyBuffer", BindingFlags.Static | BindingFlags.NonPublic );
			if( m_systemCopyBufferProperty == null )
				throw new Exception( "Can'time access internal member 'GUIUtility.systemCopyBuffer' it may have been removed / renamed" );
		}

		return m_systemCopyBufferProperty;

	}

	public static string clipBoard
	{
		get
		{
			try
			{
				PropertyInfo P = GetSystemCopyBufferProperty();
				return (string)P.GetValue( null, null );
			}
			catch
			{
				return "";
			}
		}
		set
		{
			try
			{
				PropertyInfo P = GetSystemCopyBufferProperty();
				P.SetValue( null, value, null );
			}
			catch { }
		}
	}

}

public static class dfStringExtensions
{

	/// <summary>
	/// Makes a file path relative to the Unity project's path
	/// </summary>
	public static string MakeRelativePath( this string path )
	{

		if( string.IsNullOrEmpty( path ) )
		{
			return "";
		}

		return path.Substring( path.IndexOf( "Assets/", StringComparison.InvariantCultureIgnoreCase ) );

	}

	/// <summary>
	/// Returns a value indicating whether the specified string pattern occurs
	/// within this string.
	/// </summary>
	/// <param name="pattern"></param>
	/// <param name="caseInsensitive"></param>
	/// <returns></returns>
	public static bool Contains( this string value, string pattern, bool caseInsensitive )
	{

		if( caseInsensitive )
		{
			return value.IndexOf( pattern, StringComparison.InvariantCultureIgnoreCase ) != -1;
		}

		return value.IndexOf( pattern ) != -1;

	}

}

public static class dfFloatExtensions
{

	/// <summary>
	/// Restricts the value to a discrete multiple of the value in the <paramref name="stepSize"/> parameter
	/// </summary>
	public static float Quantize( this float value, float stepSize )
	{
		if( stepSize <= 0 ) return value;
		return Mathf.Floor( value / stepSize ) * stepSize;
	}

	/// <summary>
	/// Restricts the value to a discrete multiple of the value in the <value>stepSize</value> parameter
	/// </summary>
	public static float RoundToNearest( this float value, float stepSize )
	{
		if( stepSize <= 0 ) return value;
		return Mathf.RoundToInt( value / stepSize ) * stepSize;
	}

}

public static class VectorExtensions
{

	public static Vector2 Scale( this Vector2 vector, float x, float y )
	{
		return new Vector2( vector.x * x, vector.y * y );
	}

	public static Vector3 Scale( this Vector3 vector, float x, float y, float z )
	{
		return new Vector3( vector.x * x, vector.y * y, vector.z * z );
	}

	public static Vector3 FloorToInt( this Vector3 vector )
	{
		return new Vector3(
			Mathf.FloorToInt( vector.x ),
			Mathf.FloorToInt( vector.y ),
			Mathf.FloorToInt( vector.z )
		);
	}

	public static Vector3 CeilToInt( this Vector3 vector )
	{
		return new Vector3(
			Mathf.CeilToInt( vector.x ),
			Mathf.CeilToInt( vector.y ),
			Mathf.CeilToInt( vector.z )
		);
	}

	public static Vector2 FloorToInt( this Vector2 vector )
	{
		return new Vector2(
			Mathf.FloorToInt( vector.x ),
			Mathf.FloorToInt( vector.y )
		);
	}

	public static Vector2 CeilToInt( this Vector2 vector )
	{
		return new Vector2(
			Mathf.CeilToInt( vector.x ),
			Mathf.CeilToInt( vector.y )
		);
	}

	public static Vector3 RoundToInt( this Vector3 vector )
	{
		return new Vector3( 
			Mathf.RoundToInt( vector.x ),
			Mathf.RoundToInt( vector.y ),
			Mathf.RoundToInt( vector.z )
		);
	}

	public static Vector2 RoundToInt( this Vector2 vector )
	{
		return new Vector2(
			Mathf.RoundToInt( vector.x ),
			Mathf.RoundToInt( vector.y )
		);
	}

	/// <summary>
	/// Restricts the values in the Vector2 to a discrete multiple of 
	/// the value in the <paramref name="discreteValue"/> parameter. 
	/// </summary>
	public static Vector2 Quantize( this Vector2 vector, float discreteValue )
	{
		vector.x = Mathf.RoundToInt( vector.x / discreteValue ) * discreteValue;
		vector.y = Mathf.RoundToInt( vector.y / discreteValue ) * discreteValue;
		return vector;
	}

	/// <summary>
	/// Restricts the values in the Vector2 to a discrete multiple of 
	/// the value in the <paramref name="discreteValue"/> parameter. 
	/// </summary>
	public static Vector3 Quantize( this Vector3 vector, float discreteValue )
	{
		vector.x = Mathf.RoundToInt( vector.x / discreteValue ) * discreteValue;
		vector.y = Mathf.RoundToInt( vector.y / discreteValue ) * discreteValue;
		vector.z = Mathf.RoundToInt( vector.z / discreteValue ) * discreteValue;
		return vector;
	}

}

public static class RectExtensions
{

	public static RectOffset ConstrainPadding( this RectOffset borders )
	{

		if( borders == null )
			return new RectOffset();

		borders.left = Mathf.Max( 0, borders.left );
		borders.right = Mathf.Max( 0, borders.right );
		borders.top = Mathf.Max( 0, borders.top );
		borders.bottom = Mathf.Max( 0, borders.bottom );

		return borders;

	}

	/// <summary>
	/// Returns a value indicating whether a Rect is empty (has no volume)
	/// </summary>
	public static bool IsEmpty( this Rect rect )
	{
		return ( rect.xMin == rect.xMax ) || ( rect.yMin == rect.yMax );
	}

	/// <summary>
	/// Returns the intersection of two Rect objects
	/// </summary>
	public static Rect Intersection( this Rect a, Rect b )
	{

		if( !a.Intersects( b ) )
			return new Rect();

		float xmin = Mathf.Max( a.xMin, b.xMin );
		float xmax = Mathf.Min( a.xMax, b.xMax );
		float ymin = Mathf.Max( a.yMin, b.yMin );
		float ymax = Mathf.Min( a.yMax, b.yMax );

		return Rect.MinMaxRect( xmin, ymax, xmax, ymin );

	}

	/// <summary>
	/// Returns the Union of two Rects
	/// </summary>
	public static Rect Union( this Rect a, Rect b )
	{

		float xmin = Mathf.Min( a.xMin, b.xMin );
		float xmax = Mathf.Max( a.xMax, b.xMax );
		float ymin = Mathf.Min( a.yMin, b.yMin );
		float ymax = Mathf.Max( a.yMax, b.yMax );

		return Rect.MinMaxRect( xmin, ymin, xmax, ymax );

	}

	/// <summary>
	/// Returns a value indicating whether the Rect defined by <paramref name="other"/>
	/// is fully contained within the source Rect.
	/// </summary>
	public static bool Contains( this Rect rect, Rect other )
	{

		var left = rect.x <= other.x;
		var right = rect.x + rect.width >= other.x + other.width;
		var top = rect.yMin <= other.yMin;
		var bottom = rect.y + rect.height >= other.y + other.height;

		return left && right && top && bottom;

	}

	/// <summary>
	/// Returns a value indicating whether two Rect objects are overlapping
	/// </summary>
	public static bool Intersects( this Rect rect, Rect other )
	{

		var outside =
			rect.xMax < other.xMin ||
			rect.yMax < other.xMin ||
			rect.xMin > other.xMax ||
			rect.yMin > other.yMax;

		return !outside;

	}

	public static Rect RoundToInt( this Rect rect )
	{
		return new Rect(
			Mathf.RoundToInt( rect.x ),
			Mathf.RoundToInt( rect.y ),
			Mathf.RoundToInt( rect.width ),
			Mathf.RoundToInt( rect.height )
		);
	}

	public static string Debug( this Rect rect )
	{
		return string.Format( "[{0},{1},{2},{3}]", rect.xMin, rect.yMin, rect.xMax, rect.yMax );
	}

}

public static class ReflectionExtensions
{

	/// <summary>
	/// Returns all instance fields on an object, including inherited fields
	/// </summary>
	public static FieldInfo[] GetAllFields( this Type type )
	{

		// http://stackoverflow.com/a/1155549/154165

		if( type == null )
			return new FieldInfo[ 0 ];

		BindingFlags flags = 
			BindingFlags.Public | 
			BindingFlags.NonPublic | 
			BindingFlags.Instance | 
			BindingFlags.DeclaredOnly;

		return
			type.GetFields( flags )
			.Concat( GetAllFields( type.BaseType ) )
			.Where( f => !f.IsDefined( typeof( HideInInspector ), true ) )
			.ToArray();

	}

	public static object GetProperty( this object target, string property )
	{

		if( target == null )
			throw new NullReferenceException( "Target is null" );

		var members = target.GetType().GetMember( property, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
		if( members == null || members.Length == 0 )
			throw new IndexOutOfRangeException( "Property not found: " + property );

		var member = members[ 0 ];

		if( member is FieldInfo )
		{
			return ( (FieldInfo)member ).GetValue( target );
		}

		if( member is PropertyInfo )
		{
			return ( (PropertyInfo)member ).GetValue( target, null );
		}

		throw new InvalidOperationException( "Member type not supported: " + member.MemberType );

	}

	public static void SetProperty( this object target, string property, object value )
	{

		if( target == null )
			throw new NullReferenceException( "Target is null" );

		var members = target.GetType().GetMember( property, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
		if( members == null || members.Length == 0 )
			throw new IndexOutOfRangeException( "Property not found: " + property );

		var member = members[ 0 ];

		if( member is FieldInfo )
		{
			( (FieldInfo)member ).SetValue( target, value );
			return;
		}

		if( member is PropertyInfo )
		{
			( (PropertyInfo)member ).SetValue( target, value, null );
			return;
		}

		throw new InvalidOperationException( "Member type not supported: " + member.MemberType );

	}

}

// @endcond DOXY_IGNORE
