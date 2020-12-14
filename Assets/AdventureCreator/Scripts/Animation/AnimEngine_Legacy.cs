/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"AnimEngine_Legacy.cs"
 * 
 *	This script uses the Legacy
 *	system for 3D animation.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AnimEngine_Legacy : AnimEngine
{

	public override void CharSettingsGUI ()
	{
		#if UNITY_EDITOR
		
		EditorGUILayout.BeginVertical ("Button");
		EditorGUILayout.LabelField ("Standard 3D animations:", EditorStyles.boldLabel);

		if (AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager && AdvGame.GetReferences ().settingsManager.IsTopDown ())
		{
			character.spriteChild = (Transform) EditorGUILayout.ObjectField ("Animation child:", character.spriteChild, typeof (Transform), true);
		}
		else
		{
			character.spriteChild = null;
		}

		character.talkingAnimation = (TalkingAnimation) EditorGUILayout.EnumPopup ("Talk animation style:", character.talkingAnimation);
		character.idleAnim = (AnimationClip) EditorGUILayout.ObjectField ("Idle:", character.idleAnim, typeof (AnimationClip), false);
		character.walkAnim = (AnimationClip) EditorGUILayout.ObjectField ("Walk:", character.walkAnim, typeof (AnimationClip), false);
		character.runAnim = (AnimationClip) EditorGUILayout.ObjectField ("Run:", character.runAnim, typeof (AnimationClip), false);
		if (character.talkingAnimation == TalkingAnimation.Standard)
		{
			character.talkAnim = (AnimationClip) EditorGUILayout.ObjectField ("Talk:", character.talkAnim, typeof (AnimationClip), false);
		}

		if (AdvGame.GetReferences () && AdvGame.GetReferences ().speechManager)
		{
			if (AdvGame.GetReferences () && AdvGame.GetReferences ().speechManager &&
			    AdvGame.GetReferences ().speechManager.lipSyncMode != LipSyncMode.Off && AdvGame.GetReferences ().speechManager.lipSyncMode != LipSyncMode.FaceFX)
			{
				if (AdvGame.GetReferences ().speechManager.lipSyncOutput == LipSyncOutput.PortraitAndGameObject)
				{
					if (character.GetShapeable ())
					{
						character.lipSyncGroupID = ActionBlendShape.ShapeableGroupGUI ("Phoneme shape group:", character.GetShapeable ().shapeGroups, character.lipSyncGroupID);
					}
					else
					{
						EditorGUILayout.HelpBox ("Attach a Shapeable script to show phoneme options", MessageType.Info);
					}
				}
				else if (AdvGame.GetReferences ().speechManager.lipSyncOutput == LipSyncOutput.GameObjectTexture)
				{
					if (character.GetComponent <LipSyncTexture>() == null)
					{
						EditorGUILayout.HelpBox ("Attach a LipSyncTexture script to allow texture lip-syncing.", MessageType.Info);
					}
				}
			}
		}

		character.turnLeftAnim = (AnimationClip) EditorGUILayout.ObjectField ("Turn left:", character.turnLeftAnim, typeof (AnimationClip), false);
		character.turnRightAnim = (AnimationClip) EditorGUILayout.ObjectField ("Turn right:", character.turnRightAnim, typeof (AnimationClip), false);
		character.headLookLeftAnim = (AnimationClip) EditorGUILayout.ObjectField ("Head look left:", character.headLookLeftAnim, typeof (AnimationClip), false);
		character.headLookRightAnim = (AnimationClip) EditorGUILayout.ObjectField ("Head look right:", character.headLookRightAnim, typeof (AnimationClip), false);
		character.headLookUpAnim = (AnimationClip) EditorGUILayout.ObjectField ("Head look up:", character.headLookUpAnim, typeof (AnimationClip), false);
		character.headLookDownAnim = (AnimationClip) EditorGUILayout.ObjectField ("Head look down:", character.headLookDownAnim, typeof (AnimationClip), false);
		if (character is Player)
		{
			Player player = (Player) character;
			player.jumpAnim = (AnimationClip) EditorGUILayout.ObjectField ("Jump:", player.jumpAnim, typeof (AnimationClip), false);
		}
		EditorGUILayout.EndVertical ();
		
		EditorGUILayout.BeginVertical ("Button");
		EditorGUILayout.LabelField ("Bone transforms:", EditorStyles.boldLabel);
		
		character.upperBodyBone = (Transform) EditorGUILayout.ObjectField ("Upper body:", character.upperBodyBone, typeof (Transform), true);
		character.neckBone = (Transform) EditorGUILayout.ObjectField ("Neck bone:", character.neckBone, typeof (Transform), true);
		character.leftArmBone = (Transform) EditorGUILayout.ObjectField ("Left arm:", character.leftArmBone, typeof (Transform), true);
		character.rightArmBone = (Transform) EditorGUILayout.ObjectField ("Right arm:", character.rightArmBone, typeof (Transform), true);
		character.leftHandBone = (Transform) EditorGUILayout.ObjectField ("Left hand:", character.leftHandBone, typeof (Transform), true);
		character.rightHandBone = (Transform) EditorGUILayout.ObjectField ("Right hand:", character.rightHandBone, typeof (Transform), true);
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

		if (action.method == ActionCharAnim.AnimMethodChar.PlayCustom || action.method == ActionCharAnim.AnimMethodChar.StopCustom)
		{
			action.clip = (AnimationClip) EditorGUILayout.ObjectField ("Clip:", action.clip, typeof (AnimationClip), true);
			
			if (action.method == ActionCharAnim.AnimMethodChar.PlayCustom)
			{
				action.layer = (AnimLayer) EditorGUILayout.EnumPopup ("Layer:", action.layer);
				
				if (action.layer == AnimLayer.Base)
				{
					EditorGUILayout.LabelField ("Blend mode:", "Blend");
					action.playModeBase = (AnimPlayModeBase) EditorGUILayout.EnumPopup ("Play mode:", action.playModeBase);
				}
				else
				{
					action.blendMode = (AnimationBlendMode) EditorGUILayout.EnumPopup ("Blend mode:", action.blendMode);
					action.playMode = (AnimPlayMode) EditorGUILayout.EnumPopup ("Play mode:", action.playMode);
				}
			}
			
			action.fadeTime = EditorGUILayout.Slider ("Transition time:", action.fadeTime, 0f, 1f);
			action.willWait = EditorGUILayout.Toggle ("Wait until finish?", action.willWait);
		}
		
		else if (action.method == ActionCharAnim.AnimMethodChar.SetStandard)
		{
			action.clip = (AnimationClip) EditorGUILayout.ObjectField ("Clip:", action.clip, typeof (AnimationClip), true);
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
		if (action.animChar == null)
		{
			return 0f;
		}
		
		Animation animation = null;
		
		if (action.animChar.spriteChild && action.animChar.spriteChild.GetComponent <Animation>())
		{
			animation = action.animChar.spriteChild.GetComponent <Animation>();
		}
		if (character.GetComponent <Animation>())
		{
			animation = action.animChar.GetComponent <Animation>();
		}
		
		if (!action.isRunning)
		{
			action.isRunning = true;
			
			if (action.method == ActionCharAnim.AnimMethodChar.PlayCustom && action.clip)
			{
				AdvGame.CleanUnusedClips (animation);
				
				WrapMode wrap = WrapMode.Once;
				Transform mixingTransform = null;
				
				if (action.layer == AnimLayer.Base)
				{
					action.animChar.charState = CharState.Custom;
					action.blendMode = AnimationBlendMode.Blend;
					action.playMode = (AnimPlayMode) action.playModeBase;
				}
				else if (action.layer == AnimLayer.UpperBody)
				{
					mixingTransform = action.animChar.upperBodyBone;
				}
				else if (action.layer == AnimLayer.LeftArm)
				{
					mixingTransform = action.animChar.leftArmBone;
				}
				else if (action.layer == AnimLayer.RightArm)
				{
					mixingTransform = action.animChar.rightArmBone;
				}
				else if (action.layer == AnimLayer.Neck || action.layer == AnimLayer.Head || action.layer == AnimLayer.Face || action.layer == AnimLayer.Mouth)
				{
					mixingTransform = action.animChar.neckBone;
				}
				
				if (action.playMode == AnimPlayMode.PlayOnceAndClamp)
				{
					wrap = WrapMode.ClampForever;
				}
				else if (action.playMode == AnimPlayMode.Loop)
				{
					wrap = WrapMode.Loop;
				}

				AdvGame.PlayAnimClip (animation, AdvGame.GetAnimLayerInt (action.layer), action.clip, action.blendMode, wrap, action.fadeTime, mixingTransform, false);
			}
			
			else if (action.method == ActionCharAnim.AnimMethodChar.StopCustom && action.clip)
			{
				if (action.clip != action.animChar.idleAnim && action.clip != action.animChar.walkAnim)
				{
					animation.Blend (action.clip.name, 0f, action.fadeTime);
				}
			}
			
			else if (action.method == ActionCharAnim.AnimMethodChar.ResetToIdle)
			{
				action.animChar.ResetBaseClips ();
				
				action.animChar.charState = CharState.Idle;
				AdvGame.CleanUnusedClips (animation);
			}
			
			else if (action.method == ActionCharAnim.AnimMethodChar.SetStandard)
			{
				if (action.clip != null)
				{
					if (action.standard == AnimStandard.Idle)
					{
						action.animChar.idleAnim = action.clip;
					}
					else if (action.standard == AnimStandard.Walk)
					{
						action.animChar.walkAnim = action.clip;
					}
					else if (action.standard == AnimStandard.Run)
					{
						action.animChar.runAnim = action.clip;
					}
					else if (action.standard == AnimStandard.Talk)
					{
						action.animChar.talkAnim = action.clip;
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
			
			if (action.willWait && action.clip)
			{
				if (action.method == ActionCharAnim.AnimMethodChar.PlayCustom)
				{
					return action.defaultPauseTime;
				}
				else if (action.method == ActionCharAnim.AnimMethodChar.StopCustom)
				{
					return action.fadeTime;
				}
			}
		}	
		
		else
		{
			if (action.animChar._animation [action.clip.name] && action.animChar._animation [action.clip.name].normalizedTime < 1f && action.animChar._animation.IsPlaying (action.clip.name))
			{
				return action.defaultPauseTime;
			}
			else
			{
				action.isRunning = false;
				
				if (action.playMode == AnimPlayMode.PlayOnce)
				{
					action.animChar._animation.Blend (action.clip.name, 0f, action.fadeTime);
					
					if (action.layer == AnimLayer.Base && action.method == ActionCharAnim.AnimMethodChar.PlayCustom)
					{
						action.animChar.charState = CharState.Idle;
						action.animChar.ResetBaseClips ();
					}
				}
				
				AdvGame.CleanUnusedClips (animation);
				
				return 0f;
			}
		}
		
		return 0f;
	}


	public override void ActionCharAnimSkip (ActionCharAnim action)
	{
		if (action.animChar == null)
		{
			return;
		}
		
		Animation animation = null;

		if (action.animChar.spriteChild && action.animChar.spriteChild.GetComponent <Animation>())
		{
			animation = action.animChar.spriteChild.GetComponent <Animation>();
		}
		if (character._animation)
		{
			animation = action.animChar._animation;
		}

		if (action.method == ActionCharAnim.AnimMethodChar.PlayCustom && action.clip)
		{
			if (action.layer == AnimLayer.Base)
			{
				action.animChar.charState = CharState.Custom;
				action.blendMode = AnimationBlendMode.Blend;
				action.playMode = (AnimPlayMode) action.playModeBase;
			}

			if (action.playMode == AnimPlayMode.PlayOnce)
			{
				if (action.layer == AnimLayer.Base && action.method == ActionCharAnim.AnimMethodChar.PlayCustom)
				{
					action.animChar.charState = CharState.Idle;
					action.animChar.ResetBaseClips ();
				}
			}
			else
			{
				AdvGame.CleanUnusedClips (animation);
				
				WrapMode wrap = WrapMode.Once;
				Transform mixingTransform = null;

				if (action.layer == AnimLayer.UpperBody)
				{
					mixingTransform = action.animChar.upperBodyBone;
				}
				else if (action.layer == AnimLayer.LeftArm)
				{
					mixingTransform = action.animChar.leftArmBone;
				}
				else if (action.layer == AnimLayer.RightArm)
				{
					mixingTransform = action.animChar.rightArmBone;
				}
				else if (action.layer == AnimLayer.Neck || action.layer == AnimLayer.Head || action.layer == AnimLayer.Face || action.layer == AnimLayer.Mouth)
				{
					mixingTransform = action.animChar.neckBone;
				}
				
				if (action.playMode == AnimPlayMode.PlayOnceAndClamp)
				{
					wrap = WrapMode.ClampForever;
				}
				else if (action.playMode == AnimPlayMode.Loop)
				{
					wrap = WrapMode.Loop;
				}

				AdvGame.PlayAnimClipFrame (animation, AdvGame.GetAnimLayerInt (action.layer), action.clip, action.blendMode, wrap, action.fadeTime, mixingTransform, 1f);
			}

			AdvGame.CleanUnusedClips (animation);
		}
		
		else if (action.method == ActionCharAnim.AnimMethodChar.StopCustom && action.clip)
		{
			if (action.clip != action.animChar.idleAnim && action.clip != action.animChar.walkAnim)
			{
				animation.Blend (action.clip.name, 0f, 0f);
			}
		}
		
		else if (action.method == ActionCharAnim.AnimMethodChar.ResetToIdle)
		{
			action.animChar.ResetBaseClips ();
			
			action.animChar.charState = CharState.Idle;
			AdvGame.CleanUnusedClips (animation);
		}
		
		else if (action.method == ActionCharAnim.AnimMethodChar.SetStandard)
		{
			if (action.clip != null)
			{
				if (action.standard == AnimStandard.Idle)
				{
					action.animChar.idleAnim = action.clip;
				}
				else if (action.standard == AnimStandard.Walk)
				{
					action.animChar.walkAnim = action.clip;
				}
				else if (action.standard == AnimStandard.Run)
				{
					action.animChar.runAnim = action.clip;
				}
				else if (action.standard == AnimStandard.Talk)
				{
					action.animChar.talkAnim = action.clip;
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


	public override bool ActionCharHoldPossible ()
	{
		return true;
	}


	public override void ActionSpeechGUI (ActionSpeech action)
	{
		#if UNITY_EDITOR

		if (action.speaker.talkingAnimation == TalkingAnimation.CustomFace)
		{
			action.headClip = (AnimationClip) EditorGUILayout.ObjectField ("Head animation:", action.headClip, typeof (AnimationClip), true);
			action.mouthClip = (AnimationClip) EditorGUILayout.ObjectField ("Mouth animation:", action.mouthClip, typeof (AnimationClip), true);
		}

		if (GUI.changed)
		{
			try
			{
				EditorUtility.SetDirty (action);
			} catch {}
		}

		#endif
	}


	public override void ActionSpeechRun (ActionSpeech action)
	{
		if (action.speaker.talkingAnimation == TalkingAnimation.CustomFace && (action.headClip || action.mouthClip))
		{
			AdvGame.CleanUnusedClips (action.speaker.GetComponent <Animation>());	
			
			if (action.headClip)
			{
				AdvGame.PlayAnimClip (action.speaker.GetComponent <Animation>(), AdvGame.GetAnimLayerInt (AnimLayer.Head), action.headClip, AnimationBlendMode.Additive, WrapMode.Once, 0f, action.speaker.neckBone, false);
			}
			
			if (action.mouthClip)
			{
				AdvGame.PlayAnimClip (action.speaker.GetComponent <Animation>(), AdvGame.GetAnimLayerInt (AnimLayer.Mouth), action.mouthClip, AnimationBlendMode.Additive, WrapMode.Once, 0f, action.speaker.neckBone, false);
			}
		}
	}


	public override void ActionSpeechSkip (ActionSpeech action)
	{
		if (action.speaker.talkingAnimation == TalkingAnimation.CustomFace && (action.headClip || action.mouthClip))
		{
			AdvGame.CleanUnusedClips (action.speaker.GetComponent <Animation>());	
			
			if (action.headClip)
			{
				AdvGame.PlayAnimClipFrame (action.speaker.GetComponent <Animation>(), AdvGame.GetAnimLayerInt (AnimLayer.Head), action.headClip, AnimationBlendMode.Additive, WrapMode.Once, 0f, action.speaker.neckBone, 1f);
			}
			
			if (action.mouthClip)
			{
				AdvGame.PlayAnimClipFrame (action.speaker.GetComponent <Animation>(), AdvGame.GetAnimLayerInt (AnimLayer.Mouth), action.mouthClip, AnimationBlendMode.Additive, WrapMode.Once, 0f, action.speaker.neckBone, 1f);
			}
		}
	}


	public override void ActionAnimGUI (ActionAnim action, List<ActionParameter> parameters)
	{
		#if UNITY_EDITOR

		action.method = (AnimMethod) EditorGUILayout.EnumPopup ("Method:", action.method);

		if (action.method == AnimMethod.PlayCustom || action.method == AnimMethod.StopCustom)
		{
			action.parameterID = Action.ChooseParameterGUI ("Object:", parameters, action.parameterID, ParameterType.GameObject);
			if (action.parameterID >= 0)
			{
				action.constantID = 0;
				action._anim = null;
			}
			else
			{
				action._anim = (Animation) EditorGUILayout.ObjectField ("Object:", action._anim, typeof (Animation), true);
				
				action.constantID = action.FieldToID <Animation> (action._anim, action.constantID);
				action._anim = action.IDToField <Animation> (action._anim, action.constantID, false);
			}

			action.clip = (AnimationClip) EditorGUILayout.ObjectField ("Clip:", action.clip, typeof (AnimationClip), true);

			if (action.method == AnimMethod.PlayCustom)
			{
				action.playMode = (AnimPlayMode) EditorGUILayout.EnumPopup ("Play mode:", action.playMode);
				action.blendMode = (AnimationBlendMode) EditorGUILayout.EnumPopup ("Blend mode:",action.blendMode);
			}

			action.fadeTime = EditorGUILayout.Slider ("Transition time:", action.fadeTime, 0f, 1f);
		}
		else if (action.method == AnimMethod.BlendShape)
		{
			action.isPlayer = EditorGUILayout.Toggle ("Is player?", action.isPlayer);
			if (!action.isPlayer)
			{
				action.parameterID = Action.ChooseParameterGUI ("Object:", parameters, action.parameterID, ParameterType.GameObject);
				if (action.parameterID >= 0)
				{
					action.constantID = 0;
					action.shapeObject = null;
				}
				else
				{
					action.shapeObject = (Shapeable) EditorGUILayout.ObjectField ("Object:", action.shapeObject, typeof (Shapeable), true);
					
					action.constantID = action.FieldToID <Shapeable> (action.shapeObject, action.constantID);
					action.shapeObject = action.IDToField <Shapeable> (action.shapeObject, action.constantID, false);
				}
			}

			action.shapeKey = EditorGUILayout.IntField ("Shape key:", action.shapeKey);
			action.shapeValue = EditorGUILayout.Slider ("Shape value:", action.shapeValue, 0f, 100f);
			action.fadeTime = EditorGUILayout.Slider ("Transition time:", action.fadeTime, 0f, 2f);
		}
		
		action.willWait = EditorGUILayout.Toggle ("Wait until finish?", action.willWait);

		if (GUI.changed)
		{
			EditorUtility.SetDirty (action);
		}

		#endif
	}


	public override string ActionAnimLabel (ActionAnim action)
	{
		string label = "";
		
		if (action._anim)
		{
			label = action._anim.name;
			
			if (action.method == AnimMethod.PlayCustom && action.clip)
			{
				label += " - Play " + action.clip.name;
			}
			else if (action.method == AnimMethod.StopCustom && action.clip)
			{
				label += " - Stop " + action.clip.name;
			}
			else if (action.method == AnimMethod.BlendShape)
			{
				label += " - Shapekey";
			}
		}
		
		return label;
	}


	public override void ActionAnimAssignValues (ActionAnim action, List<ActionParameter> parameters)
	{
		action._anim = action.AssignFile <Animation> (parameters, action.parameterID, action.constantID, action._anim);
		action.shapeObject = action.AssignFile <Shapeable> (parameters, action.parameterID, action.constantID, action.shapeObject);
	}


	public override float ActionAnimRun (ActionAnim action)
	{
		if (!action.isRunning)
		{
			action.isRunning = true;
			
			if (action.method == AnimMethod.PlayCustom && action._anim && action.clip)
			{
				AdvGame.CleanUnusedClips (action._anim);
				
				WrapMode wrap = WrapMode.Once;
				if (action.playMode == AnimPlayMode.PlayOnceAndClamp)
				{
					wrap = WrapMode.ClampForever;
				}
				else if (action.playMode == AnimPlayMode.Loop)
				{
					wrap = WrapMode.Loop;
				}
				
				AdvGame.PlayAnimClip (action._anim, 0, action.clip, action.blendMode, wrap, action.fadeTime, null, false);
			}
			
			else if (action.method == AnimMethod.StopCustom && action._anim && action.clip)
			{
				AdvGame.CleanUnusedClips (action._anim);
				action._anim.Blend (action.clip.name, 0f, action.fadeTime);
			}
			
			else if (action.method == AnimMethod.BlendShape && action.shapeKey > -1)
			{
				if (action.shapeObject)
				{
					action.shapeObject.Change (action.shapeKey, action.shapeValue, action.fadeTime);

					if (action.willWait)
					{
						return (action.fadeTime);
					}
				}
			}
			
			if (action.willWait)
			{
				return (action.defaultPauseTime);
			}
		}
		else
		{

			if (action.method == AnimMethod.PlayCustom && action._anim && action.clip)
			{
				if (!action._anim.IsPlaying (action.clip.name))
				{
					action.isRunning = false;
					return 0f;
				}
				else
				{
					return action.defaultPauseTime;
				}
			}
			else if (action.method == AnimMethod.BlendShape && action.shapeObject)
			{
				action.isRunning = false;
				return 0f;
			}
		}

		return 0f;
	}


	public override void ActionAnimSkip (ActionAnim action)
	{
		if (action.method == AnimMethod.PlayCustom && action._anim && action.clip)
		{
			AdvGame.CleanUnusedClips (action._anim);
			
			WrapMode wrap = WrapMode.Once;
			if (action.playMode == AnimPlayMode.PlayOnceAndClamp)
			{
				wrap = WrapMode.ClampForever;
			}
			else if (action.playMode == AnimPlayMode.Loop)
			{
				wrap = WrapMode.Loop;
			}
			
			AdvGame.PlayAnimClipFrame (action._anim, 0, action.clip, action.blendMode, wrap, 0f, null, 1f);
		}
		
		else if (action.method == AnimMethod.StopCustom && action._anim && action.clip)
		{
			AdvGame.CleanUnusedClips (action._anim);
			action._anim.Blend (action.clip.name, 0f, 0f);
		}
		
		else if (action.method == AnimMethod.BlendShape && action.shapeKey > -1)
		{
			if (action.shapeObject)
			{
				action.shapeObject.Change (action.shapeKey, action.shapeValue, 0f);
			}
		}
	}


	public override void ActionCharRenderGUI (ActionCharRender action)
	{
		#if UNITY_EDITOR
		
		EditorGUILayout.Space ();
		action.renderLock_scale = (RenderLock) EditorGUILayout.EnumPopup ("Character scale:", action.renderLock_scale);
		if (action.renderLock_scale == RenderLock.Set)
		{
			action.scale = EditorGUILayout.IntField ("New scale (%):", action.scale);
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

			if (action._char.spriteChild != null)
			{
				action._char.spriteScale = (float) action.scale / 100f;
			}
			else
			{
				float _scale = (float) action.scale;
				action._char.transform.localScale = new Vector3 (_scale, _scale, _scale);
			}
		}
		else if (action.renderLock_scale == RenderLock.Release)
		{
			action._char.lockScale = false;
		}

		return 0f;
	}


	public override void PlayIdle ()
	{
		PlayStandardAnim (character.idleAnim, true, false);
	}
	
	
	public override void PlayWalk ()
	{
		PlayStandardAnim (character.walkAnim, true, character.isReversing);
	}
	
	
	public override void PlayRun ()
	{
		PlayStandardAnim (character.runAnim, true, character.isReversing);
	}


	public override void PlayTalk ()
	{
		PlayStandardAnim (character.talkAnim, true, false);
	}


	public override void PlayJump ()
	{
		if (character is Player)
		{
			Player player = (Player) character;

			if (player.jumpAnim)
			{
				PlayStandardAnim (player.jumpAnim, false, false);
			}
			else
			{
				PlayIdle ();
			}
		}
		else
		{
			PlayIdle ();
		}
	}

	
	public override void PlayTurnLeft ()
	{
		if (character.turnLeftAnim)
		{
			PlayStandardAnim (character.turnLeftAnim, false, false);
		}
		else
		{
			PlayIdle ();
		}
	}
	
	
	public override void PlayTurnRight ()
	{
		if (character.turnRightAnim)
		{
			PlayStandardAnim (character.turnRightAnim, false, false);
		}
		else
		{
			PlayIdle ();
		}
	}


	public override void TurnHead (Vector2 angles)
	{
		if (character == null)
		{
			return;
		}

		Animation animation = null;
		
		if (character.spriteChild && character.spriteChild.GetComponent <Animation>())
		{
			animation = character.spriteChild.GetComponent <Animation>();
		}
		if (character.GetComponent <Animation>())
		{
			animation = character.GetComponent <Animation>();
		}

		if (animation == null)
		{
			return;
		}

		// Horizontal
		if (character.headLookLeftAnim && character.headLookRightAnim)
		{
			if (angles.x < 0f)
			{
				animation.Stop (character.headLookRightAnim.name);
				AdvGame.PlayAnimClipFrame (animation, AdvGame.GetAnimLayerInt (AnimLayer.Neck), character.headLookLeftAnim, AnimationBlendMode.Additive, WrapMode.ClampForever, 0f, character.neckBone, 1f);
				animation [character.headLookLeftAnim.name].weight = -angles.x;
				animation [character.headLookLeftAnim.name].speed = 0f;
			}
			else if (angles.x > 0f)
			{
				animation.Stop (character.headLookLeftAnim.name);
				AdvGame.PlayAnimClipFrame (animation, AdvGame.GetAnimLayerInt (AnimLayer.Neck), character.headLookRightAnim, AnimationBlendMode.Additive, WrapMode.ClampForever, 0f, character.neckBone, 1f);
				animation [character.headLookRightAnim.name].weight = angles.x;
				animation [character.headLookRightAnim.name].speed = 0f;
			}
			else
			{
				animation.Stop (character.headLookLeftAnim.name);
				animation.Stop (character.headLookRightAnim.name);
			}
		}

		// Vertical
		if (character.headLookUpAnim && character.headLookDownAnim)
		{
			if (angles.y < 0f)
			{
				animation.Stop (character.headLookUpAnim.name);
				AdvGame.PlayAnimClipFrame (animation, AdvGame.GetAnimLayerInt (AnimLayer.Neck) +1, character.headLookDownAnim, AnimationBlendMode.Additive, WrapMode.ClampForever, 0f, character.neckBone, 1f);
				animation [character.headLookDownAnim.name].weight = -angles.y;
				animation [character.headLookDownAnim.name].speed = 0f;
			}
			else if (angles.y > 0f)
			{
				animation.Stop (character.headLookDownAnim.name);
				AdvGame.PlayAnimClipFrame (animation, AdvGame.GetAnimLayerInt (AnimLayer.Neck) +1, character.headLookUpAnim, AnimationBlendMode.Additive, WrapMode.ClampForever, 0f, character.neckBone, 1f);
				animation [character.headLookUpAnim.name].weight = angles.y;
				animation [character.headLookUpAnim.name].speed = 0f;
			}
			else
			{
				animation.Stop (character.headLookDownAnim.name);
				animation.Stop (character.headLookUpAnim.name);
			}
		}
	}


	private void PlayStandardAnim (AnimationClip clip, bool doLoop, bool reverse)
	{
		if (character == null)
		{
			return;
		}

		Animation animation = null;
		
		if (character.spriteChild && character.spriteChild.GetComponent <Animation>())
		{
			animation = character.spriteChild.GetComponent <Animation>();
		}
		if (character.GetComponent <Animation>())
		{
			animation = character.GetComponent <Animation>();
		}

		if (animation != null)
		{
			if (clip != null && animation[clip.name] != null)
			{
				if (!animation [clip.name].enabled)
				{
					if (doLoop)
					{
						AdvGame.PlayAnimClip (animation, AdvGame.GetAnimLayerInt (AnimLayer.Base), clip, AnimationBlendMode.Blend, WrapMode.Loop, character.animCrossfadeSpeed, null, reverse);
					}
					else
					{
						AdvGame.PlayAnimClip (animation, AdvGame.GetAnimLayerInt (AnimLayer.Base), clip, AnimationBlendMode.Blend, WrapMode.Once, character.animCrossfadeSpeed, null, reverse);
					}
				}
			}
			else
			{
				if (doLoop)
				{
					AdvGame.PlayAnimClip (animation, AdvGame.GetAnimLayerInt (AnimLayer.Base), clip, AnimationBlendMode.Blend, WrapMode.Loop, character.animCrossfadeSpeed, null, reverse);
				}
				else
				{
					AdvGame.PlayAnimClip (animation, AdvGame.GetAnimLayerInt (AnimLayer.Base), clip, AnimationBlendMode.Blend, WrapMode.Once, character.animCrossfadeSpeed, null, reverse);
				}
			}
		}
	}

}

