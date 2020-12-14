// *******************************************************
// Copyright 2013 Daikon Forge, all rights reserved under 
// US Copyright Law and international treaties
// *******************************************************
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class dfUpgradeHelper
{

	private static List<GameObject> allPrefabsInProject = null;

	[MenuItem( "GameObject/Daikon Forge/Upgrade Texture Atlas Prefabs", false, 1 )]
	[MenuItem( "Assets/Daikon Forge/Upgrade Texture Atlas Prefabs", false, 1 )]
	public static void UpgradeAtlases()
	{

		var atlases = findPrefabsOfType<dfAtlas>();

		EditorUtility.DisplayProgressBar( "Upgrade", "Upgrading atlases", 0 );

		var count = atlases.Count;
		for( int i = 0; i < count; i++ )
		{

			var atlas = atlases[ i ];

			EditorUtility.DisplayProgressBar( "Upgrade", "Upgrading atlas: " + atlas.name, (float)i / (float)count );
			Debug.Log( "Upgrading Texture Atlas: " + atlas.name, atlas );

			UpgradeAtlas( atlas );

		}

		EditorUtility.ClearProgressBar();

	}

	public static void UpgradeAtlas( dfAtlas atlas )
	{

		try
		{

			var sprites = atlas.Items;
			for( int i = 0; i < sprites.Count; i++ )
			{

				var sprite = sprites[ i ];

				if( sprite.texture != null )
				{

					var spritePath = AssetDatabase.GetAssetPath( sprite.texture );
					var guid = AssetDatabase.AssetPathToGUID( spritePath );

					sprite.sizeInPixels = new Vector2( sprite.texture.width, sprite.texture.height );
					sprite.textureGUID = guid;
					sprite.texture = null;

				}
				else if( !string.IsNullOrEmpty( sprite.textureGUID ) )
				{

					var path = AssetDatabase.GUIDToAssetPath( sprite.textureGUID );
					var texture = AssetDatabase.LoadAssetAtPath( path, typeof( Texture2D ) ) as Texture2D;

					sprite.sizeInPixels = new Vector2( texture.width, texture.height );

				}

			}

			EditorUtility.SetDirty( atlas );

		}
		catch( Exception err )
		{
			Debug.LogError( "Error upgrading atlas " + atlas.name + ": " + err.Message, atlas );
		}

	}

	private static List<T> findPrefabsOfType<T>() where T : MonoBehaviour
	{

		if( allPrefabsInProject == null )
		{

			allPrefabsInProject = new List<GameObject>();

			var progressTime = Environment.TickCount;

			var allAssetPaths = AssetDatabase.GetAllAssetPaths();
			for( int i = 0; i < allAssetPaths.Length; i++ )
			{

				if( Environment.TickCount - progressTime > 250 )
				{
					progressTime = Environment.TickCount;
					EditorUtility.DisplayProgressBar( "Daikon Forge GUI", "Loading prefabs", (float)i / (float)allAssetPaths.Length );
				}

				var path = allAssetPaths[ i ];
				if( !path.EndsWith( ".prefab", StringComparison.InvariantCultureIgnoreCase ) )
					continue;

				var gameObject = AssetDatabase.LoadMainAssetAtPath( path ) as GameObject;
				if( IsPrefab( gameObject ) )
				{
					allPrefabsInProject.Add( gameObject );
				}

			}

			EditorUtility.ClearProgressBar();

			allPrefabsInProject.Sort( ( GameObject lhs, GameObject rhs ) =>
			{
				return lhs.name.CompareTo( rhs.name );
			} );

		}

		var result = new List<T>();

		foreach( var item in allPrefabsInProject )
		{

			var component = item.GetComponent( typeof( T ) );
			if( component != null )
				result.Add( (T)component );

		}

		return result;

	}

	private static bool IsPrefab( GameObject item )
	{
		return
			item != null &&
			PrefabUtility.GetPrefabParent( item ) == null &&
			PrefabUtility.GetPrefabObject( item ) != null;
	}

}
