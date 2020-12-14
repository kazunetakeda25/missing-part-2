/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"SceneChanger.cs"
 * 
 *	This script handles the changing of the scene, and stores
 *	which scene was previously loaded, for use by PlayerStart.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class SceneChanger : MonoBehaviour
	{

		public int previousScene = -1;
		public string previousSceneName = "";
		private Player playerOnTransition;

		
		public void ChangeScene (string sceneName, int sceneNumber, bool saveRoomData)
		{
			bool useLoadingScreen = false;
			if (KickStarter.settingsManager != null && KickStarter.settingsManager.useLoadingScreen)
			{
				useLoadingScreen = true;
			}

			KickStarter.mainCamera.FadeOut (0f);

			if (KickStarter.player)
			{
				KickStarter.player.Halt ();
			
				if (KickStarter.settingsManager.movementMethod == MovementMethod.UltimateFPS)
				{
					UltimateFPSIntegration.SetCameraEnabled (false, true);
				}
			}

			Sound[] sounds = FindObjectsOfType (typeof (Sound)) as Sound[];
			foreach (Sound sound in sounds)
			{
				if (sound.canDestroy)
				{
					if (sound.GetComponent <RememberSound>())
					{
						DestroyImmediate (sound.GetComponent <RememberSound>());
					}
					DestroyImmediate (sound);
				}
			}

			KickStarter.playerMenus.ClearParents ();
			KickStarter.dialog.KillDialog (true, true);

			if (saveRoomData)
			{
				KickStarter.levelStorage.StoreCurrentLevelData ();
				previousScene = Application.loadedLevel;
				previousSceneName = Application.loadedLevelName;
			}
			
			KickStarter.stateHandler.gameState = GameState.Normal;
			playerOnTransition = KickStarter.player;
			
			LoadLevel (sceneName, sceneNumber, useLoadingScreen);
		}


		public Player GetPlayerOnTransition ()
		{
			return playerOnTransition;
		}


		public void DestroyOldPlayer ()
		{
			if (playerOnTransition)
			{
				Debug.Log ("New player prefab found - " + playerOnTransition.name + " deleted");
				DestroyImmediate (playerOnTransition.gameObject);
			}
		}


		private void LoadLevel (string sceneName, int sceneNumber)
		{
			if (KickStarter.settingsManager != null && KickStarter.settingsManager.useLoadingScreen)
			{
				LoadLevel (sceneName, sceneNumber, true);
			}
			else
			{
				LoadLevel (sceneName, sceneNumber, false);
			}
		}


		private void LoadLevel (string sceneName, int sceneNumber, bool useLoadingScreen)
		{
			if (useLoadingScreen)
			{
				if (KickStarter.player)
				{
					KickStarter.player.transform.position += new Vector3 (0f, -10000f, 0f);
				}

				GameObject go = new GameObject ("LevelManager");
				LoadingScreen loadingScreen = go.AddComponent <LoadingScreen>();
				loadingScreen.StartCoroutine (loadingScreen.InnerLoad (sceneName, sceneNumber, AdvGame.GetSceneName (KickStarter.settingsManager.loadingSceneIs, KickStarter.settingsManager.loadingSceneName), KickStarter.settingsManager.loadingScene));
			}
			else
			{
				if (sceneName != "")
				{
					UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
				}
				else
				{
					UnityEngine.SceneManagement.SceneManager.LoadScene(sceneNumber);
				}
			}
		}

	}

}