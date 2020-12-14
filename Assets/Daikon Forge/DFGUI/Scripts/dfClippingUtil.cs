/* Copyright 2013 Daikon Forge */
using UnityEngine;

using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Provides triangle clipping functionality for the <see cref="dfGUIManager"/> class
/// </summary>
public class dfClippingUtil
{

	private static int[] inside = new int[ 3 ];

	private static ClipTriangle[] clipSource;
	private static ClipTriangle[] clipDest;

	static dfClippingUtil()
	{
		// Create working buffers that should be large enough to clip 
		// a single triangle against a reasonably large number of 
		// planes.
		clipSource = initClipBuffer( 1024 );
		clipDest = initClipBuffer( 1024 );
	}

	/// <summary>
	/// Clips a <see cref="dfRenderData"/> instance containing control rendering data
	/// against a list of <see cref="Plane"/> objects defined by the current clipping 
	/// region, and outputs the clipped data into <paramref name="dest"/>
	/// </summary>
	/// <param name="planes">The list of planes to clip against</param>
	/// <param name="source">The control rendering data to be clipped</param>
	/// <param name="dest">The output buffer that will hold the resulting clipped data</param>
	public static void Clip( IList<Plane> planes, dfRenderData source, dfRenderData dest )
	{

		dest.EnsureCapacity( dest.Vertices.Count + source.Vertices.Count );

		for( int sourceIndex = 0; sourceIndex < source.Triangles.Count; sourceIndex += 3 )
		{

			for( int i = 0; i < 3; i++ )
			{

				var index = source.Triangles[ sourceIndex + i ];

				clipSource[ 0 ].corner[ i ] = source.Transform.MultiplyPoint( source.Vertices[ index ] );
				clipSource[ 0 ].uv[ i ] = source.UV[ index ];
				clipSource[ 0 ].color[ i ] = source.Colors[ index ];

			}

			var count = 1;
			for( int planeIndex = 0; planeIndex < planes.Count; planeIndex++ )
			{

				count = clipToPlane( planes[ planeIndex ], clipSource, clipDest, count );

				var temp = clipSource;
				clipSource = clipDest;
				clipDest = temp;

			}

			for( int i = 0; i < count; i++ )
			{
				clipSource[ i ].CopyTo( dest );
			}

		}

	}

	private static int clipToPlane( Plane plane, ClipTriangle[] source, ClipTriangle[] dest, int count )
	{

		var newCount = 0;
		for( int i = 0; i < count; i++ )
		{
			newCount += clipToPlane( plane, source[ i ], dest, newCount );
		}

		return newCount;

	}

	private static int clipToPlane( Plane plane, ClipTriangle triangle, ClipTriangle[] dest, int destIndex )
	{

		var verts = triangle.corner;
		var numInside = 0;
		var outside = 0;

		var planeNormal = plane.normal;
		var planeDist = plane.distance;

		for( int i = 0; i < 3; i++ )
		{
			if( Vector3.Dot( planeNormal, verts[ i ] ) + planeDist > 0 )
				inside[ numInside++ ] = i;
			else
				outside = i;
		}

		// Entire triangle is in front of the plane
		if( numInside == 3 )
		{
			triangle.CopyTo( dest[ destIndex ] );
			return 1;
		}

		// Entire triangle is behind the plane
		if( numInside == 0 )
		{
			return 0;
		}

		// We've got vertices on either side of the plane, need to slice...
		// TODO: Currently always splits in the same direction. Modify so split retains largest triangle area?
		if( numInside == 1 )
		{

			var i0 = inside[ 0 ];
			var i1 = ( i0 + 1 ) % 3;
			var i2 = ( i0 + 2 ) % 3;

			var va = verts[ i0 ];
			var vb = verts[ i1 ];
			var vc = verts[ i2 ];

			var uva = triangle.uv[ i0 ];
			var uvb = triangle.uv[ i1 ];
			var uvc = triangle.uv[ i2 ];

			var ca = triangle.color[ i0 ];
			var cb = triangle.color[ i1 ];
			var cc = triangle.color[ i2 ];

			var distance = 0f;

			var dir = vb - va;
			var ray = new Ray( va, dir.normalized );
			plane.Raycast( ray, out distance );
			var lerpDist = distance / dir.magnitude;

			var v1 = ray.origin + ray.direction * distance;
			var uv1 = Vector2.Lerp( uva, uvb, lerpDist );
			var c1 = Color.Lerp( ca, cb, lerpDist );

			dir = vc - va;
			ray = new Ray( va, dir.normalized );
			plane.Raycast( ray, out distance );
			lerpDist = distance / dir.magnitude;

			var v2 = ray.origin + ray.direction * distance;
			var uv2 = Vector2.Lerp( uva, uvc, lerpDist );
			var c2 = Color.Lerp( ca, cc, lerpDist );

			dest[ destIndex ].corner[ 0 ] = va;
			dest[ destIndex ].corner[ 1 ] = v1;
			dest[ destIndex ].corner[ 2 ] = v2;

			dest[ destIndex ].uv[ 0 ] = uva;
			dest[ destIndex ].uv[ 1 ] = uv1;
			dest[ destIndex ].uv[ 2 ] = uv2;

			dest[ destIndex ].color[ 0 ] = ca;
			dest[ destIndex ].color[ 1 ] = c1;
			dest[ destIndex ].color[ 2 ] = c2;

			return 1;

		}
		else
		{

			var i0 = outside;
			var i1 = ( i0 + 1 ) % 3;
			var i2 = ( i0 + 2 ) % 3;

			var va = verts[ i0 ];
			var vb = verts[ i1 ];
			var vc = verts[ i2 ];

			var uva = triangle.uv[ i0 ];
			var uvb = triangle.uv[ i1 ];
			var uvc = triangle.uv[ i2 ];

			var ca = triangle.color[ i0 ];
			var cb = triangle.color[ i1 ];
			var cc = triangle.color[ i2 ];

			var dir = vb - va;
			var ray = new Ray( va, dir.normalized );
			var distance = 0f;
			plane.Raycast( ray, out distance );
			var lerpDist = distance / dir.magnitude;

			var v1 = ray.origin + ray.direction * distance;
			var uv1 = Vector2.Lerp( uva, uvb, lerpDist );
			var c1 = Color.Lerp( ca, cb, lerpDist );

			dir = vc - va;
			ray = new Ray( va, dir.normalized );
			plane.Raycast( ray, out distance );
			lerpDist = distance / dir.magnitude;

			var v2 = ray.origin + ray.direction * distance;
			var uv2 = Vector2.Lerp( uva, uvc, lerpDist );
			var c2 = Color.Lerp( ca, cc, lerpDist );

			dest[ destIndex ].corner[ 0 ] = v1;
			dest[ destIndex ].corner[ 1 ] = vb;
			dest[ destIndex ].corner[ 2 ] = v2;
			dest[ destIndex ].uv[ 0 ] = uv1;
			dest[ destIndex ].uv[ 1 ] = uvb;
			dest[ destIndex ].uv[ 2 ] = uv2;
			dest[ destIndex ].color[ 0 ] = c1;
			dest[ destIndex ].color[ 1 ] = cb;
			dest[ destIndex ].color[ 2 ] = c2;

			destIndex++;

			dest[ destIndex ].corner[ 0 ] = v2;
			dest[ destIndex ].corner[ 1 ] = vb;
			dest[ destIndex ].corner[ 2 ] = vc;
			dest[ destIndex ].uv[ 0 ] = uv2;
			dest[ destIndex ].uv[ 1 ] = uvb;
			dest[ destIndex ].uv[ 2 ] = uvc;
			dest[ destIndex ].color[ 0 ] = c2;
			dest[ destIndex ].color[ 1 ] = cb;
			dest[ destIndex ].color[ 2 ] = cc;

			return 2;

		}

	}

	private static ClipTriangle[] initClipBuffer( int size )
	{

		var buffer = new ClipTriangle[ size ];

		for( int i = 0; i < size; i++ )
		{
			buffer[ i ].corner = new Vector3[ 3 ];
			buffer[ i ].uv = new Vector2[ 3 ];
			buffer[ i ].color = new Color32[ 3 ];
		}

		return buffer;

	}

	#region Nested classes 
		
	protected struct ClipTriangle
	{

		#region Public fields

		public Vector3[] corner;
		public Vector2[] uv;
		public Color32[] color;

		#endregion

		#region Public methods 

		public void CopyTo( ClipTriangle target )
		{
			Array.Copy( this.corner, target.corner, 3 );
			Array.Copy( this.uv, target.uv, 3 );
			Array.Copy( this.color, target.color, 3 );
		}

		public void CopyTo( dfRenderData buffer )
		{

			var baseIndex = buffer.Vertices.Count;

			buffer.Vertices.AddRange( corner );
			buffer.UV.AddRange( uv );
			buffer.Colors.AddRange( color );

			buffer.Triangles.Add( baseIndex + 0 );
			buffer.Triangles.Add( baseIndex + 1 );
			buffer.Triangles.Add( baseIndex + 2 );
			
		}

		#endregion

	}

	#endregion

}
