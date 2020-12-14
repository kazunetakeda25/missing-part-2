/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionVarSequence.cs"
 * 
 *	This action runs an Integer Variable through a sequence
 *	and performs different follow-up Actions accordingly.
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
	public class ActionVarSequence : ActionCheckMultiple
	{
		
		public int parameterID = -1;
		public int variableID;
		public int variableNumber;
		public bool doLoop = false;
		
		public VariableLocation location = VariableLocation.Global;

		
		public ActionVarSequence ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Variable;
			title = "Run sequence";
			description = "Uses the value of an integer Variable to determine which Action is run next. The value is incremented by one each time (and reset to zero when a limit is reached), allowing for different subsequent Actions to play each time the Action is run.";
		}
		
		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			variableID = AssignVariableID (parameters, parameterID, variableID);
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
			
			GVar var = null;
			
			if (location == VariableLocation.Local && !isAssetFile)
			{
				var = LocalVariables.GetVariable (variableID);
			}
			else
			{
				var = RuntimeVariables.GetVariable (variableID);
			}
			
			if (var != null)
			{
				if (var.type == VariableType.Integer)
				{
					var.Download ();
					if (var.val < 1)
					{
						var.val = 1;
					}
					int originalValue = var.val-1;
					var.val ++;
					if (var.val > numSockets)
					{
						if (doLoop)
						{
							var.val = 1;
						}
						else
						{
							var.val = numSockets;
						}
					}
					var.Upload ();
					return ProcessResult (originalValue, actions);
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
					parameterID = Action.ChooseParameterGUI ("Integer variable:", parameters, parameterID, ParameterType.GlobalVariable);
					if (parameterID >= 0)
					{
						variableID = ShowVarGUI (AdvGame.GetReferences ().variablesManager.vars, variableID, false);
					}
					else
					{
						variableID = ShowVarGUI (AdvGame.GetReferences ().variablesManager.vars, variableID, true);
					}
				}
			}
			
			else if (location == VariableLocation.Local)
			{
				if (KickStarter.localVariables)
				{
					parameterID = Action.ChooseParameterGUI ("Integer variable:", parameters, parameterID, ParameterType.LocalVariable);
					if (parameterID >= 0)
					{
						variableID = ShowVarGUI (KickStarter.localVariables.localVars, variableID, false);
					}
					else
					{
						variableID = ShowVarGUI (KickStarter.localVariables.localVars, variableID, true);
					}
				}
			}
			
			numSockets = EditorGUILayout.IntSlider ("# of possible values:", numSockets, 1, 10);
			doLoop = EditorGUILayout.Toggle ("Run on a loop?", doLoop);
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
					if (vars[variableNumber].type != VariableType.Integer)
					{
						EditorGUILayout.HelpBox ("The selected Variable must be an Integer!", MessageType.Warning);
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
		
		#endif
		
	}
	
}