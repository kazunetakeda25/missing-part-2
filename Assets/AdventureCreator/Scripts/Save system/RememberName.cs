/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RememberName.cs"
 * 
 *	This script is attached to gameObjects in the scene
 *	with a name we wish to save.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class RememberName : Remember
	{

		public override string SaveData ()
		{
			NameData nameData = new NameData();
			nameData.objectID = constantID;
			nameData.newName = gameObject.name;

			return Serializer.SaveScriptData <NameData> (nameData);
		}
		
		
		public override void LoadData (string stringData)
		{
			NameData data = Serializer.LoadScriptData <NameData> (stringData);
			if (data == null) return;

			gameObject.name = data.newName;
		}

	}


	[System.Serializable]
	public class NameData : RememberData
	{
		public string newName;
		public NameData () { }
	}

}