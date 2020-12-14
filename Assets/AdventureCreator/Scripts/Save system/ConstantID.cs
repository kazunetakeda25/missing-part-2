/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ConstantID.cs"
 * 
 *	This script is used by the Serialization classes to store a permanent ID
 *	of the gameObject (like InstanceID, only retained after reloading the project).
 *	To save a reference to an arbitrary object in a scene, this script must be attached to it.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[ExecuteInEditMode]
	public class ConstantID : MonoBehaviour
	{

		public int constantID;
		public bool retainInPrefab = false;
		public AutoManual autoManual = AutoManual.Automatic;

		#if UNITY_EDITOR
		private bool isNewInstance = true;
		#endif


		public virtual string SaveData ()
		{
			return "";
		}
		
		
		public virtual void LoadData (string stringData)
		{}


		public virtual void LoadData (string stringData, bool restoringSaveFile)
		{
			LoadData (stringData);
		}

			
		protected bool GameIsPlaying ()
		{
			#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				return false;
			}
			#endif
			return true;
		}


		#if UNITY_EDITOR

		public int AssignInitialValue (bool forcePrefab)
		{
			if (forcePrefab)
			{
				retainInPrefab = true;
				SetNewID_Prefab ();
			}
			else if (gameObject.activeInHierarchy)
			{
				retainInPrefab = false;
				SetNewID ();
			}
			else
			{
				retainInPrefab = true;
				SetNewID_Prefab ();
			}
			return constantID;
		}

		
		protected void Update ()
		{
			if (gameObject.activeInHierarchy)
			{
				if (constantID == 0)
				{
					SetNewID ();
				}
				
				if (isNewInstance)
				{
					isNewInstance = false;
					CheckForDuplicateIDs ();
				}
			}
		}


		public void SetNewID_Prefab ()
		{
			SetNewID ();
			isNewInstance = false;
		}
		

		private void SetNewID ()
		{
			// Share ID if another ID script already exists on object
			ConstantID[] idScripts = GetComponents <ConstantID>();
			foreach (ConstantID idScript in idScripts)
			{
				if (idScript != this)
				{
					constantID = idScript.constantID;
					EditorUtility.SetDirty (this);
					return;
				}
			}

			constantID = GetInstanceID ();
			if (constantID < 0)
			{
				constantID *= -1;
			}

			EditorUtility.SetDirty (this);
			Debug.Log ("Set new ID for " + this.name + ": " + constantID);
		}
		
		
		private void CheckForDuplicateIDs ()
		{
			ConstantID[] idScripts = FindObjectsOfType (typeof (ConstantID)) as ConstantID[];
			
			foreach (ConstantID idScript in idScripts)
			{
				if (idScript.constantID == constantID && idScript.gameObject != this.gameObject && constantID != 0)
				{
					Debug.Log ("Duplicate ID found: " + idScript.gameObject.name + " and " + this.name + " : " + constantID);
					SetNewID ();
					break;
				}
			}
		}
		
		#endif


		protected bool[] StringToBoolArray (string _string)
		{
			if (_string == null || _string == "" || _string.Length == 0)
			{
				return null;
			}
			
			string[] boolArray = _string.Split ("|"[0]);
			List<bool> boolList = new List<bool>();
			
			foreach (string chunk in boolArray)
			{
				if (chunk == "False")
				{
					boolList.Add (false);
				}
				else
				{
					boolList.Add (true);
				}
			}
			
			return boolList.ToArray ();
		}


		protected int[] StringToIntArray (string _string)
		{
			if (_string == null || _string == "" || _string.Length == 0)
			{
				return null;
			}
			
			string[] intArray = _string.Split ("|"[0]);
			List<int> intList = new List<int>();
			
			foreach (string chunk in intArray)
			{
				intList.Add (int.Parse (chunk));
			}
			
			return intList.ToArray ();
		}


		protected float[] StringToFloatArray (string _string)
		{
			if (_string == null || _string == "" || _string.Length == 0)
			{
				return null;
			}
			
			string[] floatArray = _string.Split ("|"[0]);
			List<float> floatList = new List<float>();
			
			foreach (string chunk in floatArray)
			{
				floatList.Add (float.Parse (chunk));
			}
			
			return floatList.ToArray ();
		}


		protected string[] StringToStringArray (string _string)
		{
			if (_string == null || _string == "" || _string.Length == 0)
			{
				return null;
			}
			
			string[] stringArray = _string.Split ("|"[0]);
			return stringArray;
		}
		
		
		protected string ArrayToString <T> (T[] _list)
		{
			System.Text.StringBuilder _string = new System.Text.StringBuilder ();
			
			foreach (T state in _list)
			{
				_string.Append (state.ToString() + "|");
			}
			if (_string.Length > 0)
			{
				_string.Remove (_string.Length-1, 1);
			}
			return _string.ToString ();
		}

	}

		
	[System.Serializable]
	public class Remember : ConstantID
	{}
	

	[System.Serializable]

	public class RememberData
	{
		public int objectID;
		public RememberData () { }
	}
	
}