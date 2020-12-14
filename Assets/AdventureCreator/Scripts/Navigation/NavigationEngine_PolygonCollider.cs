/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"NavigationEngine_PolygonCollider.cs"
 * 
 *	This script uses a Polygon Collider 2D to
 *	allow pathfinding in a scene. Since v1.37,
 *	it uses the Dijkstra algorithm, as found on
 *	http://rosettacode.org/wiki/Dijkstra%27s_algorithm
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NavigationEngine_PolygonCollider : NavigationEngine
{
	
	public static Collider2D[] results = new Collider2D[1];
	private int MAXNODES = 1000;

	
	public override Vector3[] GetPointsArray (AC.Char _char, Vector3 _originPos, Vector3 _targetPos)
	{
		PolygonCollider2D poly = KickStarter.sceneSettings.navMesh.transform.GetComponent <PolygonCollider2D>();
		bool changesMade = KickStarter.sceneSettings.navMesh.AddCharHoles (_char);

		List<Vector3> pointsList3D = new List<Vector3> ();
		if (IsLineClear (_originPos, _targetPos))
		{
			pointsList3D.Add (_targetPos);
			//ResetHoles (KickStarter.sceneSettings.navMesh, changesMade);
			return pointsList3D.ToArray ();
		}
		
		Vector2[] pointsList = CreateVertexArray (poly);

		Vector2 originPos = GetNearestToMesh (_originPos, poly);
		Vector2 targetPos = GetNearestToMesh (_targetPos, poly);

		pointsList = AddEndsToList (pointsList, originPos, targetPos);
		
		float[,] weight = pointsToWeight (pointsList);
		
		int[] precede = buildSpanningTree (0, 1, weight);
		if (precede == null)
		{
			Debug.LogWarning ("Pathfinding error");
			pointsList3D.Add (_targetPos);
			return pointsList3D.ToArray ();
		}
		int[] _path = getShortestPath (0, 1, precede);
		
		foreach (int i in _path)
		{
			Vector3 vertex = new Vector3 (pointsList[i].x, pointsList[i].y, _originPos.z);
			pointsList3D.Insert (0, vertex);
		}
		
		if (pointsList3D[0] == _originPos || (pointsList3D[0].x == originPos.x && pointsList3D[0].y == originPos.y))
		{
			pointsList3D.RemoveAt (0);	// Remove origin point from start
		}

		ResetHoles (KickStarter.sceneSettings.navMesh, changesMade);
		return pointsList3D.ToArray ();
	}


	private void ResetHoles (NavigationMesh navMesh, bool changesMade)
	{
		if (changesMade)
		{
			navMesh.ResetHoles ();
		}
	}
	
	
	private int[] buildSpanningTree (int source, int destination, float[,] weight)
	{
		int n = (int) Mathf.Sqrt (weight.Length);
		
		bool[] visit = new bool[n];
		float[] distance = new float[n];
		int[] precede = new int[n];
		
		for (int i=0 ; i<n ; i++)
		{
			distance[i] = Mathf.Infinity;
			precede[i] = 100000;
		}
		distance[source] = 0;
		
		int current = source;
		while (current != destination)
		{
			if (current < 0)
			{
				return null;
			}
			
			float distcurr = distance[current];
			float smalldist = Mathf.Infinity;
			int k = -1;
			visit[current] = true;
			
			for (int i=0; i<n; i++)
			{
				if (visit[i])
				{
					continue;
				}
				
				float newdist = distcurr + weight[current,i];
				if (weight[current,i] == -1f)
				{
					newdist = Mathf.Infinity;
				}
				if (newdist < distance[i])
				{
					distance[i] = newdist;
					precede[i] = current;
				}
				if (distance[i] < smalldist)
				{
					smalldist = distance[i];
					k = i;
				}
			}
			current = k;
		}
		
		return precede;
	}
	
	
	private int[] getShortestPath (int source, int destination, int[] precede)
	{
		int i = destination;
		int finall = 0;
		int[] path = new int[MAXNODES];
		
		path[finall] = destination;
		finall++;
		while (precede[i] != source)
		{
			i = precede[i];
			path[finall] = i;
			finall ++;
		}
		path[finall] = source;
		
		int[] result = new int[finall+1];
		
		for (int j=0; j<finall+1; j++)
		{
			result[j] = path[j];
		}
		
		return result;
	}
	
	
	private float[,] pointsToWeight (Vector2[] points)
	{
		int n = points.Length;
		
		float[,] graph = new float [n, n];
		for (int i=0; i<n; i++)
		{
			for (int j=i; j<n; j++)
			{
				if (i==j)
				{
					graph[i,j] = -1;
				}
				else if (!IsLineClear (points[i], points[j]))
				{
					graph[i,j] = graph[j,i] = -1f;
				}
				else
				{
					graph[i,j] = Vector2.Distance (points[i], points[j]);
					graph[j,i] = Vector2.Distance (points[i], points[j]);
				}
			}
		}
		return graph;
	}


	private Vector2 GetNearestToMesh (Vector2 vertex, PolygonCollider2D poly)
	{
		// Test to make sure starting on the collision mesh
		RaycastHit2D hit = Physics2D.Raycast (vertex - new Vector2 (0.005f, 0f), new Vector2 (1f, 0f), 0.01f, 1 << KickStarter.sceneSettings.navMesh.gameObject.layer);
		if (!hit)
		{
			Transform t = KickStarter.sceneSettings.navMesh.transform;
			float minDistance = -1;
			Vector2 nearestPoint = vertex;

			for (int i=0; i<poly.pathCount; i++)
			{
				Vector2[] path = poly.GetPath (i);

				for (int j=0; j<path.Length; j++)
				{
					Vector2 startPoint = t.TransformPoint (path[j]);
					Vector2 endPoint = Vector2.zero;
					if (j==path.Length-1)
					{
						endPoint = t.TransformPoint (path[0]);
					}
					else
					{
						endPoint = t.TransformPoint (path[j+1]);
					}

					Vector2 direction = endPoint - startPoint;
					for (float k=0f; k<=1f; k+=0.1f)
					{
						float distance = Vector2.Distance (vertex, startPoint + (direction * k));

						if (distance < minDistance || minDistance < 0f)
						{
							minDistance = distance;
							nearestPoint = startPoint + (direction * k);
						}
					}
				}
			}
			return nearestPoint;
		}
		return (vertex);	
	}

	
	private Vector2[] CreateVertexArray (PolygonCollider2D poly)
	{
		List <Vector2> vertexData = new List<Vector2>();
		Transform t = KickStarter.sceneSettings.navMesh.transform;
		
		for (int i=0; i<poly.pathCount; i++)
		{
			Vector2[] _vertices = poly.GetPath (i);
			
			foreach (Vector2 _vertex in _vertices)
			{
				Vector3 vertex3D = t.TransformPoint (new Vector3 (_vertex.x, _vertex.y, t.position.z));
				vertexData.Add (new Vector2 (vertex3D.x, vertex3D.y));
			}
		}
		return vertexData.ToArray ();
	}
	
	
	private Vector2[] AddEndsToList (Vector2[] points, Vector2 originPos, Vector2 targetPos)
	{
		List<Vector2> newPoints = new List<Vector2>();
		foreach (Vector2 point in points)
		{
			if (point != originPos && point != targetPos)
			{
				newPoints.Add (point);
			}
		}
		
		newPoints.Insert (0, targetPos);
		newPoints.Insert (0, originPos);
		
		return newPoints.ToArray ();
	}
	
	
	private bool IsLineClear (Vector2 startPos, Vector2 endPos)
	{
		// This will test if points can "see" each other, by doing a circle overlap check along the line between them

		Vector2 actualPos = startPos;

		float radius = (endPos - startPos).magnitude * 0.02f;
		for (float i=0f; i<1f; i+= 0.025f)
		{
			actualPos = startPos + ((endPos - startPos) * i);

			if (Physics2D.OverlapCircleNonAlloc (actualPos, radius, NavigationEngine_PolygonCollider.results, 1 << KickStarter.sceneSettings.navMesh.gameObject.layer) != 1)
			{
				return false;
			}
		}
		return true;
	}
	

	public override string GetPrefabName ()
	{
		return ("NavMesh2D");
	}
	
	
	public override void SetVisibility (bool visibility)
	{
		#if UNITY_EDITOR
		NavigationMesh[] navMeshes = FindObjectsOfType (typeof (NavigationMesh)) as NavigationMesh[];
		Undo.RecordObjects (navMeshes, "Navigation visibility");
		
		foreach (NavigationMesh navMesh in navMeshes)
		{
			navMesh.showInEditor = visibility;
			EditorUtility.SetDirty (navMesh);
		}
		#endif
	}
	
	
	public override void SceneSettingsGUI ()
	{
		#if UNITY_EDITOR
		KickStarter.sceneSettings.navMesh = (NavigationMesh) EditorGUILayout.ObjectField ("Default NavMesh:", KickStarter.sceneSettings.navMesh, typeof (NavigationMesh), true);
		if (KickStarter.settingsManager && !KickStarter.settingsManager.IsUnity2D ())
		{
			EditorGUILayout.HelpBox ("This method is only compatible with 'Unity 2D' mode.", MessageType.Warning);
		}
		#endif
	}
	
}
