/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"NavMeshSegment.cs"
 * 
 *	This script is used for the NavMeshSegment prefab, which defines
 *	the area to be baked by the Unity Navigation window.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

public class NavMeshSegment : NavMeshBase
{

	private void Awake ()
	{
		BaseAwake ();

		if (KickStarter.sceneSettings.navigationMethod == AC_NavigationMethod.UnityNavigation)
		{
			if (LayerMask.NameToLayer (KickStarter.settingsManager.navMeshLayer) == -1)
			{
				Debug.LogWarning ("No 'NavMesh' layer exists - please define one in the Tags Manager.");
			}
			else
			{
				gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.navMeshLayer);
			}
		}
	}

}
