using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor(typeof(AC_Trigger))]

[System.Serializable]
public class AC_TriggerEditor : CutsceneEditor
{

	private string[] Options = { "On enter", "Continuous", "On exit" };


	public override void OnInspectorGUI()
    {
		AC_Trigger _target = (AC_Trigger) target;
   
		EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Trigger properties", EditorStyles.boldLabel);
			_target.source = (ActionListSource) EditorGUILayout.EnumPopup ("Actions source:", _target.source);
			if (_target.source == ActionListSource.AssetFile)
			{
				_target.assetFile = (ActionListAsset) EditorGUILayout.ObjectField ("ActionList asset:", _target.assetFile, typeof (ActionListAsset), false);
			}
			_target.actionListType = (ActionListType) EditorGUILayout.EnumPopup ("When running:", _target.actionListType);
			if (_target.actionListType == ActionListType.PauseGameplay)
			{
				_target.isSkippable = EditorGUILayout.Toggle ("Is skippable?", _target.isSkippable);
			}
			_target.triggerType = EditorGUILayout.Popup ("Trigger type:", _target.triggerType, Options);
			_target.cancelInteractions = EditorGUILayout.Toggle ("Cancels interactions?", _target.cancelInteractions);
			_target.useParameters = EditorGUILayout.Toggle ("Set collider as parameter?", _target.useParameters);
			
			EditorGUILayout.Space ();
			_target.detects = (TriggerDetects) EditorGUILayout.EnumPopup ("Trigger detects:", _target.detects);
			if (_target.detects == TriggerDetects.AnyObjectWithComponent)
			{
				_target.detectComponent = EditorGUILayout.TextField ("Component name:", _target.detectComponent);
			}
			else if (_target.detects == TriggerDetects.SetObject)
			{
				_target.obToDetect = (GameObject) EditorGUILayout.ObjectField ("Object to detect:", _target.obToDetect, typeof (GameObject), true);
			}
		EditorGUILayout.EndVertical ();

		DrawSharedElements ();

		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
    }

}
