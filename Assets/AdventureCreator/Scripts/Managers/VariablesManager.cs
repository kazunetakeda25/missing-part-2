/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"VariablesManager.cs"
 * 
 *	This script handles the "Variables" tab of the main wizard.
 *	Boolean and integer, which can be used regardless of scene, are defined here.
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
	public class VariablesManager : ScriptableObject
	{

		public List<GVar> vars = new List<GVar>();
		public bool updateRuntime = false;
		
		#if UNITY_EDITOR

		private GVar selectedVar;
		private Texture2D icon;
		private int sideVar = -1;
		private VariableLocation sideVarLocation = VariableLocation.Global;
		private string[] boolType = {"False", "True"};
		private string filter = "";

		private Vector2 scrollPos;
		private bool showGlobal = true;
		private bool showLocal = false;


		public void ShowGUI ()
		{
			if (icon == null)
			{
				icon = (Texture2D) AssetDatabase.LoadAssetAtPath ("Assets/AdventureCreator/Graphics/Textures/inspector-use.png", typeof (Texture2D));
			}

			EditorGUILayout.Space ();
			GUILayout.BeginHorizontal ();
			if (GUILayout.Toggle (showGlobal, "Global", "toolbarbutton"))
			{
				SetTab (0);
			}
			if (GUILayout.Toggle (showLocal, "Local", "toolbarbutton"))
			{
				SetTab (1);
			}
			GUILayout.EndHorizontal ();
			EditorGUILayout.Space ();

			updateRuntime = EditorGUILayout.Toggle ("Show realtime values?", updateRuntime);
			filter = EditorGUILayout.TextField ("Filter by name:", filter);
			EditorGUILayout.Space ();

			if (showGlobal)
			{
				if (Application.isPlaying && updateRuntime && KickStarter.runtimeVariables != null)
				{
					ShowVarList (KickStarter.runtimeVariables.globalVars, VariableLocation.Global, false);
				}
				else
				{
					ShowVarList (vars, VariableLocation.Global, true);
				}
			}
			else if (showLocal)
			{
				if (KickStarter.localVariables != null)
				{
					if (Application.isPlaying && updateRuntime)
					{
						ShowVarList (KickStarter.localVariables.localVars, VariableLocation.Local, false);
					}
					else
					{
						ShowVarList (KickStarter.localVariables.localVars, VariableLocation.Local, true);
					}
				}
				else
				{
					EditorGUILayout.LabelField ("Local variables", EditorStyles.boldLabel);
					EditorGUILayout.HelpBox ("A GameEngine prefab must be present in the scene before Local variables can be defined", MessageType.Info);
				}
			}

			EditorGUILayout.Space ();
			if (selectedVar != null && (!Application.isPlaying || !updateRuntime))
			{
				if (vars.Contains (selectedVar))
				{
					ShowVarGUI (VariableLocation.Global);
				}
				else if (KickStarter.localVariables != null && KickStarter.localVariables.localVars.Contains (selectedVar))
				{
					ShowVarGUI (VariableLocation.Local);
				}
			}

			if (GUI.changed)
			{
				EditorUtility.SetDirty (this);

				if (KickStarter.localVariables != null)
				{
					EditorUtility.SetDirty (KickStarter.localVariables);
				}
			}
		}


		private void ResetFilter ()
		{
			filter = "";
		}


		private void SideMenu (GVar _var, List<GVar> _vars, VariableLocation location)
		{
			GenericMenu menu = new GenericMenu ();
			sideVar = _vars.IndexOf (_var);
			sideVarLocation = location;

			menu.AddItem (new GUIContent ("Insert after"), false, Callback, "Insert after");
			if (_vars.Count > 0)
			{
				menu.AddItem (new GUIContent ("Delete"), false, Callback, "Delete");
			}
			if (sideVar > 0 || sideVar < _vars.Count-1)
			{
				menu.AddSeparator ("");
			}
			if (sideVar > 0)
			{
				menu.AddItem (new GUIContent ("Move up"), false, Callback, "Move up");
			}
			if (sideVar < _vars.Count-1)
			{
				menu.AddItem (new GUIContent ("Move down"), false, Callback, "Move down");
			}
			
			menu.ShowAsContext ();
		}
		
		
		private void Callback (object obj)
		{
			if (sideVar >= 0)
			{
				ResetFilter ();
				List<GVar> _vars = new List<GVar>();

				if (sideVarLocation == VariableLocation.Global)
				{
					_vars = vars;
				}
				else
				{
					_vars = KickStarter.localVariables.localVars;
				}
				GVar tempVar = _vars[sideVar];

				switch (obj.ToString ())
				{
				case "Insert after":
					Undo.RecordObject (this, "Insert variable");
					_vars.Insert (sideVar+1, new GVar (GetIDArray (_vars)));
					DeactivateAllVars ();
					break;
					
				case "Delete":
					Undo.RecordObject (this, "Delete variable");
					_vars.RemoveAt (sideVar);
					DeactivateAllVars ();
					break;

				case "Move up":
					Undo.RecordObject (this, "Move variable up");
					_vars.RemoveAt (sideVar);
					_vars.Insert (sideVar-1, tempVar);
					break;

				case "Move down":
					Undo.RecordObject (this, "Move variable down");
					_vars.RemoveAt (sideVar);
					_vars.Insert (sideVar+1, tempVar);
					break;
				}
			}

			sideVar = -1;

			if (sideVarLocation == AC.VariableLocation.Global)
			{
				EditorUtility.SetDirty (this);
				AssetDatabase.SaveAssets ();
			}
			else
			{
				if (KickStarter.localVariables)
				{
					EditorUtility.SetDirty (KickStarter.localVariables);
				}
			}
		}


		private void ActivateVar (GVar var)
		{
			var.isEditing = true;
			selectedVar = var;
		}
		
		
		private void DeactivateAllVars ()
		{
			if (KickStarter.localVariables)
			{
				foreach (GVar var in KickStarter.localVariables.localVars)
				{
					var.isEditing = false;
				}
			}

			foreach (GVar var in vars)
			{
				var.isEditing = false;
			}
			selectedVar = null;
		}


		private int[] GetIDArray (List<GVar> _vars)
		{
			// Returns a list of id's in the list
			
			List<int> idArray = new List<int>();
			
			foreach (GVar variable in _vars)
			{
				idArray.Add (variable.id);
			}
			
			idArray.Sort ();
			return idArray.ToArray ();
		}


		private void ShowVarList (List<GVar> _vars, VariableLocation location, bool allowEditing)
		{
			EditorGUILayout.LabelField (location + " variables", EditorStyles.boldLabel);

			scrollPos = EditorGUILayout.BeginScrollView (scrollPos, GUILayout.Height (Mathf.Min (_vars.Count * 21, 235f)+5));
			foreach (GVar _var in _vars)
			{
				if (filter == "" || _var.label.ToLower ().Contains (filter.ToLower ()))
				{
					EditorGUILayout.BeginHorizontal ();
					
					string buttonLabel = _var.id + ": ";
					if (buttonLabel == "")
					{
						_var.label += "(Untitled)";	
					}
					else
					{
						buttonLabel += _var.label;

						if (buttonLabel.Length > 30)
						{
							buttonLabel = buttonLabel.Substring (0, 30);
						}
					}

					string varValue = _var.GetValue ();
					if (varValue.Length > 20)
					{
						varValue = varValue.Substring (0, 20);
					}

					buttonLabel += " (" + _var.type.ToString () + " - " + varValue + ")";

					if (allowEditing)
					{
						if (GUILayout.Toggle (_var.isEditing, buttonLabel, "Button"))
						{
							if (selectedVar != _var)
							{
								DeactivateAllVars ();
								ActivateVar (_var);
							}
						}
						
						if (GUILayout.Button (icon, GUILayout.Width (20f), GUILayout.Height (15f)))
						{
							SideMenu (_var, _vars, location);
						}
					}
					else
					{
						GUILayout.Label (buttonLabel, "Button");
					}
					
					EditorGUILayout.EndHorizontal ();
				}
			}
			EditorGUILayout.EndScrollView ();

			if (allowEditing)
			{
				EditorGUILayout.Space ();
				if (GUILayout.Button("Create new " + location + " variable"))
				{
					ResetFilter ();
					Undo.RecordObject (this, "Add " + location + " variable");
					_vars.Add (new GVar (GetIDArray (_vars)));
					DeactivateAllVars ();
					ActivateVar (_vars [_vars.Count-1]);
				}
			}
		}


		private void ShowVarGUI (VariableLocation location)
		{
			EditorGUILayout.LabelField (location + " variable '" + selectedVar.label + "' properties", EditorStyles.boldLabel);
			EditorGUILayout.BeginVertical("Button");
			
			selectedVar.label = EditorGUILayout.TextField ("Label:", selectedVar.label);
			selectedVar.type = (VariableType) EditorGUILayout.EnumPopup ("Type:", selectedVar.type);

			if (location == VariableLocation.Local)
			{
				EditorGUILayout.LabelField ("Replacement token:", "[localvar:" + selectedVar.id.ToString () + "]");
			}
			else
			{
				EditorGUILayout.LabelField ("Replacement token:", "[var:" + selectedVar.id.ToString () + "]");
			}
			
			if (selectedVar.type == VariableType.Boolean)
			{
				if (selectedVar.val != 1)
				{
					selectedVar.val = 0;
				}
				selectedVar.val = EditorGUILayout.Popup ("Initial value:", selectedVar.val, boolType);
			}
			else if (selectedVar.type == VariableType.Integer)
			{
				selectedVar.val = EditorGUILayout.IntField ("Initial value:", selectedVar.val);
			}
			else if (selectedVar.type == VariableType.PopUp)
			{
				selectedVar.popUps = PopupsGUI (selectedVar.popUps);
				selectedVar.val = EditorGUILayout.Popup ("Initial value:", selectedVar.val, selectedVar.popUps);
			}
			else if (selectedVar.type == VariableType.String)
			{
				selectedVar.textVal = EditorGUILayout.TextField ("Initial value:", selectedVar.textVal);
			}
			else if (selectedVar.type == VariableType.Float)
			{
				selectedVar.floatVal = EditorGUILayout.FloatField ("Initial value:", selectedVar.floatVal);
			}

			if (location == VariableLocation.Local)
			{
				selectedVar.link = VarLink.None;
			}
			else
			{
				EditorGUILayout.Space ();
				selectedVar.link = (VarLink) EditorGUILayout.EnumPopup ("Link to:", selectedVar.link);
				if (selectedVar.link == VarLink.PlaymakerGlobalVariable)
				{
					if (PlayMakerIntegration.IsDefinePresent ())
					{
						selectedVar.pmVar = EditorGUILayout.TextField ("Playmaker Global Variable:", selectedVar.pmVar);
						selectedVar.updateLinkOnStart = EditorGUILayout.Toggle ("Use PM for initial value?", selectedVar.updateLinkOnStart);
					}
					else
					{
						EditorGUILayout.HelpBox ("The 'PlayMakerIsPresent' Scripting Define Symbol must be listed in the\nPlayer Settings. Please set it from Edit -> Project Settings -> Player", MessageType.Warning);
					}
				}
				else if (selectedVar.link == VarLink.OptionsData)
				{
					EditorGUILayout.HelpBox ("This Variable will be stored in PlayerPrefs, and not in saved game files.", MessageType.Info);
				}
			}
			EditorGUILayout.EndVertical ();
		}


		private string[] PopupsGUI (string[] popUps)
		{
			List<string> popUpList = new List<string>();
			if (popUps != null && popUps.Length > 0)
			{
				foreach (string p in popUps)
				{
					popUpList.Add (p);
				}
			}

			int numValues = popUpList.Count;
			numValues = EditorGUILayout.IntField ("Number of values:", numValues);
			if (numValues < 0)
			{
				numValues = 0;
			}
			
			if (numValues < popUpList.Count)
			{
				popUpList.RemoveRange (numValues, popUpList.Count - numValues);
			}
			else if (numValues > popUpList.Count)
			{
				if (numValues > popUpList.Capacity)
				{
					popUpList.Capacity = numValues;
				}
				for (int i=popUpList.Count; i<numValues; i++)
				{
					popUpList.Add ("");
				}
			}
			
			for (int i=0; i<popUpList.Count; i++)
			{
				popUpList[i] = EditorGUILayout.TextField (i.ToString ()+":", popUpList[i]);
			}

			return popUpList.ToArray ();
		}


		private void SetTab (int tab)
		{
			showGlobal = false;
			showLocal = false;
			selectedVar = null;

			if (tab == 0)
			{
				showGlobal = true;
			}
			else if (tab == 1)
			{
				showLocal = true;
			}
		}


		#endif

		
		public GVar GetVariable (int _id)
		{
			foreach (GVar _var in vars)
			{
				if (_var.id == _id)
				{
					return _var;
				}
			}
			return null;
		}

	}

}