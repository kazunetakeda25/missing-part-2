/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionTeleport.cs"
 * 
 *	This action moves an object to a specified GameObject's position.
 *	Markers are helpful in this regard.
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
	public class ActionTeleport : Action
	{

		public int obToMoveParameterID = -1;
		public int obToMoveID = 0;
		public int markerParameterID = -1;
		public int markerID = 0;

		public PositionRelativeTo positionRelativeTo = PositionRelativeTo.Nothing;

		public bool isPlayer;
		public GameObject obToMove;
		public Marker teleporter;
		public bool copyRotation;
		

		public ActionTeleport ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Object;
			title = "Teleport";
			description = "Moves a GameObject to a Marker instantly. Can also copy the Marker's rotation. The final position can optionally be made relative to the active camera, or the player. For example, if the Marker's position is (0, 0, 1) and Positon relative to is set to Relative To Active Camera, then the object will be teleported in front of the camera.";
		}
		
		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			obToMove = AssignFile (parameters, obToMoveParameterID, obToMoveID, obToMove);
			teleporter = AssignFile <Marker> (parameters, markerParameterID, markerID, teleporter);

			if (isPlayer && KickStarter.player)
			{
				obToMove = KickStarter.player.gameObject;
			}
		}
		
		
		override public float Run ()
		{
			if (teleporter && obToMove)
			{
				Vector3 position = teleporter.transform.position;
				Quaternion rotation = teleporter.transform.rotation;

				if (positionRelativeTo == PositionRelativeTo.RelativeToActiveCamera)
				{
					Transform mainCam = KickStarter.mainCamera.transform;

					float right = teleporter.transform.position.x;
					float up = teleporter.transform.position.y;
					float forward = teleporter.transform.position.z;

					position = mainCam.position + (mainCam.forward * forward) + (mainCam.right * right) + (mainCam.up * up);
					rotation.eulerAngles += mainCam.transform.rotation.eulerAngles;
				}
				else if (positionRelativeTo == PositionRelativeTo.RelativeToPlayer && !isPlayer)
				{
					if (KickStarter.player)
					{
						Transform playerTranform = KickStarter.player.transform;

						float right = teleporter.transform.position.x;
						float up = teleporter.transform.position.y;
						float forward = teleporter.transform.position.z;
						
						position = playerTranform.position + (playerTranform.forward * forward) + (playerTranform.right * right) + (playerTranform.up * up);
						rotation.eulerAngles += playerTranform.rotation.eulerAngles;
					}
				}

				if (obToMove.GetComponent <Char>())
				{
					obToMove.GetComponent <Char>().Teleport (position);
				}
				else
				{
					obToMove.transform.position = position;
				}
				
				if (copyRotation)
				{
					obToMove.transform.rotation = rotation;

					if (obToMove.GetComponent <Char>())
					{
						// Is a character, so set the lookDirection, otherwise will revert back to old rotation
						obToMove.GetComponent <Char>().SetLookDirection (teleporter.transform.forward, true);
						obToMove.GetComponent <Char>().Halt ();
					}
				}
			}
			
			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			isPlayer = EditorGUILayout.Toggle ("Is Player?", isPlayer);
			if (!isPlayer)
			{
				obToMoveParameterID = Action.ChooseParameterGUI ("Object to move:", parameters, obToMoveParameterID, ParameterType.GameObject);
				if (obToMoveParameterID >= 0)
				{
					obToMoveID = 0;
					obToMove = null;
				}
				else
				{
					obToMove = (GameObject) EditorGUILayout.ObjectField ("Object to move:", obToMove, typeof(GameObject), true);
					
					obToMoveID = FieldToID (obToMove, obToMoveID);
					obToMove = IDToField (obToMove, obToMoveID, false);
				}
			}

			markerParameterID = Action.ChooseParameterGUI ("Teleport to:", parameters, markerParameterID, ParameterType.GameObject);
			if (markerParameterID >= 0)
			{
				markerID = 0;
				teleporter = null;
			}
			else
			{
				teleporter = (Marker) EditorGUILayout.ObjectField ("Teleport to:", teleporter, typeof (Marker), true);
				
				markerID = FieldToID <Marker> (teleporter, markerID);
				teleporter = IDToField <Marker> (teleporter, markerID, false);
			}
			
			positionRelativeTo = (PositionRelativeTo) EditorGUILayout.EnumPopup ("Position relative to:", positionRelativeTo);
			copyRotation = EditorGUILayout.Toggle ("Copy rotation?", copyRotation);

			AfterRunningOption ();
		}
		
		
		override public string SetLabel ()
		{
			string labelAdd = "";
			
			if (teleporter)
			{
				if (obToMove)
				{
					labelAdd = " (" + obToMove.name + " to " + teleporter.name + ")";
				}
				else if (isPlayer)
				{
					labelAdd = " (Player to " + teleporter.name + ")";
				}
			}
			
			return labelAdd;
		}
		
		#endif
	}

}