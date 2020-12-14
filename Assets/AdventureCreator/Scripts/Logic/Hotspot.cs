/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Hotspot.cs"
 * 
 *	This script handles all the possible
 *	interactions on both hotspots and NPCs.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace AC
{
	
	public class Hotspot : MonoBehaviour
	{
		
		public bool showInEditor = true;
		public InteractionSource interactionSource;
		
		public string hotspotName;
		public Highlight highlight;
		public bool playUseAnim;
		public Marker walkToMarker;
		public int lineID = -1;
		public Transform centrePoint;

		public bool provideUseInteraction;
		public Button useButton = new Button(); // This is now deprecated, and replaced by useButtons
		
		public List<Button> useButtons = new List<Button>();
		public bool oneClick = false;
		
		public bool provideLookInteraction;
		public Button lookButton = new Button();
		
		public bool provideInvInteraction;
		public List<Button> invButtons = new List<Button>();
		
		private float iconAlpha = 0;
		
		public Collider _collider;
		public Collider2D _collider2D;
		
		public bool drawGizmos = true;

		public int lastInteractionIndex = 0;
		public int displayLineID = -1;

		
		private void Awake ()
		{
			if (KickStarter.settingsManager && KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
			{
				UpgradeSelf ();
			}
			
			if (GetComponent <Collider>())
			{
				_collider = GetComponent <Collider>();
			}
			else if (GetComponent <Collider2D>())
			{
				_collider2D = GetComponent <Collider2D>();
			}

			lastInteractionIndex = FindFirstEnabledInteraction ();
			displayLineID = lineID;
		}


		private void FindFirstInteractionIndex ()
		{
			lastInteractionIndex = 0;

			foreach (Button button in useButtons)
			{
				if (!button.isDisabled)
				{
					lastInteractionIndex = useButtons.IndexOf (button);
					break;
				}
			}
		}


		public void SetProximity (bool isGameplay)
		{
			if (highlight != null)
			{
				if (!isGameplay || !IsOn ())
				{
					highlight.SetMinHighlight (0f);
				}
				else
				{
					float amount = Vector2.Distance (GetIconScreenPosition (), KickStarter.playerInput.GetMousePosition ()) / Vector2.Distance (Vector2.zero, AdvGame.GetMainGameViewSize ());
					if (amount < 0f)
					{
						amount = 0f;
					}
					else if (amount > 1f)
					{
						amount = 1f;
					}
				
					highlight.SetMinHighlight (1f - (amount * KickStarter.settingsManager.highlightProximityFactor));
				}
			}
		}
		
		
		public bool UpgradeSelf ()
		{
			if (useButton.IsButtonModified ())
			{
				Button newUseButton = new Button ();
				newUseButton.CopyButton (useButton);
				useButtons.Add (newUseButton);
				useButton = new Button ();
				provideUseInteraction = true;

				if (Application.isPlaying)
				{
					Debug.Log ("Hotspot '" + gameObject.name + "' has been temporarily upgraded - please view it's Inspector when the game ends and save the scene.");
				}
				else
				{
					Debug.Log ("Upgraded Hotspot '" + gameObject.name + "', please save the scene.");
				}

				return true;
			}
			return false;
		}
		
		
		public void DrawHotspotIcon ()
		{
			if (IsOn ())
			{
				Vector3 direction = (transform.position - Camera.main.transform.position);
				if (Vector3.Angle (direction, Camera.main.transform.forward) > 90f)
				{
					iconAlpha = 0f;
					return;
				}
				
				if (KickStarter.settingsManager.cameraPerspective != CameraPerspective.TwoD && KickStarter.settingsManager.occludeIcons)
				{
					// Is icon occluded?
					Ray ray = new Ray (Camera.main.transform.position, GetIconPosition () - Camera.main.transform.position);
					RaycastHit hit;
					if (Physics.Raycast (ray, out hit, KickStarter.settingsManager.hotspotRaycastLength, 1 << LayerMask.NameToLayer (KickStarter.settingsManager.hotspotLayer)))
					{
						if (hit.collider.gameObject != this.gameObject)
						{
							iconAlpha = 0f;
							return;
						}
					}
				}
				
				iconAlpha = 1f;
				
				if (KickStarter.settingsManager.hotspotIconDisplay == HotspotIconDisplay.OnlyWhenHighlighting)
				{
					if (highlight)
					{
						iconAlpha = highlight.GetHighlightAlpha ();
					}
					else
					{
						Debug.LogWarning ("Cannot display correct Hotspot Icon alpha on " + name + " because it has no associated Highlight object.");
					}
				}
			}
			else
			{
				iconAlpha = 0f;
				return;
			}
			
			if (iconAlpha > 0f)
			{
				Color c = GUI.color;
				Color tempColor = c;
				c.a = iconAlpha;
				GUI.color = c;
				
				if (KickStarter.settingsManager.hotspotIcon == HotspotIcon.UseIcon)
				{
					CursorIconBase icon = GetMainIcon ();
					if (icon != null)
					{
						icon.Draw (GetIconScreenPosition ());
					}
				}
				else if (KickStarter.settingsManager.hotspotIconTexture != null)
				{
					GUI.DrawTexture (AdvGame.GUIBox (GetIconScreenPosition (), KickStarter.settingsManager.hotspotIconSize), KickStarter.settingsManager.hotspotIconTexture, ScaleMode.ScaleToFit, true, 0f);
				}
				
				GUI.color = tempColor;
			}
		}
		
		
		public Button GetFirstUseButton ()
		{
			foreach (Button button in useButtons)
			{
				if (button != null && !button.isDisabled)
				{
					return button;
				}
			}
			return null;
		}
		
		
		public void TurnOn ()
		{
			gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.hotspotLayer);
		}
		
		
		public void TurnOff ()
		{
			gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.deactivatedLayer);
		}
		
		
		public bool IsOn ()
		{
			if (gameObject.layer == LayerMask.NameToLayer (KickStarter.settingsManager.deactivatedLayer))
			{
				return false;
			}
			
			return true;
		}
		
		
		public void Select ()
		{

			if (highlight)
			{
				highlight.HighlightOn ();
			}
		}
		
		
		public void Deselect ()
		{
			if (highlight)
			{
				highlight.HighlightOff ();
			}
		}
		
		
		public bool IsSingleInteraction ()
		{
			if (oneClick && provideUseInteraction && useButtons != null && GetFirstUseButton () != null)// && (invButtons == null || invButtons.Count == 0))
			{
				Debug.Log ("xxx");
				return true;
			}
			return false;
		}
		
		
		public void DeselectInstant ()
		{
			if (highlight)
			{
				highlight.HighlightOffInstant ();
			}
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
			if (this.GetComponent <AC.Char>() == null && drawGizmos)
			{
				if (GetComponent <PolygonCollider2D>())
				{
					AdvGame.DrawPolygonCollider (transform, GetComponent <PolygonCollider2D>(), new Color (1f, 1f, 0f, 0.6f));
				}
				else
				{
					AdvGame.DrawCubeCollider (transform, new Color (1f, 1f, 0f, 0.6f));
				}
			}
		}
		
		
		public Vector2 GetIconScreenPosition ()
		{
			Vector3 screenPosition = Camera.main.WorldToScreenPoint (GetIconPosition ());
			return new Vector3 (screenPosition.x, screenPosition.y);
		}
		
		
		public Vector3 GetIconPosition ()
		{
			Vector3 worldPoint = transform.position;

			if (centrePoint != null)
			{
				return centrePoint.position;
			}
			
			if (_collider != null)
			{
				if (_collider is BoxCollider)
				{
					BoxCollider boxCollider = (BoxCollider) _collider;
					worldPoint += boxCollider.center;
				}
				else if (_collider is CapsuleCollider)
				{
					CapsuleCollider capsuleCollider = (CapsuleCollider) _collider;
					worldPoint += capsuleCollider.center;
				}
			}
			else if (_collider2D != null)
			{
				if (_collider2D is BoxCollider2D)
				{
					BoxCollider2D boxCollider = (BoxCollider2D) _collider2D;
					#if UNITY_5
					worldPoint += new Vector3 (boxCollider.offset.x, boxCollider.offset.y * transform.localScale.y, 0f);
					#else
					worldPoint += new Vector3 (boxCollider.offset.x, boxCollider.offset.y * transform.localScale.y, 0f);
					#endif
				}
			}
			
			return worldPoint;
		}
		
		
		private CursorIconBase GetMainIcon ()
		{
			if (KickStarter.cursorManager == null)
			{
				return null;
			}
			
			if (provideUseInteraction && useButton != null && useButton.iconID >= 0 && !useButton.isDisabled)
			{
				return KickStarter.cursorManager.GetCursorIconFromID (useButton.iconID);
			}
			
			if (provideLookInteraction && lookButton != null && lookButton.iconID >= 0 && !lookButton.isDisabled)
			{
				return KickStarter.cursorManager.GetCursorIconFromID (lookButton.iconID);
			}
			
			if (provideUseInteraction && useButtons != null && useButtons.Count > 0 && !useButtons[0].isDisabled)
			{
				return KickStarter.cursorManager.GetCursorIconFromID (useButtons[0].iconID);
			}
			
			return null;
		}
		
		
		public bool HasContextUse ()
		{
			if ((oneClick || KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive) && provideUseInteraction && useButtons != null && GetFirstUseButton () != null)
			{
				return true;
			}
			
			return false;
		}
		
		
		public bool HasContextLook ()
		{
			if (provideLookInteraction && lookButton != null && !lookButton.isDisabled)
			{
				return true;
			}
			
			return false;
		}


		public int GetNextInteraction (int i, int numInvInteractions)
		{
			if (i < useButtons.Count)
			{
				i ++;
				while (i < useButtons.Count && useButtons [i].isDisabled)
				{
					i++;
				}

				if (i >= useButtons.Count + numInvInteractions)
				{
					return FindFirstEnabledInteraction ();
				}
				else
				{
					return i;
				}
			}
			else if (i == useButtons.Count - 1 + numInvInteractions)
			{
				return FindFirstEnabledInteraction ();
			}

			return (i+1);
		}


		public int FindFirstEnabledInteraction ()
		{
			if (useButtons != null && useButtons.Count > 0)
			{
				for (int i=0; i<useButtons.Count; i++)
				{
					if (!useButtons[i].isDisabled)
					{
						return i;
					}
				}
			}
			return 0;
		}


		private int FindLastEnabledInteraction (int numInvInteractions)
		{
			if (numInvInteractions > 0)
			{
				if (useButtons != null)
				{
					return (useButtons.Count - 1 + numInvInteractions);
				}
				return (numInvInteractions - 1);
			}

			if (useButtons != null && useButtons.Count > 0)
			{
				for (int i=useButtons.Count-1; i>=0; i--)
				{
					if (!useButtons[i].isDisabled)
					{
						return i;
					}
				}
			}
			return 0;
		}


		public int GetPreviousInteraction (int i, int numInvInteractions)
		{
			if (i > useButtons.Count && numInvInteractions > 0)
			{
				return (i-1);
			}
			else if (i == 0)
			{
				return FindLastEnabledInteraction (numInvInteractions);
			}
			else if (i <= useButtons.Count)
			{
				i --;
				while (i > 0 && useButtons [i].isDisabled)
				{
					i --;
				}

				if (i < 0)
				{
					return FindLastEnabledInteraction (numInvInteractions);
				}
				else
				{
					if (i == 0 && useButtons.Count > 0 && useButtons[0].isDisabled)
					{
						return FindLastEnabledInteraction (numInvInteractions);
					}
					return i;
				}
			}

			return (i-1);
		}


		public string GetName (int languageNumber)
		{
			if (languageNumber > 0)
			{
				return SpeechManager.GetTranslation (gameObject.name, displayLineID, languageNumber);
			}
			else if (hotspotName != "")
			{
				return hotspotName;
			}
			else
			{
				return gameObject.name;
			}
		}


		public void SetName (string newName, int _lineID)
		{
			hotspotName = newName;

			if (_lineID >= 0)
			{
				displayLineID = _lineID;
			}
			else
			{
				displayLineID = lineID;
			}
		}


		private int GetNumInteractions (int numInvInteractions)
		{
			int num = 0;
			foreach (Button _button in useButtons)
			{
				if (!_button.isDisabled)
				{
					num ++;
				}
			}
			return (num + numInvInteractions);
		}

	}
	
}