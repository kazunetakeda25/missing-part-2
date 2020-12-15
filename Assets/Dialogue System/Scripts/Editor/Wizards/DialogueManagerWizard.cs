using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using PixelCrushers.DialogueSystem.Examples;

namespace PixelCrushers.DialogueSystem.Editors {

	/// <summary>
	/// This wizard sets up the Dialogue Manager object.
	/// </summary>
	public class DialogueManagerWizard : EditorWindow {
		
		[MenuItem("Window/Dialogue System/Tools/Wizards/Dialogue Manager Wizard", false, 1)]
		public static void Init() {
			(EditorWindow.GetWindow(typeof(DialogueManagerWizard), false, "Dialogue Manager") as DialogueManagerWizard).minSize = new Vector2(720, 500);
		}
		
		// Private fields for the window:
		
		private enum Stage {
			Database,
			UI,
			Localization,
			Subtitles,
			Cutscenes,
			Inputs,
			Alerts,
			Review
		};
		
		private Stage stage = Stage.Database;
		
		private string[] stageLabels = new string[] { "Database", "UI", "Localization", "Subtitles", "Cutscenes", "Input", "Alerts", "Review" };

		private bool createNewDatabase = false;
		
		/// <summary>
		/// Draws the window.
		/// </summary>
		void OnGUI() {
			DrawProgressIndicator();
			DrawCurrentStage();
			CheckCreateNewDatabase();
		}

		private void CheckCreateNewDatabase() {
			if (createNewDatabase) {
				createNewDatabase = false;
				DialogueManager.Instance.initialDatabase = DialogueSystemMenuItems.CreateDialogueDatabaseInstance();
				DialogueSystemMenuItems.CreateAsset(DialogueManager.Instance.initialDatabase, "Dialogue Database");
			}
		}
		
		private void DrawProgressIndicator() {
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Toolbar((int) stage, stageLabels, GUILayout.Width(720));
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
			switch (stage) {
			case Stage.Database: DrawDatabaseStage(); break;
			case Stage.UI: DrawUIStage(); break;
			case Stage.Localization: DrawLocalizationStage(); break;
			case Stage.Subtitles: DrawSubtitlesStage(); break;
			case Stage.Cutscenes: DrawCutscenesStage(); break;
			case Stage.Inputs: DrawInputsStage(); break;
			case Stage.Alerts: DrawAlertsStage(); break;
			case Stage.Review: DrawReviewStage(); break;
			}
		}
		
		private void DrawDatabaseStage() {
			EditorGUILayout.LabelField("Dialogue Database", EditorStyles.boldLabel);
			EditorWindowTools.StartIndentedSection();
			EditorGUILayout.HelpBox("This wizard will help you configure the Dialogue Manager object. The first step is to assign an initial dialogue database asset. This asset contains your conversations and related data.", MessageType.Info);
			EditorGUILayout.BeginHorizontal();
			if (DialogueManager.Instance == null) {
				EditorGUILayout.LabelField("Dialogue Manager object is null. Creating a new instance...");
			} else {
				DialogueManager.Instance.initialDatabase = EditorGUILayout.ObjectField("Database", DialogueManager.Instance.initialDatabase, typeof(DialogueDatabase), false) as DialogueDatabase;
				bool disabled = (DialogueManager.Instance.initialDatabase != null);
				EditorGUI.BeginDisabledGroup(disabled);
				if (GUILayout.Button("Create New", GUILayout.Width (100))) createNewDatabase = true;
				EditorGUI.EndDisabledGroup();
			}
			EditorGUILayout.EndHorizontal();
			EditorWindowTools.EndIndentedSection();
			DrawNavigationButtons(false, (DialogueManager.Instance.initialDatabase != null), false);
		}

		private void DrawUIStage() {
			EditorGUILayout.LabelField("User Interface", EditorStyles.boldLabel);
			EditorWindowTools.StartIndentedSection();
			EditorGUILayout.HelpBox("Assign a dialogue UI. You can find pre-built Unity GUI dialogue UI prefabs in Prefabs/Unity Dialogue UIs. To assign a prefab for a different GUI system, import its support package found in Third Party Support. You can also assign a UI scene object.", MessageType.Info);
			if (DialogueManager.Instance.displaySettings == null) DialogueManager.Instance.displaySettings = new DisplaySettings();
			DialogueManager.Instance.displaySettings.dialogueUI = EditorGUILayout.ObjectField("Dialogue UI", DialogueManager.Instance.displaySettings.dialogueUI, typeof(GameObject), true) as GameObject;
			if (DialogueManager.Instance.displaySettings.dialogueUI == null) {
				EditorGUILayout.HelpBox("If you continue without assigning a UI, the Dialogue System will use a very plain default UI.", MessageType.Warning);
			} else {
				IDialogueUI ui = DialogueManager.Instance.displaySettings.dialogueUI.GetComponent(typeof(IDialogueUI)) as IDialogueUI;
				if (ui == null) DialogueManager.Instance.displaySettings.dialogueUI = null;
			}
			EditorWindowTools.EndIndentedSection();
			DrawNavigationButtons(true, true, false);
		}

		private void DrawLocalizationStage() {
			if (DialogueManager.Instance.displaySettings.localizationSettings == null) DialogueManager.Instance.displaySettings.localizationSettings = new DisplaySettings.LocalizationSettings();
			EditorGUILayout.LabelField("Localization", EditorStyles.boldLabel);
			EditorWindowTools.StartIndentedSection();
			EditorGUILayout.HelpBox("The Dialogue System supports language localization. If you don't want to set up localization right now, you can just click Next.", MessageType.Info);
			EditorGUILayout.BeginHorizontal();
			DialogueManager.Instance.displaySettings.localizationSettings.language = EditorGUILayout.TextField("Override Language", DialogueManager.Instance.displaySettings.localizationSettings.language);
			EditorGUILayout.HelpBox("Use the language defined by a language code (e.g., 'ES-es') instead of the default language.", MessageType.None);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			DialogueManager.Instance.displaySettings.localizationSettings.useSystemLanguage = EditorGUILayout.Toggle("Use System Language", DialogueManager.Instance.displaySettings.localizationSettings.useSystemLanguage);
			EditorGUILayout.HelpBox("Tick to use the current system language instead of the default language.", MessageType.None);
			EditorGUILayout.EndHorizontal();
			EditorWindowTools.EndIndentedSection();
			DrawNavigationButtons(true, true, false);
		}

		private void DrawSubtitlesStage() {
			if (DialogueManager.Instance.displaySettings.subtitleSettings == null) DialogueManager.Instance.displaySettings.subtitleSettings = new DisplaySettings.SubtitleSettings();
			EditorGUILayout.LabelField("Subtitles", EditorStyles.boldLabel);
			EditorWindowTools.StartIndentedSection();
			EditorGUILayout.HelpBox("In this section, you'll specify how subtitles are displayed.", MessageType.Info);
			EditorGUILayout.BeginHorizontal();
			DialogueManager.Instance.displaySettings.subtitleSettings.showNPCSubtitlesDuringLine = EditorGUILayout.Toggle("NPC Subtitles During Line", DialogueManager.Instance.displaySettings.subtitleSettings.showNPCSubtitlesDuringLine);
			EditorGUILayout.HelpBox("Tick to display NPC subtitles while NPCs are speaking. Subtitles are the lines of dialogue defined in your dialogue database asset. You might untick this if, for example, you use lip-synced voiceovers without onscreen text.", MessageType.None);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			DialogueManager.Instance.displaySettings.subtitleSettings.showNPCSubtitlesWithResponses = EditorGUILayout.Toggle("     With Response Menu", DialogueManager.Instance.displaySettings.subtitleSettings.showNPCSubtitlesWithResponses);
			EditorGUILayout.HelpBox("Tick to display the last NPC subtitle during the player response menu. This helps remind the players what they're responding to.", MessageType.None);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			DialogueManager.Instance.displaySettings.subtitleSettings.showPCSubtitlesDuringLine = EditorGUILayout.Toggle("PC Subtitles", DialogueManager.Instance.displaySettings.subtitleSettings.showPCSubtitlesDuringLine);
			EditorGUILayout.HelpBox("Tick to display PC subtitles while the PC is speaking. If you use different Menu Text and Dialogue Text, this should probably be ticked. Otherwise you may choose to leave it unticked.", MessageType.None);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			DialogueManager.Instance.displaySettings.subtitleSettings.subtitleCharsPerSecond = EditorGUILayout.FloatField("Chars/Second", DialogueManager.Instance.displaySettings.subtitleSettings.subtitleCharsPerSecond);
			EditorGUILayout.HelpBox("Determines how long the subtitle is displayed before moving to the next stage of the conversation. If the subtitle is 90 characters and Chars/Second is 30, then it will be displayed for 3 seconds.", MessageType.None);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			DialogueManager.Instance.displaySettings.subtitleSettings.minSubtitleSeconds = EditorGUILayout.FloatField("Min Seconds", DialogueManager.Instance.displaySettings.subtitleSettings.minSubtitleSeconds);
			EditorGUILayout.HelpBox("Min Seconds below is the guaranteed minimum amount of time that a subtitle will be displayed (if its corresponding checkbox is ticked).", MessageType.None);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			DialogueManager.Instance.displaySettings.subtitleSettings.continueButton = (DisplaySettings.SubtitleSettings.ContinueButtonMode) EditorGUILayout.EnumPopup("Continue Button", DialogueManager.Instance.displaySettings.subtitleSettings.continueButton);
				//.waitForContinueButton = EditorGUILayout.Toggle("Use Continue Button", DialogueManager.Instance.displaySettings.subtitleSettings.waitForContinueButton);
			EditorGUILayout.HelpBox("- Never: Conversation automatically moves to next stage when subtitle is done." +
			                        "\n- Always: Requires player to click a continue button to progress past each subtitle." +
			                        "\n- Optional Before Response Menu: If player response menu is next, shows but doesn't require clicking." +
			                        "\n- Never Before Response Menu: If player response menu is next, doesn't show." +
			                        "\nFor any setting other than Never, your UI must contain continue button(s).", MessageType.None);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			DialogueManager.Instance.displaySettings.subtitleSettings.richTextEmphases = EditorGUILayout.Toggle("Rich Text", DialogueManager.Instance.displaySettings.subtitleSettings.richTextEmphases);
			EditorGUILayout.HelpBox("By default, emphasis tags embedded in dialogue text are applied to the entire subtitle. To convert them to rich text tags instead, tick this checkbox. This allows emphases to affect only parts of the text, but your GUI system must support rich text.", MessageType.None);
			EditorGUILayout.EndHorizontal();
			EditorWindowTools.EndIndentedSection();
			DrawNavigationButtons(true, true, false);
		}

		private enum DefaultSequenceStyle {
			Closeups,
			WaitForSubtitle,
			Custom
		};

		private const string DefaultCloseupSequence = "Camera(Closeup); required Camera(Closeup,listener)@{{end}}";
		private const string DefaultWaitForSubtitleSequence = "Delay({{end}})";

		private void DrawCutscenesStage() {
			if (DialogueManager.Instance.displaySettings.cameraSettings == null) DialogueManager.Instance.displaySettings.cameraSettings = new DisplaySettings.CameraSettings();
			EditorGUILayout.LabelField("Cutscene Sequences", EditorStyles.boldLabel);
			EditorWindowTools.StartIndentedSection();
			EditorGUILayout.HelpBox("The Dialogue System uses an integrated cutscene sequencer. Every line of dialogue can have a cutscene sequence -- for example to move the camera, play animations on the speaker, or play a lip-synced voiceover.", MessageType.Info);
			EditorWindowTools.DrawHorizontalLine();
			EditorGUILayout.HelpBox("You can set up a camera object or prefab specifically for sequences. This can be useful to apply depth of field effects or other filters that you wouldn't normally apply to your gameplay camera. If you've set up a sequencer camera, assign it below. Otherwise the sequencer will just use the current main camera.", MessageType.None);
			DialogueManager.Instance.displaySettings.cameraSettings.sequencerCamera = EditorGUILayout.ObjectField("Sequencer Camera", DialogueManager.Instance.displaySettings.cameraSettings.sequencerCamera, typeof(Camera), true) as Camera;
			EditorWindowTools.DrawHorizontalLine();
			EditorGUILayout.HelpBox("Cutscene sequence commands can reference camera angles defined on a camera angle prefab. If you've set up a camera angle prefab, assign it below. Otherwise the sequencer will use a default camera angle prefab with basic angles such as Closeup, Medium, and Wide.", MessageType.None);
			DialogueManager.Instance.displaySettings.cameraSettings.cameraAngles = EditorGUILayout.ObjectField("Camera Angles", DialogueManager.Instance.displaySettings.cameraSettings.cameraAngles, typeof(GameObject), true) as GameObject;
			EditorWindowTools.DrawHorizontalLine();
			EditorGUILayout.HelpBox("If a dialogue entry doesn't define its own cutscene sequence, it will use the default sequence below.", MessageType.None);
			EditorGUILayout.BeginHorizontal();
			DefaultSequenceStyle style = string.Equals(DefaultCloseupSequence, DialogueManager.Instance.displaySettings.cameraSettings.defaultSequence)
				? DefaultSequenceStyle.Closeups
					: (string.Equals(DefaultWaitForSubtitleSequence, DialogueManager.Instance.displaySettings.cameraSettings.defaultSequence)
					   ? DefaultSequenceStyle.WaitForSubtitle
					   : DefaultSequenceStyle.Custom);
			DefaultSequenceStyle newStyle = (DefaultSequenceStyle) EditorGUILayout.EnumPopup("Default Sequence", style);
			if (newStyle != style) {
				style = newStyle;
				switch (style) {
				case DefaultSequenceStyle.Closeups: DialogueManager.Instance.displaySettings.cameraSettings.defaultSequence = DefaultCloseupSequence; break;
				case DefaultSequenceStyle.WaitForSubtitle: DialogueManager.Instance.displaySettings.cameraSettings.defaultSequence = DefaultWaitForSubtitleSequence; break;
				default: break;
				}
			}
			switch (style) {
			case DefaultSequenceStyle.Closeups: EditorGUILayout.HelpBox("Does a camera closeup of the speaker. At the end of the subtitle, changes to a closeup of the listener. Don't use this if your player is a body-less first person controller, since a closeup doesn't make sense in this case.", MessageType.None); break;
			case DefaultSequenceStyle.WaitForSubtitle: EditorGUILayout.HelpBox("Just waits for the subtitle to finish. Doesn't touch the camera.", MessageType.None); break;
			default: EditorGUILayout.HelpBox("Custom default sequence defined below.", MessageType.None); break;
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.HelpBox("In the default sequence, you can use '{{end}}' to refer to the duration of the subtitle as determined by Chars/Second and Min Seconds.", MessageType.None);
			DialogueManager.Instance.displaySettings.cameraSettings.defaultSequence = EditorGUILayout.TextField("Default Sequence", DialogueManager.Instance.displaySettings.cameraSettings.defaultSequence);
			EditorWindowTools.EndIndentedSection();
			DrawNavigationButtons(true, true, false);
		}

		private const float DefaultResponseTimeoutDuration = 10f;

		private void DrawInputsStage() {
			if (DialogueManager.Instance.displaySettings.inputSettings == null) DialogueManager.Instance.displaySettings.inputSettings = new DisplaySettings.InputSettings();
			EditorGUILayout.LabelField("Input Settings", EditorStyles.boldLabel);
			EditorWindowTools.StartIndentedSection();
			EditorGUILayout.HelpBox("In this section, you'll specify input settings for the dialogue UI.", MessageType.Info);
			EditorWindowTools.StartIndentedSection();

			EditorWindowTools.DrawHorizontalLine();
			EditorGUILayout.LabelField("Player Response Menu", EditorStyles.boldLabel);
			EditorGUILayout.BeginHorizontal();
			DialogueManager.Instance.displaySettings.inputSettings.alwaysForceResponseMenu = EditorGUILayout.Toggle("Always Force Menu", DialogueManager.Instance.displaySettings.inputSettings.alwaysForceResponseMenu);
			EditorGUILayout.HelpBox("Tick to always force the response menu. If unticked, then when the player only has one valid response, the UI will automatically select it without showing the response menu.", MessageType.None);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			bool useTimeout = EditorGUILayout.Toggle("Timer", (DialogueManager.Instance.displaySettings.inputSettings.responseTimeout > 0));
			EditorGUILayout.HelpBox("Tick to make the response menu timed. If unticked, players can take as long as they want to make their selection.", MessageType.None);
			EditorGUILayout.EndHorizontal();
			if (useTimeout) {
				if (Tools.ApproximatelyZero(DialogueManager.Instance.displaySettings.inputSettings.responseTimeout)) DialogueManager.Instance.displaySettings.inputSettings.responseTimeout = DefaultResponseTimeoutDuration;
				DialogueManager.Instance.displaySettings.inputSettings.responseTimeout = EditorGUILayout.FloatField("Timeout Seconds", DialogueManager.Instance.displaySettings.inputSettings.responseTimeout);
				DialogueManager.Instance.displaySettings.inputSettings.responseTimeoutAction = (ResponseTimeoutAction) EditorGUILayout.EnumPopup("If Time Runs Out", DialogueManager.Instance.displaySettings.inputSettings.responseTimeoutAction);
			} else {
				DialogueManager.Instance.displaySettings.inputSettings.responseTimeout = 0;
			}

			EditorWindowTools.DrawHorizontalLine();
			EditorGUILayout.LabelField("Quick Time Event (QTE) Trigger Buttons", EditorStyles.boldLabel);
			EditorGUILayout.HelpBox("QTE trigger buttons may be defined on the Dialogue Manager object's inspector under Display Settings > Input Settings > Qte Buttons.", MessageType.None);
			if (GUILayout.Button("Inspect Dialogue Manager object", GUILayout.Width(240))) Selection.activeObject = DialogueManager.Instance;

			EditorWindowTools.DrawHorizontalLine();
			EditorGUILayout.LabelField("Cancel", EditorStyles.boldLabel);
			EditorGUILayout.BeginHorizontal();
			DialogueManager.Instance.displaySettings.inputSettings.cancel.key = (KeyCode) EditorGUILayout.EnumPopup("Key", DialogueManager.Instance.displaySettings.inputSettings.cancel.key);
			EditorGUILayout.HelpBox("Pressing this key cancels the response menu or conversation.", MessageType.None);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			DialogueManager.Instance.displaySettings.inputSettings.cancel.buttonName = EditorGUILayout.TextField("Button Name", DialogueManager.Instance.displaySettings.inputSettings.cancel.buttonName);
			EditorGUILayout.HelpBox("Pressing this button cancels the response menu or conversation.", MessageType.None);
			EditorGUILayout.EndHorizontal();

			EditorWindowTools.EndIndentedSection();
			EditorWindowTools.EndIndentedSection();
			DrawNavigationButtons(true, true, false);
		}

		private const float DefaultAlertCheckFrequency = 2f;

		private void DrawAlertsStage() {
			if (DialogueManager.Instance.displaySettings.alertSettings == null) DialogueManager.Instance.displaySettings.alertSettings = new DisplaySettings.AlertSettings();
			EditorGUILayout.LabelField("Alerts", EditorStyles.boldLabel);
			EditorWindowTools.StartIndentedSection();
			EditorGUILayout.HelpBox("Alerts are gameplay messages. They can be delivered from conversations or other sources.", MessageType.Info);
			EditorGUILayout.BeginHorizontal();
			DialogueManager.Instance.displaySettings.alertSettings.allowAlertsDuringConversations = EditorGUILayout.Toggle("Allow In Conversations", DialogueManager.Instance.displaySettings.alertSettings.allowAlertsDuringConversations);
			EditorGUILayout.HelpBox("Tick to allow alerts to be displayed while in conversations. If unticked, alerts won't be displayed until the conversation has ended.", MessageType.None);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			bool monitorAlerts = EditorGUILayout.Toggle("Monitor Alerts", (DialogueManager.Instance.displaySettings.alertSettings.alertCheckFrequency > 0));
			EditorGUILayout.HelpBox("Tick to constantly monitor the Lua value \"Variable['Alert']\" and display its contents as an alert. This runs as a background process. The value is automatically checked at the end of conversations, and you can check it manually. Unless you have a need to constantly monitor the value, it's more efficient to leave this unticked.", MessageType.None);
			EditorGUILayout.EndHorizontal();
			if (monitorAlerts) {
				if (Tools.ApproximatelyZero(DialogueManager.Instance.displaySettings.alertSettings.alertCheckFrequency)) DialogueManager.Instance.displaySettings.alertSettings.alertCheckFrequency = DefaultAlertCheckFrequency;
				DialogueManager.Instance.displaySettings.alertSettings.alertCheckFrequency = EditorGUILayout.FloatField("Frequency (Seconds)", DialogueManager.Instance.displaySettings.alertSettings.alertCheckFrequency);
			} else {
				DialogueManager.Instance.displaySettings.alertSettings.alertCheckFrequency = 0;
			}
			EditorWindowTools.EndIndentedSection();
			DrawNavigationButtons(true, true, false);
		}

		private void DrawReviewStage() {
			EditorGUILayout.LabelField("Review", EditorStyles.boldLabel);
			EditorWindowTools.StartIndentedSection();
			EditorGUILayout.HelpBox("Your Dialogue Manager is ready! Below is a brief summary of the configuration.", MessageType.Info);
			EditorGUILayout.LabelField(string.Format("Dialogue database: {0}", DialogueManager.Instance.initialDatabase.name));
			EditorGUILayout.LabelField(string.Format("Dialogue UI: {0}", (DialogueManager.Instance.displaySettings.dialogueUI == null) ? "(use default)" : DialogueManager.Instance.displaySettings.dialogueUI.name));
			EditorGUILayout.LabelField(string.Format("Show subtitles: {0}", GetSubtitlesInfo()));
			EditorGUILayout.LabelField(string.Format("Always force response menu: {0}", DialogueManager.Instance.displaySettings.inputSettings.alwaysForceResponseMenu));
			EditorGUILayout.LabelField(string.Format("Response menu timeout: {0}", Tools.ApproximatelyZero(DialogueManager.Instance.displaySettings.inputSettings.responseTimeout) ? "Unlimited Time" : DialogueManager.Instance.displaySettings.inputSettings.responseTimeout.ToString()));
			EditorGUILayout.LabelField(string.Format("Allow alerts during conversations: {0}", DialogueManager.Instance.displaySettings.alertSettings.allowAlertsDuringConversations));
			EditorWindowTools.EndIndentedSection();
			DrawNavigationButtons(true, true, true);
		}

		private string GetSubtitlesInfo() {
			if ((DialogueManager.Instance.displaySettings.subtitleSettings.showNPCSubtitlesDuringLine == true) &&
			    (DialogueManager.Instance.displaySettings.subtitleSettings.showNPCSubtitlesWithResponses == true) &&
			    (DialogueManager.Instance.displaySettings.subtitleSettings.showPCSubtitlesDuringLine == true)) {
				return "All";
			} else if ((DialogueManager.Instance.displaySettings.subtitleSettings.showNPCSubtitlesDuringLine == true) &&
			           (DialogueManager.Instance.displaySettings.subtitleSettings.showNPCSubtitlesWithResponses == true) &&
			           (DialogueManager.Instance.displaySettings.subtitleSettings.showPCSubtitlesDuringLine == false)) {
				return "NPC only";
			} else if ((DialogueManager.Instance.displaySettings.subtitleSettings.showNPCSubtitlesDuringLine == true) &&
			           (DialogueManager.Instance.displaySettings.subtitleSettings.showNPCSubtitlesWithResponses == false) &&
			           (DialogueManager.Instance.displaySettings.subtitleSettings.showPCSubtitlesDuringLine == true)) {
				return "While speaking lines";
			} else if ((DialogueManager.Instance.displaySettings.subtitleSettings.showNPCSubtitlesDuringLine == true) &&
			           (DialogueManager.Instance.displaySettings.subtitleSettings.showNPCSubtitlesWithResponses == false) &&
			           (DialogueManager.Instance.displaySettings.subtitleSettings.showPCSubtitlesDuringLine == false)) {
				return "Only while NPC is speaking";
			} else if ((DialogueManager.Instance.displaySettings.subtitleSettings.showNPCSubtitlesDuringLine == false) &&
			           (DialogueManager.Instance.displaySettings.subtitleSettings.showNPCSubtitlesWithResponses == true) &&
			           (DialogueManager.Instance.displaySettings.subtitleSettings.showPCSubtitlesDuringLine == true)) {
				return "Only PC and response menu reminder";
			} else if ((DialogueManager.Instance.displaySettings.subtitleSettings.showNPCSubtitlesDuringLine == false) &&
			           (DialogueManager.Instance.displaySettings.subtitleSettings.showNPCSubtitlesWithResponses == true) &&
			           (DialogueManager.Instance.displaySettings.subtitleSettings.showPCSubtitlesDuringLine == false)) {
				return "Only response menu reminder";
			} else if ((DialogueManager.Instance.displaySettings.subtitleSettings.showNPCSubtitlesDuringLine == false) &&
			           (DialogueManager.Instance.displaySettings.subtitleSettings.showNPCSubtitlesWithResponses == false) &&
			           (DialogueManager.Instance.displaySettings.subtitleSettings.showPCSubtitlesDuringLine == true)) {
				return "PC Only";
			} else if ((DialogueManager.Instance.displaySettings.subtitleSettings.showNPCSubtitlesDuringLine == false) &&
			           (DialogueManager.Instance.displaySettings.subtitleSettings.showNPCSubtitlesWithResponses == false) &&
			           (DialogueManager.Instance.displaySettings.subtitleSettings.showPCSubtitlesDuringLine == false)) {
				return "None";
			} else {
				return "See inspector";
			}
		}

	}

}
