/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionSpriteFade.cs"
 * 
 *	Fades a sprite in or out.
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
	public class ActionSpriteFade : Action
	{
		
		public int constantID = 0;
		public int parameterID = -1;
		public SpriteFader spriteFader;
		
		public FadeType fadeType = FadeType.fadeIn;
		public float fadeSpeed;

		
		public ActionSpriteFade ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Object;
			title = "Fade sprite";
			description = "Fades a sprite in or out.";
		}
		
		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			spriteFader = AssignFile <SpriteFader> (parameters, parameterID, constantID, spriteFader);
		}
		
		
		override public float Run ()
		{
			if (spriteFader == null)
			{
				return 0f;
			}

			if (!isRunning)
			{
				isRunning = true;

				spriteFader.Fade (fadeType, fadeSpeed);

				if (willWait)
				{
					return fadeSpeed;
				}
			}
			else
			{
				isRunning = false;
			}
			
			return 0f;
		}


		override public void Skip ()
		{
			if (spriteFader != null)
			{
				spriteFader.Fade (fadeType, 0f);
			}
		}
	
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			parameterID = Action.ChooseParameterGUI ("Sprite to fade:", parameters, parameterID, ParameterType.GameObject);
			if (parameterID >= 0)
			{
				constantID = 0;
				spriteFader = null;
			}
			else
			{
				spriteFader = (SpriteFader) EditorGUILayout.ObjectField ("Sprite to fade:", spriteFader, typeof (SpriteFader), true);
				
				constantID = FieldToID <SpriteFader> (spriteFader, constantID);
				spriteFader = IDToField <SpriteFader> (spriteFader, constantID, false);
			}

			fadeType = (FadeType) EditorGUILayout.EnumPopup ("Type:", fadeType);
			
			fadeSpeed = EditorGUILayout.Slider ("Time to fade:", fadeSpeed, 0, 3);
			willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);

			AfterRunningOption ();
		}
		
		
		override public string SetLabel ()
		{
			string labelAdd = "";
			
			if (spriteFader != null)
			{
				labelAdd = " (" + fadeType.ToString () + " " + spriteFader.gameObject.name + ")";
			}
			
			return labelAdd;
		}
		
		#endif
		
	}
	
}