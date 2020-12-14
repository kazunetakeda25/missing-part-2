using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor (typeof (GameCamera2D))]

public class GameCamera2DEditor : Editor
{

	public override void OnInspectorGUI ()
	{
		GameCamera2D _target = (GameCamera2D) target;

		EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Horizontal movement", EditorStyles.boldLabel);
		
			_target.lockHorizontal = EditorGUILayout.Toggle ("Lock?", _target.lockHorizontal);
			if (!_target.GetComponent <Camera>().orthographic || !_target.lockHorizontal)
			{
				_target.afterOffset.x = EditorGUILayout.FloatField ("Offset:", _target.afterOffset.x);
			}
		
			if (!_target.lockHorizontal)
			{
				_target.freedom.x = EditorGUILayout.FloatField ("Track freedom:",_target.freedom.x);
				_target.directionInfluence.x = EditorGUILayout.FloatField ("Target direction fac.:", _target.directionInfluence.x);
				_target.limitHorizontal = EditorGUILayout.BeginToggleGroup ("Constrain?", _target.limitHorizontal);
			
				EditorGUILayout.BeginVertical ("Button");
					_target.constrainHorizontal[0] = EditorGUILayout.FloatField ("Minimum:", _target.constrainHorizontal[0]);
					_target.constrainHorizontal[1] = EditorGUILayout.FloatField ("Maximum:", _target.constrainHorizontal[1]);
				EditorGUILayout.EndVertical ();
			
				EditorGUILayout.EndToggleGroup ();
			}
		EditorGUILayout.EndVertical ();
		
		EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Vertical movement", EditorStyles.boldLabel);
		
			_target.lockVertical = EditorGUILayout.Toggle ("Lock?", _target.lockVertical);
			if (!_target.GetComponent <Camera>().orthographic || !_target.lockVertical)
			{
				_target.afterOffset.y = EditorGUILayout.FloatField ("Offset:", _target.afterOffset.y);
			}

			if (!_target.lockVertical)
			{
				_target.freedom.y = EditorGUILayout.FloatField ("Track freedom:",_target.freedom.y);
				_target.directionInfluence.y = EditorGUILayout.FloatField ("Target direction fac.:", _target.directionInfluence.y);
				_target.limitVertical = EditorGUILayout.BeginToggleGroup ("Constrain?", _target.limitVertical);
			
				EditorGUILayout.BeginVertical ("Button");
					_target.constrainVertical[0] = EditorGUILayout.FloatField ("Minimum:", _target.constrainVertical[0]);
					_target.constrainVertical[1] = EditorGUILayout.FloatField ("Maximum:", _target.constrainVertical[1]);
				EditorGUILayout.EndVertical ();
			
				EditorGUILayout.EndToggleGroup ();
			}
		EditorGUILayout.EndVertical ();
		
		if (!_target.lockHorizontal || !_target.lockVertical)
		{
			EditorGUILayout.BeginVertical ("Button");
				EditorGUILayout.LabelField ("Target object to control camera movement", EditorStyles.boldLabel);
				
				_target.targetIsPlayer = EditorGUILayout.Toggle ("Target is player?", _target.targetIsPlayer);
				
				if (!_target.targetIsPlayer)
				{
					_target.target = (Transform) EditorGUILayout.ObjectField ("Target:", _target.target, typeof(Transform), true);
				}
				
				_target.dampSpeed = EditorGUILayout.FloatField ("Follow speed", _target.dampSpeed);
			EditorGUILayout.EndVertical ();
		}
		
		if (!_target.IsCorrectRotation ())
		{
			if (GUILayout.Button ("Set correct rotation"))
			{
				Undo.RecordObject (_target, "Clear " + _target.name + " rotation");
				_target.SetCorrectRotation ();
			}
		}

		if (!Application.isPlaying)
		{
			_target.GetComponent <Camera>().ResetProjectionMatrix ();
			if (!_target.GetComponent <Camera>().orthographic)
			{
				_target.SetCameraComponent ();
				_target.SnapToOffset ();
			}
		}

		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
	}
}
