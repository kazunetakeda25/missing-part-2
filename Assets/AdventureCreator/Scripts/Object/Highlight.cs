/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Highlight.cs"
 * 
 *	This script is attached to any gameObject that glows
 *	when a cursor is placed over it's associated interaction
 *	object.  These are not always the same object.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	public class Highlight : MonoBehaviour
	{

		public bool brightenMaterials = true;
		public bool affectChildren = true;

		private float minHighlight = 1f;
		private float maxHighlight = 2f;
		private float highlight = 1f;
		private int direction = 1;
		private float fadeStartTime;
		private float fadeTime = 0.3f;
		private HighlightState highlightState = HighlightState.None;
		private List<Color> originalColors = new List<Color>();
		private Renderer _renderer;


		public float GetHighlightIntensity ()
		{
			return (highlight - 1f) / maxHighlight;
		}


		public void SetMinHighlight (float _minHighlight)
		{
			minHighlight = _minHighlight + 1f;

			if (minHighlight < 1f)
			{
				minHighlight = 1f;
			}
			else if (minHighlight > maxHighlight)
			{
				minHighlight = maxHighlight;
			}
		}
		
		
		private void Awake ()
		{
			// Go through own materials
			if (GetComponent <Renderer>())
			{
				_renderer = GetComponent <Renderer>();
				foreach (Material material in _renderer.materials)
				{
					if (material.HasProperty ("_Color"))
					{
						originalColors.Add (material.color);
					}
				}
			}
			
			// Go through any child materials
			Component[] children;
			children = GetComponentsInChildren <Renderer>();
			foreach (Renderer childRenderer in children)
			{
				foreach (Material material in childRenderer.materials)
				{
					if (material.HasProperty ("_Color"))
					{
						originalColors.Add (material.color);
					}
				}
			}

			if (GetComponent <GUITexture>())
			{
				originalColors.Add (GetComponent <GUITexture>().color);
			}
		}
		
		
		public void _Update ()
		{
			if (highlightState != HighlightState.None)
			{	
				if (direction == 1)
				{
					// Add highlight
					highlight = Mathf.Lerp (minHighlight, maxHighlight, AdvGame.Interpolate (fadeStartTime, fadeTime, MoveMethod.Linear, null));

					if (highlight >= maxHighlight)
					{
						highlight = maxHighlight;
						
						if (highlightState == HighlightState.Flash || highlightState == HighlightState.Pulse)
						{
							direction = -1;
							fadeStartTime = Time.time;
						}
						else
						{
							highlightState = HighlightState.On;
						}
					}
				}
				else
				{
					// Remove highlight
					highlight = Mathf.Lerp (maxHighlight, minHighlight, AdvGame.Interpolate (fadeStartTime, fadeTime, AC.MoveMethod.Linear, null));

					if (highlight <= 1f)
					{
						highlight = 1f;

						if (highlightState == HighlightState.Pulse)
						{
							direction = 1;
							fadeStartTime = Time.time;
						}
						else
						{
							highlightState = HighlightState.None;
						}
					}
				}

				if (brightenMaterials)
				{
					UpdateMaterials ();
				}
			}
			else
			{
				if (highlight != minHighlight)
				{
					highlight = minHighlight;
					if (brightenMaterials)
					{
						UpdateMaterials ();
					}
				}
			}
		}
		
		
		public void HighlightOn ()
		{
			highlightState = HighlightState.Normal;
			direction = 1;
			fadeStartTime = Time.time;
			
			if (highlight > minHighlight)
			{
				fadeStartTime -= (highlight - minHighlight) / (maxHighlight - minHighlight) * fadeTime;
			}
			else
			{
				highlight = minHighlight;
			}
		}


		public void HighlightOnInstant ()
		{
			highlightState = HighlightState.On;
			highlight = maxHighlight;
			
			UpdateMaterials ();
		}
		
		
		public void HighlightOff ()
		{
			highlightState = HighlightState.Normal;
			direction = -1;
			fadeStartTime = Time.time;
			
			if (highlight < maxHighlight)
			{
				fadeStartTime -= (maxHighlight - highlight) / (maxHighlight - minHighlight) * fadeTime;
			}
			else
			{
				highlight = maxHighlight;
			}
		}
		
		
		public void Flash ()
		{
			if (highlightState != HighlightState.Flash && (highlightState == HighlightState.None || direction == -1))
			{
				highlightState = HighlightState.Flash;
				highlight = minHighlight;
				direction = 1;
				fadeStartTime = Time.time;
			}
		}


		public float GetFlashTime ()
		{
			return fadeTime * 2f;
		}


		public float GetFadeTime ()
		{
			return fadeTime;
		}


		public void Pulse ()
		{
			highlightState = HighlightState.Pulse;
			highlight = minHighlight;
			direction = 1;
			fadeStartTime = Time.time;
		}


		public float GetHighlightAlpha ()
		{
			return (highlight - 1f);
		}
		
		
		public void HighlightOffInstant ()
		{
			minHighlight = 1f;
			highlightState = HighlightState.None;
			highlight = minHighlight;

			UpdateMaterials ();
		}


		private void UpdateMaterials ()
		{
			int i = 0;
			float alpha;

			// Go through own materials
			if (_renderer)
			{
				foreach (Material material in _renderer.materials)
				{
					if (material.HasProperty ("_Color"))
					{
						alpha = material.color.a;
						Color newColor = originalColors[i] * highlight;
						newColor.a = alpha;
						material.color = newColor;
						i++;
					}
				}
			}

			if (affectChildren)
			{
				// Go through materials
				Component[] children;
				children = GetComponentsInChildren <Renderer>();
				foreach (Renderer childRenderer in children)
				{
					foreach (Material material in childRenderer.materials)
					{
						if (originalColors.Count <= i)
						{
							break;
						}
						
						if (material.HasProperty ("_Color"))
						{
							alpha = material.color.a;
							Color newColor = originalColors[i] * highlight;
							newColor.a = alpha;
							material.color = newColor;
							i++;
						}
					}
				}
			}

			if (GetComponent <GUITexture>())
			{
				alpha = Mathf.Lerp (0.2f, 1f, highlight - 1f); // highlight is between 1 and 2
				Color newColor = originalColors[i];
				newColor.a = alpha;
				GetComponent <GUITexture>().color = newColor;
			}
		}

	}

}