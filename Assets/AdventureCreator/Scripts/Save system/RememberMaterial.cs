/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RememberMaterial.cs"
 * 
 *	This script is attached to renderers with materials we wish to record changes in.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	[RequireComponent (typeof (Renderer))]
	public class RememberMaterial : Remember
	{
		
		public override string SaveData ()
		{
			MaterialData materialData = new MaterialData ();
			materialData.objectID = constantID;

			List<string> materialIDs = new List<string>();
			Material[] mats = GetComponent <Renderer>().materials;

			foreach (Material material in mats)
			{
				materialIDs.Add (AssetLoader. GetAssetInstanceID (material));
			}
			materialData._materialIDs = ArrayToString <string> (materialIDs.ToArray ());

			return Serializer.SaveScriptData <MaterialData> (materialData);
		}
		
		
		public override void LoadData (string stringData)
		{
			MaterialData data = Serializer.LoadScriptData <MaterialData> (stringData);
			if (data == null) return;

			Material[] mats = GetComponent <Renderer>().materials;

			string[] materialIDs = StringToStringArray (data._materialIDs);

			for (int i=0; i<materialIDs.Length; i++)
			{
				if (mats.Length >= i)
				{
					Material _material = AssetLoader.RetrieveAsset (mats[i], materialIDs[i]);
					if (_material != null)
					{
						mats[i] = _material;
					}
				}
			}
			
			GetComponent <Renderer>().materials = mats;
		}
		
	}
	
	
	[System.Serializable]
	public class MaterialData : RememberData
	{
		public string _materialIDs;
		
		public MaterialData () { }
	}
	
}
