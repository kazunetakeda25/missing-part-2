/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionVarSequence.cs"
 * 
 *	This action reads a Popup Variable and performs
 *	different follow-up Actions based on it's value.
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
	public class ActionVarPopup : ActionCheckMultiple
	{
		
		public int variableID;
		public int variableNumber;

		public VariableLocation location = VariableLocation.Global;

		
		public ActionVarPopup ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Variable;
			title = "Pop Up switch";
			description = "Uses the value of a Pop Up Variable to determine which Action is run next. An option for each possible value the Variable can take will be displayed, allowing for different subsequent Actions to run.";
		}
		
		
		override public ActionEnd End (List<Action> actions)
		{
			if (numSockets <= 0)
			{
				Debug.LogWarning ("Could not compute Random check because no values were possible!");
				return GenerateStopActionEnd ();
			}
			
			if (variableID == -1)
			{
				return GenerateStopActionEnd ();
			}
			
			GVar var = GetVariable ();

			if (var != null)
			{
				if (var.type == VariableType.PopUp)
				{
					var.Download ();
					return ProcessResult (var.val, actions);
				}
				else
				{
					Debug.LogWarning ("Variable: Run sequence Action is referencing a Variable that does not exist!");
				}
			}
			
			return GenerateStopActionEnd ();
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			if (isAssetFile)
			{
				location = VariableLocation.Global;
			}
			else
			{
				location = (VariableLocation) EditorGUILayout.EnumPopup ("Source:", location);
			}
			
			if (location == VariableLocation.Global)
			{
				if (AdvGame.GetReferences ().variablesManager)
				{
					variableID = ShowVarGUI (AdvGame.GetReferences ().variablesManager.vars, variableID, true);
				}
			}
			
			else if (location == VariableLocation.Local)
			{
				if (KickStarter.localVariables)
				{
					variableID = ShowVarGUI (KickStarter.localVariables.localVars, variableID, true);
				}
			}

			GVar _var = GetVariable ();
			if (_var != null)
			{
				numSockets = _var.popUps.Length;
			}
		}


		private int ShowVarSelectorGUI (List<GVar> vars, int ID)
		{
			variableNumber = -1;
			
			List<string> labelList = new List<string>();
			foreach (GVar _var in vars)
			{
				labelList.Add (_var.label);
			}
			
			variableNumber = GetVarNumber (vars, ID);
			
			if (variableNumber == -1)
			{
				// Wasn't found (variable was deleted?), so revert to zero
				Debug.LogWarning ("Previously chosen variable no longer exists!");
				variableNumber = 0;
				ID = 0;
			}
			
			variableNumber = EditorGUILayout.Popup ("Variable:", variableNumber, labelList.ToArray());
			ID = vars[variableNumber].id;
			
			return ID;
		}
		
		
		private int ShowVarGUI (List<GVar> vars, int ID, bool changeID)
		{
			if (vars.Count > 0)
			{
				if (changeID)
				{
					ID = ShowVarSelectorGUI (vars, ID);
				}
				variableNumber = Mathf.Min (variableNumber, vars.Count-1);
				if (changeID)
				{
					if (vars[variableNumber].type != VariableType.PopUp)
					{
						EditorGUILayout.HelpBox ("The selected Variable must be a Pop Up!", MessageType.Warning);
					}
				}
			}
			else
			{
				EditorGUILayout.HelpBox ("No variables exist!", MessageType.Info);
				ID = -1;
				variableNumber = -1;
			}
			
			return ID;
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
		
		
		override public string SetLabel ()
		{
			if (location == VariableLocation.Local && !isAssetFile)
			{
				if (KickStarter.localVariables)
				{
					return GetLabelString (KickStarter.localVariables.localVars);
				}
			}
			else
			{
				if (AdvGame.GetReferences ().variablesManager)
				{
					return GetLabelString (AdvGame.GetReferences ().variablesManager.vars);
				}
			}
			
			return "";
		}
		
		
		private string GetLabelString (List<GVar> vars)
		{
			string labelAdd = "";
			
			if (vars.Count > 0 && vars.Count > variableNumber && variableNumber > -1)
			{
				labelAdd = " (" + vars[variableNumber].label + ")";
			}
			
			return labelAdd;
		}


		override public void SkipActionGUI (List<Action> actions, bool showGUI)
		{
			if (numSockets < 0)
			{
				numSockets = 0;
			}
		
			if (numSockets < endings.Count)
			{
				endings.RemoveRange (numSockets, endings.Count - numSockets);
			}
			else if (numSockets > endings.Count)
			{
				if (numSockets > endings.Capacity)
				{
					endings.Capacity = numSockets;
				}
				for (int i=endings.Count; i<numSockets; i++)
				{
					ActionEnd newEnd = new ActionEnd ();
					endings.Add (newEnd);
				}
			}
			
			foreach (ActionEnd ending in endings)
			{
				if (showGUI)
				{
					EditorGUILayout.Space ();
					int i = endings.IndexOf (ending);

					GVar _var = GetVariable ();
					if (_var != null)
					{
						ending.resultAction = (ResultAction) EditorGUILayout.EnumPopup ("If result is " + _var.popUps[i] + ":", (ResultAction) ending.resultAction);
					}
					else
					{
						ending.resultAction = (ResultAction) EditorGUILayout.EnumPopup ("If result is " + (i+1).ToString () + ":", (ResultAction) ending.resultAction);
					}
				}
				
				if (ending.resultAction == ResultAction.RunCutscene && showGUI)
				{
					if (isAssetFile)
					{
						ending.linkedAsset = (ActionListAsset) EditorGUILayout.ObjectField ("ActionList to run:", ending.linkedAsset, typeof (ActionListAsset), false);
					}
					else
					{
						ending.linkedCutscene = (Cutscene) EditorGUILayout.ObjectField ("Cutscene to run:", ending.linkedCutscene, typeof (Cutscene), true);
					}
				}
				else if (ending.resultAction == ResultAction.Skip)
				{
					SkipActionGUI (ending, actions, showGUI);
				}
				else
				{
					EditorGUILayout.Space ();
					EditorGUILayout.Space ();
					EditorGUILayout.Space ();
				}
			}
		}
		
		#endif


		private GVar GetVariable ()
		{
			GVar var = null;
			
			if (location == VariableLocation.Local && !isAssetFile)
			{
				var = LocalVariables.GetVariable (variableID);
			}
			else
			{
				if (Application.isPlaying)
				{
					var = RuntimeVariables.GetVariable (variableID);
				}
				else if (AdvGame.GetReferences ().variablesManager)
				{
					var = AdvGame.GetReferences ().variablesManager.GetVariable (variableID);
				}
			}
			
			if (var != null && var.type == VariableType.PopUp)
			{
				return var;
			}
			
			return null;
		}

	}
	
}