/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionSendMessage.cs"
 * 
 *	This action calls "SendMessage" on a GameObject.
 *	Both standard messages, and custom ones with paremeters, can be sent.
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
	public class ActionSendMessage : Action
	{
		
		public int constantID = 0;
		public int parameterID = -1;
		public GameObject linkedObject;
		public bool affectChildren = false;
		
		public MessageToSend messageToSend;
		public enum MessageToSend { TurnOn, TurnOff, Interact, Kill, Custom };
		
		public string customMessage;
		public bool sendValue;
		public int customValue;
		public bool ignoreWhenSkipping = false;
		
		
		public ActionSendMessage ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Object;
			title = "Send message";
			description = "Sends a given message to a GameObject. Can be either a message commonly-used by Adventure Creator (Interact, TurnOn, etc) or a custom one, with an integer argument.";
		}
		
		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			linkedObject = AssignFile (parameters, parameterID, constantID, linkedObject);
		}
		
		
		override public float Run ()
		{
			if (linkedObject)
			{
				if (messageToSend == MessageToSend.Custom)
				{
					if (affectChildren)
					{
						if (!sendValue)
						{
							linkedObject.BroadcastMessage (customMessage, SendMessageOptions.DontRequireReceiver);
						}
						else
						{
							linkedObject.BroadcastMessage (customMessage, customValue, SendMessageOptions.DontRequireReceiver);
						}
					}
					else
					{
						if (!sendValue)
						{
							linkedObject.SendMessage (customMessage);
						}
						else
						{
							linkedObject.SendMessage (customMessage, customValue);
						}
					}
				}
				else
				{
					if (affectChildren)
					{
						linkedObject.BroadcastMessage (messageToSend.ToString (), SendMessageOptions.DontRequireReceiver);
					}
					else
					{
						linkedObject.SendMessage (messageToSend.ToString ());
					}
				}
			}
			
			return 0f;
		}
		
		override public void Skip ()
		{
			if (!ignoreWhenSkipping)
			{
				Run ();
			}
		}
		
		override public ActionEnd End (List<AC.Action> actions)
		{
			// If the linkedObject is an immediately-starting ActionList, don't end the cutscene
			if (linkedObject && messageToSend == MessageToSend.Interact && linkedObject.GetComponent <Cutscene>())
			{
				Cutscene tempAction = linkedObject.GetComponent<Cutscene>();
				
				if (tempAction.triggerTime == 0f)
				{
					ActionEnd actionEnd = new ActionEnd ();
					actionEnd.resultAction = ResultAction.RunCutscene;
					return actionEnd;
				}
			}
			
			return (base.End (actions));
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI (List<ActionParameter> parameters)
		{
			parameterID = Action.ChooseParameterGUI ("Object to affect:", parameters, parameterID, ParameterType.GameObject);
			if (parameterID >= 0)
			{
				constantID = 0;
				linkedObject = null;
			}
			else
			{
				linkedObject = (GameObject) EditorGUILayout.ObjectField ("Object to affect:", linkedObject, typeof(GameObject), true);
				
				constantID = FieldToID (linkedObject, constantID);
				linkedObject = IDToField  (linkedObject, constantID, false);
			}
			
			messageToSend = (MessageToSend) EditorGUILayout.EnumPopup ("Message to send:", messageToSend);
			if (messageToSend == MessageToSend.Custom)
			{
				customMessage = EditorGUILayout.TextField ("Method name:", customMessage);
				
				sendValue = EditorGUILayout.Toggle ("Pass integer to method?", sendValue);
				if (sendValue)
				{
					customValue = EditorGUILayout.IntField ("Integer to send:", customValue);
				}
			}
			
			affectChildren = EditorGUILayout.Toggle ("Send to children too?", affectChildren);
			ignoreWhenSkipping = EditorGUILayout.Toggle ("Ignore when skipping?", ignoreWhenSkipping);
			
			AfterRunningOption ();
		}
		
		
		public override string SetLabel ()
		{
			string labelAdd = "";
			
			if (linkedObject)
			{
				if (messageToSend == MessageToSend.TurnOn)
				{
					labelAdd += " ('Turn on' ";
				}
				else if (messageToSend == MessageToSend.TurnOff)
				{
					labelAdd += " ('Turn off' ";
				}
				else if (messageToSend == MessageToSend.Interact)
				{
					labelAdd += " ('Interact' ";
				}
				else if (messageToSend == MessageToSend.Kill)
				{
					labelAdd += " ('Kill' ";
				}
				else
				{
					labelAdd += " ('" + customMessage + "' ";
				}
				
				labelAdd += " to " + linkedObject.name + ")";
			}
			
			return labelAdd;
		}
		
		#endif
		
	}
	
}