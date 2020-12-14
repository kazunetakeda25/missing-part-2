/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"LimitVisibility.cs"
 * 
 *	Attach this script to a GameObject to limit it's visibility
 *	to a specific GameCamera in your scene.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class LimitVisibility : MonoBehaviour
	{

		public _Camera limitToCamera;
		public bool affectChildren = false;

		private _Camera activeCamera;
		private bool isVisible = false;


		private void Start ()
		{
			activeCamera = KickStarter.mainCamera.attachedCamera;
			
			if (activeCamera == limitToCamera)
			{
				SetVisibility (true);
			}
			else if (activeCamera != limitToCamera)
			{
				SetVisibility (false);
			}
		}


		private void Update ()
		{
			activeCamera = KickStarter.mainCamera.attachedCamera;

			if (activeCamera == limitToCamera && !isVisible)
			{
				SetVisibility (true);
			}
			else if (activeCamera != limitToCamera && isVisible)
			{
				SetVisibility (false);
			}
		}


		private void SetVisibility (bool state)
		{
			if (GetComponent <Renderer>())
			{
				GetComponent <Renderer>().enabled = state;
			}
			else if (gameObject.GetComponent <SpriteRenderer>())
			{
				gameObject.GetComponent <SpriteRenderer>().enabled = state;
			}
			if (gameObject.GetComponent <GUITexture>())
			{
				gameObject.GetComponent <GUITexture>().enabled = state;
			}

			if (affectChildren)
			{
				Renderer[] _children = GetComponentsInChildren <Renderer>();
				foreach (Renderer child in _children)
				{
					child.enabled = state;
				}

				SpriteRenderer[] spriteChildren = GetComponentsInChildren <SpriteRenderer>();
				foreach (SpriteRenderer child in spriteChildren)
				{
					child.enabled = state;
				}

				GUITexture[] textureChildren = GetComponentsInChildren <GUITexture>();
				foreach (GUITexture child in textureChildren)
				{
					child.enabled = state;
				}
			}

			isVisible = state;
		}

	}

}