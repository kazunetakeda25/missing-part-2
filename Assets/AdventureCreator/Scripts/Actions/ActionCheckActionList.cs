/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionCheckActionList.cs"
 * 
 *	This Action will return "TRUE" if the supplied ActionList
 *	is running, and "FALSE" if it is not.
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
	public class ActionCheckActionList : ActionCheck
	{
		
		public enum ListSource { InScene, AssetFile };
		public ListSource listSource = ListSource.InScene;
		
		public ActionList actionList;
		public ActionListAsset actionListAsset;
		public int constantID = 0;
		public int parameterID = -1;

		private bool isSkipping = false;


		public ActionCheckActionList ()
		{
			this.isDisplayed = true;
			category = ActionCategory.ActionList;
			title = "Check running";
			description = "Queries whether or not a supplied ActionList is currently running. By looping the If condition is not met field back onto itself, this will effectively “wait” until the supplied ActionList has completed before continuing.";
		}


		override public float Run ()
		{
			isSkipping = false;
			return 0f;
		}


		override public void Skip ()
		{
			isSkipping = true;
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			if (listSource == ListSource.InScene)
			{
				actionList = AssignFile <ActionList> (parameters, parameterID, constantID, actionList);
			}
		}


		override public bool CheckCondition ()
		{
			if (isSkipping && IsTargetSkippable ())
			{
				return false;
			}

			if (listSource == ListSource.InScene && actionList != null)
			{
				return KickStarter.actionListManager.IsListRunning (actionList);
			}
			else if (listSource == ListSource.AssetFile && actionListAsset != null)
			{
				return KickStarter.actionListManager.IsListRunning (actionListAsset);
			}
			
			return false;
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
					constantID = 0;
					actionList = null;
				}
				else
				{
					actionList = (ActionList) EditorGUILayout.ObjectField ("ActionList:", actionList, typeof (ActionList), true);
					
					constantID = FieldToID <ActionList> (actionList, constantID);
					actionList = IDToField <ActionList> (actionList, constantID, true);
				}
			}
			else if (listSource == ListSource.AssetFile)
			{
				actionListAsset = (ActionListAsset) EditorGUILayout.ObjectField ("ActionList asset:", actionListAsset, typeof (ActionListAsset), true);
			}
		}


		public override string SetLabel ()
		{
			string labelAdd = "";
			
			if (listSource == ListSource.InScene && actionList != null)
			{
				labelAdd += " (" + actionList.name + ")";
			}
			else if (listSource == ListSource.AssetFile && actionListAsset != null)
			{
				labelAdd += " (" + actionListAsset.name + ")";
			}
			
			return labelAdd;
		}

		#endif


		public override void SetLastResult (ActionEnd _actionEnd)
		{
			if (!IsTargetSkippable ())
			{
				// When skipping, don't want to rely on last result if target can be skipped as well
				base.SetLastResult (_actionEnd);
			}
		}


		private bool IsTargetSkippable ()
		{
			if (listSource == ListSource.InScene && actionList != null)
			{
				return actionList.IsSkippable ();
			}
			else if (listSource == ListSource.AssetFile && actionListAsset != null)
			{
				return actionListAsset.IsSkippable ();
			}
			return false;
		}

	}

}
