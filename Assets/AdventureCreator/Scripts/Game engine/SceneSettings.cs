/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"SceneSettings.cs"
 * 
 *	This script defines which cutscenes play when the scene is loaded,
 *	and where the player should begin from.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{
	
	public class SceneSettings : MonoBehaviour
	{
		
		public Cutscene cutsceneOnStart;
		public Cutscene cutsceneOnLoad;
		public Cutscene cutsceneOnVarChange;
		public PlayerStart defaultPlayerStart;
		public AC_NavigationMethod navigationMethod = AC_NavigationMethod.meshCollider;
		public NavigationMesh navMesh;
		public SortingMap sortingMap;
		public Sound defaultSound;
		
		
		private void Awake ()
		{
			// Turn off all NavMesh objects
			NavigationMesh[] navMeshes = FindObjectsOfType (typeof (NavigationMesh)) as NavigationMesh[];
			foreach (NavigationMesh _navMesh in navMeshes)
			{
				if (navMesh != _navMesh)
				{
					_navMesh.TurnOff ();
				}
			}
			
			// Turn on default NavMesh if using MeshCollider method
			if (navMesh && (navMesh.GetComponent <Collider>() || navMesh.GetComponent <Collider2D>()))
			{
				navMesh.TurnOn ();
			}
		}
		
		
		private void Start ()
		{
			if (KickStarter.settingsManager.IsInLoadingScene ())
			{
				return;
			}

			if (KickStarter.saveSystem)
			{
				if (KickStarter.saveSystem.loadingGame == LoadingGame.No)
				{
					if (KickStarter.levelStorage)
					{
						KickStarter.levelStorage.ReturnCurrentLevelData (false);
					}
					FindPlayerStart ();
				}
				else
				{
					KickStarter.saveSystem.loadingGame = LoadingGame.No;
				}
			}
		}
		
		
		public void ResetPlayerReference ()
		{
			if (sortingMap)
			{
				sortingMap.GetAllFollowers ();
			}
		}
		
		
		private void FindPlayerStart ()
		{
			PlayerStart playerStart = GetPlayerStart ();
			if (playerStart != null)
			{
				playerStart.SetPlayerStart ();
			}

			bool playedGlobal = KickStarter.stateHandler.PlayGlobalOnStart ();

			if (cutsceneOnStart != null)
			{
				if (!playedGlobal)
				{
					// Place in a temporary cutscene to set everything up
					KickStarter.stateHandler.gameState = GameState.Cutscene;
				}
				Invoke ("RunCutsceneOnStart", 0.01f);
			}
		}


		private void RunCutsceneOnStart ()
		{
			KickStarter.stateHandler.gameState = GameState.Normal;
			cutsceneOnStart.Interact ();
		}
		
		
		public PlayerStart GetPlayerStart ()
		{
			PlayerStart[] starters = FindObjectsOfType (typeof (PlayerStart)) as PlayerStart[];
			foreach (PlayerStart starter in starters)
			{
				if (starter.chooseSceneBy == ChooseSceneBy.Name && starter.previousSceneName != "" && starter.previousSceneName == KickStarter.sceneChanger.previousSceneName)
				{
					return starter;
				}
				if (starter.chooseSceneBy == ChooseSceneBy.Number && starter.previousScene > -1 && starter.previousScene == KickStarter.sceneChanger.previousScene)
				{
					return starter;
				}
			}
			
			if (defaultPlayerStart)
			{
				return defaultPlayerStart;
			}
			
			return null;
		}
		
		
		public void OnLoad ()
		{
			if (cutsceneOnLoad != null)
			{
				cutsceneOnLoad.Interact ();
			}
		}
		
		
		public void PlayDefaultSound (AudioClip audioClip, bool doLoop)
		{
			if (defaultSound == null)
			{
				Debug.Log ("Cannot play sound since no Default Sound Prefab is defined - please set one in the Scene Manager.");
				return;
			}
			
			if (audioClip && defaultSound.GetComponent <AudioSource>())
			{
				defaultSound.GetComponent <AudioSource>().clip = audioClip;
				defaultSound.Play (doLoop);
			}
		}


		public void PauseGame ()
		{
			// Work out which Sounds will have to be re-played after pausing
			Sound[] sounds = FindObjectsOfType (typeof (Sound)) as Sound[];
			List<Sound> soundsToResume = new List<Sound>();
			foreach (Sound sound in sounds)
			{
				if (sound.playWhilePaused && sound.IsPlaying ())
				{
					soundsToResume.Add (sound);
				}
			}

#if UNITY_5_0_OR_NEWER
			// Disable Interactive Cloth components
			InteractiveCloth[] interactiveCloths = FindObjectsOfType (typeof (InteractiveCloth)) as InteractiveCloth[];
			foreach (InteractiveCloth interactiveCloth in interactiveCloths)
			{
				interactiveCloth.enabled = false;
			}
#endif

			Time.timeScale = 0f;
			AudioListener.pause = true;

			#if !UNITY_5
			foreach (Sound sound in soundsToResume)
			{
				sound.Play ();
			}
			#endif
		}


		public void UnpauseGame (float newScale)
		{
			Time.timeScale = newScale;

#if UNITY_5_0_OR_NEWER
			// Enable Interactive Cloth components
			InteractiveCloth[] interactiveCloths = FindObjectsOfType (typeof (InteractiveCloth)) as InteractiveCloth[];
			foreach (InteractiveCloth interactiveCloth in interactiveCloths)
			{
				interactiveCloth.enabled = true;
			}
#endif
		}

	}
	
}
