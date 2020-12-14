using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor (typeof (AC.Char))]
public class CharEditor : Editor
{

	public override void OnInspectorGUI ()
	{
		EditorGUILayout.HelpBox ("This component should not be used directly - use Player or NPC instead.", MessageType.Warning);
	}


	protected void SharedGUIOne (AC.Char _target)
	{
		if (_target.animEngine == null || !_target.animEngine.ToString ().Contains (_target.animationEngine.ToString ()))
		{
			_target.ResetAnimationEngine ();
		}

		EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Animation settings:", EditorStyles.boldLabel);
			_target.animationEngine = (AnimationEngine) EditorGUILayout.EnumPopup ("Animation engine:", _target.animationEngine);
			if (_target.animationEngine == AnimationEngine.Sprites2DToolkit && !tk2DIntegration.IsDefinePresent ())
			{
				EditorGUILayout.HelpBox ("The 'tk2DIsPresent' preprocessor define must be declared in the\ntk2DIntegration.cs script. Please open it and follow instructions.", MessageType.Warning);
			}
			if (_target.animationEngine == AnimationEngine.Custom)
			{
				_target.customAnimationClass = EditorGUILayout.TextField ("Script name:", _target.customAnimationClass);
				_target.motionControl = (MotionControl) EditorGUILayout.EnumPopup ("Motion control:", _target.motionControl);
			}
		EditorGUILayout.EndVertical ();

		_target.animEngine.CharSettingsGUI ();

		EditorGUILayout.BeginVertical ("Button");
		EditorGUILayout.LabelField ("Movement settings:", EditorStyles.boldLabel);

		if (_target.GetMotionControl () == MotionControl.Automatic)
		{
 			_target.walkSpeedScale = EditorGUILayout.FloatField ("Walk speed scale:", _target.walkSpeedScale);
			_target.runSpeedScale = EditorGUILayout.FloatField ("Run speed scale:", _target.runSpeedScale);
			_target.acceleration = EditorGUILayout.FloatField ("Acceleration:", _target.acceleration);
			_target.deceleration = EditorGUILayout.FloatField ("Deceleration:", _target.deceleration);
		}
		if (_target.GetMotionControl () != MotionControl.Manual)
		{
			_target.turnSpeed = EditorGUILayout.FloatField ("Turn speed:", _target.turnSpeed);
		}
		_target.turnBeforeWalking = EditorGUILayout.Toggle ("Turn before walking?", _target.turnBeforeWalking);
		EditorGUILayout.EndVertical ();
	}


	protected void SharedGUITwo (AC.Char _target)
	{
		EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Rigidbody settings:", EditorStyles.boldLabel);
			_target.ignoreGravity = EditorGUILayout.Toggle ("Ignore gravity?", _target.ignoreGravity);
			_target.freezeRigidbodyWhenIdle = EditorGUILayout.Toggle ("Freeze when Idle?", _target.freezeRigidbodyWhenIdle);
		EditorGUILayout.EndVertical ();
		
		
		EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Audio clips:", EditorStyles.boldLabel);
		
			_target.walkSound = (AudioClip) EditorGUILayout.ObjectField ("Walk sound:", _target.walkSound, typeof (AudioClip), false);
			_target.runSound = (AudioClip) EditorGUILayout.ObjectField ("Run sound:", _target.runSound, typeof (AudioClip), false);
			_target.soundChild = (Sound) EditorGUILayout.ObjectField ("Sound child:", _target.soundChild, typeof (Sound), true);
		EditorGUILayout.EndVertical ();
		
		EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Dialogue settings:", EditorStyles.boldLabel);

			EditorGUILayout.LabelField ("Portrait graphic:");
			_target.portraitIcon.ShowGUI (false);
			_target.speechColor = EditorGUILayout.ColorField ("Speech text colour:", _target.speechColor);
			_target.speechLabel = EditorGUILayout.TextField ("Speaker label:", _target.speechLabel);
		EditorGUILayout.EndVertical ();
	}

}
