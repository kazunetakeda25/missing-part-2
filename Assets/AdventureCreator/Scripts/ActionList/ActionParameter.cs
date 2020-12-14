/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionParameter.cs"
 * 
 *	This defines a parameter that can be used by ActionLists
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	[System.Serializable]
	public class ActionParameter
	{

		public string label = "";
		public int ID = 0;
		public int intValue = -1;
		public float floatValue = 0f;
		public string stringValue = "";
		public GameObject gameObject;
		public ParameterType parameterType = ParameterType.GameObject;


		public ActionParameter (int[] idArray)
		{
			label = "";
			ID = 0;
			intValue = -1;
			floatValue = 0f;
			stringValue = "";
			gameObject = null;
			parameterType = ParameterType.GameObject;
			
			// Update id based on array
			foreach (int _id in idArray)
			{
				if (ID == _id)
					ID ++;
			}
			
			label = "Parameter " + (ID + 1).ToString ();
		}


		public ActionParameter (int id)
		{
			label = "";
			ID = id;
			intValue = -1;
			floatValue = 0f;
			stringValue = "";
			gameObject = null;
			parameterType = ParameterType.GameObject;
			
			label = "Parameter " + (ID + 1).ToString ();
		}


		public void Reset ()
		{
			intValue = -1;
			floatValue = 0f;
			stringValue = "";
			gameObject = null;
		}


		public void SetValue (int _value)
		{
			intValue = _value;
			floatValue = 0f;
			stringValue = "";
			gameObject = null;
		}


		public void SetValue (float _value)
		{
			floatValue = _value;
			stringValue = "";
			intValue = -1;
			gameObject = null;
		}


		public void SetValue (string _value)
		{
			stringValue = _value;
			floatValue = 0f;
			intValue = -1;
			gameObject = null;
		}


		public void SetValue (bool _value)
		{
			stringValue = "";
			floatValue = 0f;
			intValue = -1;
			gameObject = null;
		}


		public void SetValue (GameObject _object)
		{
			gameObject = _object;
			floatValue = 0f;
			stringValue = "";
			intValue = -1;
		}


		public void SetValue (GameObject _object, int _value)
		{
			gameObject = _object;
			floatValue = 0f;
			stringValue = "";
			intValue = _value;
		}

	}

}