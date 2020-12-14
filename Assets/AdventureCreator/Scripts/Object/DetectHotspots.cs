/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"DetectHotspots.cs"
 * 
 *	This script is used to determine which
 *	active Hotspot is nearest the player.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	public class DetectHotspots : MonoBehaviour
	{

		private Hotspot nearestHotspot;
		private int selected = 0;
		private List<Hotspot> hotspots = new List<Hotspot>();


		private void OnLevelWasLoaded ()
		{
			hotspots.Clear ();
			selected = 0;
		}


		public Hotspot GetSelected ()
		{
			if (hotspots.Count > 0)
			{
				if (AdvGame.GetReferences ().settingsManager.hotspotsInVicinity == HotspotsInVicinity.NearestOnly)
				{
					if (hotspots [selected].gameObject.layer == LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.hotspotLayer))
					{
						return nearestHotspot;
					}
					else
					{
						nearestHotspot = null;
						hotspots.Remove (nearestHotspot);
					}
				}
				else if (AdvGame.GetReferences ().settingsManager.hotspotsInVicinity == HotspotsInVicinity.CycleMultiple)
				{
					if (selected >= hotspots.Count)
					{
						selected = hotspots.Count - 1;
					}
					else if (selected < 0)
					{
						selected = 0;
					}

					if (hotspots [selected].gameObject.layer == LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.hotspotLayer))
					{
						return hotspots [selected];
					}
					else
					{
						if (nearestHotspot == hotspots [selected])
						{
							nearestHotspot = null;
						}

						hotspots.RemoveAt (selected);
					}
				}
			}

			return null;
		}


		private void OnTriggerStay (Collider other)
		{
			if (other.GetComponent <Hotspot>() && other.gameObject.layer == LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.hotspotLayer))
			{
				if (nearestHotspot == null || (Vector3.Distance (transform.position, other.transform.position) <= Vector3.Distance (transform.position, nearestHotspot.transform.position)))
				{
					nearestHotspot = other.GetComponent <Hotspot>();
				}

				foreach (Hotspot hotspot in hotspots)
				{
					if (hotspot == other.GetComponent <Hotspot>())
					{
						return;
					}
				}

				hotspots.Add (other.GetComponent <Hotspot>());
			}
        }


		private void OnTriggerStay2D (Collider2D other)
		{
			if (other.GetComponent <Hotspot>() && other.gameObject.layer == LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.hotspotLayer))
			{
				if (nearestHotspot == null || (Vector3.Distance (transform.position, other.transform.position) <= Vector3.Distance (transform.position, nearestHotspot.transform.position)))
				{
					nearestHotspot = other.GetComponent <Hotspot>();
				}
				
				foreach (Hotspot hotspot in hotspots)
				{
					if (hotspot == other.GetComponent <Hotspot>())
					{
						return;
					}
				}
				hotspots.Add (other.GetComponent <Hotspot>());
			}
		}


		private void OnTriggerExit (Collider other)
		{
			if (other.GetComponent <Hotspot>())
			{
				Hotspot _hotspot = other.GetComponent <Hotspot>();

				if (nearestHotspot == _hotspot)
				{
					nearestHotspot = null;
				}

				if (IsHotspotInTrigger (_hotspot))
				{
					hotspots.Remove (_hotspot);
				}

				if (_hotspot.highlight != null)
				{
					_hotspot.highlight.HighlightOff ();
				}
			}
		}


		private void OnTriggerExit2D (Collider2D other)
		{
			if (other.GetComponent <Hotspot>())
			{
				Hotspot _hotspot = other.GetComponent <Hotspot>();
				
				if (nearestHotspot == _hotspot)
				{
					nearestHotspot = null;
				}
				
				if (IsHotspotInTrigger (_hotspot))
				{
					hotspots.Remove (_hotspot);
				}
				
				if (_hotspot.highlight != null)
				{
					_hotspot.highlight.HighlightOff ();
				}
			}
		}


		public void _Update ()
		{
			if (nearestHotspot && nearestHotspot.gameObject.layer == LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.deactivatedLayer))
			{
				nearestHotspot = null;
			}

			if (KickStarter.stateHandler != null && KickStarter.stateHandler.gameState == GameState.Normal)
			{
				try
				{
					if (KickStarter.playerInput.InputGetButtonDown ("CycleHotspotsLeft"))
					{
						CycleHotspots (false);
					}
					else if (KickStarter.playerInput.InputGetButtonDown ("CycleHotspotsRight"))
					{
						CycleHotspots (true);
					}
					else if (KickStarter.playerInput.InputGetAxis ("CycleHotspots") > 0.1f)
					{
						CycleHotspots (true);
					}
					else if (KickStarter.playerInput.InputGetAxis ("CycleHotspots") < -0.1f)
					{
						CycleHotspots (false);
					}
				}
				catch {}
			}
		}


		private void CycleHotspots (bool goRight)
		{
			if (goRight)
			{
				selected ++;
			}
			else
			{
				selected --;
			}

			if (selected >= hotspots.Count)
			{
				selected = 0;
			}
			else if (selected < 0)
			{
				selected = hotspots.Count - 1;
			}
		}


		public bool IsHotspotInTrigger (Hotspot hotspot)
		{
			if (hotspots.Contains (hotspot))
			{
				return true;
			}
			return false;
		}


		public void HighlightAll ()
		{
			foreach (Hotspot _hotspot in hotspots)
			{
				if (_hotspot.highlight != null)
				{
					_hotspot.highlight.HighlightOn ();
				}
			}
		}

	}

}