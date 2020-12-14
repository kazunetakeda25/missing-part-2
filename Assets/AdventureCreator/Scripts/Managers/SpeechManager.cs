/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"SpeechManager.cs"
 * 
 *	This script handles the "Speech" tab of the main wizard.
 *	It is used to auto-number lines for audio files, and handle translations.
 * 
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	[System.Serializable]
	public class SpeechManager : ScriptableObject
	{
		
		public float textScrollSpeed = 50;
		public AudioClip textScrollCLip = null;
		public bool displayForever = false;
		public float screenTimeFactor = 0.1f;
		public bool allowSpeechSkipping = false;
		public bool allowGameplaySpeechSkipping = false;
		public bool searchAudioFiles = true;
		public bool forceSubtitles = true;
		public bool translateAudio = true;
		public bool scrollSubtitles = true;
		public bool endScrollBeforeSkip = false;
		public bool keepTextInBuffer = false;
		
		public bool placeAudioInSubfolders = false;
		public bool separateLines = false;
		public float separateLinePause = 1f;
		
		public List<SpeechLine> lines = new List<SpeechLine>();
		public SpeechLine activeLine = null;
		public List<string> languages = new List<string>();
		public string[] sceneFiles;
		
		public bool ignoreOriginalText = false;
		public bool usePlayerRealName = false;
		
		// Audio ducking
		public float sfxDucking = 0f;
		public float musicDucking = 0f;
		
		// Lip sync
		public LipSyncMode lipSyncMode = LipSyncMode.Off;
		public LipSyncOutput lipSyncOutput = LipSyncOutput.Portrait;
		public List<string> phonemes = new List<string>();
		public float lipSyncSpeed = 1f;
		
		
		#if UNITY_EDITOR
		
		private List<string> sceneNames = new List<string>();
		private List<SpeechLine> tempLines = new List<SpeechLine>();
		private string sceneLabel;
		
		private string textFilter;
		private FilterSpeechLine filterSpeechLine = FilterSpeechLine.Text;
		private List<ActionListAsset> checkedAssets = new List<ActionListAsset>();
		private AC_TextType typeFilter = AC_TextType.Speech;
		private int sceneFilter;
		
		private static GUIContent
			deleteContent = new GUIContent("-", "Delete translation");
		
		
		public void ShowGUI ()
		{
			#if UNITY_WEBPLAYER
			EditorGUILayout.HelpBox ("Exporting game text cannot be performed in WebPlayer mode - please switch platform to do so.", MessageType.Warning);
			GUILayout.Space (10);
			#endif
			
			EditorGUILayout.LabelField ("Subtitles", EditorStyles.boldLabel);
			
			separateLines = EditorGUILayout.ToggleLeft ("Treat carriage returns at separate speech lines?", separateLines);
			if (separateLines)
			{
				separateLinePause = EditorGUILayout.Slider ("Split line delay (s):", separateLinePause, 0.1f, 1f);
			}
			scrollSubtitles = EditorGUILayout.ToggleLeft ("Scroll subtitle text?", scrollSubtitles);
			if (scrollSubtitles)
			{
				textScrollSpeed = EditorGUILayout.FloatField ("Text scroll speed:", textScrollSpeed);
				textScrollCLip = (AudioClip) EditorGUILayout.ObjectField ("Text scroll audio clip:", textScrollCLip, typeof (AudioClip), false);
			}
			
			displayForever = EditorGUILayout.ToggleLeft ("Display speech forever until user skips it?", displayForever);
			if (displayForever)
			{
				endScrollBeforeSkip = EditorGUILayout.ToggleLeft ("Skipping speech first displays currently-scrolling text?", endScrollBeforeSkip);
			}
			else
			{
				screenTimeFactor = EditorGUILayout.FloatField ("Display time factor:", screenTimeFactor);
				allowSpeechSkipping = EditorGUILayout.ToggleLeft ("Subtitles can be skipped?", allowSpeechSkipping);
				if (allowSpeechSkipping)
				{
					allowGameplaySpeechSkipping = EditorGUILayout.ToggleLeft ("Speech during gameplay can also be skipped?", allowGameplaySpeechSkipping);
					if (scrollSubtitles)
					{
						endScrollBeforeSkip = EditorGUILayout.ToggleLeft ("Skipping speech first displays currently-scrolling text?", endScrollBeforeSkip);
					}
				}
			}
			
			keepTextInBuffer = EditorGUILayout.ToggleLeft ("Retain subtitle text buffer once line has ended?", keepTextInBuffer);
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Speech audio", EditorStyles.boldLabel);
			
			forceSubtitles = EditorGUILayout.ToggleLeft ("Force subtitles to display when no speech audio is found?", forceSubtitles);
			searchAudioFiles = EditorGUILayout.ToggleLeft ("Auto-play speech audio files?", searchAudioFiles);
			translateAudio = EditorGUILayout.ToggleLeft ("Speech audio can be translated?", translateAudio);
			usePlayerRealName = EditorGUILayout.ToggleLeft ("Use Player prefab name in filenames?", usePlayerRealName);
			placeAudioInSubfolders = EditorGUILayout.ToggleLeft ("Place audio files in speaker subfolders?", placeAudioInSubfolders);
			sfxDucking = EditorGUILayout.Slider ("SFX reduction during:", sfxDucking, 0f, 1f);
			musicDucking = EditorGUILayout.Slider ("Music reduction during:", musicDucking, 0f, 1f);
			
			EditorGUILayout.Space ();
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Lip synching", EditorStyles.boldLabel);
			
			lipSyncMode = (LipSyncMode) EditorGUILayout.EnumPopup ("Lip syncing:", lipSyncMode);
			if (lipSyncMode == LipSyncMode.FromSpeechText || lipSyncMode == LipSyncMode.ReadPamelaFile || lipSyncMode == LipSyncMode.ReadSapiFile || lipSyncMode == LipSyncMode.ReadPapagayoFile)
			{
				lipSyncOutput = (LipSyncOutput) EditorGUILayout.EnumPopup ("Perform lipsync on:", lipSyncOutput);
				lipSyncSpeed = EditorGUILayout.FloatField ("Process speed:", lipSyncSpeed);
				
				if (GUILayout.Button ("Phonemes Editor"))
				{
					PhonemesWindow window = (PhonemesWindow) EditorWindow.GetWindow (typeof (PhonemesWindow));
					window.title = "Phonemes Editor";
					window.Repaint ();
				}
			}
			else if (lipSyncMode == LipSyncMode.FaceFX && !FaceFXIntegration.IsDefinePresent ())
			{
				EditorGUILayout.HelpBox ("The 'FaceFXIsPresent' preprocessor define must be declared in the Player Settings.", MessageType.Warning);
			}
			else if (lipSyncMode == LipSyncMode.Salsa2D)
			{
				lipSyncOutput = (LipSyncOutput) EditorGUILayout.EnumPopup ("Perform lipsync on:", lipSyncOutput);
				
				EditorGUILayout.HelpBox ("Speaking animations must have 4 frames: Rest, Small, Medium and Large.", MessageType.Info);
				
				#if !SalsaIsPresent
				EditorGUILayout.HelpBox ("The 'SalsaIsPresent' preprocessor define must be declared in the Player Settings.", MessageType.Warning);
				#endif
			}
			
			EditorGUILayout.Space ();
			LanguagesGUI ();
			
			EditorGUILayout.Space ();
			
			GUILayout.Label ("Game text", EditorStyles.boldLabel);
			GUILayout.Label ("Speech audio files must be placed in: /Resources/Speech");
			
			EditorGUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Gather text", EditorStyles.miniButtonLeft))
			{
				PopulateList ();
				
				if (sceneFiles.Length > 0)
				{
					Array.Sort (sceneFiles);
				}
			}
			if (GUILayout.Button ("Reset text", EditorStyles.miniButtonMid))
			{
				ClearList ();
			}
			if (GUILayout.Button ("Create script sheet", EditorStyles.miniButtonRight))
			{
				if (lines.Count > 0)
				{
					CreateScript ();
				}
			}
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Import all translations", EditorStyles.miniButtonLeft))
			{
				ImportGameText ();
			}
			if (GUILayout.Button ("Export all translations", EditorStyles.miniButtonRight))
			{
				ExportGameText ();
			}
			EditorGUILayout.EndHorizontal ();
			
			if (lines.Count > 0)
			{
				EditorGUILayout.Space ();
				ListLines ();
			}
			
			if (GUI.changed)
			{
				EditorUtility.SetDirty (this);
			}
		}
		
		
		private void GetSceneNames ()
		{
			sceneNames.Clear ();
			sceneNames.Add ("(No scene)");
			sceneNames.Add ("(Any or no scene)");
			foreach (string sceneFile in sceneFiles)
			{
				int slashPoint = sceneFile.LastIndexOf ("/") + 1;
				string sceneName = sceneFile.Substring (slashPoint);

				sceneNames.Add (sceneName.Substring (0, sceneName.Length - 6));
			}
		}
		
		
		private void ListLines ()
		{
			if (sceneNames == null || sceneNames == new List<string>() || sceneNames.Count != (sceneFiles.Length + 2))
			{
				sceneFiles = AdvGame.GetSceneFiles ();
				GetSceneNames ();
			}
			
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Type filter:", GUILayout.Width (65f));
			typeFilter = (AC_TextType) EditorGUILayout.EnumPopup (typeFilter);
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Scene filter:", GUILayout.Width (65f));
			sceneFilter = EditorGUILayout.Popup (sceneFilter, sceneNames.ToArray ());
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Text filter:", GUILayout.Width (65f));
			filterSpeechLine = (FilterSpeechLine) EditorGUILayout.EnumPopup (filterSpeechLine);
			textFilter = EditorGUILayout.TextField (textFilter);
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.Space ();

			string selectedScene = sceneNames[sceneFilter] + ".unity";
			foreach (SpeechLine line in lines)
			{
				if (line.textType == typeFilter && line.Matches (textFilter, filterSpeechLine))
				{
					if ((line.scene == "" && sceneFilter == 0)
					    || sceneFilter == 1
					    || (line.scene != "" && sceneFilter > 1 && line.scene.EndsWith (selectedScene)))
					{
						line.ShowGUI ();
					}
				}
			}
		}
		
		
		private void LanguagesGUI ()
		{
			GUILayout.Label ("Translations", EditorStyles.boldLabel);
			
			if (languages.Count == 0)
			{
				ClearLanguages ();
			}
			else
			{
				if (languages.Count > 1)
				{
					ignoreOriginalText = EditorGUILayout.ToggleLeft ("Prevent original language from being used?", ignoreOriginalText);
					
					for (int i=1; i<languages.Count; i++)
					{
						EditorGUILayout.BeginHorizontal ();
						languages[i] = EditorGUILayout.TextField (languages[i]);
						
						if (GUILayout.Button ("Import translation", EditorStyles.miniButtonLeft, GUILayout.MaxWidth(110f)))
						{
							ImportTranslation (i);
						}
						
						if (GUILayout.Button ("Export translation", EditorStyles.miniButtonRight, GUILayout.MaxWidth(110f)))
						{
							ExportTranslation (i);
						}
						
						if (GUILayout.Button (deleteContent, EditorStyles.miniButton, GUILayout.MaxWidth(20f)))
						{
							Undo.RecordObject (this, "Delete translation: " + languages[i]);
							DeleteLanguage (i);
						}
						EditorGUILayout.EndHorizontal ();
					}
				}
				
				if (GUILayout.Button ("Create new translation"))
				{
					Undo.RecordObject (this, "Add translation");
					CreateLanguage ("New " + languages.Count.ToString ());
				}
			}
		}
		
		
		private void CreateLanguage (string name)
		{
			languages.Add (name);
			
			foreach (SpeechLine line in lines)
			{
				line.translationText.Add (line.text);
			}
		}
		
		
		private void DeleteLanguage (int i)
		{
			languages.RemoveAt (i);
			
			foreach (SpeechLine line in lines)
			{
				line.translationText.RemoveAt (i-1);
			}
			
		}
		
		
		public void ClearLanguages ()
		{
			languages.Clear ();
			
			foreach (SpeechLine line in lines)
			{
				line.translationText.Clear ();
			}
			
			languages.Add ("Original");	
			
		}
		
		
		private void PopulateList ()
		{
			string originalScene = EditorApplication.currentScene;
			
			if (EditorApplication.SaveCurrentSceneIfUserWantsTo ())
			{
				Undo.RecordObject (this, "Update speech list");
				
				// Store the lines temporarily, so that we can update the translations afterwards
				BackupTranslations ();
				
				lines.Clear ();
				checkedAssets.Clear ();
				
				sceneFiles = AdvGame.GetSceneFiles ();
				GetSceneNames ();
				
				// First look for lines that already have an assigned lineID
				foreach (string sceneFile in sceneFiles)
				{
					GetLinesInScene (sceneFile, false);
				}
				
				GetLinesFromSettings (false);
				GetLinesFromInventory (false);
				GetLinesFromCursors (false);
				GetLinesFromMenus (false);
				
				checkedAssets.Clear ();
				
				// Now look for new lines, which don't have a unique lineID
				foreach (string sceneFile in sceneFiles)
				{
					GetLinesInScene (sceneFile, true);
				}
				
				GetLinesFromSettings (true);
				GetLinesFromInventory (true);
				GetLinesFromCursors (true);
				GetLinesFromMenus (true);
				
				RestoreTranslations ();
				checkedAssets.Clear ();
				
				if (EditorApplication.currentScene != originalScene)
				{
					EditorApplication.OpenScene (originalScene);
				}
			}
		}
		
		
		private string RemoveLineBreaks (string text)
		{
			return (text.Replace ("\n", "[break]"));
		}
		
		
		private string AddLineBreaks (string text)
		{
			return (text.Replace ("[break]", "\n"));
		}
		
		
		private void ExtractConversation (Conversation conversation, bool onlySeekNew)
		{
			foreach (ButtonDialog dialogOption in conversation.options)
			{
				ExtractDialogOption (dialogOption, onlySeekNew);
			}
			
		}
		
		
		private void ExtractHotspot (Hotspot hotspot, bool onlySeekNew)
		{
			if (hotspot.interactionSource == InteractionSource.AssetFile)
			{
				ProcessActionListAsset (hotspot.useButton.assetFile, onlySeekNew);
				ProcessActionListAsset (hotspot.lookButton.assetFile, onlySeekNew);
				
				foreach (Button _button in hotspot.useButtons)
				{
					ProcessActionListAsset (_button.assetFile, onlySeekNew);
				}
				
				foreach (Button _button in hotspot.invButtons)
				{
					ProcessActionListAsset (_button.assetFile, onlySeekNew);
				}
			}
			
			string hotspotName = hotspot.name;
			if (hotspot.hotspotName != "")
			{
				hotspotName = hotspot.hotspotName;
			}
			
			if (onlySeekNew && hotspot.lineID == -1)
			{
				// Assign a new ID on creation
				SpeechLine newLine = new SpeechLine (GetIDArray(), EditorApplication.currentScene, hotspotName, languages.Count - 1, AC_TextType.Hotspot);
				
				hotspot.lineID = newLine.lineID;
				lines.Add (newLine);
			}
			
			else if (!onlySeekNew && hotspot.lineID > -1)
			{
				// Already has an ID, so don't replace
				SpeechLine existingLine = new SpeechLine (hotspot.lineID, EditorApplication.currentScene, hotspotName, languages.Count - 1, AC_TextType.Hotspot);
				
				int lineID = SmartAddLine (existingLine);
				if (lineID >= 0) hotspot.lineID = lineID;
			}
		}
		
		
		private void ExtractDialogOption (ButtonDialog dialogOption, bool onlySeekNew)
		{
			ProcessActionListAsset (dialogOption.assetFile, onlySeekNew);
			
			if (onlySeekNew && dialogOption.lineID < 1)
			{
				// Assign a new ID on creation
				SpeechLine newLine;
				newLine = new SpeechLine (GetIDArray(), EditorApplication.currentScene, dialogOption.label, languages.Count - 1, AC_TextType.DialogueOption);
				dialogOption.lineID = newLine.lineID;
				lines.Add (newLine);
			}
			
			else if (!onlySeekNew && dialogOption.lineID > 0)
			{
				// Already has an ID, so don't replace
				SpeechLine existingLine = new SpeechLine (dialogOption.lineID, EditorApplication.currentScene, dialogOption.label, languages.Count - 1, AC_TextType.DialogueOption);
				
				int lineID = SmartAddLine (existingLine);
				if (lineID >= 0) dialogOption.lineID = lineID;
			}
		}
		
		
		private void ExtractInventory (InvItem invItem, bool onlySeekNew)
		{
			if (onlySeekNew && invItem.lineID == -1)
			{
				// Assign a new ID on creation
				SpeechLine newLine;
				string _label = invItem.label;
				if (invItem.altLabel != "")
				{
					_label = invItem.altLabel;
				}
				
				newLine = new SpeechLine (GetIDArray(), EditorApplication.currentScene, _label, languages.Count - 1, AC_TextType.InventoryItem);
				invItem.lineID = newLine.lineID;
				lines.Add (newLine);
			}
			
			else if (!onlySeekNew && invItem.lineID > -1)
			{
				// Already has an ID, so don't replace
				string _label = invItem.label;
				if (invItem.altLabel != "")
				{
					_label = invItem.altLabel;
				}
				
				SpeechLine existingLine = new SpeechLine (invItem.lineID, EditorApplication.currentScene, _label, languages.Count - 1, AC_TextType.InventoryItem);
				
				int lineID = SmartAddLine (existingLine);
				if (lineID >= 0) invItem.lineID = lineID;
			}
		}
		
		
		private void ExtractPrefix (HotspotPrefix prefix, bool onlySeekNew)
		{
			if (onlySeekNew && prefix.lineID == -1)
			{
				// Assign a new ID on creation
				SpeechLine newLine;
				newLine = new SpeechLine (GetIDArray(), "", prefix.label, languages.Count - 1, AC_TextType.HotspotPrefix);
				prefix.lineID = newLine.lineID;
				lines.Add (newLine);
			}
			else if (!onlySeekNew && prefix.lineID > -1)
			{
				// Already has an ID, so don't replace
				SpeechLine existingLine = new SpeechLine (prefix.lineID, "", prefix.label, languages.Count - 1, AC_TextType.HotspotPrefix);
				
				int lineID = SmartAddLine (existingLine);
				if (lineID >= 0) prefix.lineID = lineID;
			}
		}
		
		
		private void ExtractIcon (CursorIcon icon, bool onlySeekNew)
		{
			if (onlySeekNew && icon.lineID == -1)
			{
				// Assign a new ID on creation
				SpeechLine newLine;
				newLine = new SpeechLine (GetIDArray(), "", icon.label, languages.Count - 1, AC_TextType.CursorIcon);
				icon.lineID = newLine.lineID;
				lines.Add (newLine);
			}
			
			else if (!onlySeekNew && icon.lineID > -1)
			{
				// Already has an ID, so don't replace
				SpeechLine existingLine = new SpeechLine (icon.lineID, "", icon.label, languages.Count - 1, AC_TextType.CursorIcon);
				
				int lineID = SmartAddLine (existingLine);
				if (lineID >= 0) icon.lineID = lineID;
			}
		}
		
		
		private void ExtractElement (MenuElement element, string elementLabel, bool onlySeekNew)
		{
			if (onlySeekNew && element.lineID == -1)
			{
				// Assign a new ID on creation
				SpeechLine newLine = new SpeechLine (GetIDArray(), "", element.title, elementLabel, languages.Count - 1, AC_TextType.MenuElement);
				element.lineID = newLine.lineID;
				lines.Add (newLine);
			}
			
			else if (!onlySeekNew && element.lineID > -1)
			{
				// Already has an ID, so don't replace
				SpeechLine existingLine = new SpeechLine (element.lineID, "", element.title, elementLabel, languages.Count - 1, AC_TextType.MenuElement);
				
				int lineID = SmartAddLine (existingLine);
				if (lineID >= 0) element.lineID = lineID;
			}
		}
		
		
		private void ExtractHotspotOverride (MenuButton button, string hotspotLabel, bool onlySeekNew)
		{
			if (hotspotLabel == "")
			{
				button.hotspotLabelID = -1;
				return;
			}
			
			if (onlySeekNew && button.lineID == -1)
			{
				// Assign a new ID on creation
				SpeechLine newLine = new SpeechLine (GetIDArray(), "", button.title, hotspotLabel, languages.Count - 1, AC_TextType.MenuElement);
				button.hotspotLabelID = newLine.lineID;
				lines.Add (newLine);
			}
			
			else if (!onlySeekNew && button.hotspotLabelID > -1)
			{
				// Already has an ID, so don't replace
				SpeechLine existingLine = new SpeechLine (button.hotspotLabelID, "", button.title, hotspotLabel, languages.Count - 1, AC_TextType.MenuElement);
				
				int lineID = SmartAddLine (existingLine);
				if (lineID >= 0) button.hotspotLabelID = lineID;
			}
		}
		
		
		private void ExtractJournalElement (MenuJournal journal, List<JournalPage> pages, bool onlySeekNew)
		{
			foreach (JournalPage page in pages)
			{
				if (onlySeekNew && page.lineID == -1)
				{
					// Assign a new ID on creation
					SpeechLine newLine;
					newLine = new SpeechLine (GetIDArray(), "", journal.title, page.text, languages.Count - 1, AC_TextType.JournalEntry);
					page.lineID = newLine.lineID;
					lines.Add (newLine);
				}
				
				else if (!onlySeekNew && page.lineID > -1)
				{
					// Already has an ID, so don't replace
					SpeechLine existingLine = new SpeechLine (page.lineID, "", journal.title, page.text, languages.Count - 1, AC_TextType.JournalEntry);
					
					int lineID = SmartAddLine (existingLine);
					if (lineID >= 0) page.lineID = lineID;
				}
			}
		}
		
		
		private void ExtractSpeech (ActionSpeech action, bool onlySeekNew, bool isInScene)
		{
			string speaker = "";
			bool isPlayer = action.isPlayer;
			
			if (action.isPlayer)
			{
				if (KickStarter.settingsManager && KickStarter.settingsManager.player)
				{
					speaker = KickStarter.settingsManager.player.name;
				}
				else
				{
					speaker = "Player";
				}
			}
			else
			{
				if (!isInScene)
				{
					action.SetSpeaker ();
				}

				if (action.speaker)
				{
					speaker = action.speaker.name;
				}
				else
				{
					speaker = "Narrator";
				}
			}

			if (speaker != "" && action.messageText != "")
			{
				if (onlySeekNew && action.lineID == -1)
				{
					// Assign a new ID on creation
					string _scene = "";
					SpeechLine newLine;
					if (isInScene)
					{
						_scene = EditorApplication.currentScene;
					}
					newLine = new SpeechLine (GetIDArray(), _scene, speaker, action.messageText, languages.Count - 1, AC_TextType.Speech, isPlayer);
					
					action.lineID = newLine.lineID;
					lines.Add (newLine);
				}
				
				else if (!onlySeekNew && action.lineID > -1)
				{
					// Already has an ID, so don't replace
					string _scene = "";
					SpeechLine existingLine;
					if (isInScene)
					{
						_scene = EditorApplication.currentScene;
					}
					existingLine = new SpeechLine (action.lineID, _scene, speaker, action.messageText, languages.Count - 1, AC_TextType.Speech, isPlayer);
					
					int lineID = SmartAddLine (existingLine);
					if (lineID >= 0) action.lineID = lineID;
				}
			}
			else
			{
				// Remove from SpeechManager
				action.lineID = -1;
			}
		}


		private void ExtractHotspotName (ActionRename action, bool onlySeekNew, bool isInScene)
		{
			if (action.newName != "")
			{
				string _scene = "";
				if (isInScene)
				{
					_scene = EditorApplication.currentScene;
				}

				if (onlySeekNew && action.lineID == -1)
				{
					// Assign a new ID on creation
					SpeechLine newLine = new SpeechLine (GetIDArray(), _scene, action.newName, languages.Count - 1, AC_TextType.Hotspot);

					action.lineID = newLine.lineID;
					lines.Add (newLine);
				}
				
				else if (!onlySeekNew && action.lineID > -1)
				{
					// Already has an ID, so don't replace
					SpeechLine existingLine = new SpeechLine (action.lineID, _scene, action.newName, languages.Count - 1, AC_TextType.Hotspot);

					int lineID = SmartAddLine (existingLine);
					if (lineID >= 0) action.lineID = lineID;
				}
			}
			else
			{
				// Remove from SpeechManager
				action.lineID = -1;
			}
		}
		
		
		private int SmartAddLine (SpeechLine existingLine)
		{
			if (!DoLinesMatchText (existingLine))
			{
				if (DoLinesMatchID (existingLine.lineID))
				{
					// Same ID, different text, so re-assign ID
					int lineID = 0;
					
					foreach (int _id in GetIDArray ())
					{
						if (lineID == _id)
							lineID ++;
					}
					
					existingLine.lineID = lineID;
					lines.Add (existingLine);
					return lineID;
				}
				else
				{
					lines.Add (existingLine);
				}
			}
			return -1;
		}
		
		
		private bool DoLinesMatchID (int newLineID)
		{
			if (lines == null || lines.Count == 0)
			{
				return false;
			}
			
			foreach (SpeechLine line in lines)
			{
				if (line.lineID == newLineID)
				{
					return true;
				}
			}
			return false;
		}
		
		
		private bool DoLinesMatchText (SpeechLine newLine)
		{
			if (lines == null || lines.Count == 0)
			{
				return false;
			}
			
			foreach (SpeechLine line in lines)
			{
				if (line.IsMatch (newLine))
				{
					return true;
				}
			}
			return false;
		}
		
		
		private void ExtractJournalEntry (ActionMenuState action, bool onlySeekNew, bool isInScene)
		{
			if (action.changeType == ActionMenuState.MenuChangeType.AddJournalPage && action.journalText != "")
			{
				if (onlySeekNew && action.lineID == -1)
				{
					// Assign a new ID on creation
					SpeechLine newLine;
					if (isInScene)
					{
						newLine = new SpeechLine (GetIDArray(), EditorApplication.currentScene, action.journalText, languages.Count - 1, AC_TextType.JournalEntry);
					}
					else
					{
						newLine = new SpeechLine (GetIDArray(), "", action.journalText, languages.Count - 1, AC_TextType.JournalEntry);
					}
					action.lineID = newLine.lineID;
					lines.Add (newLine);
				}
				
				else if (!onlySeekNew && action.lineID > -1)
				{
					// Already has an ID, so don't replace
					SpeechLine existingLine;
					if (isInScene)
					{
						existingLine = new SpeechLine (action.lineID, EditorApplication.currentScene, action.journalText, languages.Count - 1, AC_TextType.JournalEntry);
					}
					else
					{
						existingLine = new SpeechLine (action.lineID, "", action.journalText, languages.Count - 1, AC_TextType.JournalEntry);
					}
					
					int lineID = SmartAddLine (existingLine);
					if (lineID >= 0) action.lineID = lineID;
				}
			}
			else
			{
				// Remove from SpeechManager
				action.lineID = -1;
			}
		}
		
		
		private void GetLinesFromSettings (bool onlySeekNew)
		{
			SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
			
			if (settingsManager)
			{
				ProcessActionListAsset (settingsManager.actionListOnStart, onlySeekNew);
				
				if (settingsManager.activeInputs != null)
				{
					foreach (ActiveInput activeInput in settingsManager.activeInputs)
					{
						ProcessActionListAsset (activeInput.actionListAsset, onlySeekNew);
					}
				}
			}
		}
		
		
		private void GetLinesFromInventory (bool onlySeekNew)
		{
			InventoryManager inventoryManager = AdvGame.GetReferences ().inventoryManager;
			
			if (inventoryManager)
			{
				ProcessActionListAsset (inventoryManager.unhandledCombine, onlySeekNew);
				ProcessActionListAsset (inventoryManager.unhandledHotspot, onlySeekNew);
				ProcessActionListAsset (inventoryManager.unhandledGive, onlySeekNew);
				
				// Item-specific events
				if (inventoryManager.items.Count > 0)
				{
					foreach (InvItem item in inventoryManager.items)
					{
						// Label
						ExtractInventory (item, onlySeekNew);

						// Prefixes
						if (item.overrideUseSyntax)
						{
							ExtractPrefix (item.hotspotPrefix1, onlySeekNew);
							ExtractPrefix (item.hotspotPrefix2, onlySeekNew);
						}

						// ActionLists
						ProcessActionListAsset (item.useActionList, onlySeekNew);
						ProcessActionListAsset (item.lookActionList, onlySeekNew);
						ProcessActionListAsset (item.unhandledActionList, onlySeekNew);
						ProcessActionListAsset (item.unhandledCombineActionList, onlySeekNew);
						foreach (ActionListAsset actionList in item.combineActionList)
						{
							ProcessActionListAsset (actionList, onlySeekNew);
						}
					}
				}
				
				foreach (Recipe recipe in inventoryManager.recipes)
				{
					ProcessActionListAsset (recipe.invActionList, onlySeekNew);
				}
				
				EditorUtility.SetDirty (inventoryManager);
			}
		}
		
		
		private void GetLinesFromMenus (bool onlySeekNew)
		{
			MenuManager menuManager = AdvGame.GetReferences ().menuManager;
			
			if (menuManager)
			{
				// Gather elements
				if (menuManager.menus.Count > 0)
				{
					foreach (AC.Menu menu in menuManager.menus)
					{
						ProcessActionListAsset (menu.actionListOnTurnOff, onlySeekNew);
						ProcessActionListAsset (menu.actionListOnTurnOn, onlySeekNew);
						
						foreach (MenuElement element in menu.elements)
						{
							if (element is MenuButton)
							{
								MenuButton button = (MenuButton) element;
								ExtractElement (element, button.label, onlySeekNew);
								ExtractHotspotOverride (button, button.hotspotLabel, onlySeekNew);
								
								if (button.buttonClickType == AC_ButtonClickType.RunActionList)
								{
									ProcessActionListAsset (button.actionList, onlySeekNew);
								}
							}
							else if (element is MenuCycle)
							{
								MenuCycle button = (MenuCycle) element;
								ExtractElement (element, button.label, onlySeekNew);
							}
							else if (element is MenuDrag)
							{
								MenuDrag button = (MenuDrag) element;
								ExtractElement (element, button.label, onlySeekNew);
							}
							else if (element is MenuInput)
							{
								MenuInput button = (MenuInput) element;
								ExtractElement (element, button.label, onlySeekNew);
							}
							else if (element is MenuLabel)
							{
								MenuLabel button = (MenuLabel) element;
								ExtractElement (element, button.label, onlySeekNew);
							}
							else if (element is MenuSavesList)
							{
								MenuSavesList button = (MenuSavesList) element;
								ExtractElement (element, button.newSaveText, onlySeekNew);
								ProcessActionListAsset (button.actionListOnSave, onlySeekNew);
							}
							else if (element is MenuSlider)
							{
								MenuSlider button = (MenuSlider) element;
								ExtractElement (element, button.label, onlySeekNew);
							}
							else if (element is MenuToggle)
							{
								MenuToggle button = (MenuToggle) element;
								ExtractElement (element, button.label, onlySeekNew);
							}
							else if (element is MenuJournal)
							{
								MenuJournal journal = (MenuJournal) element;
								ExtractJournalElement (journal, journal.pages, onlySeekNew);
							}
						}
					}
				}
				
				EditorUtility.SetDirty (menuManager);
			}
		}
		
		
		private void GetLinesFromCursors (bool onlySeekNew)
		{
			CursorManager cursorManager = AdvGame.GetReferences ().cursorManager;
			
			if (cursorManager)
			{
				// Prefixes
				ExtractPrefix (cursorManager.hotspotPrefix1, onlySeekNew);
				ExtractPrefix (cursorManager.hotspotPrefix2, onlySeekNew);
				ExtractPrefix (cursorManager.hotspotPrefix3, onlySeekNew);
				ExtractPrefix (cursorManager.hotspotPrefix4, onlySeekNew);
				ExtractPrefix (cursorManager.walkPrefix, onlySeekNew);
				
				foreach (ActionListAsset actionListAsset in cursorManager.unhandledCursorInteractions)
				{
					ProcessActionListAsset (actionListAsset, onlySeekNew);
				}
				
				// Gather icons
				if (cursorManager.cursorIcons.Count > 0)
				{
					foreach (CursorIcon icon in cursorManager.cursorIcons)
					{
						ExtractIcon (icon, onlySeekNew);
					}
				}
				
				EditorUtility.SetDirty (cursorManager);
			}
		}
		
		
		private void GetLinesInScene (string sceneFile, bool onlySeekNew)
		{
			if (EditorApplication.currentScene != sceneFile)
			{
				EditorApplication.OpenScene (sceneFile);
			}
			
			// Speech lines and journal entries
			ActionList[] actionLists = GameObject.FindObjectsOfType (typeof (ActionList)) as ActionList[];
			foreach (ActionList list in actionLists)
			{
				if (list.source == ActionListSource.AssetFile)
				{
					ProcessActionListAsset (list.assetFile, onlySeekNew);
				}
				else
				{
					ProcessActionList (list, onlySeekNew);
				}
			}
			
			// Hotspots
			Hotspot[] hotspots = GameObject.FindObjectsOfType (typeof (Hotspot)) as Hotspot[];
			foreach (Hotspot hotspot in hotspots)
			{
				ExtractHotspot (hotspot, onlySeekNew);
				EditorUtility.SetDirty (hotspot);
			}
			
			
			// Dialogue options
			Conversation[] conversations = GameObject.FindObjectsOfType (typeof (Conversation)) as Conversation[];
			foreach (Conversation conversation in conversations)
			{
				ExtractConversation (conversation, onlySeekNew);
				EditorUtility.SetDirty (conversation);
			}
			
			// Save the scene
			EditorApplication.SaveScene ();
			EditorUtility.SetDirty (this);
		}
		
		
		private int[] GetIDArray ()
		{
			// Returns a list of id's in the list
			
			List<int> idArray = new List<int>();
			
			foreach (SpeechLine line in lines)
			{
				idArray.Add (line.lineID);
			}
			
			idArray.Sort ();
			return idArray.ToArray ();
		}
		
		
		private void RestoreTranslations ()
		{
			// Match IDs for each entry in lines and tempLines, send over translation data
			foreach (SpeechLine tempLine in tempLines)
			{
				foreach (SpeechLine line in lines)
				{
					if (tempLine.lineID == line.lineID)
					{
						line.translationText = tempLine.translationText;
						line.description = tempLine.description;
						break;
					}
				}
			}
			
			tempLines = null;
		}
		
		
		private void BackupTranslations ()
		{
			tempLines = new List<SpeechLine>();
			foreach (SpeechLine line in lines)
			{
				tempLines.Add (line);
			}
		}
		
		
		private void ImportTranslation (int i)
		{
			string fileName = EditorUtility.OpenFilePanel ("Import " + languages[i] + " translation", "Assets", "csv");
			if (fileName.Length == 0)
			{
				return;
			}
			
			if (File.Exists (fileName))
			{
				string csvText = Serializer.LoadSaveFile (fileName, true);
				string [,] csvOutput = CSVReader.SplitCsvGrid (csvText);
				
				int lineID = 0;
				string translationText = "";
				string owner = "";
				
				for (int y = 1; y < csvOutput.Length; y++)
				{
					try
					{
						lineID = int.Parse (csvOutput [0, y]);
						translationText = csvOutput [3, y].Replace (CSVReader.csvTemp, CSVReader.csvComma);
						string typeText = csvOutput [1, y].Replace (CSVReader.csvTemp, CSVReader.csvComma);
						
						if (typeText.Contains ("JournalEntry (Page "))
						{
							owner = typeText.Replace ("JournalEntry (", "");
							owner = owner.Replace (")", "");
						}
						else
						{
							owner = "";
						}
						
						UpdateTranslation (i, lineID, owner, AddLineBreaks (translationText));
					}
					catch
					{}
				}
				
				EditorUtility.SetDirty (this);
			}
			else
			{
				Debug.LogWarning ("No CSV file found.  Looking for: " + fileName);
			}
		}
		
		
		private void UpdateTranslation (int i, int _lineID, string _owner, string translationText)
		{
			foreach (SpeechLine line in lines)
			{
				if (line.lineID == _lineID)
				{
					line.translationText [i-1] = translationText;
				}
			}
		}
		
		
		private void ImportGameText ()
		{
			string fileName = EditorUtility.OpenFilePanel ("Import game text", "Assets", "csv");
			if (fileName.Length == 0)
			{
				return;
			}
			
			if (File.Exists (fileName))
			{
				string csvText = Serializer.LoadSaveFile (fileName, true);
				string [,] csvOutput = CSVReader.SplitCsvGrid (csvText);
				
				int lineID = 0;
				string translationText = "";
				string owner = "";
				
				for (int y = 1; y < csvOutput.Length; y++)
				{
					try
					{
						lineID = int.Parse (csvOutput [0, y]);
						string typeText = csvOutput [1, y].Replace (CSVReader.csvTemp, CSVReader.csvComma);
						
						if (typeText.Contains ("JournalEntry (Page "))
						{
							owner = typeText.Replace ("JournalEntry (", "");
							owner = owner.Replace (")", "");
						}
						else
						{
							owner = "";
						}
						
						if (languages.Count > 1)
						{
							for (int i=1; i<languages.Count; i++)
							{
								translationText = csvOutput [i+2, y].Replace (CSVReader.csvTemp, CSVReader.csvComma);
								UpdateTranslation (i, lineID, owner, AddLineBreaks (translationText));
							}
						}
					}
					catch
					{}
				}
				
				EditorUtility.SetDirty (this);
			}
			else
			{
				Debug.LogWarning ("No CSV file found.  Looking for: " + fileName);
			}
		}
		
		
		private void ExportGameText ()
		{
			#if UNITY_WEBPLAYER
			Debug.LogWarning ("Game text cannot be exported in WebPlayer mode - please switch platform and try again.");
			return;
			#else
			
			string suggestedFilename = "";
			if (AdvGame.GetReferences ().settingsManager)
			{
				suggestedFilename = AdvGame.GetReferences ().settingsManager.saveFileName + " - ";
			}
			suggestedFilename += "AllGameText.csv";
			
			string fileName = EditorUtility.SaveFilePanel ("Export game text", "Assets", suggestedFilename, "csv");
			if (fileName.Length == 0)
			{
				return;
			}
			
			bool fail = false;
			List<string[]> output = new List<string[]>();
			
			List<string> headerList = new List<string>();
			headerList.AddRange (new [] { "ID", "Type", "Original text" });
			if (languages.Count > 1)
			{
				for (int i=1; i<languages.Count; i++)
				{
					headerList.Add (languages [i]);
				}
			}
			output.Add (headerList.ToArray ());
			
			foreach (SpeechLine line in lines)
			{
				List<string> rowList = new List<string>();
				rowList.AddRange (new [] { 
					line.lineID.ToString (), 
					line.GetInfo (),
					RemoveLineBreaks (line.text)
				});
				
				foreach (string translationText in line.translationText)
				{
					rowList.Add (RemoveLineBreaks (translationText));
					
					if (translationText.Contains (CSVReader.csvDelimiter))
					{
						fail = true;
					}
				}
				
				output.Add (rowList.ToArray ());
				
				if (line.textType != AC_TextType.JournalEntry && line.text.Contains (CSVReader.csvDelimiter))
				{
					fail = true;
				}
				
				if (fail)
				{
					Debug.LogError ("Cannot export translation since line " + line.lineID.ToString () + " contains the character '" + CSVReader.csvDelimiter + "'.");
				}
			}
			
			if (!fail)
			{
				int length = output.Count;
				
				StringBuilder sb = new StringBuilder();
				for (int j=0; j<length; j++)
				{
					sb.AppendLine (string.Join (CSVReader.csvDelimiter, output[j]));
				}
				
				Serializer.CreateSaveFile (fileName, sb.ToString ());
			}
			
			#endif
		}
		
		
		private void ExportTranslation (int i)
		{
			#if UNITY_WEBPLAYER
			Debug.LogWarning ("Game text cannot be exported in WebPlayer mode - please switch platform and try again.");
			return;
			#else
			
			string suggestedFilename = "";
			if (AdvGame.GetReferences ().settingsManager)
			{
				suggestedFilename = AdvGame.GetReferences ().settingsManager.saveFileName + " - ";
			}
			suggestedFilename += languages[i].ToString () + ".csv";
			
			string fileName = EditorUtility.SaveFilePanel ("Export " + languages[i] + " translation", "Assets", suggestedFilename, "csv");
			if (fileName.Length == 0)
			{
				return;
			}
			
			bool fail = false;
			List<string[]> output = new List<string[]>();
			output.Add (new string[] {"ID", "Type", "Original line", languages[i] + " translation"});
			
			foreach (SpeechLine line in lines)
			{
				output.Add (new string[] 
				            {
					line.lineID.ToString (),
					line.GetInfo (),
					RemoveLineBreaks (line.text),
					RemoveLineBreaks (line.translationText [i-1])
				});
				
				if (line.textType != AC_TextType.JournalEntry && (line.text.Contains (CSVReader.csvDelimiter) || line.translationText [i-1].Contains (CSVReader.csvDelimiter)))
				{
					fail = true;
					Debug.LogError ("Cannot export translation since line " + line.lineID.ToString () + " contains the character '" + CSVReader.csvDelimiter + "'.");
				}
			}
			
			if (!fail)
			{
				int length = output.Count;
				
				StringBuilder sb = new StringBuilder();
				for (int j=0; j<length; j++)
				{
					string newLine = string.Join (CSVReader.csvDelimiter, output[j]);
					newLine = newLine.Replace ("\r", "");
					sb.AppendLine (newLine);
				}
				
				Serializer.CreateSaveFile (fileName, sb.ToString ());
			}
			
			#endif
		}
		
		
		private void CreateScript ()
		{
			#if UNITY_WEBPLAYER
			Debug.LogWarning ("Game text cannot be exported in WebPlayer mode - please switch platform and try again.");
			#else
			
			string suggestedFilename = "";
			if (AdvGame.GetReferences ().settingsManager)
			{
				suggestedFilename = AdvGame.GetReferences ().settingsManager.saveFileName + " - ";
			}
			suggestedFilename += "script.txt";
			
			string fileName = EditorUtility.SaveFilePanel ("Save script file", "Assets", suggestedFilename, "txt");
			if (fileName.Length == 0)
			{
				return;
			}
			
			string script = "Script file created " + DateTime.UtcNow.ToString("HH:mm dd MMMM, yyyy");
			
			// By scene
			foreach (string scene in sceneFiles)
			{
				bool foundLinesInScene = false;
				
				foreach (SpeechLine line in lines)
				{
					if (line.scene == scene && line.textType == AC_TextType.Speech)
					{
						if (!foundLinesInScene)
						{
							script += "\n";
							script += "\n";
							script += "Scene: " + scene;
							foundLinesInScene = true;
						}
						
						script += "\n";
						script += "\n";
						script += line.Print ();
					}
				}
			}
			
			// No scene
			bool foundLinesInInventory = false;
			
			foreach (SpeechLine line in lines)
			{
				if (line.scene == "" && line.textType == AC_TextType.Speech)
				{
					if (!foundLinesInInventory)
					{
						script += "\n";
						script += "\n";
						script += "Scene-independent lines: ";
						foundLinesInInventory = true;
					}
					
					script += "\n";
					script += "\n";
					script += line.Print ();
				}
			}
			
			Serializer.CreateSaveFile (fileName, script);
			
			#endif
		}
		
		
		private void ClearList ()
		{
			if (EditorUtility.DisplayDialog ("Reset all translation lines?", "This will completely reset the IDs of every text line in your game, removing any supplied translations and invalidaing speech audio filenames. Continue?", "OK", "Cancel"))
			{
				string originalScene = EditorApplication.currentScene;
				
				if (EditorApplication.SaveCurrentSceneIfUserWantsTo ())
				{
					lines.Clear ();
					checkedAssets.Clear ();
					
					sceneFiles = AdvGame.GetSceneFiles ();
					GetSceneNames ();
					
					// First look for lines that already have an assigned lineID
					foreach (string sceneFile in sceneFiles)
					{
						ClearLinesInScene (sceneFile);
					}
					
					ClearLinesFromSettings ();
					ClearLinesFromInventory ();
					ClearLinesFromCursors ();
					ClearLinesFromMenus ();
					
					checkedAssets.Clear ();
					
					if (EditorApplication.currentScene != originalScene)
					{
						EditorApplication.OpenScene (originalScene);
					}
				}
			}
		}
		
		
		private void ClearLinesInScene (string sceneFile)
		{
			if (EditorApplication.currentScene != sceneFile)
			{
				EditorApplication.OpenScene (sceneFile);
			}
			
			// Speech lines and journal entries
			ActionList[] actionLists = GameObject.FindObjectsOfType (typeof (ActionList)) as ActionList[];
			foreach (ActionList list in actionLists)
			{
				if (list.source == ActionListSource.AssetFile)
				{
					if (list.assetFile != null)
					{
						ClearLinesFromActionListAsset (list.assetFile);
					}
				}
				else
				{
					ClearLinesFromActionList (list);
				}
			}
			
			// Hotspots
			Hotspot[] hotspots = GameObject.FindObjectsOfType (typeof (Hotspot)) as Hotspot[];
			foreach (Hotspot hotspot in hotspots)
			{
				ClearLinesFromActionListAsset (hotspot.useButton.assetFile);
				ClearLinesFromActionListAsset (hotspot.lookButton.assetFile);
				
				foreach (Button _button in hotspot.useButtons)
				{
					ClearLinesFromActionListAsset (_button.assetFile);
				}
				
				foreach (Button _button in hotspot.invButtons)
				{
					ClearLinesFromActionListAsset (_button.assetFile);
				}
				
				hotspot.lineID = -1;
				EditorUtility.SetDirty (hotspot);
			}
			
			// Dialogue options
			Conversation[] conversations = GameObject.FindObjectsOfType (typeof (Conversation)) as Conversation[];
			foreach (Conversation conversation in conversations)
			{
				foreach (ButtonDialog dialogOption in conversation.options)
				{
					ClearLinesFromActionListAsset (dialogOption.assetFile);
					dialogOption.lineID = -1;
				}
				EditorUtility.SetDirty (conversation);
			}
			
			// Save the scene
			EditorApplication.SaveScene ();
			EditorUtility.SetDirty (this);
		}
		
		
		private void ClearLinesFromActionListAsset (ActionListAsset actionListAsset)
		{
			if (actionListAsset != null && !checkedAssets.Contains (actionListAsset))
			{
				checkedAssets.Add (actionListAsset);
				ClearLines (actionListAsset.actions);
				EditorUtility.SetDirty (actionListAsset);
			}
		}
		
		
		private void ClearLinesFromActionList (ActionList actionList)
		{
			if (actionList != null)
			{
				ClearLines (actionList.actions);
				EditorUtility.SetDirty (actionList);
			}
		}
		
		
		private void ClearLines (List<Action> actions)
		{
			if (actions == null)
			{
				return;
			}
			
			foreach (Action action in actions)
			{
				if (action is ActionSpeech)
				{
					ActionSpeech actionSpeech = (ActionSpeech) action;
					actionSpeech.lineID = -1;
				}
				else if (action is ActionMenuState)
				{
					ActionMenuState actionMenuState = (ActionMenuState) action;
					actionMenuState.lineID = -1;
				}
				else if (action is ActionRunActionList)
				{
					ActionRunActionList runActionList = (ActionRunActionList) action;
					if (runActionList.listSource == ActionRunActionList.ListSource.AssetFile)
					{
						ClearLinesFromActionListAsset (runActionList.invActionList);
					}
				}
				else if (action.isAssetFile)
				{
					if (action is ActionCheck)
					{
						ActionCheck actionCheck = (ActionCheck) action;
						if (actionCheck.resultActionTrue == ResultAction.RunCutscene)
						{
							ClearLinesFromActionListAsset (actionCheck.linkedAssetTrue);
						}
						if (actionCheck.resultActionFail == ResultAction.RunCutscene)
						{
							ClearLinesFromActionListAsset (actionCheck.linkedAssetFail);
						}
					}
					else if (action is ActionCheckMultiple)
					{
						ActionCheckMultiple actionCheckMultiple = (ActionCheckMultiple) action;
						foreach (ActionEnd ending in actionCheckMultiple.endings)
						{
							if (ending.resultAction == ResultAction.RunCutscene)
							{
								ClearLinesFromActionListAsset (actionCheckMultiple.linkedAsset);
							}
						}
					}
					else if (action is ActionParallel)
					{
						ActionParallel actionParallel = (ActionParallel) action;
						foreach (ActionEnd ending in actionParallel.endings)
						{
							if (ending.resultAction == ResultAction.RunCutscene)
							{
								ClearLinesFromActionListAsset (actionParallel.linkedAsset);
							}
						}
					}
					else
					{
						if (action.endAction == ResultAction.RunCutscene)
						{
							ClearLinesFromActionListAsset (action.linkedAsset);
						}
					}
				}
			}
			
		}
		
		
		private void ClearLinesFromSettings ()
		{
			SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
			if (settingsManager)
			{
				ClearLinesFromActionListAsset (settingsManager.actionListOnStart);
			}
		}
		
		
		private void ClearLinesFromInventory ()
		{
			InventoryManager inventoryManager = AdvGame.GetReferences ().inventoryManager;
			
			if (inventoryManager)
			{
				ClearLinesFromActionListAsset (inventoryManager.unhandledCombine);
				ClearLinesFromActionListAsset (inventoryManager.unhandledHotspot);
				ClearLinesFromActionListAsset (inventoryManager.unhandledGive);
				
				foreach (Recipe recipe in inventoryManager.recipes)
				{
					ClearLinesFromActionListAsset (recipe.invActionList);
				}
				
				// Item-specific events
				if (inventoryManager.items.Count > 0)
				{
					foreach (InvItem item in inventoryManager.items)
					{
						// Label
						item.lineID = -1;
						
						ClearLinesFromActionListAsset (item.useActionList);
						ClearLinesFromActionListAsset (item.lookActionList);
						
						foreach (ActionListAsset actionList in item.combineActionList)
						{
							ClearLinesFromActionListAsset (actionList);
						}
					}
				}
				
				EditorUtility.SetDirty (inventoryManager);
			}
		}
		
		
		private void ClearLinesFromCursors ()
		{
			CursorManager cursorManager = AdvGame.GetReferences ().cursorManager;
			
			if (cursorManager)
			{
				// Prefixes
				cursorManager.hotspotPrefix1.lineID = -1;
				cursorManager.hotspotPrefix2.lineID = -1;
				cursorManager.hotspotPrefix3.lineID = -1;
				cursorManager.hotspotPrefix4.lineID = -1;
				cursorManager.walkPrefix.lineID = -1;
				
				foreach (ActionListAsset actionListAsset in cursorManager.unhandledCursorInteractions)
				{
					ClearLinesFromActionListAsset (actionListAsset);
				}
				
				// Gather icons
				if (cursorManager.cursorIcons.Count > 0)
				{
					foreach (CursorIcon icon in cursorManager.cursorIcons)
					{
						icon.lineID = -1;
					}
				}
				
				EditorUtility.SetDirty (cursorManager);
			}
		}
		
		
		private void ClearLinesFromMenus ()
		{
			MenuManager menuManager = AdvGame.GetReferences ().menuManager;
			
			if (menuManager)
			{
				// Gather elements
				if (menuManager.menus.Count > 0)
				{
					foreach (AC.Menu menu in menuManager.menus)
					{
						ClearLinesFromActionListAsset (menu.actionListOnTurnOff);
						ClearLinesFromActionListAsset (menu.actionListOnTurnOn);
						
						foreach (MenuElement element in menu.elements)
						{
							if (element is MenuButton)
							{
								MenuButton button = (MenuButton) element;
								button.lineID = -1;
								button.hotspotLabelID = -1;
								
								if (button.buttonClickType == AC_ButtonClickType.RunActionList)
								{
									ClearLinesFromActionListAsset (button.actionList);
								}
							}
							else if (element is MenuCycle)
							{
								MenuCycle button = (MenuCycle) element;
								button.lineID = -1;
							}
							else if (element is MenuDrag)
							{
								MenuDrag button = (MenuDrag) element;
								button.lineID = -1;
							}
							else if (element is MenuInput)
							{
								MenuInput button = (MenuInput) element;
								button.lineID = -1;
							}
							else if (element is MenuLabel)
							{
								MenuLabel button = (MenuLabel) element;
								button.lineID = -1;
							}
							else if (element is MenuSavesList)
							{
								MenuSavesList button = (MenuSavesList) element;
								button.lineID = -1;
								
								if (button.saveListType == AC_SaveListType.Save)
								{
									ClearLinesFromActionListAsset (button.actionListOnSave);
								}
							}
							else if (element is MenuSlider)
							{
								MenuSlider button = (MenuSlider) element;
								button.lineID = -1;
							}
							else if (element is MenuToggle)
							{
								MenuToggle button = (MenuToggle) element;
								button.lineID = -1;
							}
							else if (element is MenuJournal)
							{
								MenuJournal journal = (MenuJournal) element;
								journal.lineID = -1;
							}
						}
					}
				}
				
				EditorUtility.SetDirty (menuManager);
			}		
		}
		
		
		private void ProcessActionListAsset (ActionListAsset actionListAsset, bool onlySeekNew)
		{
			if (actionListAsset != null && !checkedAssets.Contains (actionListAsset))
			{
				checkedAssets.Add (actionListAsset);
				ProcessActions (actionListAsset.actions, onlySeekNew, false);
				EditorUtility.SetDirty (actionListAsset);
			}
		}
		
		
		private void ProcessActionList (ActionList actionList, bool onlySeekNew)
		{
			if (actionList != null)
			{
				ProcessActions (actionList.actions, onlySeekNew, true);
				EditorUtility.SetDirty (actionList);
			}
			
		}
		
		
		private void ProcessActions (List<Action> actions, bool onlySeekNew, bool isInScene)
		{
			foreach (Action action in actions)
			{
				if (action == null)
				{
					continue;
				}
				
				if (action is ActionSpeech)
				{
					ExtractSpeech (action as ActionSpeech, onlySeekNew, isInScene);
				}
				else if (action is ActionRename)
				{
					ExtractHotspotName (action as ActionRename, onlySeekNew, isInScene);
				}
				else if (action is ActionMenuState)
				{
					ExtractJournalEntry (action as ActionMenuState, onlySeekNew, isInScene);
				}
				else if (action is ActionRunActionList)
				{
					ActionRunActionList runActionList = (ActionRunActionList) action;
					if (runActionList.listSource == ActionRunActionList.ListSource.AssetFile)
					{
						ProcessActionListAsset (runActionList.invActionList, onlySeekNew);
					}
				}
				
				if (action is ActionCheck)
				{
					ActionCheck actionCheck = (ActionCheck) action;
					if (actionCheck.resultActionTrue == ResultAction.RunCutscene)
					{
						ProcessActionListAsset (actionCheck.linkedAssetTrue, onlySeekNew);
					}
					if (actionCheck.resultActionFail == ResultAction.RunCutscene)
					{
						ProcessActionListAsset (actionCheck.linkedAssetFail, onlySeekNew);
					}
				}
				else if (action is ActionCheckMultiple)
				{
					ActionCheckMultiple actionCheckMultiple = (ActionCheckMultiple) action;
					foreach (ActionEnd ending in actionCheckMultiple.endings)
					{
						if (ending.resultAction == ResultAction.RunCutscene)
						{
							ProcessActionListAsset (actionCheckMultiple.linkedAsset, onlySeekNew);
						}
					}
				}
				else if (action is ActionParallel)
				{
					ActionParallel actionParallel = (ActionParallel) action;
					foreach (ActionEnd ending in actionParallel.endings)
					{
						if (ending.resultAction == ResultAction.RunCutscene)
						{
							ProcessActionListAsset (actionParallel.linkedAsset, onlySeekNew);
						}
					}
				}
				else
				{
					if (action.endAction == ResultAction.RunCutscene)
					{
						ProcessActionListAsset (action.linkedAsset, onlySeekNew);
					}
				}
			}
		}
		
		
		#endif
		
		
		public static string GetTranslation (string originalText, int _lineID, int language)
		{
			if (language == 0)
			{
				return originalText;
			}
			
			if (_lineID == -1)
			{
				Debug.Log ("Cannot find translation because the text has not been added to the Speech Manager.");
				return originalText;
			}
			else
			{
				foreach (SpeechLine line in AdvGame.GetReferences ().speechManager.lines)
				{
					if (line.lineID == _lineID)
					{
						if (line.translationText.Count > (language-1))
						{
							return line.translationText [language-1];
						}
						else
						{
							Debug.LogWarning ("A translation is being requested that does not exist!");
						}
					}
				}
			}
			return "";
		}
		
		
		public string GetLineFilename (int _lineID)
		{
			foreach (SpeechLine line in lines)
			{
				if (line.lineID == _lineID)
				{
					return line.GetFilename ();
				}
			}
			return "";
		}


		public bool UseFileBasedLipSyncing ()
		{
			if (lipSyncMode == LipSyncMode.ReadPamelaFile || lipSyncMode == LipSyncMode.ReadPapagayoFile || lipSyncMode == LipSyncMode.ReadSapiFile)
			{
				return true;
			}
			return false;
		}
		
	}
	
}