/* Copyright 2013 Daikon Forge */
using UnityEngine;

using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using UnityMaterial = UnityEngine.Material;

/// <summary>
/// Implements a dfSprite that allows the user to use any Texture and Material they wish
/// without having to use a Texture Atlas
/// </summary>
[Serializable]
[ExecuteInEditMode]
[AddComponentMenu( "Daikon Forge/User Interface/Sprite/Texture" )]
public class dfTextureSprite : dfControl
{

	#region Static variables 

	private static int[] TRIANGLE_INDICES = new int[] { 0, 1, 3, 3, 1, 2 };

	#endregion

	#region Public events

	/// <summary>
	/// Raised whenever the value of the <see cref="Texture"/> property has changed
	/// </summary>
	public event PropertyChangedEventHandler<Texture2D> TextureChanged;

	#endregion

	#region Protected serialized members

	[SerializeField]
	protected Texture2D texture;

	[SerializeField]
	protected Material material;

	[SerializeField]
	protected dfSpriteFlip flip = dfSpriteFlip.None;

	[SerializeField]
	protected dfFillDirection fillDirection = dfFillDirection.Horizontal;

	[SerializeField]
	protected float fillAmount = 1f;

	[SerializeField]
	protected bool invertFill = false;

	#endregion

	#region Private instance variables 

	private bool createdRuntimeMaterial = false;
	private Material renderMaterial = null;

	#endregion

	#region Public properties

	/// <summary>
	/// Gets/Sets the <see cref="Texture2D"/> that will be rendered
	/// </summary>
	public Texture2D Texture
	{
		get { return this.texture; }
		set
		{
			if( value != this.texture )
			{

				this.texture = value;
				Invalidate();

				if( value != null && size.sqrMagnitude <= float.Epsilon )
				{
					size = new Vector2( value.width, value.height );
				}

				OnTextureChanged( value );

			}
		}
	}

	/// <summary>
	/// Gets/Sets the <see cref="Material"/> that will be used to render the <see cref="Texture"/>
	/// </summary>
	public Material Material
	{
		get { return this.material; }
		set
		{
			if( value != this.material )
			{

				disposeCreatedMaterial();

				renderMaterial = null;
				this.material = value;
				Invalidate();

			}
		}
	}

	/// <summary>
	/// Gets or sets which axes will be flipped when rendering the sprite
	/// </summary>
	public dfSpriteFlip Flip
	{
		get { return flip; }
		set
		{
			if( value != flip )
			{
				flip = value;
				Invalidate();
			}
		}
	}

	/// <summary>
	/// Gets or sets which direction will be used for a fill operation 
	/// during rendering
	/// </summary>
	public dfFillDirection FillDirection
	{
		get { return this.fillDirection; }
		set
		{
			if( value != this.fillDirection )
			{
				this.fillDirection = value;
				Invalidate();
			}
		}
	}

	/// <summary>
	/// The amount (from 0.0 to 1.0) of the sprite's surface to be filled
	/// </summary>
	public float FillAmount
	{
		get { return this.fillAmount; }
		set
		{
			if( !Mathf.Approximately( value, this.fillAmount ) )
			{
				this.fillAmount = Mathf.Max( 0, Mathf.Min( 1, value ) );
				Invalidate();
			}
		}
	}

	/// <summary>
	/// If set to TRUE, will invert the fill direction
	/// </summary>
	public bool InvertFill
	{
		get { return invertFill; }
		set
		{
			if( value != invertFill )
			{
				invertFill = value;
				Invalidate();
			}
		}
	}

	/// <summary>
	/// Returns a reference to the material that the control is currently 
	/// using to render the texture, which may be a different instance than
	/// the reference in the Material property
	/// </summary>
	public Material RenderMaterial
	{
		get { return this.renderMaterial; }
	}

	#endregion

	#region Unity events 

	public override void OnEnable()
	{
		base.OnEnable();
		renderMaterial = null;
	}

	public override void OnDestroy()
	{
		
		base.OnDestroy();
		if( renderMaterial != null )
		{
			DestroyImmediate( renderMaterial );
			renderMaterial = null;
		}

	}

	public override void OnDisable()
	{

		base.OnDisable();

		disposeCreatedMaterial();

		if( Application.isPlaying && renderMaterial != null )
		{
			DestroyImmediate( renderMaterial );
			renderMaterial = null;
		}

	}

	#endregion

	#region Rendering

	protected override void OnRebuildRenderData()
	{
		
		base.OnRebuildRenderData();

		if( this.texture == null )
			return;

		ensureMaterial();
		if( this.material == null )
			return;

		// renderMaterial is a Material instance used specifically for rendering,
		// and needs to be distinct from all other instances of the same Material
		// so that it doesn't get "batched" with other instances of the Material
		// using a different Material.mainTexture value.
		if( renderMaterial == null )
		{
			renderMaterial = new UnityMaterial( material ) 
			{ 
				hideFlags = HideFlags.DontSave, 
				name = material.name + " (copy)" 
			};
		}

		renderMaterial.mainTexture = this.texture;
		renderData.Material = renderMaterial;

		var p2u = PixelsToUnits();
		float meshLeft = 0;
		float meshTop = 0;
		float meshRight = size.x * p2u;
		float meshBottom = -size.y * p2u;
		var offset = pivot.TransformToUpperLeft( size ).RoundToInt() * p2u;

		renderData.Vertices.Add( new Vector3( meshLeft, meshTop, 0 ) + offset );
		renderData.Vertices.Add( new Vector3( meshRight, meshTop, 0 ) + offset );
		renderData.Vertices.Add( new Vector3( meshRight, meshBottom, 0 ) + offset );
		renderData.Vertices.Add( new Vector3( meshLeft, meshBottom, 0 ) + offset );

		renderData.Triangles.AddRange( TRIANGLE_INDICES );

		rebuildUV( renderData );

		var color = ApplyOpacity( this.color );
		renderData.Colors.Add( color );
		renderData.Colors.Add( color );
		renderData.Colors.Add( color );
		renderData.Colors.Add( color );

		if( fillAmount < 1f )
		{
			doFill( renderData );
		}

	}

	#endregion

	#region Private utility methods 

	private void disposeCreatedMaterial()
	{

		if( createdRuntimeMaterial )
		{
			DestroyImmediate( this.material );
			this.material = null;
			createdRuntimeMaterial = false;
		}

	}

	private void rebuildUV( dfRenderData renderData )
	{

		var result = renderData.UV;

		result.Add( new Vector2( 0, 1 ) );
		result.Add( new Vector2( 1, 1 ) );
		result.Add( new Vector2( 1, 0 ) );
		result.Add( new Vector2( 0, 0 ) );

		var temp = Vector2.zero;

		if( flip.IsSet( dfSpriteFlip.FlipHorizontal ) )
		{
			temp = result[ 1 ]; result[ 1 ] = result[ 0 ]; result[ 0 ] = temp;
			temp = result[ 3 ]; result[ 3 ] = result[ 2 ]; result[ 2 ] = temp;
		}

		if( flip.IsSet( dfSpriteFlip.FlipVertical ) )
		{
			temp = result[ 0 ]; result[ 0 ] = result[ 3 ]; result[ 3 ] = temp;
			temp = result[ 1 ]; result[ 1 ] = result[ 2 ]; result[ 2 ] = temp;
		}

	}

	private void doFill( dfRenderData renderData )
	{

		var verts = renderData.Vertices;
		var uv = renderData.UV;

		var ul = 0;
		var ur = 1;
		var bl = 3;
		var br = 2;

		if( invertFill )
		{
			if( fillDirection == dfFillDirection.Horizontal )
			{
				ul = 1; ur = 0;
				bl = 2; br = 3;
			}
			else
			{
				ul = 3; ur = 2;
				bl = 0; br = 1;
			}
		}

		if( fillDirection == dfFillDirection.Horizontal )
		{
			verts[ ur ] = Vector3.Lerp( verts[ ur ], verts[ ul ], 1f - fillAmount );
			verts[ br ] = Vector3.Lerp( verts[ br ], verts[ bl ], 1f - fillAmount );
			uv[ ur ] = Vector2.Lerp( uv[ ur ], uv[ ul ], 1f - fillAmount );
			uv[ br ] = Vector2.Lerp( uv[ br ], uv[ bl ], 1f - fillAmount );
		}
		else
		{
			verts[ bl ] = Vector3.Lerp( verts[ bl ], verts[ ul ], 1f - fillAmount );
			verts[ br ] = Vector3.Lerp( verts[ br ], verts[ ur ], 1f - fillAmount );
			uv[ bl ] = Vector2.Lerp( uv[ bl ], uv[ ul ], 1f - fillAmount );
			uv[ br ] = Vector2.Lerp( uv[ br ], uv[ ur ], 1f - fillAmount );
		}

	}

	protected internal virtual void OnTextureChanged( Texture2D value )
	{

		SignalHierarchy( "OnTextureChanged", value );

		if( TextureChanged != null )
		{
			TextureChanged( this, value );
		}

	}

	/// <summary>
	/// Attempts to ensure that a Material is always available
	/// </summary>
	private void ensureMaterial()
	{

		if( material != null || texture == null )
			return;

		var shader = Shader.Find( "Daikon Forge/Default UI Shader" );
		if( shader == null )
		{
			Debug.LogError( "Failed to find default shader" );
			return;
		}

		material = new UnityMaterial( shader ) 
		{ 
			name = "Default Texture Shader",
			hideFlags = HideFlags.DontSave,
			mainTexture = this.texture
		};

		createdRuntimeMaterial = true;

	}

	#endregion

}
