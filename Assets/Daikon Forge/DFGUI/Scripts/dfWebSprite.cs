/* Copyright 2013 Daikon Forge */
using UnityEngine;

using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;



using UnityMaterial = UnityEngine.Material;

/// <summary>
/// Downloads an image from a web URL
/// </summary>
[Serializable]
[ExecuteInEditMode]
[AddComponentMenu( "Daikon Forge/User Interface/Sprite/Web" )]
public class dfWebSprite : dfTextureSprite
{

	#region Protected serialized fields

	[SerializeField]
	protected string url = "";

	[SerializeField]
	protected Texture2D loadingImage;

	[SerializeField]
	protected Texture2D errorImage;

	#endregion

	#region Public properties 

	/// <summary>
	/// Gets/Sets the URL that will be used to retrieve the Texture to display
	/// </summary>
	public string URL
	{
		get { return this.url; }
		set
		{
			if( value != this.url )
			{
				this.url = value;
				if( Application.isPlaying )
				{
					StopAllCoroutines();
					StartCoroutine( downloadTexture() );
				}
			}
		}
	}

	/// <summary>
	/// Gets/Sets the <see cref="UnityEngine.Texture2D"/> that will be displayed
	/// until the web image is downloaded
	/// </summary>
	public Texture2D LoadingImage
	{
		get { return this.loadingImage; }
		set { this.loadingImage = value; }
	}

	/// <summary>
	/// Gets/Sets the <see cref="UnityEngine.Texture2D"/> that will be displayed
	/// if there is an error downloading the desired image from the web
	/// </summary>
	public Texture2D ErrorImage
	{
		get { return this.errorImage; }
		set { this.errorImage = value; }
	}

	#endregion

	#region Unity events

	public override void Start()
	{

		base.Start();

		if( Texture == null )
		{
			Texture = this.LoadingImage;
		}

		if( Application.isPlaying )
		{
			StartCoroutine( downloadTexture() );
		}

	}

	#endregion 

	#region Private utility methods 

	private IEnumerator downloadTexture()
	{

		this.Texture = this.loadingImage;

		using( var request = new WWW( this.url ) )
		{

			yield return request;

			if( !string.IsNullOrEmpty( request.error ) )
			{
				Debug.Log( "Error downloading image: " + request.error );
				this.Texture = this.errorImage ?? this.loadingImage;
			}
			else
			{
				this.Texture = request.texture;
			}

		}

	}

	#endregion

}
