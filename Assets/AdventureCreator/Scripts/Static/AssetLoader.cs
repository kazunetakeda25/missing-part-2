/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"AssetLoader.cs"
 * 
 *	This handles the management and retrieval of "Resources"
 *	assets when loading saved games.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public static class AssetLoader
	{

		private static Object[] textureAssets;
		private static Object[] audioAssets;
		private static Object[] animationAssets;
		private static Object[] materialAssets;


		public static string GetAssetInstanceID <T> (T originalFile) where T : Object
		{
			if (originalFile != null)
			{
				string name = originalFile.GetType () + originalFile.name;
				name = name.Replace (" (Instance)", "");
				return name;
			}
			return "";
		}

		
		public static T RetrieveAsset <T> (T originalFile, string _name) where T : Object
		{
			if (_name == "")
			{
				return originalFile;
			}

			if (originalFile == null)
			{
				return null;
			}
		
			Object[] assetFiles = null;

			if (originalFile is Texture2D)
			{
				if (textureAssets == null)
				{
					textureAssets = Resources.LoadAll ("", typeof (T));
				}
				assetFiles = textureAssets;
			}
			else if (originalFile is AudioClip)
			{
				if (audioAssets == null)
				{
					audioAssets = Resources.LoadAll ("", typeof (T));
				}
				assetFiles = audioAssets;
			}
			else if (originalFile is AnimationClip)
			{
				if (animationAssets == null)
				{
					animationAssets = Resources.LoadAll ("", typeof (T));
				}
				assetFiles = animationAssets;
			}
			else if (originalFile is Material)
			{
				if (materialAssets == null)
				{
					materialAssets = Resources.LoadAll ("", typeof (T));
				}
				assetFiles = materialAssets;
			}

			if (assetFiles != null && _name != null)
			{
				_name = _name.Replace (" (Instance)", "");
				foreach (Object assetFile in assetFiles)
				{
					if (assetFile != null && _name == (assetFile.GetType () + assetFile.name))//assetFile.GetHashCode () == _ID)
					{
						return (T) assetFile;
					}
				}
			}
			
			return originalFile;
		}


		public static void UnloadAssets ()
		{
			textureAssets = null;
			audioAssets = null;
			animationAssets = null;
			materialAssets = null;
			Resources.UnloadUnusedAssets ();
		}

	}

}