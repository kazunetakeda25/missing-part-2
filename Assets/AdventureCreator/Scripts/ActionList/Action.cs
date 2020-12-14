/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"Action.cs"
 * 
 *	This is the base class from which all Actions derive.
 *	We need blank functions Run, ShowGUI and SetLabel,
 *	which will be over-ridden by the subclasses.
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
	abstract public class Action : ScriptableObject
	{

		public int numSockets = 1;
		public bool willWait;
		public float defaultPauseTime = 0.1f;
		
		public bool isRunning;
		public ActionEnd lastResult = new ActionEnd (-10);
		public int id;
		
		public bool isDisplayed;
		public string title;
		public ActionCategory category = ActionCategory.Custom;
		public string description;
		
		public ResultAction endAction = ResultAction.Continue;
		public int skipAction = -1;
		public AC.Action skipActionActual;
		public Cutscene linkedCutscene;
		public ActionListAsset linkedAsset;

		public bool isMarked = false;
		public bool isEnabled = true;
		public bool isAssetFile = false;

		public Rect nodeRect = new Rect (0,0,300,60);


		public Action ()
		{
			this.isDisplayed = true;
		}
		
		
		public virtual float Run ()
		{
			return defaultPauseTime;
		}


		public virtual void Skip ()
		{
			Run ();
		}
		
		
		public virtual ActionEnd End (List<Action> actions)
		{
			return GenerateActionEnd (endAction, linkedAsset, linkedCutscene, skipAction, skipActionActual, actions);
		}
		
		
		#if UNITY_EDITOR


		public virtual void ShowGUI (List<ActionParameter> parameters)
		{
			ShowGUI ();
		}


		public virtual void ShowGUI ()
		{ }


		protected void AfterRunningOption ()
		{
			EditorGUILayout.Space ();
			endAction = (ResultAction) EditorGUILayout.EnumPopup ("After running:", (ResultAction) endAction);
			
			if (endAction == ResultAction.RunCutscene)
			{
				if (isAssetFile)
				{
					linkedAsset = ActionListAssetMenu.AssetGUI ("ActionList to run:", linkedAsset);
				}
				else
				{
					linkedCutscene = ActionListAssetMenu.CutsceneGUI ("Cutscene to run:", linkedCutscene);
				}
			}
		}
		
		
		public virtual void SkipActionGUI (List<Action> actions, bool showGUI)
		{
			if (skipAction == -1)
			{
				// Set default
				int i = actions.IndexOf (this);
				if (actions.Count > i+1)
				{
					skipAction = i+1;
				}
				else
				{
					skipAction = i;
				}
			}

			int tempSkipAction = skipAction;
			List<string> labelList = new List<string>();

			if (skipActionActual)
			{
				bool found = false;

				for (int i = 0; i < actions.Count; i++)
				{
					labelList.Add (i.ToString () + ": " + actions [i].title);

					if (skipActionActual == actions [i])
					{
						skipAction = i;
						found = true;
					}
				}
				
				if (!found)
				{
					skipAction = tempSkipAction;
				}
			}

			if (skipAction >= actions.Count)
			{
				skipAction = actions.Count - 1;
			}

			if (showGUI)
			{
				if (actions.Count > 1)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField ("  Action to skip to:");
					tempSkipAction = EditorGUILayout.Popup (skipAction, labelList.ToArray());
					EditorGUILayout.EndHorizontal();
					skipAction = tempSkipAction;
				}
				else
				{
					EditorGUILayout.HelpBox ("Cannot skip action - no further Actions available", MessageType.Warning);
					return;
				}
			}

			skipActionActual = actions [skipAction];
		}


		public virtual string SetLabel ()
		{
			return ("");
		}


		public virtual void DrawOutWires (List<Action> actions, int i, int offset)
		{
			if (endAction == ResultAction.Continue)
			{
				if (actions.Count > i+1)
				{
					AdvGame.DrawNodeCurve (nodeRect, actions[i+1].nodeRect, new Color (0.3f, 0.3f, 1f, 1f), 10, false, isDisplayed);
				}
			}
			else if (endAction == ResultAction.Skip)
			{
				if (actions.Contains (skipActionActual))
				{
					AdvGame.DrawNodeCurve (nodeRect, skipActionActual.nodeRect, new Color (0.3f, 0.3f, 1f, 1f), 10, false, isDisplayed);
				}
			}
		}


		public static int ChooseParameterGUI (string label, List<ActionParameter> _parameters, int _parameterID, ParameterType _expectedType)
		{
			if (_parameters == null || _parameters.Count == 0)
			{
				return -1;
			}

			// Don't show list if no parameters of the correct type are present
			bool found = false;
			foreach (ActionParameter _parameter in _parameters)
			{
				if (_parameter.parameterType == _expectedType)
				{
					found = true;
				}
			}
			if (!found)
			{
				return -1;
			}
			
			int chosenNumber = 0;
			List<string> labelList = new List<string>();
			labelList.Add ("(No parameter)");
			foreach (ActionParameter _parameter in _parameters)
			{
				labelList.Add ("(" + _parameter.ID + ") " + _parameter.label);
				if (_parameter.ID == _parameterID)
				{
					chosenNumber = _parameters.IndexOf (_parameter)+1;
					
					if (_parameter.parameterType != _expectedType)
					{
						EditorGUILayout.HelpBox ("This parameter type is invalid: expecting " + _expectedType, MessageType.Warning);
					}
				}
			}
			
			chosenNumber = EditorGUILayout.Popup ("-> " + label, chosenNumber, labelList.ToArray()) - 1;

			if (chosenNumber < 0)
			{
				return -1;
			}
			
			return _parameters [chosenNumber].ID;
		}


		public int FieldToID <T> (T field, int _constantID) where T : Behaviour
		{
			if (field != null)
			{
				if (isAssetFile || (!isAssetFile && !field.gameObject.activeInHierarchy))
				{
					if (field.GetComponent <ConstantID>())
					{
						if (!field.gameObject.activeInHierarchy && field.GetComponent <ConstantID>().constantID == 0)
						{
							field.GetComponent <ConstantID>().AssignInitialValue (false);
						}
						_constantID = field.GetComponent <ConstantID>().constantID;
					}
					else if (field.GetComponent <Player>() == null)
					{
						field.gameObject.AddComponent <ConstantID>();
						_constantID = field.GetComponent <ConstantID>().AssignInitialValue (false);
						AssetDatabase.SaveAssets ();
					}
					return _constantID;
				}
				if (!Application.isPlaying)
				{
					return 0;
				}
			}
			return _constantID;
		}
		
		
		public T IDToField <T> (T field, int _constantID, bool moreInfo) where T : Behaviour
		{
			if (isAssetFile || (!isAssetFile && (field == null || !field.gameObject.activeInHierarchy)))
			{
				T newField = field;
				if (_constantID != 0)
				{
					newField = Serializer.returnComponent <T> (_constantID);
					if (newField != null && !Application.isPlaying)
					{
						field = newField;
					}

					EditorGUILayout.BeginVertical ("Button");
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Recorded ConstantID: " + _constantID.ToString (), EditorStyles.miniLabel);
					if (field == null)
					{
						if (GUILayout.Button ("Search scenes", EditorStyles.miniButton))
						{
							AdvGame.FindObjectWithConstantID (_constantID);
						}
					}
					EditorGUILayout.EndHorizontal ();
					
					if (field == null && moreInfo)
					{
						EditorGUILayout.HelpBox ("Further controls cannot display because the referenced object cannot be found.", MessageType.Warning);
					}
					EditorGUILayout.EndVertical ();
				}
			}
			return field;
		}
		
		
		public int FieldToID (Transform field, int _constantID)
		{
			if (field != null)
			{
				if (isAssetFile || (!isAssetFile && !field.gameObject.activeInHierarchy))
				{
					if (field.GetComponent <ConstantID>())
					{
						if (!field.gameObject.activeInHierarchy && field.GetComponent <ConstantID>().constantID == 0)
						{
							field.GetComponent <ConstantID>().AssignInitialValue (false);
						}
						_constantID = field.GetComponent <ConstantID>().constantID;
					}
					else if (field.GetComponent <Player>() == null)
					{
						field.gameObject.AddComponent <ConstantID>();
						_constantID = field.GetComponent <ConstantID>().AssignInitialValue (false);
						AssetDatabase.SaveAssets ();
					}
					return _constantID;
				}
				if (!Application.isPlaying)
				{
					return 0;
				}
			}
			return _constantID;
		}
		
		
		public Transform IDToField (Transform field, int _constantID, bool moreInfo)
		{
			if (isAssetFile || (!isAssetFile && (field == null || !field.gameObject.activeInHierarchy)))
			{
				if (_constantID != 0)
				{
					ConstantID newID = Serializer.returnComponent <ConstantID> (_constantID);
					if (newID != null && !Application.isPlaying)
					{
						field = newID.transform;
					}

					EditorGUILayout.BeginVertical ("Button");
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Recorded ConstantID: " + _constantID.ToString (), EditorStyles.miniLabel);
					if (field == null)
					{
						if (GUILayout.Button ("Search scenes", EditorStyles.miniButton))
						{
							AdvGame.FindObjectWithConstantID (_constantID);
						}
					}
					EditorGUILayout.EndHorizontal ();
					
					if (field == null && moreInfo)
					{
						EditorGUILayout.HelpBox ("Further controls cannot display because the referenced object cannot be found.", MessageType.Warning);
					}
					EditorGUILayout.EndVertical ();
				}
			}
			return field;
		}

		
		public int FieldToID (GameObject field, int _constantID)
		{
			if (field != null)
			{
				if (isAssetFile || (!isAssetFile && !field.gameObject.activeInHierarchy))
				{
					if (field.GetComponent <ConstantID>())
					{
						if (!field.gameObject.activeInHierarchy && field.GetComponent <ConstantID>().constantID == 0)
						{
							field.GetComponent <ConstantID>().AssignInitialValue (false);
						}
						_constantID = field.GetComponent <ConstantID>().constantID;
					}
					else if (field.GetComponent <Player>() == null)
					{
						field.gameObject.AddComponent <ConstantID>();
						_constantID = field.GetComponent <ConstantID>().AssignInitialValue (false);
						AssetDatabase.SaveAssets ();
					}
					return _constantID;
				}
				if (!Application.isPlaying)
				{
					return 0;
				}
			}
			return _constantID;
		}
		
		
		public GameObject IDToField (GameObject field, int _constantID, bool moreInfo)
		{
			if (isAssetFile || (!isAssetFile && (field == null || !field.activeInHierarchy)))
			{
				if (_constantID != 0)
				{
					ConstantID newID = Serializer.returnComponent <ConstantID> (_constantID);
					if (newID != null && !Application.isPlaying)
					{
						field = newID.gameObject;
					}

					EditorGUILayout.BeginVertical ("Button");
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Recorded ConstantID: " + _constantID.ToString (), EditorStyles.miniLabel);
					if (field == null)
					{
						if (GUILayout.Button ("Search scenes", EditorStyles.miniButton))
						{
							AdvGame.FindObjectWithConstantID (_constantID);
						}
					}
					EditorGUILayout.EndHorizontal ();
					
					if (field == null && moreInfo)
					{
						EditorGUILayout.HelpBox ("Further controls cannot display because the referenced object cannot be found.", MessageType.Warning);
					}
					EditorGUILayout.EndVertical ();
				}
			}
			return field;
		}

		#endif
		
		
		public virtual void AssignValues (List<ActionParameter> parameters)
		{
			AssignValues ();
		}


		public virtual void AssignValues ()
		{ }


		protected ActionParameter GetParameterWithID (List<ActionParameter> parameters, int _id)
		{
			if (parameters != null && _id >= 0)
			{
				foreach (ActionParameter _parameter in parameters)
				{
					if (_parameter.ID == _id)
					{
						return _parameter;
					}
				}
			}
			return null;
		}


		protected string AssignString (List<ActionParameter> parameters, int _parameterID, string field)
		{
			ActionParameter parameter = GetParameterWithID (parameters, _parameterID);
			if (parameter != null && parameter.parameterType == ParameterType.String)
			{
				return (parameter.stringValue);
			}
			return field;
		}


		protected BoolValue AssignBoolean (List<ActionParameter> parameters, int _parameterID, BoolValue field)
		{
			ActionParameter parameter = GetParameterWithID (parameters, _parameterID);
			if (parameter != null && parameter.parameterType == ParameterType.Boolean)
			{
				if (parameter.intValue == 1)
				{
					return BoolValue.True;
				}
				return BoolValue.False;
			}
			return field;
		}


		protected int AssignInteger (List<ActionParameter> parameters, int _parameterID, int field)
		{
			ActionParameter parameter = GetParameterWithID (parameters, _parameterID);
			if (parameter != null && parameter.parameterType == ParameterType.Integer)
			{
				return (parameter.intValue);
			}
			return field;
		}


		protected float AssignFloat (List<ActionParameter> parameters, int _parameterID, float field)
		{
			ActionParameter parameter = GetParameterWithID (parameters, _parameterID);
			if (parameter != null && parameter.parameterType == ParameterType.Float)
			{
				return (parameter.floatValue);
			}
			return field;
		}


		protected int AssignVariableID (List<ActionParameter> parameters, int _parameterID, int field)
		{
			ActionParameter parameter = GetParameterWithID (parameters, _parameterID);
			if (parameter != null && (parameter.parameterType == ParameterType.GlobalVariable || parameter.parameterType == ParameterType.LocalVariable))
			{
				return (parameter.intValue);
			}
			return field;
		}


		protected int AssignInvItemID (List<ActionParameter> parameters, int _parameterID, int field)
		{
			ActionParameter parameter = GetParameterWithID (parameters, _parameterID);
			if (parameter != null && parameter.parameterType == ParameterType.InventoryItem)
			{
				return (parameter.intValue);
			}
			return field;
		}


		public Transform AssignFile (List<ActionParameter> parameters, int _parameterID, int _constantID, Transform field)
		{
			Transform file = field;
			
			ActionParameter parameter = GetParameterWithID (parameters, _parameterID);
			
			if (parameter != null && parameter.parameterType == ParameterType.GameObject)
			{
				if (!isAssetFile && parameter.gameObject != null)
				{
					file = parameter.gameObject.transform;
				}
				else if (parameter.intValue != 0)
				{
					ConstantID idObject = Serializer.returnComponent <ConstantID> (parameter.intValue);
					if (idObject != null)
					{
						file = idObject.gameObject.transform;
					}
				}
			}
			else if (_constantID != 0)
			{
				ConstantID idObject = Serializer.returnComponent <ConstantID> (_constantID);
				if (idObject != null)
				{
					file = idObject.gameObject.transform;
				}
				
			}
			
			return file;
		}


		protected GameObject AssignFile (List<ActionParameter> parameters, int _parameterID, int _constantID, GameObject field)
		{
			GameObject file = field;
			
			ActionParameter parameter = GetParameterWithID (parameters, _parameterID);
			
			if (parameter != null && parameter.parameterType == ParameterType.GameObject)
			{
				file = null;
				if (parameter.gameObject != null)
				{
					file = parameter.gameObject;
				}
				else if (parameter.intValue != 0)
				{
					ConstantID idObject = Serializer.returnComponent <ConstantID> (parameter.intValue);
					if (idObject != null)
					{
						file = idObject.gameObject;
					}
				}
			}
			else if (_constantID != 0)
			{
				ConstantID idObject = Serializer.returnComponent <ConstantID> (_constantID);
				if (idObject != null)
				{
					file = idObject.gameObject;
				}

			}
			
			return file;
		}


		public T AssignFile <T> (List<ActionParameter> parameters, int _parameterID, int _constantID, T field) where T : Behaviour
		{
			T file = field;
			
			ActionParameter parameter = GetParameterWithID (parameters, _parameterID);

			if (parameter != null && parameter.parameterType == ParameterType.GameObject)
			{
				file = null;

				if (!isAssetFile && parameter.gameObject != null && parameter.gameObject.GetComponent <T>())
				{
					file = parameter.gameObject.GetComponent <T>();
				}
				else if (parameter.intValue != 0)
				{
					file = Serializer.returnComponent <T> (parameter.intValue);
				}
			}
			else if (_constantID != 0)
			{
				file = Serializer.returnComponent <T> (_constantID);
			}

			return file;
		}


		public T AssignFile <T> (int _constantID, T field) where T : Behaviour
		{
			if (_constantID != 0)
			{
				T newField = Serializer.returnComponent <T> (_constantID);
				if (newField != null)
				{
					return newField;
				}
			}
			return field;
		}


		protected GameObject AssignFile (int _constantID, GameObject field)
		{
			if (_constantID != 0)
			{
				ConstantID newField = Serializer.returnComponent <ConstantID> (_constantID);
				if (newField != null)
				{
					return newField.gameObject;
				}
			}
			return field;
		}


		public Transform AssignFile (int _constantID, Transform field)
		{
			if (_constantID != 0)
			{
				ConstantID newField = Serializer.returnComponent <ConstantID> (_constantID);
				if (newField != null)
				{
					return newField.transform;
				}
			}
			return field;
		}


		#if UNITY_EDITOR

		public virtual void FixLinkAfterDeleting (Action actionToDelete, Action targetAction, List<Action> actionList)
		{
			if ((endAction == ResultAction.Skip && skipActionActual == actionToDelete) || (endAction == ResultAction.Continue && actionList.IndexOf (actionToDelete) == (actionList.IndexOf (this) + 1)))
			{
				if (targetAction == null)
				{
					endAction = ResultAction.Stop;
				}
				else
				{
					endAction = ResultAction.Skip;
					skipActionActual = targetAction;
				}
			}
		}


		public virtual void PrepareToCopy (int originalIndex, List<Action> actionList)
		{
			if (endAction == ResultAction.Continue)
			{
				if (originalIndex == actionList.Count - 1)
				{
					endAction = ResultAction.Stop;
				}
				else if (actionList [originalIndex + 1].isMarked)
				{
					endAction = ResultAction.Skip;
					skipActionActual = actionList [originalIndex + 1];
				}
				else
				{
					endAction = ResultAction.Stop;
				}
			}
			if (endAction == ResultAction.Skip)
			{
				if (skipActionActual.isMarked)
				{
					int place = 0;
					foreach (Action _action in actionList)
					{
						if (_action.isMarked)
						{
							if (_action == skipActionActual)
							{
								skipActionActual = null;
								skipAction = place;
								break;
							}
							place ++;
						}
					}
				}
				else
				{
					endAction = ResultAction.Stop;
				}
			}
		}


		public virtual void AfterCopy (List<Action> copyList)
		{
			if (endAction == ResultAction.Skip && skipActionActual == null && copyList.Count > skipAction)
			{
				skipActionActual = copyList[skipAction];
			}
		}

		#endif


		protected ActionEnd GenerateActionEnd (ResultAction _resultAction, ActionListAsset _linkedAsset, Cutscene _linkedCutscene, int _skipAction, Action _skipActionActual, List<Action> _actions)
		{
			ActionEnd actionEnd = new ActionEnd ();

			actionEnd.resultAction = _resultAction;
			actionEnd.linkedAsset = _linkedAsset;
			actionEnd.linkedCutscene = _linkedCutscene;
			
			if (_resultAction == ResultAction.RunCutscene)
			{
				if (isAssetFile && _linkedAsset != null)
				{
					actionEnd.linkedAsset = _linkedAsset;
				}
				else if (!isAssetFile && _linkedCutscene != null)
				{
					actionEnd.linkedCutscene = _linkedCutscene;
				}
			}
			else if (_resultAction == ResultAction.Skip)
			{
				int skip = _skipAction;
				if (_skipActionActual && _actions.Contains (_skipActionActual))
				{
					skip = _actions.IndexOf (_skipActionActual);
				}
				else if (skip == -1)
				{
					skip = 0;
				}
				actionEnd.skipAction = skip;
			}
			
			return actionEnd;
		}


		protected ActionEnd GenerateStopActionEnd ()
		{
			ActionEnd actionEnd = new ActionEnd ();
			actionEnd.resultAction = ResultAction.Stop;
			return actionEnd;
		}


		public virtual void SetLastResult (ActionEnd _actionEnd)
		{
			lastResult = _actionEnd;
		}

	}
	
}
