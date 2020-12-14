/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"PlayerStart.cs"
 * 
 *	This script defines a possible starting position for the
 *	player when the scene loads, based on what the previous
 *	scene was.  If no appropriate PlayerStart is found, the
 *	one define in StartSettings is used as the default.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class PlayerStart : Marker
	{
		
		public ChooseSceneBy chooseSceneBy = ChooseSceneBy.Number;
		public int previousScene;
		public string previousSceneName;

		public bool fadeInOnStart;
		public float fadeSpeed = 0.5f;
		public _Camera cameraOnStart;
		
		private GameObject playerOb;


		public void SetPlayerStart ()
		{
			if (KickStarter.mainCamera)
			{
				if (fadeInOnStart)
				{
					KickStarter.mainCamera.FadeIn (fadeSpeed);
				}
				
				if (KickStarter.settingsManager)
				{
					if (KickStarter.player)
					{
						KickStarter.player.Teleport (this.transform.position);
						KickStarter.player.SetLookDirection (this.transform.forward, true);

						if (KickStarter.settingsManager.ActInScreenSpace () && !KickStarter.settingsManager.IsUnity2D ())
						{
							KickStarter.player.transform.position = AdvGame.GetScreenNavMesh (KickStarter.player.transform.position);
						}
					}
				
					if (KickStarter.settingsManager.movementMethod == MovementMethod.FirstPerson)
					{
						KickStarter.mainCamera.SetFirstPerson ();
					}
					
					else if (cameraOnStart)
					{
						KickStarter.mainCamera.SetGameCamera (cameraOnStart);
						KickStarter.mainCamera.lastNavCamera = cameraOnStart;
						cameraOnStart.MoveCameraInstant ();
						KickStarter.mainCamera.SetGameCamera (cameraOnStart);
						KickStarter.mainCamera.SnapToAttached ();
					}
					
					else if (cameraOnStart == null)
					{
						if (!KickStarter.settingsManager.IsInFirstPerson ())
						{
							Debug.LogWarning (this.name + " has no Camera On Start");
						}
					}
				}
			}
		}
		
	}

}