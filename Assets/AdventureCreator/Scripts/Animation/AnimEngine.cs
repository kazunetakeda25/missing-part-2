	/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"AnimEngine.cs"
 * 
 *	This script is a base class for the Animation engine scripts.
 *  Create a subclass of name "AnimEngine_NewMethodName" and
 * 	add "NewMethodName" to the AnimationEngine enum to integrate
 * 	a new method into the engine.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class AnimEngine : ScriptableObject
{

	// Character variables
	public AC.Char character;
	public TurningStyle turningStyle = TurningStyle.Script;
	public bool rootMotion = false;
	public bool isSpriteBased = false;


	public virtual void Declare (AC.Char _character)
	{
		character = _character;
		turningStyle = TurningStyle.Script;
		rootMotion = false;
		isSpriteBased = false;
	}

	public virtual void CharSettingsGUI ()
	{ 
		#if UNITY_EDITOR
		#endif
	}

	public virtual void ActionCharAnimGUI (ActionCharAnim action)
	{
		#if UNITY_EDITOR
		action.method = (ActionCharAnim.AnimMethodChar) EditorGUILayout.EnumPopup ("Method:", action.method);
		#endif
	}

	public virtual float ActionCharAnimRun (ActionCharAnim action)
	{
		return 0f;
	}

	public virtual void ActionCharAnimSkip (ActionCharAnim action)
	{
		ActionCharAnimRun (action);
	}
	
	public virtual bool ActionCharHoldPossible ()
	{
		return false;
	}

	public virtual void ActionSpeechGUI (ActionSpeech action)
	{
		#if UNITY_EDITOR
		#endif
	}
	
	public virtual void ActionSpeechRun (ActionSpeech action)
	{ }

	public virtual void ActionSpeechSkip (ActionSpeech action)
	{
		ActionSpeechRun (action);
	}

	public virtual void ActionAnimGUI (ActionAnim action, List<ActionParameter> parameters)
	{
		#if UNITY_EDITOR
		#endif
	}

	public virtual string ActionAnimLabel (ActionAnim action)
	{
		return "";
	}

	public virtual void ActionAnimAssignValues (ActionAnim action, List<ActionParameter> parameters)
	{ }
	
	public virtual float ActionAnimRun (ActionAnim action)
	{
		return 0f;
	}

	public virtual void ActionAnimSkip (ActionAnim action)
	{
		ActionAnimRun (action);
	}

	public virtual void ActionCharRenderGUI (ActionCharRender action)
	{ }

	public virtual float ActionCharRenderRun (ActionCharRender action)
	{
		return 0f;
	}

	public virtual void PlayIdle ()
	{ }
	
	public virtual void PlayWalk ()
	{ }

	public virtual void PlayRun ()
	{ }
	
	public virtual void PlayTalk ()
	{ }

	public virtual void PlayVertical ()
	{ }

	public virtual void PlayJump ()
	{ 
		PlayIdle ();
	}

	public virtual void PlayTurnLeft ()
	{
		PlayIdle ();
	}
	
	public virtual void PlayTurnRight ()
	{
		PlayIdle ();
	}

	public virtual void TurnHead (Vector2 angles)
	{ }

}
