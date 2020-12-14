using UnityEngine;
using System.Collections;
using UnityEditor;
using AC;

[CustomEditor (typeof (LipSyncTexture))]
public class LipSyncTextureEditor : Editor
{
	
	private LipSyncTexture _target;
	
	
	private void OnEnable ()
	{
		_target = (LipSyncTexture) target;
	}
	
	
	public override void OnInspectorGUI ()
	{
		if (_target.GetComponent <Char>() == null)
		{
			EditorGUILayout.HelpBox ("This component must be placed alongside either the NPC or Player component.", MessageType.Warning);
		}

		_target.skinnedMeshRenderer = (SkinnedMeshRenderer) EditorGUILayout.ObjectField ("Skinned Mesh Renderer:", _target.skinnedMeshRenderer, typeof (SkinnedMeshRenderer), true);
		_target.materialIndex = EditorGUILayout.IntField ("Material to affect (index):", _target.materialIndex);
		_target.propertyName = EditorGUILayout.TextField ("Texture property name:", _target.propertyName);

		_target.LimitTextureArray ();

		for (int i=0; i<_target.textures.Count; i++)
		{
			_target.textures[i] = (Texture2D) EditorGUILayout.ObjectField ("Texture for phoneme #" + i.ToString () + ":", _target.textures[i], typeof (Texture2D), false);
		}

		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
	}

}
