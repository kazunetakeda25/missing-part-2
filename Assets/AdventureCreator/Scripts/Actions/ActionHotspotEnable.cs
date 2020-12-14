/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionHotspotEnable.cs"
 * 
 *	This Action can enable and disable a Hotspot.
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
	public class ActionHotspotEnable : Action
	{

		public int parameterID = -1;
		public int constantID = 0;
		public Hotspot hotspot;
		public bool affectChildren = false;

		public ChangeType changeType = ChangeType.Enable;

		
		public ActionHotspotEnable ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Hotspot;
			title = "Enable or disable";
			description = "Turns a Hotspot on or off. To record the state of a Hotspot in save games, be sure to add the RememberHotspot script to the Hotspot in question.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			hotspot = AssignFile <Hotspot> (parameters, parameterID, constantID, hotspot);
		}

		
		override public float Run ()
		{
			if (hotspot == null)
			{
				return 0f;
			}

			DoChange (hotspot);

			if (affectChildren)
			{
				Hotspot[] hotspots = hotspot.GetComponentsInChildren <Hotspot>();
				foreach (Hotspot _hotspot in hotspots)
				{
					if (_hotspot != hotspot)
					{
						DoChange (_hotspot);
					}
				}
			}

			return 0f;
		}


		private void DoChange (Hotspot _hotspot)
		{
			if (changeType == ChangeType.Enable)
			{
				_hotspot.TurnOn ();
			}
			else
			{
				_hotspot.TurnOff ();
			}
		}

		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			parameterID = Action.ChooseParameterGUI ("Hotspot to affect:", parameters, parameterID, ParameterType.GameObject);
			if (parameterID >= 0)
			{
				constantID = 0;
				hotspot = null;
			}
			else
			{
				hotspot = (Hotspot) EditorGUILayout.ObjectField ("Hotspot to affect:", hotspot, typeof (Hotspot), true);
				
				constantID = FieldToID <Hotspot> (hotspot, constantID);
				hotspot = IDToField <Hotspot> (hotspot, constantID, false);
			}

			changeType = (ChangeType) EditorGUILayout.EnumPopup ("Change to make:", changeType);
			affectChildren = EditorGUILayout.Toggle ("Also affect children?", affectChildren);

			AfterRunningOption ();
		}
		
		
		public override string SetLabel ()
		{
			string labelAdd = "";
			if (hotspot != null)
			{
				labelAdd = " (" + hotspot.name + " - " + changeType + ")";
			}
			return labelAdd;
		}
		
		#endif
		
	}

}