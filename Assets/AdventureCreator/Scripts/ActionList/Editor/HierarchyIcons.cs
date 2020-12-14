using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace AC
{

	[InitializeOnLoad]
	public class HierarchyIcons
	{

		private static List<int> actionListIDs;
		private static List<int> rememberIDs;


		static HierarchyIcons ()
		{
			EditorApplication.update += UpdateCB;
			EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemCB;
		}


		private static void UpdateCB ()
		{
			actionListIDs = new List<int>();
			foreach (ActionList actionList in Object.FindObjectsOfType (typeof(ActionList)) as ActionList[])
			{
				actionListIDs.Add (actionList.gameObject.GetInstanceID ());
			}

			rememberIDs = new List<int>();
			foreach (ConstantID constantID in Object.FindObjectsOfType (typeof(ConstantID)) as ConstantID[])
			{
				rememberIDs.Add (constantID.gameObject.GetInstanceID());
			}
		}


		private static void HierarchyItemCB (int instanceID, Rect selectionRect)
		{
			if (AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager && !AdvGame.GetReferences ().settingsManager.showHierarchyIcons)
			{
				return;
			}

			// place the icoon to the right of the list:
			Rect r = new Rect (selectionRect);
			r.x = r.width - 20;
			r.width = 18;

			if (actionListIDs != null && actionListIDs.Contains (instanceID))
			{
				foreach (ActionList actionList in Object.FindObjectsOfType (typeof(ActionList)) as ActionList[])
				{
					if (actionList.gameObject.GetInstanceID () == instanceID)
					{
						if (GUI.Button (r, "", ActionListEditorWindow.nodeSkin.customStyles[13]))
						{
							ActionListEditorWindow.Init (actionList);
							break;
						}
					}
				}
			}

			r.x -= 40;
			if (rememberIDs != null && rememberIDs.Contains (instanceID))
			{
				foreach (ConstantID constantID in Object.FindObjectsOfType (typeof(ConstantID)) as ConstantID[])
				{
					if (constantID.gameObject.GetInstanceID () == instanceID)
					{
						GUI.Label (r, "", ActionListEditorWindow.nodeSkin.customStyles[14]);
						break;
					}
				}
			}
		}
	}

}