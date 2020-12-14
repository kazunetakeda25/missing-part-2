/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"NavigationMesh.cs"
 * 
 *	This script is used by the MeshCollider and PolygonCollider
 *  navigation methods to define the pathfinding area.
 * 
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	public class NavigationMesh : NavMeshBase
	{

		public List<PolygonCollider2D> polygonColliderHoles = new List<PolygonCollider2D>();
		public bool showInEditor = true;
		public bool moveAroundChars = true;


		private void Awake ()
		{
			BaseAwake ();
			ResetHoles ();
		}


		public void AddHole (PolygonCollider2D newHole)
		{
			if (polygonColliderHoles.Contains (newHole))
			{
				return;
			}

			polygonColliderHoles.Add (newHole);
			ResetHoles ();

			if (GetComponent <RememberNavMesh2D>() == null)
			{
				Debug.LogWarning ("Changes to " + this.gameObject.name + "'s holes will not be saved because it has no RememberNavMesh2D script");
			}
		}


		public void RemoveHole (PolygonCollider2D oldHole)
		{
			if (polygonColliderHoles.Contains (oldHole))
			{
				polygonColliderHoles.Remove (oldHole);
				ResetHoles ();
			}
		}


		public bool AddCharHoles (Char charToExclude)
		{
			if (!moveAroundChars)
			{
				return false;
			}

			bool changesMade = false;

			if (GetComponent <PolygonCollider2D>())
			{
				PolygonCollider2D poly = GetComponent <PolygonCollider2D>();
				AC.Char[] characters = GameObject.FindObjectsOfType (typeof (AC.Char)) as AC.Char[];

				foreach (AC.Char character in characters)
				{
					CircleCollider2D circleCollider2D = character.GetComponent <CircleCollider2D>();
					if (circleCollider2D != null && character.charState == CharState.Idle
					    && (charToExclude == null || character != charToExclude)
					    && Physics2D.OverlapPointNonAlloc (character.transform.position, NavigationEngine_PolygonCollider.results, 1 << KickStarter.sceneSettings.navMesh.gameObject.layer) != 0)
					{
						circleCollider2D.isTrigger = true;

						List<Vector2> newPoints3D = new List<Vector2>();

						#if UNITY_5
						Vector2 centrePoint =  character.transform.TransformPoint (circleCollider2D.offset);
						#else
						Vector2 centrePoint =  character.transform.TransformPoint (circleCollider2D.offset);
						#endif

						float radius = circleCollider2D.radius * character.transform.localScale.x;

						newPoints3D.Add (centrePoint + Vector2.up * radius);
						newPoints3D.Add (centrePoint + Vector2.right * radius);
						newPoints3D.Add (centrePoint - Vector2.up * radius);
						newPoints3D.Add (centrePoint - Vector2.right * radius);

						poly.pathCount ++;
						
						List<Vector2> newPoints = new List<Vector2>();
						foreach (Vector3 holePoint in newPoints3D)
						{
							newPoints.Add (holePoint - transform.position);
						}
						
						poly.SetPath (poly.pathCount-1, newPoints.ToArray ());
						changesMade = true;
					}
				}
			}

			return changesMade;
		}


		public void ResetHoles ()
		{
			if (GetComponent <PolygonCollider2D>())
			{
				PolygonCollider2D poly = GetComponent <PolygonCollider2D>();
				poly.pathCount = 1;
			
				if (polygonColliderHoles.Count == 0)
				{
					return;
				}

				foreach (PolygonCollider2D hole in polygonColliderHoles)
				{
					if (hole != null)
					{
						poly.pathCount ++;
						
						List<Vector2> newPoints = new List<Vector2>();
						foreach (Vector2 holePoint in hole.points)
						{
							newPoints.Add (hole.transform.TransformPoint (holePoint) - transform.position);
						}
						
						poly.SetPath (poly.pathCount-1, newPoints.ToArray ());
						hole.gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.deactivatedLayer);
						hole.isTrigger = true;
					}
				}
			}
			else if (GetComponent <MeshCollider>())
			{
				if (GetComponent <MeshCollider>().sharedMesh == null)
				{
					if (GetComponent <MeshFilter>() && GetComponent <MeshFilter>().sharedMesh)
					{
						GetComponent <MeshCollider>().sharedMesh = GetComponent <MeshFilter>().sharedMesh;
						Debug.LogWarning (this.gameObject.name + " has no MeshCollider mesh - temporarily using MeshFilter mesh instead.");
					}
					else
					{
						Debug.LogWarning (this.gameObject.name + " has no MeshCollider mesh.");
					}
				}
			}
		}
		
		
		public void TurnOn ()
		{
			if (KickStarter.sceneSettings.navigationMethod == AC_NavigationMethod.meshCollider || KickStarter.sceneSettings.navigationMethod == AC_NavigationMethod.PolygonCollider)
			{
				if (LayerMask.NameToLayer (KickStarter.settingsManager.navMeshLayer) == -1)
				{
					Debug.LogError ("Can't find layer " + KickStarter.settingsManager.navMeshLayer + " - please define it in Unity's Tags Manager (Edit -> Project settings -> Tags and Layers).");
				}
				else if (KickStarter.settingsManager.navMeshLayer != "")
				{
					gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.navMeshLayer);
				}
				
				if (KickStarter.sceneSettings.navigationMethod == AC_NavigationMethod.meshCollider && GetComponent <Collider>() == null)
				{
					Debug.LogWarning ("A Collider component must be attached to " + this.name + " for pathfinding to work - please attach one.");
				}
				else if (KickStarter.sceneSettings.navigationMethod == AC_NavigationMethod.PolygonCollider && GetComponent <Collider2D>() == null)
				{
					Debug.LogWarning ("A 2D Collider component must be attached to " + this.name + " for pathfinding to work - please attach one.");
				}
			}
			else
			{
				Debug.LogWarning ("Cannot enable NavMesh " + this.name + " as this scene's Navigation Method is Unity Navigation.");
			}
		}
		
		
		public void TurnOff ()
		{
			gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.deactivatedLayer);
		}


		protected void OnDrawGizmos ()
		{
			if (showInEditor)
			{
				DrawGizmos ();
			}
		}
		
		
		protected void OnDrawGizmosSelected ()
		{
			DrawGizmos ();
		}


		public virtual void DrawGizmos ()
		{
			if (GetComponent <PolygonCollider2D>())
			{
				AdvGame.DrawPolygonCollider (transform, GetComponent <PolygonCollider2D>(), Color.white);
			}
		}

	}

}