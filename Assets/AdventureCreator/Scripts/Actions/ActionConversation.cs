/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionConversation.cs"
 * 
 *	This action turns on a conversation.
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
	public class ActionConversation : ActionCheckMultiple
	{

		public int parameterID = -1;
		public int constantID = 0;
		public Conversation conversation;

		public bool overrideOptions = false;


		public ActionConversation ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Dialogue;
			title = "Start conversation";
			description = "Enters Conversation mode, and displays the available dialogue options in a specified conversation.";
			numSockets = 0;
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			conversation = AssignFile <Conversation> (parameters, parameterID, constantID, conversation);
		}
				
		
		override public float Run ()
		{
			if (overrideOptions)
			{
				if (KickStarter.playerInput.lastConversationOption >= 0)
				{
					KickStarter.playerInput.ignoreNextConversationSkip = true;
					return 0f;
				}
				KickStarter.playerInput.ignoreNextConversationSkip = false;
			}

			if (conversation)
			{
				if (overrideOptions)
				{
					conversation.Interact (this);
				}
				else
				{
					conversation.Interact ();
				}
			}
			
			return 0f;
		}


		override public void Skip ()
		{
			if (KickStarter.playerInput.ignoreNextConversationSkip)
			{
				KickStarter.playerInput.ignoreNextConversationSkip = false;
				return;
			}
			Run ();
		}

		
		override public ActionEnd End (List<AC.Action> actions)
		{
			int _chosenOptionIndex = KickStarter.playerInput.lastConversationOption;
			KickStarter.playerInput.lastConversationOption = -1;

			if (overrideOptions && _chosenOptionIndex >= 0 && endings.Count > _chosenOptionIndex)
			{
				return endings[_chosenOptionIndex];
			}
			return GenerateStopActionEnd ();
		}
		
		
		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			parameterID = Action.ChooseParameterGUI ("Conversation:", parameters, parameterID, ParameterType.GameObject);
			if (parameterID >= 0)
			{
				constantID = 0;
				conversation = null;
			}
			else
			{
				conversation = (Conversation) EditorGUILayout.ObjectField ("Conversation:", conversation, typeof (Conversation), true);
				
				constantID = FieldToID <Conversation> (conversation, constantID);
				conversation = IDToField <Conversation> (conversation, constantID, false);
			}

			if (conversation)
			{
				conversation.Upgrade ();

				overrideOptions = EditorGUILayout.Toggle ("Override defaults?", overrideOptions);
				if (overrideOptions)
				{
					numSockets = conversation.options.Count;
				}
				else
				{
					numSockets = 0;
				}
			}
			else
			{
				numSockets = 0;
			}
		}


		protected override string GetSocketLabel (int i)
		{
			i -= 1;
			if (conversation != null && conversation.options.Count > i)
			{
				return ("'" + conversation.options[i].label + "':");
			}
			return i.ToString ();
		}

		
		override public string SetLabel ()
		{
			string labelAdd = "";
			
			if (conversation)
			{
				labelAdd = " (" + conversation + ")";
			}
			
			return labelAdd;
		}

		#endif
		
	}

}