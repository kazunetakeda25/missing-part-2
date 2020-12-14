using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace AC
{
	
	public class ActiveInputsWindow : EditorWindow
	{
		
		private SettingsManager settingsManager;
		
		[MenuItem ("Adventure Creator/Editors/Active Inputs Editor", false, 0)]
		public static void Init ()
		{
			ActiveInputsWindow window = (ActiveInputsWindow) EditorWindow.GetWindow (typeof (ActiveInputsWindow));
			window.title = "Active Inputs";
			window.position = new Rect (300, 200, 450, 400);
		}
		
		
		private void OnEnable ()
		{
			if (AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager)
			{
				settingsManager = AdvGame.GetReferences ().settingsManager;
			}
		}
		
		
		private void OnGUI ()
		{
			if (settingsManager == null)
			{
				EditorGUILayout.HelpBox ("A Settings Manager must be assigned before this window can display correctly.", MessageType.Warning);
				return;
			}

			settingsManager.activeInputs = ShowActiveInputsGUI (settingsManager.activeInputs);
			
			if (GUI.changed)
			{
				EditorUtility.SetDirty (settingsManager);
			}
		}
		
		
		private List<ActiveInput> ShowActiveInputsGUI (List<ActiveInput> activeInputs)
		{
			int numOptions = activeInputs.Count;
			numOptions = EditorGUILayout.IntField ("Number of active inputs:", activeInputs.Count);
			if (activeInputs.Count < 0)
			{
				activeInputs = new List<ActiveInput>();
				numOptions = 0;
			}

			if (numOptions < 0)
			{
				numOptions = 0;
			}

			if (numOptions < activeInputs.Count)
			{
				activeInputs.RemoveRange (numOptions, activeInputs.Count - numOptions);
			}
			else if (numOptions > activeInputs.Count)
			{
				if (numOptions > activeInputs.Capacity)
				{
					activeInputs.Capacity = numOptions;
				}
				for (int i=activeInputs.Count; i<numOptions; i++)
				{
					activeInputs.Add (new ActiveInput ());
				}
			}
			
			for (int i=0; i<activeInputs.Count; i++)
			{
				EditorGUILayout.LabelField ("Input #" + i.ToString (), EditorStyles.boldLabel);
				activeInputs[i].inputName = EditorGUILayout.TextField ("Input button:", activeInputs[i].inputName);
				activeInputs[i].gameState = (GameState) EditorGUILayout.EnumPopup ("Available when game is:", activeInputs[i].gameState);
				activeInputs[i].actionListAsset = ActionListAssetMenu.AssetGUI ("ActionList when triggered:", activeInputs[i].actionListAsset);
			}
			
			return activeInputs;
		}
		
		
	}
	
}
