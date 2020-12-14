using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using AC;

[CustomEditor(typeof(Paths))]
public class PathsEditor : Editor
{
	
	private static GUIContent
		insertContent = new GUIContent("+", "Insert node"),
		deleteContent = new GUIContent("-", "Delete node");

	private static GUILayoutOption
		buttonWidth = GUILayout.MaxWidth(20f);
	
	
	public override void OnInspectorGUI()
	{
		Paths _target = (Paths) target;

		if (_target.GetComponent <AC.Char>())
		{
			return;
		}

		int numNodes = _target.nodes.Count;
		if (numNodes < 1)
		{
			numNodes = 1;
			_target.nodes = ResizeList (_target.nodes, numNodes);
		}

		EditorGUILayout.BeginVertical ("Button");
		EditorGUILayout.LabelField ("Path properties", EditorStyles.boldLabel);
		_target.nodePause = EditorGUILayout.FloatField ("Wait time (s):", _target.nodePause);
 		_target.pathSpeed = (PathSpeed) EditorGUILayout.EnumPopup("Walk or run:", (PathSpeed) _target.pathSpeed);
		_target.pathType = (AC_PathType) EditorGUILayout.EnumPopup ("Path type:", (AC_PathType) _target.pathType);
		if (_target.pathType == AC_PathType.Loop)
		{
			_target.teleportToStart = EditorGUILayout.Toggle ("Teleports when looping?", _target.teleportToStart);
		}
		_target.affectY = EditorGUILayout.Toggle ("Override gravity?", _target.affectY);
		_target.commandSource = (ActionListSource) EditorGUILayout.EnumPopup ("Node commands source:", _target.commandSource);
		EditorGUILayout.EndVertical ();
		EditorGUILayout.Space ();

		// List nodes
		ResetCommandList (_target);
		for (int i=1; i<_target.nodes.Count; i++)
		{
			EditorGUILayout.BeginVertical("Button");
				EditorGUILayout.BeginHorizontal ();
					_target.nodes[i] = EditorGUILayout.Vector3Field ("Node " + i + ": ", _target.nodes[i]);
					
					if (GUILayout.Button (insertContent, EditorStyles.miniButtonLeft, buttonWidth))
					{
						Undo.RecordObject (_target, "Add path node");
						Vector3 newNodePosition;
						newNodePosition = _target.nodes[i] + new Vector3 (1.0f, 0f, 0f);
						_target.nodes.Insert (i+1, newNodePosition);
						numNodes += 1;
						ResetCommandList (_target);
					}
					if (GUILayout.Button (deleteContent, EditorStyles.miniButtonRight, buttonWidth))
					{
						Undo.RecordObject (_target, "Delete path node");
						_target.nodes.RemoveAt (i);
						numNodes -= 1;
						ResetCommandList (_target);
					}
				EditorGUILayout.EndHorizontal ();
				if (_target.nodeCommands.Count > i)
				{
					if (_target.commandSource == ActionListSource.InScene)
					{
						_target.nodeCommands[i].cutscene = (Cutscene) EditorGUILayout.ObjectField ("Cutscene on reach:", _target.nodeCommands[i].cutscene, typeof (Cutscene), true);

						if (_target.nodeCommands[i].cutscene != null && _target.nodeCommands[i].cutscene.useParameters)
						{
							_target.nodeCommands[i].parameterID = SetParametersGUI (_target.nodeCommands[i].cutscene.parameters, _target.nodeCommands[i].parameterID);
						}
					}
					else
					{
						_target.nodeCommands[i].actionListAsset = (ActionListAsset) EditorGUILayout.ObjectField ("ActionList asset on reach:", _target.nodeCommands[i].actionListAsset, typeof (ActionListAsset), false);

						if (_target.nodeCommands[i].actionListAsset != null && _target.nodeCommands[i].actionListAsset.useParameters)
						{
							_target.nodeCommands[i].parameterID = SetParametersGUI (_target.nodeCommands[i].actionListAsset.parameters, _target.nodeCommands[i].parameterID);
						}
					}
				}
			EditorGUILayout.EndVertical();
		}

		if (numNodes == 1 && GUILayout.Button("Add node"))
		{
			Undo.RecordObject (_target, "Add path node");
			numNodes += 1;
		}
		
		_target.nodes[0] = _target.transform.position;
		_target.nodes = ResizeList (_target.nodes, numNodes);

		if (GUI.changed)
		{
			EditorUtility.SetDirty ((Paths) target);
		}
	}
	
	
	private void OnSceneGUI ()
	{
		Paths _target = (Paths) target;
		
		// Go through each element in the nodesArray array and display its stuff
		for (int i=0; i<_target.nodes.Count; i++)
		{
			if (i>0 && !Application.isPlaying)
			{
				_target.nodes[i] = Handles.PositionHandle (_target.nodes[i], Quaternion.identity);
			}
			Handles.Label (_target.nodes[i], i.ToString());
		}

		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
	}

	
	private List<Vector3> ResizeList (List<Vector3> list, int listSize)
	{
		if (list.Count < listSize)
		{
			// Increase size of list
			while (list.Count < listSize)
			{
				Vector3 newNodePosition;
				if (list.Count > 0)
				{
					newNodePosition = list[list.Count-1] + new Vector3 (1.0f, 0f, 0f);
				}
				else
				{
					newNodePosition = Vector3.zero;
				}
				list.Add (newNodePosition);
			}
		}
		else if (list.Count > listSize)
		{
			// Decrease size of list
			while (list.Count > listSize)
			{
				list.RemoveAt (list.Count - 1);
			}
		}
		return (list);
	}


	private int SetParametersGUI (List<ActionParameter> externalParameters, int parameterID)
	{
		if (externalParameters == null || externalParameters.Count == 0)
		{
			return -1;
		}

		List<string> labelList = new List<string>();
		labelList.Add (" (None)");
		foreach (ActionParameter paramater in externalParameters)
		{
			labelList.Add (paramater.label);
		}

		parameterID ++;
		parameterID = EditorGUILayout.Popup ("Character parameter:", parameterID, labelList.ToArray ());
		parameterID --;

		return parameterID;
	}


	private void ResetCommandList (Paths _target)
	{
		int numNodes = _target.nodes.Count;
		int numCommands = _target.nodeCommands.Count;

		if (numNodes < numCommands)
		{
			_target.nodeCommands.RemoveRange (numNodes, numCommands - numNodes);
		}
		else if (numNodes > numCommands)
		{
			if (numNodes > _target.nodeCommands.Capacity)
			{
				_target.nodeCommands.Capacity = numNodes;
			}
			for (int i=numCommands; i<numNodes; i++)
			{
				_target.nodeCommands.Add (new NodeCommand ());
			}
		}
	}

}
