/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"NavigationEngine_meshCollider.cs"
 * 
 *	This script uses Unity's built-in Navigation
 *	system to allow pathfinding in a scene.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NavigationEngine_UnityNavigation : NavigationEngine
{

	public override Vector3[] GetPointsArray (Vector3 startPosition, Vector3 targetPosition)
	{
		UnityEngine.AI.NavMeshHit hit = new UnityEngine.AI.NavMeshHit();
		UnityEngine.AI.NavMeshPath _path = new UnityEngine.AI.NavMeshPath();

		if (!UnityEngine.AI.NavMesh.CalculatePath (startPosition, targetPosition, -1, _path))
		{
			// Could not find path with current vectors
			float maxDistance = 0.001f;

			for (maxDistance = 0.001f; maxDistance < 1f; maxDistance += 0.05f)
			{
				if (UnityEngine.AI.NavMesh.SamplePosition (startPosition, out hit, maxDistance, -1))
				{
					startPosition = hit.position;
					break;
				}
			}

			for (maxDistance = 0.001f; maxDistance < 1f; maxDistance += 0.05f)
			{
				if (UnityEngine.AI.NavMesh.SamplePosition (targetPosition, out hit, maxDistance, -1))
				{
					targetPosition = hit.position;
					break;
				}
			}

			UnityEngine.AI.NavMesh.CalculatePath (startPosition, targetPosition, -1, _path);
		}

		List<Vector3> pointArray = new List<Vector3>();
		foreach (Vector3 corner in _path.corners)
		{
			pointArray.Add (corner);
		}
		if (pointArray.Count > 1 && pointArray[0].x == startPosition.x && pointArray[0].z == startPosition.z)
		{
			pointArray.RemoveAt (0);
		}
		else if (pointArray.Count == 0)
		{
			pointArray.Clear ();
			pointArray.Add (targetPosition);
		}

		return (pointArray.ToArray ());
	}


	public override string GetPrefabName ()
	{
		return ("NavMeshSegment");
	}


	public override void SetVisibility (bool visibility)
	{
		NavMeshSegment[] navMeshSegments = FindObjectsOfType (typeof (NavMeshSegment)) as NavMeshSegment[];
		
		#if UNITY_EDITOR
		Undo.RecordObjects (navMeshSegments, "NavMesh visibility");
		#endif
		
		foreach (NavMeshSegment navMeshSegment in navMeshSegments)
		{
			if (visibility)
			{
				navMeshSegment.Show ();
			}
			else
			{
				navMeshSegment.Hide ();
			}
			
			#if UNITY_EDITOR
			EditorUtility.SetDirty (navMeshSegment);
			#endif
		}
	}

}
