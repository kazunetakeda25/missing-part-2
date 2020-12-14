/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionCharFace.cs"
 * 
 *	This action is used to make characters turn to face GameObjects.
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
	public class ActionCharFace : Action
	{

		public int charToMoveParameterID = -1;
		public int faceObjectParameterID = -1;

		public int charToMoveID = 0;
		public int faceObjectID = 0;

		public bool isInstant;
		public Char charToMove;
		public GameObject faceObject;
		public bool copyRotation;
		public bool facePlayer;
		
		public CharFaceType faceType = CharFaceType.Body;
		public bool isPlayer;
		public bool lookUpDown;
		public bool stopLooking = false;


		public ActionCharFace ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Character;
			title = "Face object";
			description = "Makes a Character turn, either instantly or over time. Can turn to face another object, or copy that object's facing direction.";
		}


		public override void AssignValues (List<ActionParameter> parameters)
		{
			charToMove = AssignFile <Char> (parameters, charToMoveParameterID, charToMoveID, charToMove);
			faceObject = AssignFile (parameters, faceObjectParameterID, faceObjectID, faceObject);

			if (isPlayer)
			{
				charToMove = KickStarter.player;
			}
			else if (facePlayer && KickStarter.player)
			{
				faceObject = KickStarter.player.gameObject;
			}
		}

		
		override public float Run ()
		{
			if (!isRunning)
			{
				isRunning = true;
			
				if (faceObject == null && (faceType == CharFaceType.Body || (faceType == CharFaceType.Head && !stopLooking)))
				{
					return 0f;
				}

				if (charToMove)
				{
					if (faceType == CharFaceType.Body)
					{
						if (lookUpDown && isPlayer && KickStarter.settingsManager.IsInFirstPerson ())
						{
							Player player = (Player) charToMove;
							player.SetTilt (faceObject.transform.position, isInstant);
						}

						charToMove.SetLookDirection (GetLookVector (KickStarter.settingsManager), isInstant);
					}
					else if (faceType == CharFaceType.Head)
					{
						if (stopLooking)
						{
							charToMove.ClearHeadTurnTarget (HeadFacing.Manual, isInstant);
						}
						else
						{
							Vector3 lookAtPosition = faceObject.transform.position;

							if (faceObject.GetComponent <Hotspot>())
							{
								lookAtPosition = faceObject.GetComponent <Hotspot>().GetIconPosition ();
							}

							charToMove.SetHeadTurnTarget (lookAtPosition, isInstant);
						}
					}

					if (isInstant)
					{
						return 0f;
					}
					else
					{
						if (faceType == CharFaceType.Body)
						{
							charToMove.EndPath ();
						}
						
						if (willWait)
						{
							return (defaultPauseTime);
						}
						else
						{
							return 0f;
						}
					}
				}

				return 0f;
			}
			else
			{
				if (faceType == CharFaceType.Head && charToMove.IsMovingHead ())
				{
					return defaultPauseTime;
				}
				else if (faceType == CharFaceType.Body && charToMove.IsTurning ())
				{
					return defaultPauseTime;
				}
				else
				{
					isRunning = false;
					return 0f;
				}
			}
		}


		override public void Skip ()
		{
			if (faceObject == null && (faceType == CharFaceType.Body || (faceType == CharFaceType.Head && !stopLooking)))
			{
				return;
			}
			
			if (charToMove)
			{
				if (faceType == CharFaceType.Body)
				{
					charToMove.SetLookDirection (GetLookVector (KickStarter.settingsManager), true);
					
					if (lookUpDown && isPlayer && KickStarter.settingsManager.IsInFirstPerson ())
					{
						Player player = (Player) charToMove;
						player.SetTilt (faceObject.transform.position, true);
					}
				}

				else if (faceType == CharFaceType.Head)
				{
					if (stopLooking)
					{
						charToMove.ClearHeadTurnTarget (HeadFacing.Manual, true);
					}
					else
					{
						charToMove.SetHeadTurnTarget (faceObject.transform.position, true);
					}
				}
			}
		}

		
		private Vector3 GetLookVector (SettingsManager settingsManager)
		{
			Vector3 lookVector = faceObject.transform.position - charToMove.transform.position;
			if (copyRotation)
			{
				lookVector = faceObject.transform.forward;
			}
			else if (KickStarter.settingsManager.ActInScreenSpace ())
			{
				lookVector = AdvGame.GetScreenDirection (charToMove.transform.position, faceObject.transform.position);
			}

			return lookVector;
		}


		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			isPlayer = EditorGUILayout.Toggle ("Affect Player?", isPlayer);
			if (!isPlayer)
			{
				charToMoveParameterID = Action.ChooseParameterGUI ("Character to turn:", parameters, charToMoveParameterID, ParameterType.GameObject);
				if (charToMoveParameterID >= 0)
				{
					charToMoveID = 0;
					charToMove = null;
				}
				else
				{
					charToMove = (Char) EditorGUILayout.ObjectField ("Character to turn:", charToMove, typeof(Char), true);
					
					charToMoveID = FieldToID <Char> (charToMove, charToMoveID);
					charToMove = IDToField <Char> (charToMove, charToMoveID, false);
				}

				facePlayer = EditorGUILayout.Toggle ("Face player?", facePlayer);
			}
			else
			{
				facePlayer = false;
				
				SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
				if (faceType == CharFaceType.Body && settingsManager && settingsManager.IsInFirstPerson ())
				{
					lookUpDown = EditorGUILayout.Toggle ("FPS head tilt?", lookUpDown);
				}
			}

			faceType = (CharFaceType) EditorGUILayout.EnumPopup ("Face with:", faceType);
			if (faceType == CharFaceType.Head)
			{
				stopLooking = EditorGUILayout.Toggle ("Stop looking?", stopLooking);
			}

			if (facePlayer || (faceType == CharFaceType.Head && stopLooking))
			{ }
			else
			{
				faceObjectParameterID = Action.ChooseParameterGUI ("Object to face:", parameters, faceObjectParameterID, ParameterType.GameObject);
				if (faceObjectParameterID >= 0)
				{
					faceObjectID = 0;
					faceObject = null;
				}
				else
				{
					faceObject = (GameObject) EditorGUILayout.ObjectField ("Object to face:", faceObject, typeof(GameObject), true);
					
					faceObjectID = FieldToID (faceObject, faceObjectID);
					faceObject = IDToField (faceObject, faceObjectID, false);
				}
			}

			if (faceType == CharFaceType.Body)
			{
				copyRotation = EditorGUILayout.Toggle ("Use object's rotation?", copyRotation);
			}

			isInstant = EditorGUILayout.Toggle ("Is instant?", isInstant);
			if (!isInstant)
			{
				willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);
			}

			AfterRunningOption ();
		}

		
		override public string SetLabel ()
		{
			string labelAdd = "";
			
			if (charToMove && faceObject)
			{
				labelAdd = " (" + charToMove.name + " to " + faceObject.name + ")";
			}
			else if (isPlayer && faceObject)
			{
				labelAdd = " (Player to " + faceObject.name + ")";
			}
			
			return labelAdd;
		}

		#endif
		
	}

}