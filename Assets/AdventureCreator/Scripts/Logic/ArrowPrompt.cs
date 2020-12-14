/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ArrowPrompt.cs"
 * 
 *	This script allows for "Walking Dead"-style on-screen arrows,
 *	which respond to player input.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class ArrowPrompt : MonoBehaviour
	{
		
		public ArrowPromptType arrowPromptType = ArrowPromptType.KeyAndClick;
		public Arrow upArrow;
		public Arrow downArrow;
		public Arrow leftArrow;
		public Arrow rightArrow;
		public bool disableHotspots = true;
		
		private bool isOn = false;
		
		private AC_Direction directionToAnimate;
		private float alpha = 0f;
		private float arrowSize = 0.05f;

		
		public void DrawArrows ()
		{
			if (alpha > 0f)
			{
				if (directionToAnimate != AC_Direction.None)
				{
					SetGUIAlpha (alpha);
					
					if (directionToAnimate == AC_Direction.Up)
					{
						upArrow.rect = KickStarter.mainCamera.LimitMenuToAspect (AdvGame.GUIRect (0.5f, 0.1f, arrowSize*2, arrowSize));
					}
					
					else if (directionToAnimate == AC_Direction.Down)
					{
						downArrow.rect = KickStarter.mainCamera.LimitMenuToAspect (AdvGame.GUIRect (0.5f, 0.9f, arrowSize*2, arrowSize));
					}
					
					else if (directionToAnimate == AC_Direction.Left)
					{
						leftArrow.rect = KickStarter.mainCamera.LimitMenuToAspect (AdvGame.GUIRect (0.05f, 0.5f, arrowSize, arrowSize*2));
					}
					
					else if (directionToAnimate == AC_Direction.Right)
					{
						rightArrow.rect = KickStarter.mainCamera.LimitMenuToAspect (AdvGame.GUIRect (0.95f, 0.5f, arrowSize, arrowSize*2));
					}
				}
				
				else
				{
					SetGUIAlpha (alpha);
					
					if (upArrow.isPresent)
					{
						upArrow.rect = KickStarter.mainCamera.LimitMenuToAspect (AdvGame.GUIRect (0.5f, 0.1f, 0.1f, 0.05f));
					}
		
					if (downArrow.isPresent)
					{
						downArrow.rect = KickStarter.mainCamera.LimitMenuToAspect (AdvGame.GUIRect (0.5f, 0.9f, 0.1f, 0.05f));
					}
				
					if (leftArrow.isPresent)
					{
						leftArrow.rect = KickStarter.mainCamera.LimitMenuToAspect (AdvGame.GUIRect (0.05f, 0.5f, 0.05f, 0.1f));
					}
					
					if (rightArrow.isPresent)
					{
						rightArrow.rect = KickStarter.mainCamera.LimitMenuToAspect (AdvGame.GUIRect (0.95f, 0.5f, 0.05f, 0.1f));
					}
				}
			
				upArrow.Draw ();
				downArrow.Draw ();
				leftArrow.Draw ();
				rightArrow.Draw ();
			}
		}

		
		public void TurnOn ()
		{
			if (upArrow.isPresent || downArrow.isPresent || leftArrow.isPresent || rightArrow.isPresent)
			{
				if (KickStarter.playerInput)
				{
					KickStarter.playerInput.activeArrows = this;
				}
				
				StartCoroutine ("FadeIn");
				directionToAnimate = AC_Direction.None;
				arrowSize = 0.05f;
			}
		}
		
		
		private void Disable ()
		{
			if (KickStarter.playerInput)
			{
				KickStarter.playerInput.activeArrows = null;
			}
			
			isOn = false;
		}
		
		
		public void TurnOff ()
		{
			Disable ();
			StopCoroutine ("FadeIn");
			alpha = 0f;
		}
		
		
		public void DoUp ()
		{
			if (upArrow.isPresent && isOn && directionToAnimate == AC_Direction.None)
			{
				StartCoroutine (FadeOut (AC_Direction.Up));
				Disable ();
				upArrow.Run ();
			}
		}
		
		
		public void DoDown ()
		{
			if (downArrow.isPresent && isOn && directionToAnimate == AC_Direction.None)
			{
				StartCoroutine (FadeOut (AC_Direction.Down));
				Disable ();
				downArrow.Run ();
			}
		}
		
		
		public void DoLeft ()
		{
			if (leftArrow.isPresent && isOn && directionToAnimate == AC_Direction.None)
			{
				StartCoroutine (FadeOut (AC_Direction.Left));
				Disable ();
				leftArrow.Run ();
			}
		}
		
		
		public void DoRight ()
		{
			if (rightArrow.isPresent && isOn && directionToAnimate == AC_Direction.None)
			{
				StartCoroutine (FadeOut (AC_Direction.Right));
				Disable ();
				rightArrow.Run ();
			}
		}
		
		
		private IEnumerator FadeIn ()
		{
			alpha = 0f;
			
			if (alpha < 1f)
			{
				while (alpha < 0.95f)
				{
					alpha += 0.05f;
					alpha = Mathf.Clamp01 (alpha);
					yield return new WaitForFixedUpdate();
				}
				
				alpha = 1f;
				isOn = true;
			}
		}
		
		
		private IEnumerator FadeOut (AC_Direction direction)
		{
			arrowSize = 0.05f;
			alpha = 1f;
			directionToAnimate = direction;
			
			if (alpha > 0f)
			{
				while (alpha > 0.05f)
				{
					arrowSize += 0.005f;
					
					alpha -= 0.05f;
					alpha = Mathf.Clamp01 (alpha);
					yield return new WaitForFixedUpdate();
				}
				alpha = 0f;

			}
		}
		
		
		private void SetGUIAlpha (float alpha)
		{
			Color tempColor = GUI.color;
			tempColor.a = alpha;
			GUI.color = tempColor;
		}

	}


	[System.Serializable]
	public class Arrow
	{
			
		public bool isPresent;
		public Texture2D texture;
		public Cutscene linkedCutscene;
		public Rect rect;
		
		
		public Arrow ()
		{
			isPresent = false;
		}
		
		
		public void Run ()
		{
			if (linkedCutscene)
			{
				linkedCutscene.SendMessage ("Interact");
			}
		}
		
		
		public void Draw ()
		{
			if (texture)
			{
				GUI.DrawTexture (rect, texture, ScaleMode.StretchToFill, true);
			}
		}

	}

}