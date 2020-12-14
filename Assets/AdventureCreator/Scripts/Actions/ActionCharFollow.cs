/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionCharFollow.cs"
 * 
 *	This action causes NPCs to follow other characters.
 *	If they are moved in any other way, their following
 *	state will reset
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
	public class ActionCharFollow : Action
	{

		public int npcToMoveParameterID = -1;
		public int charToFollowParameterID = -1;

		public int npcToMoveID = 0;
		public int charToFollowID = 0;

		public NPC npcToMove;
		public Char charToFollow;
		public bool followPlayer;
		public float updateFrequency = 2f;
		public float followDistance = 1f;
		public float followDistanceMax = 15f;
		public enum FollowType { StartFollowing, StopFollowing };
		public FollowType followType;
		
		
		public ActionCharFollow ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Character;
			title = "NPC follow";
			description = "Makes an NPC follow another Character, whether it be a fellow NPC or the Player. If they exceed a maximum distance from their target, they will run towards them. Note that making an NPC move via another Action will make them stop following anyone.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			npcToMove = AssignFile <NPC> (parameters, npcToMoveParameterID, npcToMoveID, npcToMove);
			charToFollow = AssignFile <Char> (parameters, charToFollowParameterID, charToFollowID, charToFollow);
		}
		
		
		override public float Run ()
		{
			if (npcToMove)
			{
				if (followType == FollowType.StopFollowing)
				{
					npcToMove.FollowReset ();
					return 0f;
				}

				if (followPlayer || charToFollow != (Char) npcToMove)
				{
					npcToMove.FollowAssign (charToFollow, followPlayer, updateFrequency, followDistance, followDistanceMax);
				}
			}

			return 0f;
		}

		
		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			npcToMoveParameterID = Action.ChooseParameterGUI ("NPC to affect:", parameters, npcToMoveParameterID, ParameterType.GameObject);
			if (npcToMoveParameterID >= 0)
			{
				npcToMoveID = 0;
				npcToMove = null;
			}
			else
			{
				npcToMove = (NPC) EditorGUILayout.ObjectField ("NPC to affect:", npcToMove, typeof(NPC), true);
				
				npcToMoveID = FieldToID <NPC> (npcToMove, npcToMoveID);
				npcToMove = IDToField <NPC> (npcToMove, npcToMoveID, false);
			}

			followType = (FollowType) EditorGUILayout.EnumPopup ("Follow type:", followType);
			if (followType == FollowType.StartFollowing)
			{
				followPlayer = EditorGUILayout.Toggle ("Follow Player?", followPlayer);
				
				if (!followPlayer)
				{
					charToFollowParameterID = Action.ChooseParameterGUI ("Character to follow:", parameters, charToFollowParameterID, ParameterType.GameObject);
					if (charToFollowParameterID >= 0)
					{
						charToFollowID = 0;
						charToFollow = null;
					}
					else
					{
						charToFollow = (Char) EditorGUILayout.ObjectField ("Character to follow:", charToFollow, typeof(Char), true);
						
						if (charToFollow && charToFollow == (Char) npcToMove)
						{
							charToFollow = null;
							Debug.LogWarning ("An NPC cannot follow themselves!");
						}
						else
						{
							charToFollowID = FieldToID <Char> (charToFollow, charToFollowID);
							charToFollow = IDToField <Char> (charToFollow, charToFollowID, false);
						}
					}

				}

				updateFrequency = EditorGUILayout.FloatField ("Update frequency (s):", updateFrequency);
				if (updateFrequency == 0f || updateFrequency < 0f)
				{
					EditorGUILayout.HelpBox ("Update frequency must be greater than zero.", MessageType.Warning);
				}
				followDistance = EditorGUILayout.FloatField ("Minimum distance:", followDistance);
				if (followDistance <= 0f)
				{
					EditorGUILayout.HelpBox ("Minimum distance must be greater than zero.", MessageType.Warning);
				}
				followDistanceMax = EditorGUILayout.FloatField ("Maximum distance:", followDistanceMax);
				if (followDistanceMax <= 0f || followDistanceMax < followDistance)
				{
					EditorGUILayout.HelpBox ("Maximum distance must be greater than minimum distance.", MessageType.Warning);
				}
			}
			
			AfterRunningOption ();
		}
		
		
		override public string SetLabel ()
		{
			if (npcToMove)
			{
				if (followType == FollowType.StopFollowing)
				{
					return (" (Stop " + npcToMove + ")");
				}
				else
				{
					if (followPlayer)
					{
						return (" (" + npcToMove.name + " to Player)");
					}
					else if (charToFollow)
					{
							return (" (" + npcToMove.name + " to " + charToFollow.name + ")");
					}
				}
			}

			return "";
		}

		#endif
		
	}

}