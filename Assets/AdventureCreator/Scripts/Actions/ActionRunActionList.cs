/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionRunActionList.cs"
 * 
 *	This Action runs other ActionLists
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
	public class ActionRunActionList : Action
	{
		
		public enum ListSource { InScene, AssetFile };
		public ListSource listSource = ListSource.InScene;

		public ActionList actionList;
		public ActionListAsset invActionList;
		public int constantID = 0;
		public int parameterID = -1;

		public bool runFromStart = true;
		public int jumpToAction;
		public int jumpToActionParameterID = -1;
		public AC.Action jumpToActionActual;
		public bool runInParallel = false; // No longer visible, but needed for legacy upgrades

		public bool isSkippable = false; // Important: Set by ActionList, to determine whether or not the ActionList it runs should be added to the skip queue

		public List<ActionParameter> localParameters = new List<ActionParameter>();

		private RuntimeActionList runtimeActionList = null;


		public ActionRunActionList ()
		{
			this.isDisplayed = true;
			category = ActionCategory.ActionList;
			title = "Run";
			description = "Runs any ActionList (either scene-based like Cutscenes, Triggers and Interactions, or ActionList assets). If the new ActionList takes parameters, this Action can be used to set them.";
		}


		private void Upgrade ()
		{
			if (!runInParallel)
			{
				numSockets = 1;
				runInParallel = true;
				endAction = ResultAction.Stop;
			}
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			if (listSource == ListSource.InScene)
			{
				actionList = AssignFile <ActionList> (parameters, parameterID, constantID, actionList);
				jumpToAction = AssignInteger (parameters, jumpToActionParameterID, jumpToAction);
			}
		}


		override public float Run ()
		{
			if (!isRunning)
			{
				Upgrade ();

				isRunning = true;
				runtimeActionList = null;

				if (listSource == ListSource.InScene && actionList != null)
				{
					KickStarter.actionListManager.EndList (actionList);

					if (actionList.useParameters)
					{
						SendParameters (actionList.parameters, false);
					}

					if (runFromStart)
					{
						actionList.Interact (0, !isSkippable);
					}
					else
					{
						actionList.Interact (GetSkipIndex (actionList.actions), !isSkippable);
					}
				}
				else if (listSource == ListSource.AssetFile && invActionList != null)
				{
					KickStarter.actionListManager.EndAssetList (invActionList);

					if (invActionList.useParameters)
					{
						SendParameters (invActionList.parameters, true);
					}

					if (runFromStart)
					{
						runtimeActionList = AdvGame.RunActionListAsset (invActionList, 0, !isSkippable);
					}
					else
					{
						runtimeActionList = AdvGame.RunActionListAsset (invActionList, GetSkipIndex (invActionList.actions), !isSkippable);
					}
				}

				if (!runInParallel || (runInParallel && willWait))
				{
					return defaultPauseTime;
				}
			}
			else
			{
				if (listSource == ListSource.InScene && actionList != null)
				{
					if (KickStarter.actionListManager.IsListRunning (actionList))
					{
						return defaultPauseTime;
					}
					else
					{
						isRunning = false;
					}
				}
				else if (listSource == ListSource.AssetFile && invActionList != null)
				{
					if (KickStarter.actionListManager.IsListRunning (runtimeActionList))
					{
						return defaultPauseTime;
					}
					else
					{
						isRunning = false;
					}
				}
			}

			return 0f;
		}


		override public void Skip ()
		{
			if (listSource == ListSource.InScene && actionList != null)
			{
				/*if (actionList.actionListType == ActionListType.RunInBackground)
				{
					return;
				}*/

				if (actionList.useParameters)
				{
					SendParameters (actionList.parameters, false);
				}

				if (runFromStart)
				{
					actionList.Skip ();
				}
				else
				{
					actionList.Skip (GetSkipIndex (actionList.actions));
				}
			}
			else if (listSource == ListSource.AssetFile && invActionList != null)
			{
				/*if (invActionList.actionListType == ActionListType.RunInBackground)
				{
					return;
				}*/

				if (invActionList.useParameters)
				{
					SendParameters (invActionList.parameters, true);
				}

				if (runFromStart)
				{
					runtimeActionList = AdvGame.SkipActionListAsset (invActionList);
				}
				else
				{
					runtimeActionList = AdvGame.SkipActionListAsset (invActionList, GetSkipIndex (invActionList.actions));
				}
			}
		}


		private int GetSkipIndex (List<Action> _actions)
		{
			int skip = jumpToAction;
			if (jumpToActionActual && _actions.IndexOf (jumpToActionActual) > 0)
			{
				skip = _actions.IndexOf (jumpToActionActual);
			}
			return skip;
		}


		private void SendParameters (List<ActionParameter> externalParameters, bool sendingToAsset)
		{
			for (int i=0; i<externalParameters.Count; i++)
			{
				if (localParameters.Count > i)
				{
					if (externalParameters[i].parameterType == ParameterType.String)
					{
						externalParameters[i].SetValue (localParameters[i].stringValue);
					}
					else if (externalParameters[i].parameterType == ParameterType.Float)
					{
						externalParameters[i].SetValue (localParameters[i].floatValue);
					}
					else if (externalParameters[i].parameterType != ParameterType.GameObject)
					{
						externalParameters[i].SetValue (localParameters[i].intValue);
					}
					else
					{
						if (sendingToAsset)
						{
							if (isAssetFile)
							{
								externalParameters[i].SetValue (localParameters[i].intValue);
							}
							else if (localParameters[i].gameObject != null)
							{
								int idToSend = 0;
								if (localParameters[i].gameObject && localParameters[i].gameObject.GetComponent <ConstantID>())
								{
									idToSend = localParameters[i].gameObject.GetComponent <ConstantID>().constantID;
								}
								else
								{
									Debug.LogWarning (localParameters[i].gameObject.name + " requires a ConstantID script component!");
								}
								externalParameters[i].SetValue (localParameters[i].gameObject, idToSend);
							}
							else
							{
								externalParameters[i].SetValue (localParameters[i].intValue);
							}
						}
						else if (localParameters[i].gameObject != null)
						{
							externalParameters[i].SetValue (localParameters[i].gameObject);
						}
						else
						{
							externalParameters[i].SetValue (localParameters[i].intValue);
						}
					}
				}
			}
		}


		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			listSource = (ListSource) EditorGUILayout.EnumPopup ("Source:", listSource);
			if (listSource == ListSource.InScene)
			{
				parameterID = Action.ChooseParameterGUI ("ActionList:", parameters, parameterID, ParameterType.GameObject);
				if (parameterID >= 0)
				{
					localParameters.Clear ();
					constantID = 0;
					actionList = null;
				}
				else
				{
					actionList = (ActionList) EditorGUILayout.ObjectField ("ActionList:", actionList, typeof (ActionList), true);
					
					constantID = FieldToID <ActionList> (actionList, constantID);
					actionList = IDToField <ActionList> (actionList, constantID, true);
				}

				if (actionList != null && actionList.useParameters && actionList.parameters.Count > 0)
				{
					SetParametersGUI (actionList.parameters);
				}

				runFromStart = EditorGUILayout.Toggle ("Run from start?", runFromStart);

				if (!runFromStart)
				{
					jumpToActionParameterID = Action.ChooseParameterGUI ("Action # to skip to:", parameters, jumpToActionParameterID, ParameterType.Integer);
					if (jumpToActionParameterID == -1 && actionList != null && actionList.actions.Count > 1)
					{
						JumpToActionGUI (actionList.actions);
					}
				}
			}
			else if (listSource == ListSource.AssetFile)
			{
				invActionList = (ActionListAsset) EditorGUILayout.ObjectField ("ActionList asset:", invActionList, typeof (ActionListAsset), true);

				if (invActionList != null && invActionList.useParameters && invActionList.parameters.Count > 0)
				{
					SetParametersGUI (invActionList.parameters);
				}

				runFromStart = EditorGUILayout.Toggle ("Run from start?", runFromStart);
				
				if (!runFromStart && invActionList != null && invActionList.actions.Count > 1)
				{
					JumpToActionGUI (invActionList.actions);
				}
			}

			if (!runInParallel)
			{
				Upgrade ();
			}

			willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);
			AfterRunningOption ();
		}


		private void JumpToActionGUI (List<Action> actions)
		{
			int tempSkipAction = jumpToAction;
			List<string> labelList = new List<string>();
			
			if (jumpToActionActual)
			{
				bool found = false;
				
				for (int i = 0; i < actions.Count; i++)
				{
					labelList.Add (i.ToString () + ": " + actions [i].title);
					
					if (jumpToActionActual == actions [i])
					{
						jumpToAction = i;
						found = true;
					}
				}

				if (!found)
				{
					jumpToAction = tempSkipAction;
				}
			}
			
			if (jumpToAction < 0)
			{
				jumpToAction = 0;
			}
			
			if (jumpToAction >= actions.Count)
			{
				jumpToAction = actions.Count - 1;
			}
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField ("  Action to skip to:");
			tempSkipAction = EditorGUILayout.Popup (jumpToAction, labelList.ToArray());
			jumpToAction = tempSkipAction;
			EditorGUILayout.EndHorizontal();
			jumpToActionActual = actions [jumpToAction];
		}


		private int ShowVarSelectorGUI (string label, List<GVar> vars, int ID)
		{
			int variableNumber = -1;
			
			List<string> labelList = new List<string>();
			labelList.Add (" (None)");
			foreach (GVar _var in vars)
			{
				labelList.Add (_var.label);
			}
			
			variableNumber = GetVarNumber (vars, ID) + 1;
			variableNumber = EditorGUILayout.Popup (label, variableNumber, labelList.ToArray()) - 1;

			if (variableNumber >= 0)
			{
				return vars[variableNumber].id;
			}

			return -1;
		}


		private int ShowInvItemSelectorGUI (string label, List<InvItem> items, int ID)
		{
			int invNumber = -1;
			
			List<string> labelList = new List<string>();
			labelList.Add (" (None)");
			foreach (InvItem _item in items)
			{
				labelList.Add (_item.label);
			}
			
			invNumber = GetInvNumber (items, ID) + 1;
			invNumber = EditorGUILayout.Popup (label, invNumber, labelList.ToArray()) - 1;

			if (invNumber >= 0)
			{
				return items[invNumber].id;
			}
			return -1;
		}


		private int GetVarNumber (List<GVar> vars, int ID)
		{
			int i = 0;
			foreach (GVar _var in vars)
			{
				if (_var.id == ID)
				{
					return i;
				}
				i++;
			}
			return -1;
		}


		private int GetInvNumber (List<InvItem> items, int ID)
		{
			int i = 0;
			foreach (InvItem _item in items)
			{
				if (_item.id == ID)
				{
					return i;
				}
				i++;
			}
			return -1;
		}


		private void SetParametersGUI (List<ActionParameter> externalParameters)
		{
			// Ensure target and local parameter lists match
			
			int numParameters = externalParameters.Count;
			if (numParameters < localParameters.Count)
			{
				localParameters.RemoveRange (numParameters, localParameters.Count - numParameters);
			}
			else if (numParameters > localParameters.Count)
			{
				if (numParameters > localParameters.Capacity)
				{
					localParameters.Capacity = numParameters;
				}
				for (int i=localParameters.Count; i<numParameters; i++)
				{
					ActionParameter newParameter = new ActionParameter (externalParameters [i].ID);
					localParameters.Add (newParameter);
				}
			}

			EditorGUILayout.BeginVertical ("Button");
			for (int i=0; i<externalParameters.Count; i++)
			{
				string label = externalParameters[i].label;
				
				if (externalParameters[i].parameterType == ParameterType.GameObject)
				{
					if (isAssetFile)
					{
						// ID
						localParameters[i].intValue = EditorGUILayout.IntField (label + " (ID):", localParameters[i].intValue);
						localParameters[i].gameObject = null;
					}
					else
					{
						/// Gameobject
						localParameters[i].gameObject = (GameObject) EditorGUILayout.ObjectField (label + ":", localParameters[i].gameObject, typeof (GameObject), true);
						localParameters[i].intValue = 0;
						if (localParameters[i].gameObject != null && localParameters[i].gameObject.GetComponent <ConstantID>() == null)
						{
							localParameters[i].gameObject.AddComponent <ConstantID>();
						}
					}
				}
				else if (externalParameters[i].parameterType == ParameterType.GlobalVariable)
				{
					if (AdvGame.GetReferences () && AdvGame.GetReferences ().variablesManager)
					{
						VariablesManager variablesManager = AdvGame.GetReferences ().variablesManager;
						localParameters[i].intValue = ShowVarSelectorGUI (label + ":", variablesManager.vars, localParameters[i].intValue);
					}
					else
					{
						EditorGUILayout.HelpBox ("A Variables Manager is required to pass Global Variables.", MessageType.Warning);
					}
				}
				else if (externalParameters[i].parameterType == ParameterType.InventoryItem)
				{
					if (AdvGame.GetReferences () && AdvGame.GetReferences ().inventoryManager)
					{
						InventoryManager inventoryManager = AdvGame.GetReferences ().inventoryManager;
						localParameters[i].intValue = ShowInvItemSelectorGUI (label + ":", inventoryManager.items, localParameters[i].intValue);
					}
					else
					{
						EditorGUILayout.HelpBox ("An Inventory Manager is required to pass Inventory items.", MessageType.Warning);
					}
				}
				else if (externalParameters[i].parameterType == ParameterType.LocalVariable)
				{
					if (KickStarter.localVariables)
					{
						localParameters[i].intValue = ShowVarSelectorGUI (label + ":", KickStarter.localVariables.localVars, localParameters[i].intValue);
					}
					else
					{
						EditorGUILayout.HelpBox ("A GameEngine prefab is required to pass Local Variables.", MessageType.Warning);
					}
				}
				else if (externalParameters[i].parameterType == ParameterType.String)
				{
					localParameters[i].stringValue = EditorGUILayout.TextField (label + ":", localParameters[i].stringValue);
				}
				else if (externalParameters[i].parameterType == ParameterType.Float)
				{
					localParameters[i].floatValue = EditorGUILayout.FloatField (label + ":", localParameters[i].floatValue);
				}
				else if (externalParameters[i].parameterType == ParameterType.Integer)
				{
					localParameters[i].intValue = EditorGUILayout.IntField (label + ":", localParameters[i].intValue);
				}
				else if (externalParameters[i].parameterType == ParameterType.Boolean)
				{
					BoolValue boolValue = BoolValue.False;
					if (localParameters[i].intValue == 1)
					{
						boolValue = BoolValue.True;
					}

					boolValue = (BoolValue) EditorGUILayout.EnumPopup (label + ":", boolValue);

					if (boolValue == BoolValue.True)
					{
						localParameters[i].intValue = 1;
					}
					else
					{
						localParameters[i].intValue = 0;
					}
				}
			}
			EditorGUILayout.EndVertical ();
		}


		public override string SetLabel ()
		{
			string labelAdd = "";
			
			if (listSource == ListSource.InScene && actionList != null)
			{
				labelAdd += " (" + actionList.name + ")";
			}
			else if (listSource == ListSource.AssetFile && invActionList != null)
			{
				labelAdd += " (" + invActionList.name + ")";
			}
			
			return labelAdd;
		}
		
		#endif
		
	}

}