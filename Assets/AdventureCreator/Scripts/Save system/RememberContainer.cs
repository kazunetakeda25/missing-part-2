/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RememberContainer.cs"
 * 
 *	This script is attached to container objects in the scene
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{
	
	public class RememberContainer : Remember
	{
		
		public override string SaveData ()
		{
			ContainerData containerData = new ContainerData();
			containerData.objectID = constantID;
			
			if (GetComponent <Container>())
			{
				Container container = GetComponent <Container>();
				List<int> linkedIDs = new List<int>();
				List<int> counts = new List<int>();
				List<int> IDs = new List<int>();

				for (int i=0; i<container.items.Count; i++)
				{
					linkedIDs.Add (container.items[i].linkedID);
					counts.Add (container.items[i].count);
					IDs.Add (container.items[i].id);
				}

				containerData._linkedIDs = ArrayToString <int> (linkedIDs.ToArray ());
				containerData._counts = ArrayToString <int> (counts.ToArray ());
				containerData._IDs = ArrayToString <int> (IDs.ToArray ());
			}
			
			return Serializer.SaveScriptData <ContainerData> (containerData);
		}
		
		
		public override void LoadData (string stringData)
		{
			ContainerData data = Serializer.LoadScriptData <ContainerData> (stringData);
			if (data == null) return;

			if (GetComponent <Container>())
			{
				Container container = GetComponent <Container>();
				container.items.Clear ();

				int[] linkedIDs = StringToIntArray (data._linkedIDs);
				int[] counts = StringToIntArray (data._counts);
				int[] IDs = StringToIntArray (data._IDs);

				for (int i=0; i<IDs.Length; i++)
				{
					ContainerItem newItem = new ContainerItem (linkedIDs[i], counts[i], IDs[i]);
					container.items.Add (newItem);
				}
			}
		}
		
	}
	
	
	[System.Serializable]
	public class ContainerData : RememberData
	{
		public string _linkedIDs;
		public string _counts;
		public string _IDs;

		public ContainerData () { }
	}
	
}