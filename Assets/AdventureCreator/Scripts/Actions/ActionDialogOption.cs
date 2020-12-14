/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionDialogOption.cs"
 * 
 *	This action changes the visibility of dialogue options.
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
	public class ActionDialogOption : Action
	{
		
		public enum SwitchType { On, Off, OnForever, OffForever };
		public SwitchType switchType;
		public int optionNumber; // This is now the ID number minus one

		public int constantID;
		public Conversation linkedConversation;
		
		
		public ActionDialogOption ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Dialogue;
			title = "Toggle option";
			description = "Sets the display of a dialogue option. Can hide, show, and lock options.";
		}


		override public void AssignValues ()
		{
			linkedConversation = AssignFile <Conversation> (constantID, linkedConversation);
		}

		
		override public float Run ()
		{
			bool setOption = false;
			bool clampOption = false;
			
			if (switchType == SwitchType.On || switchType == SwitchType.OnForever)
			{
				setOption = true;
			}
			
			if (switchType == SwitchType.OffForever || switchType == SwitchType.OnForever)
			{
				clampOption = true;
			}
			
			if (linkedConversation)
			{
				linkedConversation.SetOption (optionNumber+1, setOption, clampOption);
			}
			
			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI ()
		{
			linkedConversation = (Conversation) EditorGUILayout.ObjectField ("Conversation:", linkedConversation, typeof (Conversation), true);

			if (linkedConversation)
			{
				linkedConversation.Upgrade ();
			}

			constantID = FieldToID <Conversation> (linkedConversation, constantID);
			linkedConversation = IDToField <Conversation> (linkedConversation, constantID, true);

			if (linkedConversation)
			{
				optionNumber = ShowOptionGUI (linkedConversation.options, optionNumber);
			}

			switchType = (SwitchType) EditorGUILayout.EnumPopup ("Set to:", switchType);
			
			AfterRunningOption ();
		}


		private int ShowOptionGUI (List<ButtonDialog> options, int optionID)
		{
			// Create a string List of the field's names (for the PopUp box)
			List<string> labelList = new List<string>();
			
			int i = 0;
			int tempNumber = -1;

			if (options.Count > 0)
			{
				foreach (ButtonDialog option in options)
				{
					string label = option.ID.ToString () + ": " + option.label;
					if (option.label == "")
					{
						label += "(Untitled option)";
					}
					labelList.Add (label);
					
					if (option.ID == (optionID+1))
					{
						tempNumber = i;
					}
					
					i ++;
				}
				
				if (tempNumber == -1)
				{
					// Wasn't found (variable was deleted?), so revert to zero
					Debug.LogWarning ("Previously chosen option no longer exists!");
					tempNumber = 0;
					optionID = 0;
				}

				tempNumber = EditorGUILayout.Popup (tempNumber, labelList.ToArray());
				optionID = options [tempNumber].ID-1;
			}
			else
			{
				EditorGUILayout.HelpBox ("No options exist!", MessageType.Info);
				optionID = -1;
				tempNumber = -1;
			}
			
			return optionID;
		}
		
		#endif
		
	}

}