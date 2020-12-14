using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor (typeof (SortingMap))]

[System.Serializable]
public class SortingMapEditor : Editor
{
	
	private static GUIContent
		insertContent = new GUIContent("+", "Insert node"),
		deleteContent = new GUIContent("-", "Delete node");
	
	private static GUILayoutOption
		labelWidth = GUILayout.MaxWidth (40f),
		buttonWidth = GUILayout.MaxWidth (20f);


	private void OnEnable ()
	{
		if (AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager && AdvGame.GetReferences ().settingsManager.IsUnity2D ())
		{
			SortingMap _target = (SortingMap) target;
			_target.transform.forward = Vector3.down;
		}
	}

	
	public override void OnInspectorGUI()
	{
		SortingMap _target = (SortingMap) target;

		EditorGUILayout.BeginVertical ("Button");
			_target.mapType = (SortingMapType) EditorGUILayout.EnumPopup ("Affect sprite's:", _target.mapType);
			_target.affectScale = EditorGUILayout.Toggle ("Affect Character scale?", _target.affectScale);
			if (_target.affectScale)
			{
				_target.affectSpeed = EditorGUILayout.Toggle ("Affect Character speed?", _target.affectSpeed);
				_target.originScale = EditorGUILayout.IntField ("Start scale (%):", _target.originScale);
			}
		EditorGUILayout.EndVertical ();

		EditorGUILayout.Space ();

		foreach (SortingArea area in _target.sortingAreas)
		{
			int i = _target.sortingAreas.IndexOf (area);

			EditorGUILayout.BeginVertical("Button");
			EditorGUILayout.BeginHorizontal ();

			area.color = EditorGUILayout.ColorField (area.color);

			EditorGUILayout.LabelField ("Position:", GUILayout.Width (50f));
			area.z = EditorGUILayout.FloatField (area.z, GUILayout.Width (80f));

			if (_target.mapType == SortingMapType.OrderInLayer)
			{
				EditorGUILayout.LabelField ("Order:", labelWidth);
				area.order = EditorGUILayout.IntField (area.order);
			}
			else if (_target.mapType == SortingMapType.SortingLayer)
			{
				EditorGUILayout.LabelField ("Layer:", labelWidth);
				area.layer = EditorGUILayout.TextField (area.layer);
			}

			if (GUILayout.Button (insertContent, EditorStyles.miniButtonLeft, buttonWidth))
			{
				Undo.RecordObject (_target, "Add area");
				if (i < _target.sortingAreas.Count - 1)
				{
					_target.sortingAreas.Insert (i+1, new SortingArea (area, _target.sortingAreas[i+1]));
				}
				else
				{
					_target.sortingAreas.Insert (i+1, new SortingArea (area));
				}
				break;
			}
			if (GUILayout.Button (deleteContent, EditorStyles.miniButtonRight, buttonWidth))
			{
				Undo.RecordObject (_target, "Delete area");
				_target.sortingAreas.Remove (area);
				break;
			}

			EditorGUILayout.EndHorizontal ();

			if (_target.affectScale)
			{
				area.scale = EditorGUILayout.IntField ("End scale (%):", area.scale);
			}

			EditorGUILayout.EndVertical();
		}

		if (GUILayout.Button ("Add area"))
		{
			Undo.RecordObject (_target, "Add area");

			if (_target.sortingAreas.Count > 0)
			{
				SortingArea lastArea = _target.sortingAreas [_target.sortingAreas.Count - 1];
				_target.sortingAreas.Add (new SortingArea (lastArea));
			}
			else
			{
				_target.sortingAreas.Add (new SortingArea (_target.transform.position.z + 1f, 1));
			}
		}

		EditorGUILayout.Space ();

		if (AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager && AdvGame.GetReferences ().settingsManager.IsTopDown ())
		{}
		else if (AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager && AdvGame.GetReferences ().settingsManager.IsUnity2D ())
		{}
		else
		{
			if (GUILayout.Button ("Face active camera"))
			{
				Undo.RecordObject (_target, "Face active camera");
				Vector3 forwardVector = Camera.main.transform.forward;
				_target.transform.forward = -forwardVector;
				EditorUtility.SetDirty (_target);
			}
		}

		if (_target.affectScale && _target.sortingAreas.Count > 1)
		{
			if (GUILayout.Button ("Interpolate in-between scales"))
			{
				Undo.RecordObject (_target, "Interpolate scales");
				_target.SetInBetweenScales ();
				EditorUtility.SetDirty (_target);
			}
		}
		
		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
	}


	private void OnSceneGUI ()
	{
		SortingMap _target = (SortingMap) target;
		
		for (int i=0; i<_target.sortingAreas.Count; i++)
		{
			Vector3 newPosition = _target.GetAreaPosition (i);
			newPosition = Handles.PositionHandle (newPosition, Quaternion.identity);
			_target.sortingAreas [i].z = (newPosition - _target.transform.position).magnitude;

			Vector3 midPoint = _target.transform.position;
			if (i == 0)
			{
				midPoint += _target.transform.forward * _target.sortingAreas [i].z / 2f;
			}
			else
			{
				midPoint += _target.transform.forward * (_target.sortingAreas [i].z + _target.sortingAreas [i-1].z) / 2f;
			}

			Handles.Label (midPoint, _target.sortingAreas [i].order.ToString());
		}
		
		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
	}

	
}
