/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionCharPathFind.cs"
 * 
 *	This action moves characters by generating a path to a specified point.
 *	If a player is moved, the game will automatically pause.
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

	[System.Serializable]
	public class ActionCharPathFind : Action
	{

		public int charToMoveParameterID = -1;
		public int markerParameterID = -1;

		public int charToMoveID = 0;
		public int markerID = 0;
		
		public Marker marker;
		public bool isPlayer;
		public Char charToMove;
		public PathSpeed speed;
		public bool pathFind = true;
		public bool doFloat = false;
		
		
		public ActionCharPathFind ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Character;
			title = "Move to point";
			description = "Moves a character to a given Marker object. By default, the character will attempt to pathfind their way to the marker, but can optionally just move in a straight line.";
		}


		public override void AssignValues (List<ActionParameter> parameters)
		{
			charToMove = AssignFile <Char> (parameters, charToMoveParameterID, charToMoveID, charToMove);
			marker = AssignFile <Marker> (parameters, markerParameterID, markerID, marker);

			if (isPlayer)
			{
				charToMove = KickStarter.player;
			}
		}
		
		
		override public float Run ()
		{
			if (!isRunning)
			{
				isRunning = true;

				if (charToMove && marker)
				{
					Paths path = charToMove.GetComponent <Paths>();
					if (path == null)
					{
						Debug.LogWarning ("Cannot move a character with no Paths component");
					}
					else
					{
						if (charToMove is NPC)
						{
							NPC npcToMove = (NPC) charToMove;
							npcToMove.FollowReset ();
						}

						path.pathType = AC_PathType.ForwardOnly;
						path.pathSpeed = speed;
						path.affectY = true;

						Vector3[] pointArray;
						Vector3 targetPosition = marker.transform.position;

						if (KickStarter.settingsManager.ActInScreenSpace ())
						{
							targetPosition = AdvGame.GetScreenNavMesh (targetPosition);
						}

						if (pathFind && KickStarter.navigationManager)
						{
							pointArray = KickStarter.navigationManager.navigationEngine.GetPointsArray (charToMove, charToMove.transform.position, targetPosition);
						}
						else
						{
							List<Vector3> pointList = new List<Vector3>();
							pointList.Add (targetPosition);
							pointArray = pointList.ToArray ();
						}

						if (speed == PathSpeed.Walk)
						{
							charToMove.MoveAlongPoints (pointArray, false);
						}
						else
						{
							charToMove.MoveAlongPoints (pointArray, true);
						}

						if (charToMove.activePath)
						{
							if (!pathFind && doFloat)
							{
								charToMove.activePath.affectY = true;
							}
							else
							{
								charToMove.activePath.affectY = false;
							}
						}

						if (willWait)
						{
							return defaultPauseTime;
						}
					}
				}

				return 0f;
			}
			else
			{
				if (!charToMove.IsPathfinding ())
				{
					isRunning = false;
					return 0f;
				}
				else
				{
					return (defaultPauseTime);
				}
			}
		}


		override public void Skip ()
		{
			if (charToMove && marker)
			{
				charToMove.EndPath ();

				if (charToMove is NPC)
				{
					NPC npcToMove = (NPC) charToMove;
					npcToMove.FollowReset ();
				}
				
				Vector3[] pointArray;
				Vector3 targetPosition = marker.transform.position;
				
				if (KickStarter.settingsManager.ActInScreenSpace ())
				{
					targetPosition = AdvGame.GetScreenNavMesh (targetPosition);
				}
				
				if (pathFind && KickStarter.navigationManager)
				{
					pointArray = KickStarter.navigationManager.navigationEngine.GetPointsArray (charToMove.transform.position, targetPosition);
				}
				else
				{
					List<Vector3> pointList = new List<Vector3>();
					pointList.Add (targetPosition);
					pointArray = pointList.ToArray ();
				}
				
				int i = pointArray.Length-1;

				if (i>0)
				{
					charToMove.SetLookDirection (pointArray[i] - pointArray[i-1], true);
				}
				else
				{
					charToMove.SetLookDirection (pointArray[i] - charToMove.transform.position, true);
				}

				charToMove.Teleport (pointArray [i]);
			}
		}

		
		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			isPlayer = EditorGUILayout.Toggle ("Is Player?", isPlayer);

			if (!isPlayer)
			{
				charToMoveParameterID = Action.ChooseParameterGUI ("Character to move:", parameters, charToMoveParameterID, ParameterType.GameObject);
				if (charToMoveParameterID >= 0)
				{
					charToMoveID = 0;
					charToMove = null;
				}
				else
				{
					charToMove = (Char) EditorGUILayout.ObjectField ("Character to move:", charToMove, typeof (Char), true);
					
					charToMoveID = FieldToID <Char> (charToMove, charToMoveID);
					charToMove = IDToField <Char> (charToMove, charToMoveID, false);
				}
			}

			markerParameterID = Action.ChooseParameterGUI ("Marker to reach:", parameters, markerParameterID, ParameterType.GameObject);
			if (markerParameterID >= 0)
			{
				markerID = 0;
				marker = null;
			}
			else
			{
				marker = (Marker) EditorGUILayout.ObjectField ("Marker to reach:", marker, typeof (Marker), true);
				
				markerID = FieldToID <Marker> (marker, markerID);
				marker = IDToField <Marker> (marker, markerID, false);
			}

			speed = (PathSpeed) EditorGUILayout.EnumPopup ("Move speed:" , speed);
			pathFind = EditorGUILayout.Toggle ("Pathfind?", pathFind);
			if (!pathFind)
			{
				doFloat = EditorGUILayout.Toggle ("Ignore gravity?", doFloat);
			}
			willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);

			AfterRunningOption ();
		}
		
		
		override public string SetLabel ()
		{
			string labelAdd = "";
			
			if (marker)
			{
				if (charToMove)
				{
					labelAdd = " (" + charToMove.name + " to " + marker.name + ")";
				}
				else if (isPlayer)
				{
					labelAdd = " (Player to " + marker.name + ")";
				}
			}
			
			return labelAdd;
		}

		#endif
		
	}

}