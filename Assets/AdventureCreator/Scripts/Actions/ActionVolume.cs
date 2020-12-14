/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionVolume.cs"
 * 
 *	This action alters the "relative volume" of any Sound script
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
	public class ActionVolume : Action
	{
		
		public int constantID = 0;
		public int parameterID = -1;
		public Sound soundObject;
		
		public float newRelativeVolume = 1f;
		
		public ActionVolume ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Sound;
			title = "Change volume";
			description = "Alters the 'relative volume' of any Sound object.";
		}
		
		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			soundObject = AssignFile <Sound> (parameters, parameterID, constantID, soundObject);
		}
		
		
		override public float Run ()
		{
			if (soundObject)
			{
				soundObject.relativeVolume = newRelativeVolume;
				soundObject.SetMaxVolume ();
			}
			
			return 0f;
		}
				
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			parameterID = Action.ChooseParameterGUI ("Sound object:", parameters, parameterID, ParameterType.GameObject);
			if (parameterID >= 0)
			{
				constantID = 0;
				soundObject = null;
			}
			else
			{
				soundObject = (Sound) EditorGUILayout.ObjectField ("Sound object:", soundObject, typeof(Sound), true);
				
				constantID = FieldToID <Sound> (soundObject, constantID);
				soundObject = IDToField <Sound> (soundObject, constantID, false);
			}
			
			newRelativeVolume = EditorGUILayout.Slider ("New volume:", newRelativeVolume, 0f, 1f);
			
			AfterRunningOption ();
		}
		
		
		override public string SetLabel ()
		{
			string labelAdd = "";
			if (soundObject)
			{
				labelAdd = " (" + soundObject.name + " to " + newRelativeVolume.ToString () + ")";
			}
			
			return labelAdd;
		}
		
		#endif
		
	}
	
}