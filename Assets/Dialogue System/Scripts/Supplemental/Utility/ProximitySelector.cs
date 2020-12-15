using UnityEngine;
using System;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem.UnityGUI;

namespace PixelCrushers.DialogueSystem.Examples {
	
	/// <summary>
	/// This component implements a proximity-based selector that allows the player to move into
	/// range and use a usable object. 
	/// 
	/// To mark an object usable, add the Usable component and a collider to it. The object's
	/// layer should be in the layer mask specified on the Selector component.
	/// 
	/// The proximity selector tracks the most recent usable object whose trigger the player has
	/// entered. It displays a targeting reticle and information about the object. If the target
	/// is in range, the inRange reticle texture is displayed; otherwise the outOfRange texture is
	/// displayed.
	/// 
	/// If the player presses the use button (which defaults to spacebar and Fire2), the targeted
	/// object will receive an "OnUse" message.
	/// 
	/// You can hook into SelectedUsableObject and DeselectedUsableObject to get notifications
	/// when the current target has changed.
	/// </summary>
	public class ProximitySelector : MonoBehaviour {
		
		/// <summary>
		/// This class defines the textures and size of the targeting reticle.
		/// </summary>
		[System.Serializable]
		public class Reticle {
			public Texture2D inRange;
			public Texture2D outOfRange;
			public float width = 64f;
			public float height = 64f;
		}
		
		/// <summary>
		/// If <c>true</c>, uses a default OnGUI to display a selection message and
		/// targeting reticle.
		/// </summary>
		public bool useDefaultGUI = true;
		
		/// <summary>
		/// The GUI skin to use for the target's information (name and use message).
		/// </summary>
		public GUISkin guiSkin;
		
		/// <summary>
		/// The color of the information labels when the target is in range.
		/// </summary>
		public Color color = Color.yellow;
		
		/// <summary>
		/// The text style for the text.
		/// </summary>
		public TextStyle textStyle = TextStyle.Shadow;
		
		/// <summary>
		/// The color of the text style's outline or shadow.
		/// </summary>
		public Color textStyleColor = Color.black;
		
		/// <summary>
		/// The default use message. This can be overridden in the target's Usable component.
		/// </summary>
		public string defaultUseMessage = "(spacebar to interact)";
		
		/// <summary>
		/// The key that sends an OnUse message.
		/// </summary>
		public KeyCode useKey = KeyCode.Space;
		
		/// <summary>
		/// The button that sends an OnUse message.
		/// </summary>
		public string useButton = "Fire2";

		/// <summary>
		/// Tick to enable touch triggering.
		/// </summary>
		public bool enableTouch = false;

		/// <summary>
		/// If touch triggering is enabled and there's a touch in this area,
		/// the selector triggers.
		/// </summary>
		public ScaledRect touchArea = new ScaledRect(ScaledRect.empty);
		
		/// <summary>
		/// If ticked, the OnUse message is broadcast to the usable object's children.
		/// </summary>
		public bool broadcastToChildren = true;
		
		/// <summary>
		/// Occurs when the selector has targeted a usable object.
		/// </summary>
		public event SelectedUsableObjectDelegate SelectedUsableObject = null;
		
		/// <summary>
		/// Occurs when the selector has untargeted a usable object.
		/// </summary>
		public event DeselectedUsableObjectDelegate DeselectedUsableObject = null;
		
		/// <summary>
		/// Keeps track of which usable objects' triggers the selector is currently inside.
		/// </summary>
		private List<Usable> usablesInRange = new List<Usable>();
		
		/// <summary>
		/// The current usable that will receive an OnUse message if the player hits the use button.
		/// </summary>
		private Usable currentUsable = null;
		
		/// <summary>
		/// Caches the GUI style to use when displaying the selection message in OnGUI.
		/// </summary>
		private GUIStyle guiStyle = null;
		
		/// <summary>
		/// Sends an OnUse message to the current selection if the player presses the use button.
		/// </summary>
		void Update() {
			// Exit if disabled or paused:
			if (!enabled || (Time.timeScale <= 0)) return;
			
			// If the player presses the use key/button, send the OnUse message:
			if (IsUseButtonDown() && (currentUsable != null)) {
				if (broadcastToChildren) {
					currentUsable.gameObject.BroadcastMessage("OnUse", this.transform, SendMessageOptions.DontRequireReceiver);
				} else {
					currentUsable.gameObject.SendMessage("OnUse", this.transform, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
		
		/// <summary>
		/// Checks whether the player has just pressed the use button.
		/// </summary>
		/// <returns>
		/// <c>true</c> if the use button/key is down; otherwise, <c>false</c>.
		/// </returns>
		private bool IsUseButtonDown() {
			if (enableTouch && IsTouchDown()) return true;
			return ((useKey != KeyCode.None) && Input.GetKeyDown(useKey))
				|| (!string.IsNullOrEmpty(useButton)  && Input.GetButtonUp(useButton));
		}
		
		private bool IsTouchDown() {
			if (Input.touchCount >= 1){
				foreach (Touch touch in Input.touches) {
					Vector2 screenPosition = new Vector2(touch.position.x, Screen.height - touch.position.y);
					if (touchArea.GetPixelRect().Contains(screenPosition)) return true;
				}
			}
			return false;
		}
		
		/// <summary>
		/// If we entered a trigger, check if it's a usable object. If so, update the selection
		/// and raise the SelectedUsableObject event.
		/// </summary>
		/// <param name='other'>
		/// The trigger collider.
		/// </param>
		void OnTriggerEnter(Collider other) {
			CheckTriggerEnter(other.gameObject);
		}
		
		/// <summary>
		/// If we entered a 2D trigger, check if it's a usable object. If so, update the selection
		/// and raise the SelectedUsableObject event.
		/// </summary>
		/// <param name='other'>
		/// The 2D trigger collider.
		/// </param>
		void OnTriggerEnter2D(Collider2D other) {
			CheckTriggerEnter(other.gameObject);
		}
		
		/// <summary>
		/// If we just left a trigger, check if it's the current selection. If so, clear the
		/// selection and raise the DeselectedUsableObject event. If we're still in range of
		/// any other usables, select one of them.
		/// </summary>
		/// <param name='other'>
		/// The trigger collider.
		/// </param>
		void OnTriggerExit(Collider other) {
			CheckTriggerExit(other.gameObject);
		}
		
		/// <summary>
		/// If we just left a 2D trigger, check if it's the current selection. If so, clear the
		/// selection and raise the DeselectedUsableObject event. If we're still in range of
		/// any other usables, select one of them.
		/// </summary>
		/// <param name='other'>
		/// The 2D trigger collider.
		/// </param>
		void OnTriggerExit2D(Collider2D other) {
			CheckTriggerExit(other.gameObject);
		}
		
		private void CheckTriggerEnter(GameObject other) {
			Usable usable = other.GetComponent<Usable>();
			if (usable != null) {
				currentUsable = usable;
				if (!usablesInRange.Contains(usable)) usablesInRange.Add(usable);
				if (SelectedUsableObject != null) SelectedUsableObject(usable);
			}
		}
		
		private void CheckTriggerExit(GameObject other) {
			Usable usable = other.GetComponent<Usable>();
			if (usable != null) {
				if (usablesInRange.Contains(usable)) usablesInRange.Remove(usable);
				if (currentUsable == usable) {
					if (DeselectedUsableObject != null) DeselectedUsableObject(usable);
					currentUsable = null;
					if (usablesInRange.Count > 0) {
						currentUsable = usablesInRange[0];
						if (SelectedUsableObject != null) SelectedUsableObject(currentUsable);
					}
				}
			}
		}
		
		/// <summary>
		/// If useDefaultGUI is <c>true</c> and a usable object has been targeted, this method
		/// draws a selection message and targeting reticle.
		/// </summary>
		void OnGUI() {
			if (useDefaultGUI) {
				GUI.skin = UnityGUITools.GetValidGUISkin(guiSkin);
				if (guiStyle == null) {
					guiStyle = new GUIStyle(GUI.skin.label);
					guiStyle.alignment = TextAnchor.UpperCenter;
					guiStyle.normal.textColor = color;
				}
				Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
				if (currentUsable != null) {
					string heading = string.IsNullOrEmpty(currentUsable.overrideName) ? currentUsable.name : currentUsable.overrideName;
					string useMessage = string.IsNullOrEmpty(currentUsable.overrideUseMessage) ? defaultUseMessage : currentUsable.overrideUseMessage;
					UnityGUITools.DrawText(screenRect, heading, guiStyle, textStyle, textStyleColor);
					UnityGUITools.DrawText(new Rect(0, guiStyle.CalcSize(new GUIContent("Ay")).y, Screen.width, Screen.height), useMessage, guiStyle, textStyle, textStyleColor);
				}
			}
		}
		
	}
	
}
