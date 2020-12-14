/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"AnimEngine_Sprites2DToolkit.cs"
 * 
 *	This script uses the 2D Toolkit
 *	sprite engine for animation.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AnimEngine_Sprites2DToolkit : AnimEngine
{

	public override void Declare (AC.Char _character)
	{
		character = _character;
		turningStyle = TurningStyle.Linear;
		rootMotion = false;
		isSpriteBased = true;
	}


	public override void CharSettingsGUI ()
	{
		#if UNITY_EDITOR

		EditorGUILayout.BeginVertical ("Button");
		EditorGUILayout.LabelField ("Standard 2D animations:", EditorStyles.boldLabel);

		character.talkingAnimation = TalkingAnimation.Standard;
		character.spriteChild = (Transform) EditorGUILayout.ObjectField ("Sprite child:", character.spriteChild, typeof (Transform), true);
		character.idleAnimSprite = EditorGUILayout.TextField ("Idle name:", character.idleAnimSprite);
		character.walkAnimSprite = EditorGUILayout.TextField ("Walk name:", character.walkAnimSprite);
		character.runAnimSprite = EditorGUILayout.TextField ("Run name:", character.runAnimSprite);
		character.talkAnimSprite = EditorGUILayout.TextField ("Talk name:", character.talkAnimSprite);
		character.doDiagonals = EditorGUILayout.Toggle ("Diagonal sprites?", character.doDiagonals);
		character.frameFlipping = (AC_2DFrameFlipping) EditorGUILayout.EnumPopup ("Frame flipping:", character.frameFlipping);
		if (character.frameFlipping != AC_2DFrameFlipping.None)
		{
			character.flipCustomAnims = EditorGUILayout.Toggle ("Flip custom animations?", character.flipCustomAnims);
		}
		EditorGUILayout.EndVertical ();

		if (GUI.changed)
		{
			EditorUtility.SetDirty (character);
		}

		#endif
	}


	public override void ActionCharAnimGUI (ActionCharAnim action)
	{
		#if UNITY_EDITOR

		action.method = (ActionCharAnim.AnimMethodChar) EditorGUILayout.EnumPopup ("Method:", action.method);

		if (action.method == ActionCharAnim.AnimMethodChar.PlayCustom)
		{
			action.clip2D = EditorGUILayout.TextField ("Clip:", action.clip2D);
			action.includeDirection = EditorGUILayout.Toggle ("Add directional suffix?", action.includeDirection);
			
			action.playMode = (AnimPlayMode) EditorGUILayout.EnumPopup ("Play mode:", action.playMode);
			if (action.playMode == AnimPlayMode.Loop)
			{
				action.willWait = false;
			}
			else
			{
				action.willWait = EditorGUILayout.Toggle ("Wait until finish?", action.willWait);
			}
			
			action.layer = AnimLayer.Base;
		}
		else if (action.method == ActionCharAnim.AnimMethodChar.StopCustom)
		{
			EditorGUILayout.HelpBox ("This Action does not work for Sprite-based characters.", MessageType.Info);
		}
		else if (action.method == ActionCharAnim.AnimMethodChar.SetStandard)
		{
			action.clip2D = EditorGUILayout.TextField ("Clip:", action.clip2D);
			action.standard = (AnimStandard) EditorGUILayout.EnumPopup ("Change:", action.standard);

			if (action.standard == AnimStandard.Walk || action.standard == AnimStandard.Run)
			{
				action.changeSound = EditorGUILayout.Toggle ("Change sound?", action.changeSound);
				if (action.changeSound)
				{
					action.newSound = (AudioClip) EditorGUILayout.ObjectField ("New sound:", action.newSound, typeof (AudioClip), false);
				}
				action.changeSpeed = EditorGUILayout.Toggle ("Change speed?", action.changeSpeed);
				if (action.changeSpeed)
				{
					action.newSpeed = EditorGUILayout.FloatField ("New speed:", action.newSpeed);
				}
			}
		}

		if (GUI.changed)
		{
			EditorUtility.SetDirty (action);
		}

		#endif
	}


	public override float ActionCharAnimRun (ActionCharAnim action)
	{
		string clip2DNew = action.clip2D;
		if (action.includeDirection)
		{
			clip2DNew += action.animChar.GetSpriteDirection ();
		}
		
		if (!action.isRunning)
		{
			action.isRunning = true;
			
			if (action.method == ActionCharAnim.AnimMethodChar.PlayCustom && action.clip2D != "")
			{
				action.animChar.charState = CharState.Custom;
				
				if (action.playMode == AnimPlayMode.Loop)
				{
					tk2DIntegration.PlayAnimation (action.animChar.spriteChild, clip2DNew, true, WrapMode.Loop);
					action.willWait = false;
				}
				else
				{
					tk2DIntegration.PlayAnimation (action.animChar.spriteChild, clip2DNew, true, WrapMode.Once);
				}
			}

			else if (action.method == ActionCharAnim.AnimMethodChar.ResetToIdle)
			{
				action.animChar.ResetBaseClips ();
			}
			
			else if (action.method == ActionCharAnim.AnimMethodChar.SetStandard)
			{
				if (action.clip2D != "")
				{
					if (action.standard == AnimStandard.Idle)
					{
						action.animChar.idleAnimSprite = action.clip2D;
					}
					else if (action.standard == AnimStandard.Walk)
					{
						action.animChar.walkAnimSprite = action.clip2D;
					}
					else if (action.standard == AnimStandard.Talk)
					{
						action.animChar.talkAnimSprite = action.clip2D;
					}
					else if (action.standard == AnimStandard.Run)
					{
						action.animChar.runAnimSprite = action.clip2D;
					}
				}

				if (action.changeSpeed)
				{
					if (action.standard == AnimStandard.Walk)
					{
						action.animChar.walkSpeedScale = action.newSpeed;
					}
					else if (action.standard == AnimStandard.Run)
					{
						action.animChar.runSpeedScale = action.newSpeed;
					}
				}

				if (action.changeSound)
				{
					if (action.standard == AnimStandard.Walk)
					{
						if (action.newSound != null)
						{
							action.animChar.walkSound = action.newSound;
						}
						else
						{
							action.animChar.walkSound = null;
						}
					}
					else if (action.standard == AnimStandard.Run)
					{
						if (action.newSound != null)
						{
							action.animChar.runSound = action.newSound;
						}
						else
						{
							action.animChar.runSound = null;
						}
					}
				}
			}
			
			if (action.willWait && action.clip2D != "")
			{
				if (action.method == ActionCharAnim.AnimMethodChar.PlayCustom)
				{
					return (action.defaultPauseTime);
				}
			}
		}	
		
		else
		{
			if (action.animChar.spriteChild && action.clip2D != "")
			{
				if (!tk2DIntegration.IsAnimationPlaying (action.animChar.spriteChild, action.clip2D))
				{
					action.isRunning = false;
					return 0f;
				}
				else
				{
					return (action.defaultPauseTime / 6f);
				}
			}
		}

		return 0f;
	}


	public override void ActionCharAnimSkip (ActionCharAnim action)
	{
		string clip2DNew = action.clip2D;
		if (action.includeDirection)
		{
			clip2DNew += action.animChar.GetSpriteDirection ();
		}

		if (action.method == ActionCharAnim.AnimMethodChar.PlayCustom && action.clip2D != "")
		{
			if (!action.willWait || action.playMode == AnimPlayMode.Loop)
			{
				action.animChar.charState = CharState.Custom;
				
				if (action.playMode == AnimPlayMode.Loop)
				{
					tk2DIntegration.PlayAnimation (action.animChar.spriteChild, clip2DNew, true, WrapMode.Loop);
					action.willWait = false;
				}
				else
				{
					tk2DIntegration.PlayAnimation (action.animChar.spriteChild, clip2DNew, true, WrapMode.Once);
				}
			}
			else
			{
				if (action.playMode == AnimPlayMode.PlayOnce)
				{
					action.animChar.charState = CharState.Idle;
				}
			}
		}
		
		else if (action.method == ActionCharAnim.AnimMethodChar.ResetToIdle)
		{
			action.animChar.ResetBaseClips ();
		}

		else if (action.method == ActionCharAnim.AnimMethodChar.SetStandard)
		{
			if (action.clip2D != "")
			{
				if (action.standard == AnimStandard.Idle)
				{
					action.animChar.idleAnimSprite = action.clip2D;
				}
				else if (action.standard == AnimStandard.Walk)
				{
					action.animChar.walkAnimSprite = action.clip2D;
				}
				else if (action.standard == AnimStandard.Talk)
				{
					action.animChar.talkAnimSprite = action.clip2D;
				}
				else if (action.standard == AnimStandard.Run)
				{
					action.animChar.runAnimSprite = action.clip2D;
				}
			}
			
			if (action.changeSpeed)
			{
				if (action.standard == AnimStandard.Walk)
				{
					action.animChar.walkSpeedScale = action.newSpeed;
				}
				else if (action.standard == AnimStandard.Run)
				{
					action.animChar.runSpeedScale = action.newSpeed;
				}
			}
			
			if (action.changeSound)
			{
				if (action.standard == AnimStandard.Walk)
				{
					if (action.newSound != null)
					{
						action.animChar.walkSound = action.newSound;
					}
					else
					{
						action.animChar.walkSound = null;
					}
				}
				else if (action.standard == AnimStandard.Run)
				{
					if (action.newSound != null)
					{
						action.animChar.runSound = action.newSound;
					}
					else
					{
						action.animChar.runSound = null;
					}
				}
			}
		}
	}


	public override void ActionAnimGUI (ActionAnim action, List<ActionParameter> parameters)
	{
		#if UNITY_EDITOR

		action.method = (AnimMethod) EditorGUILayout.EnumPopup ("Method:", action.method);

		action.parameterID = AC.Action.ChooseParameterGUI ("Object:", parameters, action.parameterID, ParameterType.GameObject);
		if (action.parameterID >= 0)
		{
			action.constantID = 0;
			action._anim2D = null;
		}
		else
		{
			action._anim2D = (Transform) EditorGUILayout.ObjectField ("Object:", action._anim2D, typeof (Transform), true);
			
			action.constantID = action.FieldToID (action._anim2D, action.constantID);
			action._anim2D = action.IDToField (action._anim2D, action.constantID, false);
		}
		
		if (action.method == AnimMethod.PlayCustom)
		{
			action.clip2D = EditorGUILayout.TextField ("Clip:", action.clip2D);
			action.wrapMode2D = (ActionAnim.WrapMode2D) EditorGUILayout.EnumPopup ("Play mode:", action.wrapMode2D);
			
			if (action.wrapMode2D == ActionAnim.WrapMode2D.Once)
			{
				action.willWait = EditorGUILayout.Toggle ("Wait until finish?", action.willWait);
			}
			else
			{
				action.willWait = false;
			}
		}
		else if (action.method == AnimMethod.BlendShape)
		{
			EditorGUILayout.HelpBox ("BlendShapes are not available in 2D animation.", MessageType.Info);
		}

		if (GUI.changed)
		{
			EditorUtility.SetDirty (action);
		}

		#endif
	}


	public override string ActionAnimLabel (ActionAnim action)
	{
		string label = "";
		
		if (action._anim2D)
		{
			label = action._anim2D.name;
			
			if (action.method == AnimMethod.PlayCustom && action.clip2D != "")
			{
				label += " - " + action.clip2D;
			}
		}
		
		return label;
	}


	public override void ActionAnimAssignValues (ActionAnim action, List<ActionParameter> parameters)
	{
		action._anim2D = action.AssignFile (parameters, action.parameterID, action.constantID, action._anim2D);
	}

	
	public override float ActionAnimRun (ActionAnim action)
	{
		if (!action.isRunning)
		{
			action.isRunning = true;

			if (action._anim2D && action.clip2D != "")
			{
				if (action.method == AnimMethod.PlayCustom)
				{
					if (action.wrapMode2D == ActionAnim.WrapMode2D.Loop)
					{
						tk2DIntegration.PlayAnimation (action._anim2D, action.clip2D, true, WrapMode.Loop);
					}
					else if (action.wrapMode2D == ActionAnim.WrapMode2D.PingPong)
					{
						tk2DIntegration.PlayAnimation (action._anim2D, action.clip2D, true, WrapMode.PingPong);
					}
					else
					{
						tk2DIntegration.PlayAnimation (action._anim2D, action.clip2D, true, WrapMode.Once);
					}
					
					if (action.willWait)
					{
						return (action.defaultPauseTime);
					}
				}
				
				else if (action.method == AnimMethod.StopCustom)
				{
					tk2DIntegration.StopAnimation (action._anim2D);
				}
				
				else if (action.method == AnimMethod.BlendShape)
				{
					Debug.LogWarning ("BlendShapes not available for 2D animation.");
					return 0f;
				}
			}
		}
		else
		{
			if (action._anim2D && action.clip2D != "")
			{
				if (!tk2DIntegration.IsAnimationPlaying (action._anim2D, action.clip2D))
				{
					action.isRunning = false;
				}
				else
				{
					return (Time.deltaTime);
				}
			}
		}

		return 0f;
	}


	public override void ActionAnimSkip (ActionAnim action)
	{
		if (!action.isRunning)
		{
			action.isRunning = true;
			
			if (action._anim2D && action.clip2D != "")
			{
				if (action.method == AnimMethod.PlayCustom)
				{
					if (action.wrapMode2D == ActionAnim.WrapMode2D.Loop)
					{
						tk2DIntegration.PlayAnimation (action._anim2D, action.clip2D, true, WrapMode.Loop);
					}
					else if (action.wrapMode2D == ActionAnim.WrapMode2D.PingPong)
					{
						tk2DIntegration.PlayAnimation (action._anim2D, action.clip2D, true, WrapMode.PingPong);
					}
					else
					{
						tk2DIntegration.PlayAnimation (action._anim2D, action.clip2D, true, WrapMode.Once);
					}
				}
				
				else if (action.method == AnimMethod.StopCustom)
				{
					tk2DIntegration.StopAnimation (action._anim2D);
				}
			}
		}
	}


	public override void ActionCharRenderGUI (ActionCharRender action)
	{
		#if UNITY_EDITOR
		
		EditorGUILayout.Space ();
		action.renderLock_scale = (RenderLock) EditorGUILayout.EnumPopup ("Sprite scale:", action.renderLock_scale);
		if (action.renderLock_scale == RenderLock.Set)
		{
			action.scale = EditorGUILayout.IntField ("New scale (%):", action.scale);
		}
		
		EditorGUILayout.Space ();
		action.renderLock_direction = (RenderLock) EditorGUILayout.EnumPopup ("Sprite direction:", action.renderLock_direction);
		if (action.renderLock_direction == RenderLock.Set)
		{
			action.direction = (CharDirection) EditorGUILayout.EnumPopup ("New direction:", action.direction);
		}

		if (GUI.changed)
		{
			EditorUtility.SetDirty (action);
		}
		
		#endif
	}


	public override float ActionCharRenderRun (ActionCharRender action)
	{
		if (action.renderLock_scale == RenderLock.Set)
		{
			action._char.lockScale = true;
			action._char.spriteScale = (float) action.scale / 100f;
		}
		else if (action.renderLock_scale == RenderLock.Release)
		{
			action._char.lockScale = false;
		}

		if (action.renderLock_direction == RenderLock.Set)
		{
			action._char.lockDirection = true;
			action._char.SetSpriteDirection (action.direction);
		}
		else if (action.renderLock_direction == RenderLock.Release)
		{
			action._char.lockDirection = false;
		}
	
		return 0f;
	}


	public override void PlayIdle ()
	{
		PlayStandardAnim (character.idleAnimSprite, true);
	}
	
	
	public override void PlayWalk ()
	{
		PlayStandardAnim (character.walkAnimSprite, true);
	}
	
	
	public override void PlayRun ()
	{
		PlayStandardAnim (character.runAnimSprite, true);
	}
	
	
	public override void PlayTalk ()
	{

		if (character.LipSyncGameObject ())
		{
			PlayStandardAnim (character.talkAnimSprite, true, character.GetLipSyncFrame ());
		}
		else
		{
			PlayStandardAnim (character.talkAnimSprite, true);
		}
	}
	
	
	private void PlayStandardAnim (string clip, bool includeDirection)
	{
		PlayStandardAnim (clip, includeDirection, -1);
	}


	private void PlayStandardAnim (string clip, bool includeDirection, int frame)
	{
		if (clip != "" && character != null)
		{
			string newClip = clip;
			
			if (includeDirection)
			{
				newClip += character.GetSpriteDirection ();
			}
			
			if (tk2DIntegration.PlayAnimation (character.spriteChild, newClip, frame) == false)
			{
				tk2DIntegration.PlayAnimation (character.spriteChild, clip, frame);
			}
		}
	}

}
