using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor(typeof(GameCameraThirdPerson))]

public class GameCameraThirdPersonEditor : Editor
{
	
	public override void OnInspectorGUI()
	{
		GameCameraThirdPerson _target = (GameCameraThirdPerson) target;

		// Target
		EditorGUILayout.BeginVertical ("Button");
		EditorGUILayout.LabelField ("Target", EditorStyles.boldLabel);
		_target.targetIsPlayer = EditorGUILayout.Toggle ("Is player?", _target.targetIsPlayer);
		if (!_target.targetIsPlayer)
		{
			_target.target = (Transform) EditorGUILayout.ObjectField ("Target transform:", _target.target, typeof (Transform), true);
		}
		_target.verticalOffset = EditorGUILayout.FloatField ("Vertical offset:", _target.verticalOffset);
		_target.horizontalOffset = EditorGUILayout.FloatField ("Horizontal offset:", _target.horizontalOffset);
		EditorGUILayout.EndVertical ();

		// Distance
		EditorGUILayout.BeginVertical ("Button");
		EditorGUILayout.LabelField ("Distance", EditorStyles.boldLabel);
		_target.distance = EditorGUILayout.FloatField ("Distance from target:", _target.distance);
		_target.allowMouseWheelZooming = EditorGUILayout.Toggle ("Mousewheel zooming?", _target.allowMouseWheelZooming);
		_target.detectCollisions = EditorGUILayout.Toggle ("Detect wall collisions?", _target.detectCollisions);

		if (_target.allowMouseWheelZooming || _target.detectCollisions)
		{
			_target.minDistance = EditorGUILayout.FloatField ("Mininum distance:", _target.minDistance);
		}
		if (_target.allowMouseWheelZooming)
		{
			_target.maxDistance = EditorGUILayout.FloatField ("Maximum distance:", _target.maxDistance);
		}
		EditorGUILayout.EndVertical ();

		// Spin
		EditorGUILayout.BeginVertical ("Button");
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("Spin rotation", EditorStyles.boldLabel, GUILayout.Width (130f));
		_target.spinLock = (GameCameraThirdPerson.RotationLock) EditorGUILayout.EnumPopup (_target.spinLock);
		EditorGUILayout.EndHorizontal ();
		if (_target.spinLock != GameCameraThirdPerson.RotationLock.Locked)
		{
			_target.spinSpeed = EditorGUILayout.FloatField ("Speed:", _target.spinSpeed);
			_target.spinAccleration = EditorGUILayout.FloatField ("Acceleration:", _target.spinAccleration);
			_target.spinDeceleration = EditorGUILayout.FloatField ("Deceleration:", _target.spinDeceleration);
			_target.isDragControlled = EditorGUILayout.Toggle ("Drag-controlled?", _target.isDragControlled);
			if (!_target.isDragControlled)
			{
				_target.spinAxis = EditorGUILayout.TextField ("Input axis:", _target.spinAxis);
			}
			_target.invertSpin = EditorGUILayout.Toggle ("Invert?", _target.invertSpin);
			_target.toggleCursor = EditorGUILayout.Toggle ("Cursor must be locked?", _target.toggleCursor);
			_target.resetSpinWhenSwitch = EditorGUILayout.Toggle ("Reset angle on switch?", _target.resetSpinWhenSwitch);

			if (_target.spinLock == GameCameraThirdPerson.RotationLock.Limited)
			{
				_target.maxSpin = EditorGUILayout.FloatField ("Maximum angle:", _target.maxSpin);
			}
		}
		else
		{
			_target.alwaysBehind = EditorGUILayout.Toggle ("Always behind target?", _target.alwaysBehind);
			if (_target.alwaysBehind)
			{
				_target.spinAccleration = EditorGUILayout.FloatField ("Acceleration:", _target.spinAccleration);
				_target.spinOffset = EditorGUILayout.FloatField ("Offset angle:", _target.spinOffset);
			}
		}
		EditorGUILayout.EndVertical ();

		// Pitch
		EditorGUILayout.BeginVertical ("Button");
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("Pitch rotation", EditorStyles.boldLabel, GUILayout.Width (130f));
		_target.pitchLock = (GameCameraThirdPerson.RotationLock) EditorGUILayout.EnumPopup (_target.pitchLock);
		EditorGUILayout.EndHorizontal ();
		if (_target.pitchLock != GameCameraThirdPerson.RotationLock.Locked)
		{
			_target.pitchSpeed = EditorGUILayout.FloatField ("Speed:", _target.pitchSpeed);
			_target.pitchAccleration = EditorGUILayout.FloatField ("Acceleration:", _target.pitchAccleration);
			_target.pitchDeceleration = EditorGUILayout.FloatField ("Deceleration:", _target.pitchDeceleration);
			_target.isDragControlled = EditorGUILayout.Toggle ("Drag-controlled?", _target.isDragControlled);
			if (!_target.isDragControlled)
			{
				_target.pitchAxis = EditorGUILayout.TextField ("Input axis:", _target.pitchAxis);
			}
			_target.invertPitch = EditorGUILayout.Toggle ("Invert?", _target.invertPitch);
			_target.resetPitchWhenSwitch = EditorGUILayout.Toggle ("Reset angle on switch?", _target.resetPitchWhenSwitch);

			if (_target.pitchLock == GameCameraThirdPerson.RotationLock.Limited)
			{
				_target.maxPitch = EditorGUILayout.FloatField ("Maximum angle:", _target.maxPitch);
			}
		}
		else
		{
			_target.maxPitch = EditorGUILayout.FloatField ("Fixed angle:", _target.maxPitch);
		}
		EditorGUILayout.EndVertical ();

		DisplayInputList (_target);

		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
	}


	private void DisplayInputList (GameCameraThirdPerson _target)
	{
		string result = "";
		
		if (_target.allowMouseWheelZooming)
		{
			result += "\n";
			result += "- Mouse ScrollWheel";
		}
		if (!_target.isDragControlled)
		{
			if (_target.spinLock == GameCameraThirdPerson.RotationLock.Free)
			{
				result += "\n";
				result += "- " + _target.spinAxis;
			}
			if (_target.pitchLock == GameCameraThirdPerson.RotationLock.Free)
			{
				result += "\n";
				result += "- " + _target.pitchAxis;
			}
		}
		if (_target.toggleCursor)
		{
			result += "\n";
			result += "- ToggleCursor";
		}

		if (result != "")
		{
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Required inputs:", EditorStyles.boldLabel);
			EditorGUILayout.HelpBox ("The following input axes are available for the chosen settings:" + result, MessageType.Info);
		}
	}

}
