using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor(typeof(Cutscene))]

[System.Serializable]
public class CutsceneEditor : ActionListEditor
{

	public override void OnInspectorGUI ()
	{
		Cutscene _target = (Cutscene) target;

		EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Cutscene properties", EditorStyles.boldLabel);
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
			_target.triggerTime = EditorGUILayout.Slider ("Start delay (s):", _target.triggerTime, 0f, 10f);
			_target.autosaveAfter = EditorGUILayout.Toggle ("Auto-save after?", _target.autosaveAfter);
			if (_target.source == ActionListSource.InScene)
			{
				_target.useParameters = EditorGUILayout.Toggle ("Use parameters?", _target.useParameters);
			}
		EditorGUILayout.EndVertical ();

		DrawSharedElements ();

		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
    }

}