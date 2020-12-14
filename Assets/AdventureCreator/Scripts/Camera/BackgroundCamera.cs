/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"BackgroundCamera.cs"
 * 
 *	The BackgroundCamera is used to display background images underneath the scene geometry.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class BackgroundCamera : MonoBehaviour
	{

		private void Awake ()
		{
			UpdateRect ();

			if (KickStarter.settingsManager)
			{
				if (LayerMask.NameToLayer (KickStarter.settingsManager.backgroundImageLayer) == -1)
				{
					Debug.LogWarning ("No '" + KickStarter.settingsManager.backgroundImageLayer + "' layer exists - please define one in the Tags Manager.");
				}
				else
				{
					GetComponent <Camera>().cullingMask = (1 << LayerMask.NameToLayer (KickStarter.settingsManager.backgroundImageLayer));
				}
			}
			else
			{
				Debug.LogWarning ("A Settings Manager is required for this camera type");
			}
		}


		public void UpdateRect ()
		{
			if (KickStarter.mainCamera)
			{
				GetComponent <Camera>().rect = KickStarter.mainCamera.GetComponent <Camera>().rect;
			}
		}
		
	}

}