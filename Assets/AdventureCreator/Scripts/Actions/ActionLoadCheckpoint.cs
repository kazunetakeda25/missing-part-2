/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionParent.cs"
 * 
 *	This action is used to set and clear the parent of GameObjects.
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
	public class ActionLoadCheckpoint : Action
	{
		[System.Serializable]
		public class CheckpointMarker
		{
			public int checkpointVal = 0;
			public Cutscene cutsceneToSkipTo = null;
		}

		public int checkpointToLoadIfNoSavePresent = 0;
		
		public CheckpointMarker[] checkpointMarkers = new CheckpointMarker[5];

		public ActionLoadCheckpoint ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Custom;
			title = "Load Checkpoint";
			description = "Fastforward to a checkpoint position in the scene.";
		}
		
		override public float Run ()
		{
			if(MissingComplete.GameLoader.Instance.IsGameLoaded == false) {
				LoadDebugCheckpoint();
				return 0f;
			}

			LoadSavedCheckpoint();

			return 0f;
		}

		private void LoadSavedCheckpoint()
		{
			int checkpoint = MissingComplete.GameLoader.Instance.GetLoadedCheckpoint();

			Debug.Log("Loading Checkpoint: " + checkpoint);

			for(int i = 0; i < checkpointMarkers.Length; i++) {
				if(checkpointMarkers[i].checkpointVal == checkpoint) {
					checkpointMarkers[i].cutsceneToSkipTo.Interact(0, false);
				}
			}
		}

		private void LoadDebugCheckpoint()
		{
			Debug.Log("Skipping to: " + checkpointToLoadIfNoSavePresent);

			for(int i = 0; i < checkpointMarkers.Length; i++) {
				if(checkpointMarkers[i].checkpointVal == checkpointToLoadIfNoSavePresent) {
					checkpointMarkers[i].cutsceneToSkipTo.Interact(0, false);
				}
			}
		}
		
		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			checkpointToLoadIfNoSavePresent = (int)EditorGUILayout.IntField("Checkpoint To Load if No Save Present", checkpointToLoadIfNoSavePresent);

			if(checkpointMarkers.Length >= 1) {
				EditorGUILayout.BeginVertical();

				for(int i = 0; i < checkpointMarkers.Length; i++) {
					GameObject go = null;
					checkpointMarkers[i].checkpointVal = (int)EditorGUILayout.IntField("Checkpoint " + i.ToString() + ":", checkpointMarkers[i].checkpointVal);
					checkpointMarkers[i].cutsceneToSkipTo = (Cutscene) EditorGUILayout.ObjectField ("Object to affect:", checkpointMarkers[i].cutsceneToSkipTo, typeof(Cutscene), true);
				}

				EditorGUILayout.EndVertical();
			}

			AfterRunningOption ();
		}
		
		
		override public string SetLabel ()
		{
			string labelAdd = "";

			labelAdd += "Autocheckpoint set to: " + checkpointToLoadIfNoSavePresent;
			
			return labelAdd;
		}

		#endif

	}

}