/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ClickMarker.cs"
 * 
 *	This script demonstrates how to script a prefab that appears at the Player's
 *	intended destination during Point And Click mode.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class ClickMarker : MonoBehaviour
	{

		public float lifeTime = 0.5f;
		private float startTime;

		private Vector3 startScale;
		private Vector3 endScale = Vector3.zero;

		private void Start ()
		{
			Destroy (this.gameObject, lifeTime);

			if (lifeTime > 0f)
			{
				startTime = Time.time;
				startScale = transform.localScale;
			}

			StartCoroutine ("ShrinkMarker");
		}


		private IEnumerator ShrinkMarker ()
		{
			while (lifeTime > 0f)
			{
				transform.localScale = Vector3.Lerp (startScale, endScale, AdvGame.Interpolate (startTime, lifeTime, MoveMethod.EaseIn, null));
				yield return new WaitForFixedUpdate ();
			}
		}

	}

}