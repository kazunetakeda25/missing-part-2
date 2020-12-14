/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"CustomAssetUtility.cs"
 * 
 *	This script allows assets to be created based on a supplied script.
 *	It is based on code written by Jacob Pennock (http://www.jacobpennock.com/Blog/?p=670)
 *  and code by Steven van Rossum (https://gist.github.com/JvetS/5208916)
 * 
 */

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.IO;

namespace AC
{

	public static class CustomAssetUtility
	{
		
		public static string GetUniqueAssetPathNameOrFallback (string filename)
		{
			string path;
			try
			{
				System.Type assetdatabase = typeof (UnityEditor.AssetDatabase);
				path = (string) assetdatabase.GetMethod ("GetUniquePathNameAtSelectedPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).Invoke(assetdatabase, new object[] { filename });
			}
			catch
			{
				path = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/" + filename);
			}
			return path;
		}
		

		public static T CreateAsset<T> (string filename) where T : ScriptableObject
		{
			T asset = ScriptableObject.CreateInstance<T> ();
			string assetPathAndName = GetUniqueAssetPathNameOrFallback (filename + ".asset");
			
			AssetDatabase.CreateAsset (asset, assetPathAndName);

			AssetDatabase.SaveAssets ();
			EditorUtility.FocusProjectWindow ();
			Selection.activeObject = asset;

			return asset;
		}
		
		
		public static T CreateAsset<T> (string filename, string path) where T : ScriptableObject
		{
			T asset = ScriptableObject.CreateInstance<T> ();
			string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath ("Assets" + Path.DirectorySeparatorChar.ToString () + path + Path.DirectorySeparatorChar.ToString () + filename + ".asset");
			AssetDatabase.CreateAsset (asset, assetPathAndName);
			
			AssetDatabase.SaveAssets ();
			EditorUtility.FocusProjectWindow ();

			return asset;
		}
		

		public static T CreateAndReturnAsset<T> (string path) where T : ScriptableObject
		{
			T asset = ScriptableObject.CreateInstance<T> ();
			string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath ("Assets" + Path.DirectorySeparatorChar.ToString () + path + Path.DirectorySeparatorChar.ToString () + typeof(T).ToString() + ".asset");
			AssetDatabase.CreateAsset (asset, assetPathAndName);
			
			AssetDatabase.SaveAssets ();
			EditorUtility.FocusProjectWindow ();
			
			return asset;
		}

	}
}

#endif