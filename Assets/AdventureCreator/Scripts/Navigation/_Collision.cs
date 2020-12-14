/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"_Collision.cs"
 * 
 *	This script allows colliders that block the Player's movement
 *	to be turned on and off easily via actions.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class _Collision : MonoBehaviour
	{
		
		[HideInInspector] public bool showInEditor = false;


		private void TurnOn ()
		{
			if (GetComponent <Collider>())
			{
				GetComponent <Collider>().enabled = true;
			}
			else if (GetComponent <Collider2D>())
			{
				GetComponent <Collider2D>().enabled = true;
			}
			gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.hotspotLayer);
		}
		
		
		private void TurnOff ()
		{
			if (GetComponent <Collider>())
			{
				GetComponent <Collider>().enabled = false;
			}
			else if (GetComponent <Collider2D>())
			{
				GetComponent <Collider2D>().enabled = false;
			}
			gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.deactivatedLayer);
		}
		
		
		private void OnDrawGizmos ()
		{
			if (showInEditor)
			{
				DrawGizmos ();
			}
		}
		
		
		private void OnDrawGizmosSelected ()
		{
			DrawGizmos ();
		}
		
		
		private void DrawGizmos ()
		{
			AdvGame.DrawCubeCollider (transform, new Color (0f, 1f, 1f, 0.8f));
		}
		
	}

}