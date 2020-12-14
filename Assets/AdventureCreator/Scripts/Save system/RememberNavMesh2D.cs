/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RememberNavMesh2D.cs"
 * 
 *	This script is attached to NavMesh2D prefabs
 *	who have their "holes" changed during gameplay.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{
	
	public class RememberNavMesh2D : Remember
	{
		
		public override string SaveData ()
		{
			NavMesh2DData navMesh2DData = new NavMesh2DData ();
			
			navMesh2DData.objectID = constantID;
			
			if (GetComponent <NavigationMesh>())
			{
				NavigationMesh navMesh = GetComponent <NavigationMesh>();
				List<int> linkedIDs = new List<int>();

				for (int i=0; i<navMesh.polygonColliderHoles.Count; i++)
				{
					if (navMesh.polygonColliderHoles[i].GetComponent <ConstantID>())
					{
						linkedIDs.Add (navMesh.polygonColliderHoles[i].GetComponent <ConstantID>().constantID);
					}
					else
					{
						Debug.LogWarning ("Cannot save " + this.gameObject.name + "'s holes because " + navMesh.polygonColliderHoles[i].gameObject.name + " has no Constant ID!");
					}
				}

				navMesh2DData._linkedIDs = ArrayToString <int> (linkedIDs.ToArray ());
			}
			
			return Serializer.SaveScriptData <NavMesh2DData> (navMesh2DData);
		}
		
		
		public override void LoadData (string stringData)
		{
			NavMesh2DData data = Serializer.LoadScriptData <NavMesh2DData> (stringData);
			if (data == null) return;

			if (GetComponent <NavigationMesh>())
			{
				NavigationMesh navMesh = GetComponent <NavigationMesh>();
				navMesh.polygonColliderHoles.Clear ();

				int[] linkedIDs = StringToIntArray (data._linkedIDs);

				for (int i=0; i<linkedIDs.Length; i++)
				{
					PolygonCollider2D polyHole = Serializer.returnComponent <PolygonCollider2D> (linkedIDs[i]);
					if (polyHole != null)
					{
						navMesh.AddHole (polyHole);
					}
				}
			}
		}
		
	}
	
	
	[System.Serializable]
	public class NavMesh2DData : RememberData
	{
		public string _linkedIDs;

		public NavMesh2DData () { }
	}
	
}