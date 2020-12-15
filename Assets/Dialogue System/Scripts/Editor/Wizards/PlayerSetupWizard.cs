using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using PixelCrushers.DialogueSystem.Examples;

namespace PixelCrushers.DialogueSystem.Editors {

	/// <summary>
	/// Player setup wizard.
	/// </summary>
	public class PlayerSetupWizard : EditorWindow {
		
		[MenuItem("Window/Dialogue System/Tools/Wizards/Player Setup Wizard", false, 1)]
		public static void Init() {
			(EditorWindow.GetWindow(typeof(PlayerSetupWizard), false, "Player Setup") as PlayerSetupWizard).minSize = new Vector2(700, 500);
		}
		
		// Private fields for the window:
		
		private enum Stage {
			SelectPC,
			Control,
			Camera,
			Targeting,
			Transition,
			Persistence,
			Review
		};
		
		private Stage stage = Stage.SelectPC;
		
		private string[] stageLabels = new string[] { "Player", "Control", "Camera", "Targeting", "Transition", "Persistence", "Review" };
		
		private const float ToggleWidth = 16;

		private GameObject pcObject = null;
		
		private bool setEnabledFlag = false;
		
		/// <summary>
		/// Draws the window.
		/// </summary>
		void OnGUI() {
			DrawProgressIndicator();
			DrawCurrentStage();
		}
		
		private void DrawProgressIndicator() {
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Toolbar((int) stage, stageLabels, GUILayout.Width(700));
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
			EditorGUI.EndDisabledGroup();
			EditorWindowTools.DrawHorizontalLine();
		}
		
		private void DrawNavigationButtons(bool backEnabled, bool nextEnabled, bool nextCloses) {
			GUILayout.FlexibleSpace();
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Cancel", GUILayout.Width(100))) {
				this.Close();
			} else if (backEnabled && GUILayout.Button("Back", GUILayout.Width(100))) {
				stage--;
			} else {
				EditorGUI.BeginDisabledGroup(!nextEnabled);
				if (GUILayout.Button(nextCloses ? "Finish" : "Next", GUILayout.Width(100))) {
					if (nextCloses) {
						Close();
					} else {
						stage++;
					}
				}
				EditorGUI.EndDisabledGroup();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.LabelField(string.Empty, GUILayout.Height(2));
		}
		
		private void DrawCurrentStage() {
			if (pcObject == null) stage = Stage.SelectPC;
			switch (stage) {
			case Stage.SelectPC: DrawSelectPCStage(); break;
			case Stage.Control: DrawControlStage(); break;
			case Stage.Camera: DrawCameraStage(); break;
			case Stage.Targeting: DrawTargetingStage(); break;
			case Stage.Transition: DrawTransitionStage(); break;
			case Stage.Persistence: DrawPersistenceStage(); break;
			case Stage.Review: DrawReviewStage(); break;
			}
		}
		
		private void DrawSelectPCStage() {
			EditorGUILayout.LabelField("Select Player Object", EditorStyles.boldLabel);
			EditorWindowTools.StartIndentedSection();
			EditorGUILayout.HelpBox("This wizard will help you configure a Player object to work with the Dialogue System. First, assign the Player's GameObject below.", MessageType.Info);
			pcObject = EditorGUILayout.ObjectField("Player Object", pcObject, typeof(GameObject), true) as GameObject;
			EditorWindowTools.EndIndentedSection();
			DrawNavigationButtons(false, (pcObject != null), false);
		}
		
		private enum ControlStyle {
			ThirdPersonShooter,
			FollowMouseClicks,
			Custom
		};
		
		private void DrawControlStage() {
			EditorGUILayout.LabelField("Control", EditorStyles.boldLabel);
			EditorWindowTools.StartIndentedSection();
			SimpleController simpleController = pcObject.GetComponent<SimpleController>();
			NavigateOnMouseClick navigateOnMouseClick = pcObject.GetComponent<NavigateOnMouseClick>();
			ControlStyle controlStyle = (simpleController != null)
				? ControlStyle.ThirdPersonShooter
				: (navigateOnMouseClick != null) 
					? ControlStyle.FollowMouseClicks
					: ControlStyle.Custom;
			EditorGUILayout.HelpBox("How will the player control movement? (Select Custom to provide your own control components instead of using the Dialogue System's.)", MessageType.Info);
			controlStyle = (ControlStyle) EditorGUILayout.EnumPopup("Control", controlStyle);
			switch (controlStyle) {
			case ControlStyle.ThirdPersonShooter:
				DestroyImmediate(navigateOnMouseClick);
				DrawSimpleControllerSection(simpleController ?? pcObject.AddComponent<SimpleController>());
				break;
			case ControlStyle.FollowMouseClicks:
				DestroyImmediate(simpleController);
				DrawNavigateOnMouseClickSection(navigateOnMouseClick ?? pcObject.AddComponent<NavigateOnMouseClick>());
				break;
			default:
				DestroyImmediate(simpleController);
				DestroyImmediate(navigateOnMouseClick);
				break;
			}
			if (GUILayout.Button("Select Player", GUILayout.Width(100))) Selection.activeGameObject = pcObject;
			EditorWindowTools.EndIndentedSection();
			DrawNavigationButtons(true, true, false);
		}
		
		private void DrawSimpleControllerSection(SimpleController simpleController) {
			EditorWindowTools.StartIndentedSection();
			if ((simpleController.idle == null) || (simpleController.runForward == null)) {
				EditorGUILayout.HelpBox("The player uses third-person shooter style controls. At a minimum, Idle and Run animations are required. Click Select Player to customize further.", MessageType.Info);
			}
			simpleController.idle = EditorGUILayout.ObjectField("Idle Animation", simpleController.idle, typeof(AnimationClip), false) as AnimationClip;
			simpleController.runForward = EditorGUILayout.ObjectField("Run Animation", simpleController.runForward, typeof(AnimationClip), false) as AnimationClip;
			EditorWindowTools.StartIndentedSection();
			EditorGUILayout.LabelField("Optional", EditorStyles.boldLabel);
			simpleController.runSpeed = EditorGUILayout.FloatField("Run Speed", simpleController.runSpeed);
			simpleController.runBack = EditorGUILayout.ObjectField("Run Back", simpleController.runBack, typeof(AnimationClip), false) as AnimationClip;
			simpleController.aim = EditorGUILayout.ObjectField("Aim", simpleController.aim, typeof(AnimationClip), false) as AnimationClip;
			simpleController.fire = EditorGUILayout.ObjectField("Fire", simpleController.fire, typeof(AnimationClip), false) as AnimationClip;
			if (simpleController.fire != null) {
				if (simpleController.upperBodyMixingTransform == null) EditorGUILayout.HelpBox("Specify the upper body mixing transform for the fire animation.", MessageType.Info);
				simpleController.upperBodyMixingTransform = EditorGUILayout.ObjectField("Upper Body Transform", simpleController.upperBodyMixingTransform, typeof(Transform), true) as Transform;
				simpleController.fireLayerMask = EditorGUILayout.LayerField("Fire Layer", simpleController.fireLayerMask);
				simpleController.fireSound = EditorGUILayout.ObjectField("Fire Sound", simpleController.fireSound, typeof(AudioClip), false) as AudioClip;
				AudioSource audioSource = pcObject.GetComponent<AudioSource>();
				if (audioSource == null) {
					audioSource = pcObject.AddComponent<AudioSource>();
					audioSource.playOnAwake = false;
					audioSource.loop = false;
				}
				
			}
			EditorWindowTools.EndIndentedSection();
			EditorWindowTools.EndIndentedSection();
		}
		
		private void DrawNavigateOnMouseClickSection(NavigateOnMouseClick navigateOnMouseClick) {
			EditorWindowTools.StartIndentedSection();
			if ((navigateOnMouseClick.idle == null) || (navigateOnMouseClick.run == null)) {
				EditorGUILayout.HelpBox("The player clicks on the map to move. At a minimum, Idle and Run animations are required. Click Select Player to customize further.", MessageType.Info);
			}
			navigateOnMouseClick.idle = EditorGUILayout.ObjectField("Idle Animation", navigateOnMouseClick.idle, typeof(AnimationClip), false) as AnimationClip;
			navigateOnMouseClick.run = EditorGUILayout.ObjectField("Run Animation", navigateOnMouseClick.run, typeof(AnimationClip), false) as AnimationClip;
			navigateOnMouseClick.mouseButton = (NavigateOnMouseClick.MouseButtonType) EditorGUILayout.EnumPopup("Mouse Button", navigateOnMouseClick.mouseButton);
			EditorWindowTools.EndIndentedSection();
		}
		
		private void DrawCameraStage() {
			EditorGUILayout.LabelField("Camera", EditorStyles.boldLabel);
			EditorWindowTools.StartIndentedSection();
			Camera playerCamera = pcObject.GetComponentInChildren<Camera>() ?? Camera.main;
			SmoothCameraWithBumper smoothCamera = (playerCamera != null) ? playerCamera.GetComponent<SmoothCameraWithBumper>() : null;
			EditorGUILayout.BeginHorizontal();
			bool useSmoothCamera = EditorGUILayout.Toggle((smoothCamera != null) , GUILayout.Width(ToggleWidth));
			EditorGUILayout.LabelField("Use Smooth Follow Camera", EditorStyles.boldLabel);
			EditorGUILayout.EndHorizontal();
			if (useSmoothCamera) {
				if (playerCamera == null) {
					GameObject playerCameraObject = new GameObject("Player Camera");
					playerCameraObject.transform.parent = pcObject.transform;
					playerCamera = playerCameraObject.AddComponent<Camera>();
					playerCamera.tag = "MainCamera";
				}
				smoothCamera = playerCamera.GetComponentInChildren<SmoothCameraWithBumper>() ?? playerCamera.gameObject.AddComponent<SmoothCameraWithBumper>();
				EditorWindowTools.StartIndentedSection();
				if (smoothCamera.target == null) {
					EditorGUILayout.HelpBox("Specify the transform (usually the head) that the camera should follow.", MessageType.Info);
				}
				smoothCamera.target = EditorGUILayout.ObjectField("Target", smoothCamera.target, typeof(Transform), true) as Transform;
				EditorWindowTools.EndIndentedSection();
			} else {
				DestroyImmediate(smoothCamera);
			}
			if (GUILayout.Button("Select Camera", GUILayout.Width(100))) Selection.activeObject = playerCamera;
			EditorWindowTools.EndIndentedSection();
			DrawNavigationButtons(true, true, false);
		}
		
		private void DrawTargetingStage() {
			EditorGUILayout.LabelField("Targeting", EditorStyles.boldLabel);
			EditorWindowTools.StartIndentedSection();
			SelectorType selectorType = GetSelectorType();
			if (selectorType == SelectorType.None) EditorGUILayout.HelpBox("Specify how the player will target NPCs to trigger conversations and barks.", MessageType.Info);
			selectorType = (SelectorType) EditorGUILayout.EnumPopup("Target NPCs By", selectorType);
			switch (selectorType) {
			case SelectorType.Proximity:
				DrawProximitySelector();
				break;
			case SelectorType.CenterOfScreen:
			case SelectorType.MousePosition:
			case SelectorType.CustomPosition:
				DrawSelector(selectorType);
				break;
			default:
				DrawNoSelector();
				break;
			}
			EditorWindowTools.EndIndentedSection();
			EditorWindowTools.DrawHorizontalLine();
			DrawOverrideNameSubsection();
			if (GUILayout.Button("Select Player", GUILayout.Width(100))) Selection.activeGameObject = pcObject;
			DrawNavigationButtons(true, true, false);
		}
		
		private enum SelectorType {
			CenterOfScreen,
			MousePosition,
			Proximity,
			CustomPosition,
			None,
		};

		private enum MouseButtonChoice {
			LeftMouseButton,
			RightMouseButton
		}

		private SelectorType GetSelectorType() {
			if (pcObject.GetComponent<ProximitySelector>() != null) {
				return SelectorType.Proximity;
			} else {
				Selector selector = pcObject.GetComponent<Selector>();
				if (selector != null) {
					switch (selector.selectAt) {
					case Selector.SelectAt.CenterOfScreen:
						return SelectorType.CenterOfScreen;
					case Selector.SelectAt.MousePosition:
						return SelectorType.MousePosition;
					default:
						return SelectorType.CustomPosition;
					}
				} else {
					return SelectorType.None;
				}
			}
		}

		private void DrawNoSelector() {
			DestroyImmediate(pcObject.GetComponent<Selector>());
			DestroyImmediate(pcObject.GetComponent<ProximitySelector>());
			EditorWindowTools.StartIndentedSection();
			EditorGUILayout.HelpBox("The player will not use a Dialogue System-provided targeting component.", MessageType.None);
			EditorWindowTools.EndIndentedSection();
		}
		
		private void DrawProximitySelector() {
			DestroyImmediate(pcObject.GetComponent<Selector>());
			ProximitySelector proximitySelector = pcObject.GetComponent<ProximitySelector>() ?? pcObject.AddComponent<ProximitySelector>();
			EditorWindowTools.StartIndentedSection();
			EditorGUILayout.HelpBox("The player can target usable objects (e.g., conversations on NPCs) when inside their trigger areas. Click Select Player Inspect to customize the Proximity Selector.", MessageType.None);
			proximitySelector.useKey = (KeyCode) EditorGUILayout.EnumPopup("'Use' Key", proximitySelector.useKey);
			proximitySelector.useButton = EditorGUILayout.TextField("'Use' Button", proximitySelector.useButton);
			EditorWindowTools.EndIndentedSection();
		}

		private void DrawSelector(SelectorType selectorType) {
			DestroyImmediate(pcObject.GetComponent<ProximitySelector>());
			Selector selector = pcObject.GetComponent<Selector>() ?? pcObject.AddComponent<Selector>();
			EditorWindowTools.StartIndentedSection();
			switch (selectorType) {
			case SelectorType.CenterOfScreen:
				EditorGUILayout.HelpBox("Usable objects in the center of the screen will be targeted.", MessageType.None);
				selector.selectAt = Selector.SelectAt.CenterOfScreen;
				break;
			case SelectorType.MousePosition:
				EditorGUILayout.HelpBox("Usable objects under the mouse cursor will be targeted. Specify which mouse button activates the targeted object.", MessageType.None);
				selector.selectAt = Selector.SelectAt.MousePosition;
				MouseButtonChoice mouseButtonChoice = string.Equals(selector.useButton, "Fire2") ? MouseButtonChoice.RightMouseButton : MouseButtonChoice.LeftMouseButton;
				mouseButtonChoice = (MouseButtonChoice) EditorGUILayout.EnumPopup("Select With", mouseButtonChoice);
				selector.useButton = (mouseButtonChoice == MouseButtonChoice.RightMouseButton) ? "Fire2" : "Fire1";
				break;
			default:
			case SelectorType.CustomPosition:
				EditorGUILayout.HelpBox("Usable objects will be targeted at a custom screen position. You are responsible for setting the Selector component's CustomPosition property.", MessageType.None);
				selector.selectAt = Selector.SelectAt.CustomPosition;
				break;
			}
			if (selector.reticle != null) {
				selector.reticle.inRange = EditorGUILayout.ObjectField("In-Range Reticle", selector.reticle.inRange, typeof(Texture2D), false) as Texture2D;
				selector.reticle.outOfRange = EditorGUILayout.ObjectField("Out-of-Range Reticle", selector.reticle.outOfRange, typeof(Texture2D), false) as Texture2D;
			}
			selector.useKey = (KeyCode) EditorGUILayout.EnumPopup("'Use' Key", selector.useKey);
			selector.useButton = EditorGUILayout.TextField("'Use' Button", selector.useButton);
			EditorGUILayout.HelpBox("Click Select Player Inspect to customize the Selector.", MessageType.None);
			EditorWindowTools.EndIndentedSection();
		}

		private void DrawOverrideNameSubsection() {
			NPCSetupWizard.DrawOverrideNameSubsection(pcObject);
		}

		private void DrawTransitionStage() {
			EditorGUILayout.LabelField("Gameplay/Conversation Transition", EditorStyles.boldLabel);
			EditorWindowTools.StartIndentedSection();
			SetEnabledOnDialogueEvent setEnabled = pcObject.GetComponent<SetEnabledOnDialogueEvent>();
			setEnabledFlag = setEnabledFlag || (setEnabled != null);
			if (!setEnabledFlag) EditorGUILayout.HelpBox("Gameplay components, such as movement and camera control, will interfere with conversations. If you want to disable gameplay components during conversations, tick the checkbox below.", MessageType.None);
			EditorGUILayout.BeginHorizontal();
			setEnabledFlag = EditorGUILayout.Toggle(setEnabledFlag, GUILayout.Width(ToggleWidth));
			EditorGUILayout.LabelField("Disable gameplay components during conversations", EditorStyles.boldLabel);
			EditorGUILayout.EndHorizontal();
			DrawDisableControlsSection();
			DrawShowCursorSection();
			if (GUILayout.Button("Select Player", GUILayout.Width(100))) Selection.activeGameObject = pcObject;
			EditorWindowTools.EndIndentedSection();
			DrawNavigationButtons(true, true, false);
		}
		
		private void DrawDisableControlsSection() {
			EditorWindowTools.StartIndentedSection();
			SetEnabledOnDialogueEvent enabler = FindConversationEnabler();
			if (setEnabledFlag) {
				if (enabler == null) enabler = pcObject.AddComponent<SetEnabledOnDialogueEvent>();
				enabler.trigger = DialogueEvent.OnConversation;
				enabler.onStart = GetPlayerControls(enabler.onStart, Toggle.False);
				enabler.onEnd = GetPlayerControls(enabler.onEnd, Toggle.True);
				ShowDisabledComponents(enabler.onStart);
			} else {
				DestroyImmediate(enabler);
			}
			EditorWindowTools.EndIndentedSection();
		}
		
		private SetEnabledOnDialogueEvent FindConversationEnabler() {
			foreach (var component in pcObject.GetComponents<SetEnabledOnDialogueEvent>()) {
				if (component.trigger == DialogueEvent.OnConversation) return component;
			}
			return null;
		}
		
		private void ShowDisabledComponents(SetEnabledOnDialogueEvent.SetEnabledAction[] actionList) {
			EditorGUILayout.LabelField("The following components will be disabled during conversations:");
			EditorWindowTools.StartIndentedSection();
			foreach (SetEnabledOnDialogueEvent.SetEnabledAction action in actionList) {
				if (action.target != null) {
					EditorGUILayout.LabelField(action.target.GetType().Name);
				}							
			}
			EditorWindowTools.EndIndentedSection();
		}

		private SetEnabledOnDialogueEvent.SetEnabledAction[] GetPlayerControls(SetEnabledOnDialogueEvent.SetEnabledAction[] oldList, Toggle state) {
			List<SetEnabledOnDialogueEvent.SetEnabledAction> actions = new List<SetEnabledOnDialogueEvent.SetEnabledAction>();
			if (oldList != null) {
				actions.AddRange(oldList);
			}
			foreach (var component in pcObject.GetComponents<MonoBehaviour>()) {
				if (IsPlayerControlComponent(component) && !IsInActionList(actions, component)) {
					AddToActionList(actions, component, state);
				}
			}
			SmoothCameraWithBumper smoothCamera = pcObject.GetComponentInChildren<SmoothCameraWithBumper>();
			if (smoothCamera == null) smoothCamera = Camera.main.GetComponent<SmoothCameraWithBumper>();
			if ((smoothCamera != null) && !IsInActionList(actions, smoothCamera)) {
				AddToActionList(actions, smoothCamera, state);
			}
			actions.RemoveAll(a => ((a == null) || (a.target == null)));
			return actions.ToArray();
		}

		private bool IsPlayerControlComponent(MonoBehaviour component) {
			return (component is Selector) ||
					(component is ProximitySelector) ||
					(component is SimpleController) ||
					(component is NavigateOnMouseClick);
		}

		private bool IsInActionList(List<SetEnabledOnDialogueEvent.SetEnabledAction> actions, MonoBehaviour component) {
			return (actions.Find(a => (a.target == component)) != null);
		}

		private void AddToActionList(List<SetEnabledOnDialogueEvent.SetEnabledAction> actions, MonoBehaviour component, Toggle state) {
			SetEnabledOnDialogueEvent.SetEnabledAction newAction = new SetEnabledOnDialogueEvent.SetEnabledAction();
			newAction.state = state;
			newAction.target = component;
			actions.Add(newAction);
		}

		
		private void DrawShowCursorSection() {
			EditorWindowTools.DrawHorizontalLine();
			ShowCursorOnConversation showCursor = pcObject.GetComponent<ShowCursorOnConversation>();
			bool showCursorFlag = (showCursor != null);
			if (!showCursorFlag) EditorGUILayout.HelpBox("If regular gameplay hides the mouse cursor, tick Show Mouse Cursor to enable it during conversations.", MessageType.Info);
			EditorGUILayout.BeginHorizontal();
			showCursorFlag = EditorGUILayout.Toggle(showCursorFlag, GUILayout.Width(ToggleWidth));
			EditorGUILayout.LabelField("Show mouse cursor during conversations", EditorStyles.boldLabel);
			EditorGUILayout.EndHorizontal();
			if (showCursorFlag) {
				if (showCursor == null) showCursor = pcObject.AddComponent<ShowCursorOnConversation>();
			} else {
				DestroyImmediate(showCursor);
			}
		}

		private void DrawPersistenceStage() {
			EditorGUILayout.LabelField("Persistence", EditorStyles.boldLabel);
			PersistentPositionData persistentPositionData = pcObject.GetComponent<PersistentPositionData>();
			EditorWindowTools.StartIndentedSection();
			if (persistentPositionData == null) EditorGUILayout.HelpBox("The player can be configured to record its position in the Dialogue System's Lua environment so it will be preserved when saving and loading games.", MessageType.Info);
			EditorGUILayout.BeginHorizontal();
			bool hasPersistentPosition = EditorGUILayout.Toggle((persistentPositionData != null), GUILayout.Width(ToggleWidth));
			EditorGUILayout.LabelField("Player records position for saved games", EditorStyles.boldLabel);
			EditorGUILayout.EndHorizontal();
			if (hasPersistentPosition) {
				if (persistentPositionData == null) {
					persistentPositionData = pcObject.AddComponent<PersistentPositionData>();
					persistentPositionData.overrideActorName = "Player";
				}
				if (string.IsNullOrEmpty(persistentPositionData.overrideActorName)) {
					EditorGUILayout.HelpBox(string.Format("Position data will be saved to the Actor['{0}'] (the name of the GameObject) or the Override Actor Name if defined. You can override the name below.", pcObject.name), MessageType.None);
				} else {
					EditorGUILayout.HelpBox(string.Format("Position data will be saved to the Actor['{0}']. To use the name of the GameObject instead, clear the field below.", persistentPositionData.overrideActorName), MessageType.None);
				}
				persistentPositionData.overrideActorName = EditorGUILayout.TextField("Actor Name", persistentPositionData.overrideActorName);
			} else {
				DestroyImmediate(persistentPositionData);
			}
			EditorWindowTools.EndIndentedSection();
			DrawNavigationButtons(true, true, false);
		}
		
		private void DrawReviewStage() {
			EditorGUILayout.LabelField("Review", EditorStyles.boldLabel);
			EditorWindowTools.StartIndentedSection();
			EditorGUILayout.HelpBox("Your Player is ready! Below is a summary of the configuration.", MessageType.Info);
			SimpleController simpleController = pcObject.GetComponent<SimpleController>();
			NavigateOnMouseClick navigateOnMouseClick = pcObject.GetComponent<NavigateOnMouseClick>();
			if (simpleController != null) {
				EditorGUILayout.LabelField("Control: Third-Person Shooter Style");
			} else if (navigateOnMouseClick != null) {
				EditorGUILayout.LabelField("Control: Follow Mouse Clicks");
			} else {
				EditorGUILayout.LabelField("Control: Custom");
			}
			switch (GetSelectorType()) {
			case SelectorType.CenterOfScreen: EditorGUILayout.LabelField("Targeting: Center of Screen"); break;
			case SelectorType.CustomPosition: EditorGUILayout.LabelField("Targeting: Custom Position (you must set Selector.CustomPosition)"); break;
			case SelectorType.MousePosition: EditorGUILayout.LabelField("Targeting: Mouse Position"); break;
			case SelectorType.Proximity: EditorGUILayout.LabelField("Targeting: Proximity"); break;
			default: EditorGUILayout.LabelField("Targeting: None"); break;
			}			
			SetEnabledOnDialogueEvent enabler = FindConversationEnabler();
			if (enabler != null) ShowDisabledComponents(enabler.onStart);
			ShowCursorOnConversation showCursor = pcObject.GetComponentInChildren<ShowCursorOnConversation>();
			if (showCursor != null) EditorGUILayout.LabelField("Show Cursor During Conversations: Yes");
			PersistentPositionData persistentPositionData = pcObject.GetComponentInChildren<PersistentPositionData>();
			EditorGUILayout.LabelField(string.Format("Save Position: {0}", (persistentPositionData != null) ? "Yes" : "No"));
			EditorWindowTools.EndIndentedSection();
			DrawNavigationButtons(true, true, true);
		}
		
	}

}
