using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem.UnityGUI;

namespace PixelCrushers.DialogueSystem.Examples {

	/// <summary>
	/// When you attach this script to the Dialogue Manager object (or a child),
	/// it will display tracked quests. It updates when the player toggles tracking
	/// in the quest log window and at the end of conversations. If you change the
	/// state of a quest elsewhere, you must manually call UpdateTracker().
	/// </summary>
	public class QuestTracker : MonoBehaviour {

		/// <summary>
		/// The screen rect that will contain the tracker.
		/// </summary>
		public ScaledRect rect = new ScaledRect(ScaledRectAlignment.TopRight, ScaledRectAlignment.TopRight, 
		                                        ScaledValue.FromPixelValue(0), ScaledValue.FromPixelValue(0), 
		                                        ScaledValue.FromNormalizedValue(0.25f), ScaledValue.FromNormalizedValue(1f), 
		                                        64f, 32f);

		/// <summary>
		/// The GUI skin to use for the tracker.
		/// </summary>
		public GUISkin guiSkin;

		/// <summary>
		/// The GUI style to use for quest titles.
		/// </summary>
		public string TitleStyle;

		/// <summary>
		/// The GUI style to use for active quest entries.
		/// </summary>
		public string ActiveEntryStyle;
		
		/// <summary>
		/// The GUI style to use for successful quest entries.
		/// </summary>
		public string SuccessEntryStyle;
		
		/// <summary>
		/// The GUI style to use for failed quest entries.
		/// </summary>
		public string FailureEntryStyle;

		private class QuestTrackerLine {
			public string guiStyleName;
			public GUIStyle guiStyle;
			public string text;
		}

		private Rect screenRect;

		private List<QuestTrackerLine> lines = new List<QuestTrackerLine>();

		/// <summary>
		/// Wait one frame after starting to update the tracker in case other start
		/// methods change the state of quests.
		/// </summary>
		public void Start() {
			StartCoroutine(UpdateTrackerAfterOneFrame());
		}

		private IEnumerator UpdateTrackerAfterOneFrame() {
			yield return null;
			UpdateTracker();
		}

		/// <summary>
		/// The quest log window sends this message when the player toggles tracking.
		/// </summary>
		/// <param name="quest">Quest.</param>
		public void OnQuestTrackingEnabled(string quest) {
			UpdateTracker();
		}
		
		/// <summary>
		/// The quest log window sends this message when the player toggles tracking.
		/// </summary>
		/// <param name="quest">Quest.</param>
		public void OnQuestTrackingDisabled(string quest) {
			UpdateTracker();
		}

		/// <summary>
		/// Quests are often completed in conversations. This handles changes in quest states
		/// after conversations.
		/// </summary>
		/// <param name="actor">Actor.</param>
		public void OnConversationEnd(Transform actor) {
			UpdateTracker();
		}
		
		public void UpdateTracker() {
			screenRect = rect.GetPixelRect();
			lines.Clear();
			foreach (string quest in QuestLog.GetAllQuests()) {
				if (QuestLog.IsQuestActive(quest) && QuestLog.IsQuestTrackingEnabled(quest)) {
					AddQuestTitle(quest);
					AddQuestEntries(quest);
				}
			}
		}

		private void AddQuestTitle(string quest) {
			QuestTrackerLine line = new QuestTrackerLine();
			line.text = FormattedText.Parse(quest, DialogueManager.MasterDatabase.emphasisSettings).text;
			line.guiStyleName = TitleStyle;
			line.guiStyle = null;
			lines.Add(line);
		}

		private void AddQuestEntries(string quest) {
			int entryCount = QuestLog.GetQuestEntryCount(quest);
			for (int i = 1; i <= entryCount; i++) {
				QuestState entryState = QuestLog.GetQuestEntryState(quest, i);
				if (entryState != QuestState.Unassigned) {
					QuestTrackerLine line = new QuestTrackerLine();
					line.text = FormattedText.Parse(QuestLog.GetQuestEntry(quest, i), DialogueManager.MasterDatabase.emphasisSettings).text;
					line.guiStyleName = GetEntryStyleName(entryState);
					line.guiStyle = null;
					lines.Add(line);
				}
			}
		}

		private string GetEntryStyleName(QuestState entryState) {
			switch (entryState) {
			case QuestState.Active: return ActiveEntryStyle;
			case QuestState.Success: return SuccessEntryStyle;
			case QuestState.Failure: return FailureEntryStyle;
			default: return ActiveEntryStyle;
			}
		}

		void OnGUI() {
			if (guiSkin != null) GUI.skin = guiSkin;
			GUILayout.BeginArea(screenRect);
			foreach (var line in lines) {
				if (line.guiStyle == null) line.guiStyle = UnityGUITools.GetGUIStyle(line.guiStyleName, GUI.skin.label);
				GUILayout.Label(line.text, line.guiStyle);
			}
			GUILayout.EndArea();
		}

	}

}
