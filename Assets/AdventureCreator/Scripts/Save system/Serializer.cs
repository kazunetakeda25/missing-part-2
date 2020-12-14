/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Serializer.cs"
 * 
 *	This script serializes saved game data and performs the file handling.
 * 
 * 	It is partially based on Zumwalt's code here:
 * 	http://wiki.unity3d.com/index.php?title=Save_and_Load_from_XML
 *  and uses functions by Nitin Pande:
 *  http://www.eggheadcafe.com/articles/system.xml.xmlserialization.asp 
 * 
 */

using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using System;
#if !(UNITY_WP8 || UNITY_WINRT)
using System.Runtime.Serialization.Formatters.Binary;
#endif
using System.IO;
using System.Xml; 
using System.Xml.Serialization;

namespace AC
{

	public class Serializer : MonoBehaviour
	{
		
		public static T returnComponent <T> (int constantID) where T : Component
		{
			if (constantID != 0)
			{
				T[] objects = FindObjectsOfType (typeof(T)) as T[];
				
				foreach (T _object in objects)
				{
					if (_object.GetComponent <ConstantID>())
					{
						ConstantID[] idScripts = _object.GetComponents <ConstantID>();
						foreach (ConstantID idScript in idScripts)
						{
							if (idScript.constantID == constantID)
							{
								// Found it
								return _object;
							}
						}
					}
				}
			}
			
			return null;
		}

		
		public static int GetConstantID (GameObject _gameObject)
		{
			if (_gameObject.GetComponent <ConstantID>())
			{
				if (_gameObject.GetComponent <ConstantID>().constantID != 0)
				{
					return (_gameObject.GetComponent <ConstantID>().constantID);
				}
				else
				{	
					Debug.LogWarning ("GameObject " + _gameObject.name + " was not saved because it does not have an ID.");
				}
			}
			else
			{
				Debug.LogWarning ("GameObject " + _gameObject.name + " was not saved because it does not have a constant ID script!");
			}
			
			return 0;
		}
		
		
		public static string SerializeObjectBinary (object pObject)
		{
			#if UNITY_WP8 || UNITY_WINRT
			return "";
			#else
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			MemoryStream memoryStream = new MemoryStream ();
			binaryFormatter.Serialize (memoryStream, pObject);
			return (Convert.ToBase64String (memoryStream.GetBuffer ()));
			#endif
		}
		
		
		public static T DeserializeObjectBinary <T> (string pString)
		{
			if (pString == null || pString.Length == 0) return default (T);
			#if UNITY_WP8 || UNITY_WINRT
			return default (T);
			#else
			if (pString.Contains ("<?xml"))
			{
				// Fix converted Options Data
				PlayerPrefs.DeleteKey ("Options");
				return default (T);
			}
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			MemoryStream memoryStream = new MemoryStream (Convert.FromBase64String (pString));
			return (T) binaryFormatter.Deserialize (memoryStream);
			#endif
		}
		
		
		public static string SerializeObjectXML <T> (object pObject) 
		{ 
			string XmlizedString = null; 
			
			MemoryStream memoryStream = new MemoryStream(); 
			XmlSerializer xs = new XmlSerializer (typeof (T)); 
			XmlTextWriter xmlTextWriter = new XmlTextWriter (memoryStream, Encoding.UTF8); 
			
			xs.Serialize (xmlTextWriter, pObject); 
			memoryStream = (MemoryStream) xmlTextWriter.BaseStream; 
			XmlizedString = UTF8ByteArrayToString (memoryStream.ToArray());
			
			return XmlizedString;
		}
		
		
		public static object DeserializeObjectXML <T> (string pXmlizedString) 
		{ 
			XmlSerializer xs = new XmlSerializer (typeof (T)); 
			MemoryStream memoryStream = new MemoryStream (StringToUTF8ByteArray (pXmlizedString)); 
			return xs.Deserialize(memoryStream); 
		}
		
		
		private static string UTF8ByteArrayToString (byte[] characters) 
		{		
			UTF8Encoding encoding = new UTF8Encoding(); 
			string constructedString = encoding.GetString (characters, 0, characters.Length);
			return (constructedString); 
		}
		
		
		private static byte[] StringToUTF8ByteArray (string pXmlString) 
		{ 
			UTF8Encoding encoding = new UTF8Encoding(); 
			byte[] byteArray = encoding.GetBytes (pXmlString); 
			return byteArray; 
		}


		public static T DeserializeRememberData <T> (string pString) where T : RememberData
		{
			#if !(UNITY_WP8 || UNITY_WINRT)
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			MemoryStream memoryStream = new MemoryStream (Convert.FromBase64String (pString));
			T myObject;//
			myObject = binaryFormatter.Deserialize (memoryStream) as T;//
			return myObject;//
			//return binaryFormatter.Deserialize (memoryStream) as T;
			#else
			return null;
			#endif
		}

		
		public static List<SingleLevelData> DeserializeRoom (string pString)
		{
			#if !(UNITY_WP8 || UNITY_WINRT)
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			MemoryStream memoryStream = new MemoryStream (Convert.FromBase64String (pString));
			return (List<SingleLevelData>) binaryFormatter.Deserialize (memoryStream);
			#else
			return null;
			#endif
		}
		
		
		public static void CreateSaveFile (string fullFileName, string _data)
		{
			#if UNITY_WEBPLAYER || UNITY_WINRT
			
			PlayerPrefs.SetString (fullFileName, _data);
			Debug.Log ("PlayerPrefs key written: " + fullFileName);
			
			#else
			
			StreamWriter writer;
			
			FileInfo t = new FileInfo (fullFileName);
			
			if (!t.Exists)
			{
				writer = t.CreateText ();
			}
			
			else
			{
				t.Delete ();
				writer = t.CreateText ();
			}
			
			writer.Write (_data);
			writer.Close ();

			Debug.Log ("File written: " + fullFileName);
			#endif
		}
		
		
		public static string LoadSaveFile (string fullFileName, bool doLog)
		{
			string _data = "";
			
			#if UNITY_WEBPLAYER || UNITY_WINRT
			
			_data = PlayerPrefs.GetString(fullFileName, "");
			
			#else
			
			StreamReader r = File.OpenText (fullFileName);
			
			string _info = r.ReadToEnd ();
			r.Close ();
			_data = _info;
			
			#endif

			if (doLog)
			{
				Debug.Log ("File Read: " + fullFileName);
			}
			return (_data);
		}
		
		
		public static Paths RestorePathData (Paths path, string pathData)
		{
			if (pathData == null)
			{
				return null;
			}
			
			path.affectY = true;
			path.pathType = AC_PathType.ForwardOnly;
			path.nodePause = 0;
			path.nodes = new List<Vector3>();
			
			if (pathData.Length > 0)
			{
				string[] nodesArray = pathData.Split ("|"[0]);
				
				foreach (string chunk in nodesArray)
				{
					string[] chunkData = chunk.Split (":"[0]);
					
					float _x = 0;
					float.TryParse (chunkData[0], out _x);
					
					float _y = 0;
					float.TryParse (chunkData[1], out _y);
					
					float _z = 0;
					float.TryParse (chunkData[2], out _z);
					
					path.nodes.Add (new Vector3 (_x, _y, _z));
				}
			}
			
			return path;
		}
		
		
		public static string CreatePathData (Paths path)
		{
			System.Text.StringBuilder pathString = new System.Text.StringBuilder ();
			
			foreach (Vector3 node in path.nodes)
			{
				pathString.Append (node.x.ToString ());
				pathString.Append (":");
				pathString.Append (node.y.ToString ());
				pathString.Append (":");
				pathString.Append (node.z.ToString ());
				pathString.Append ("|");
			}
			
			if (path.nodes.Count > 0)
			{
				pathString.Remove (pathString.Length-1, 1);
			}
			
			return pathString.ToString ();
		}


		public static void SaveScreenshot (Texture2D screenshotTex, string fileName)
		{
			#if !UNITY_WEBPLAYER && !UNITY_ANDROID && !UNITY_WINRT
			byte[] bytes = screenshotTex.EncodeToJPG ();
			File.WriteAllBytes (fileName, bytes);
			#endif
		}


		public static Texture2D LoadScreenshot (string fileName)
		{
			#if !UNITY_WEBPLAYER && !UNITY_ANDROID && !UNITY_WINRT
			if (File.Exists (fileName))
			{
				byte[] bytes = File.ReadAllBytes (fileName);
				Texture2D screenshotTex = new Texture2D (Screen.width, Screen.height, TextureFormat.RGB24, false);
				screenshotTex.LoadImage (bytes);
				return screenshotTex;
			}
			#endif
			return null;
		}


		public static string SaveScriptData <T> (object pObject)
		{
			if (SaveSystem.GetSaveMethod () == SaveMethod.XML)
			{
				return Serializer.SerializeObjectXML <T> (pObject);
			}
			else
			{
				return Serializer.SerializeObjectBinary (pObject);
			}
		}



		public static T LoadScriptData <T> (string stringData) where T : RememberData
		{
			if (SaveSystem.GetSaveMethod () == SaveMethod.XML)
			{
				return (T) Serializer.DeserializeObjectXML <T> (stringData);
			}
			else
			{
				return (T) Serializer.DeserializeRememberData <T> (stringData);
			}
		}

	}

}