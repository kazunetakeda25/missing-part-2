/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionCharAnim.cs"
 * 
 *	This action is used to control character animation.
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
	public class ActionCharAnim : Action
	{

		public int parameterID = -1;
		public int constantID = 0;
		public AnimEngine editingAnimEngine;

		public bool isPlayer;
		public Char animChar;
		public AnimationClip clip;
		public string clip2D;

		public enum AnimMethodChar { PlayCustom, StopCustom, ResetToIdle, SetStandard };
		public AnimMethodChar method;
		
		public AnimationBlendMode blendMode;
		public AnimLayer layer = AnimLayer.Base;
		public AnimStandard standard;
		public bool includeDirection = false;

		public bool changeSound = false;
		public AudioClip newSound;

		public int layerInt;
		public bool idleAfter = true;
		public bool idleAfterCustom = false;

		public AnimPlayMode playMode;
		public AnimPlayModeBase playModeBase = AnimPlayModeBase.PlayOnceAndClamp;

		public float fadeTime = 0f;

		public bool changeSpeed = false;
		public float newSpeed = 0f;

		public AnimMethodCharMecanim methodMecanim;
		public MecanimCharParameter mecanimCharParameter;
		public MecanimParameterType mecanimParameterType;
		public string parameterName;
		public float parameterValue;

		
		public ActionCharAnim ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Character;
			title = "Animate";
			description = "Affects a Character's animation. Can play or stop a custom animation, change a standard animation (idle, walk or run), change a footstep sound, or revert the Character to idle.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			animChar = AssignFile <Char> (parameters, parameterID, constantID, animChar);

			if (isPlayer)
			{
				animChar = KickStarter.player;
			}

			if (animChar && animChar.animEngine == null)
			{
				animChar.ResetAnimationEngine ();
			}
		}

		
		override public float Run ()
		{
			if (animChar)
			{
				if (animChar.animEngine == null)
				{
					animChar.ResetAnimationEngine ();
				}

				if (animChar.animEngine != null)
				{
					return animChar.animEngine.ActionCharAnimRun (this);
				}
				else
				{
					Debug.LogWarning ("Could not create animation engine for " + animChar.name);
				}
			}
			else
			{
				Debug.LogWarning ("Could not create animation engine!");
			}

			return 0f;
		}


		override public void Skip ()
		{
			if (animChar)
			{
				if (animChar.animEngine != null)
				{
					animChar.animEngine.ActionCharAnimSkip (this);
				}
			}
		}


		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			isPlayer = EditorGUILayout.Toggle ("Is Player?", isPlayer);
			if (isPlayer)
			{
				if (Application.isPlaying)
				{
					animChar = KickStarter.player;
				}
				else
				{
					animChar = AdvGame.GetReferences ().settingsManager.GetDefaultPlayer ();
				}
			}
			else
			{
				parameterID = Action.ChooseParameterGUI ("Character:", parameters, parameterID, ParameterType.GameObject);
				if (parameterID >= 0)
				{
					constantID = 0;
					animChar = null;
				}
				else
				{
					animChar = (Char) EditorGUILayout.ObjectField ("Character:", animChar, typeof (Char), true);
					
					constantID = FieldToID <Char> (animChar, constantID);
					animChar = IDToField <Char> (animChar, constantID, true);
				}
			}

			if (animChar)
			{
				ResetAnimationEngine (animChar.animationEngine);
			}

			if (editingAnimEngine != null)
			{
				editingAnimEngine.ActionCharAnimGUI (this);
			}
			else
			{
				EditorGUILayout.HelpBox ("This Action requires a Character before more options will show.", MessageType.Info);
			}

			AfterRunningOption ();
		}

		
		override public string SetLabel ()
		{
			string labelAdd = "";
			
			if (isPlayer)
			{
				labelAdd = " (Player)";
			}
			else if (animChar)
			{
				labelAdd = " (" + animChar.name + ")";
			}
			
			return labelAdd;
		}


		private void ResetAnimationEngine (AnimationEngine animationEngine)
		{
			string className = "AnimEngine_" + animationEngine.ToString ();
			
			if (editingAnimEngine == null || editingAnimEngine.ToString () != className)
			{
				editingAnimEngine = (AnimEngine) ScriptableObject.CreateInstance (className);
			}
		}

		
		#endif

	}

}