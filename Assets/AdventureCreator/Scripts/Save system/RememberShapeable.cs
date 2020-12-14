/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RememberShapeable.cs"
 * 
 *	This script is attached to shapeable scripts in the scene
 *	with shapekey values we wish to save.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{
	
	public class RememberShapeable : Remember
	{
		
		public override string SaveData ()
		{
			ShapeableData shapeableData = new ShapeableData();
			shapeableData.objectID = constantID;
			
			if (GetComponent <Shapeable>())
			{
				Shapeable shapeable = GetComponent <Shapeable>();
				List<int> activeKeyIDs = new List<int>();
				List<float> values = new List<float>();
				
				foreach (ShapeGroup shapeGroup in shapeable.shapeGroups)
				{
					activeKeyIDs.Add (shapeGroup.GetActiveKeyID ());
					values.Add (shapeGroup.GetActiveKeyValue ());
				}

				shapeableData._activeKeyIDs = ArrayToString <int> (activeKeyIDs.ToArray ());
				shapeableData._values = ArrayToString <float> (values.ToArray ());
			}

			return Serializer.SaveScriptData <ShapeableData> (shapeableData);
		}
		
		
		public override void LoadData (string stringData)
		{
			ShapeableData data = Serializer.LoadScriptData <ShapeableData> (stringData);
			if (data == null) return;

			if (GetComponent <Shapeable>())
			{
				Shapeable shapeable = GetComponent <Shapeable>();

				int[] activeKeyIDs = StringToIntArray (data._activeKeyIDs);
				float[] values = StringToFloatArray (data._values);

				for (int i=0; i<activeKeyIDs.Length; i++)
				{
					if (values.Length > i)
					{
						shapeable.shapeGroups[i].SetActive (activeKeyIDs[i], values[i], 0f, MoveMethod.Linear, null);
					}
				}
			}
		}
	
	}


	[System.Serializable]
	public class ShapeableData : RememberData
	{
		public string _activeKeyIDs;
		public string _values;
		
		public ShapeableData () { }
	}

}
