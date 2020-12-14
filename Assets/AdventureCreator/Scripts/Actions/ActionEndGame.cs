/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionEndGame.cs"
 * 
 *	This Action will force the game to either
 *	restart an autosave, or quit.
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
	public class ActionEndGame : Action
	{
		
		public enum AC_EndGameType { QuitGame, LoadAutosave, ResetScene, RestartGame };
		public AC_EndGameType endGameType;
		public ChooseSceneBy chooseSceneBy = ChooseSceneBy.Number;
		public int sceneNumber;
		public string sceneName;
		
		
		public ActionEndGame ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Engine;
			title = "End game";
			description = "Ends the current game, either by loading an autosave, restarting or quitting the game executable.";
			numSockets = 0;
		}
		
		
		override public float Run ()
		{
			if (endGameType == AC_EndGameType.QuitGame)
			{
				#if UNITY_EDITOR
					UnityEditor.EditorApplication.isPlaying = false;
				#else
					Application.Quit ();
				#endif
			}
			else if (endGameType == AC_EndGameType.LoadAutosave)
			{
				SaveSystem.LoadAutoSave ();
			}
			else
			{
				KickStarter.runtimeInventory.SetNull ();
				KickStarter.runtimeInventory.RemoveRecipes ();

				DestroyImmediate (GameObject.FindWithTag (Tags.player));

				if (endGameType == AC_EndGameType.RestartGame)
				{
					KickStarter.saveSystem.ClearAllData ();
					KickStarter.levelStorage.ClearAllLevelData ();
					KickStarter.runtimeInventory.Start ();
					KickStarter.runtimeVariables.Start ();

					KickStarter.stateHandler.playedGlobalOnStart = false;
					KickStarter.sceneChanger.ChangeScene (AdvGame.GetSceneName (chooseSceneBy, sceneName), sceneNumber, false);
				}
				else if (endGameType == AC_EndGameType.ResetScene)
				{
					KickStarter.levelStorage.ClearCurrentLevelData ();
					KickStarter.sceneChanger.ChangeScene ("", Application.loadedLevel, false);
				}
			}

			return 0f;
		}
		
		
		override public ActionEnd End (List<Action> actions)
		{
			return GenerateStopActionEnd ();
		}
		
		
		#if UNITY_EDITOR

		override public void ShowGUI ()
		{
			endGameType = (AC_EndGameType) EditorGUILayout.EnumPopup ("Command:", endGameType);

			if (endGameType == AC_EndGameType.RestartGame)
			{
				if (chooseSceneBy == ChooseSceneBy.Name)
				{
					sceneName = EditorGUILayout.TextField ("Scene to restart to:", sceneName);
				}
				else
				{
					sceneNumber = EditorGUILayout.IntField ("Scene to restart to:", sceneNumber);
				}
			}
		}
		

		public override string SetLabel ()
		{
			return (" (" + endGameType.ToString () + ")");
		}

		#endif
		
	}

}