/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionListAsset.cs"
 * 
 *	This script stores a list of Actions in an asset file.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionListAsset : ScriptableObject
	{
		public bool isSkippable = true;
		public ActionListType actionListType = ActionListType.PauseGameplay;
		public bool useParameters = false;
		public bool unfreezePauseMenus = true;
		public List<ActionParameter> parameters = new List<ActionParameter>();

		public List<AC.Action> actions = new List<AC.Action>();


		public bool IsSkippable ()
		{
			if (isSkippable && actionListType == ActionListType.PauseGameplay)
			{
				return true;
			}
			return false;
		}

	}


	public class ActionListAssetMenu
	{

		#if UNITY_EDITOR
		[MenuItem ("Assets/Create/Adventure Creator/ActionList")]
		public static ActionListAsset CreateAsset ()
		{
			ScriptableObject t = CustomAssetUtility.CreateAsset <ActionListAsset> ("New ActionList");
			return (ActionListAsset) t;
		}


		public static ActionListAsset AssetGUI (string label, ActionListAsset actionListAsset)
		{
			EditorGUILayout.BeginHorizontal ();
			actionListAsset = (ActionListAsset) EditorGUILayout.ObjectField (label, actionListAsset, typeof (ActionListAsset), false);

			if (actionListAsset == null)
			{
				if (GUILayout.Button ("Create", GUILayout.MaxWidth (60f)))
				{
					actionListAsset = ActionListAssetMenu.CreateAsset ();
				}
			}

			EditorGUILayout.EndHorizontal ();
			return actionListAsset;
		}


		public static Cutscene CutsceneGUI (string label, Cutscene cutscene)
		{
			EditorGUILayout.BeginHorizontal ();
			cutscene = (Cutscene) EditorGUILayout.ObjectField (label, cutscene, typeof (Cutscene), true);

			if (cutscene == null)
			{
				if (GUILayout.Button ("Create", GUILayout.MaxWidth (60f)))
				{
					cutscene = SceneManager.AddPrefab ("Logic", "Cutscene", true, false, true).GetComponent <Cutscene>();
					cutscene.Initialise ();
				}
			}

			EditorGUILayout.EndHorizontal ();
			return cutscene;
		}

		#endif


	}

}