using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor(typeof(GameCameraAnimated))]

public class GameCameraAnimatedEditor : Editor
{
	
	public override void OnInspectorGUI()
	{
		GameCameraAnimated _target = (GameCameraAnimated) target;

		if (_target.GetComponent <Animation>() == null)
		{
			EditorGUILayout.HelpBox ("This camera type requires an Animation component.", MessageType.Warning);
		}

		EditorGUILayout.BeginVertical ("Button");
			_target.animatedCameraType = (AnimatedCameraType) EditorGUILayout.EnumPopup ("Animated camera type:", _target.animatedCameraType);
			_target.clip = (AnimationClip) EditorGUILayout.ObjectField ("Animation clip:", _target.clip, typeof (AnimationClip), false);

			if (_target.animatedCameraType == AnimatedCameraType.PlayWhenActive)
			{
				_target.loopClip = EditorGUILayout.Toggle ("Loop animation?", _target.loopClip);
				_target.playOnStart = EditorGUILayout.Toggle ("Play on start?", _target.playOnStart);
			}
			else if (_target.animatedCameraType == AnimatedCameraType.SyncWithTargetMovement)
			{
				_target.pathToFollow = (Paths) EditorGUILayout.ObjectField ("Path to follow:", _target.pathToFollow, typeof (Paths), true);
				_target.targetIsPlayer = EditorGUILayout.Toggle ("Target is player?", _target.targetIsPlayer);
				
				if (!_target.targetIsPlayer)
				{
					_target.target = (Transform) EditorGUILayout.ObjectField ("Target:", _target.target, typeof(Transform), true);
				}
			}
		EditorGUILayout.EndVertical ();

		if (_target.animatedCameraType == AnimatedCameraType.SyncWithTargetMovement)
		{
			EditorGUILayout.Space ();
			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Cursor influence", EditorStyles.boldLabel);
			_target.followCursor = EditorGUILayout.Toggle ("Follow cursor?", _target.followCursor);
			if (_target.followCursor)
			{
				_target.cursorInfluence = EditorGUILayout.Vector2Field ("Panning factor", _target.cursorInfluence);
			}
			EditorGUILayout.EndVertical ();
		}

		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
	}
}
