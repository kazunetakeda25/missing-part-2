using UnityEngine;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.UnityGUI;

namespace PixelCrushers.DialogueSystem.Examples {
	
	/// <summary>
	/// This script provides a rudimentary main menu for the Feature Demo.
	/// </summary>
	public class FeatureDemo : MonoBehaviour {
		
		public GUISkin guiSkin;
		public QuestLogWindow questLogWindow;
		
		private bool isMenuOpen = false;
		private Rect windowRect = new Rect(0, 0, 500, 500);
		private ScaledRect scaledRect = ScaledRect.FromOrigin(ScaledRectAlignment.MiddleCenter, ScaledValue.FromPixelValue(300), ScaledValue.FromPixelValue(320));
		
		void Start() {
			DialogueManager.ShowAlert("Press Escape for Menu");
		}
		
		void Update() {
			if (Input.GetKeyDown(KeyCode.Escape) && !DialogueManager.IsConversationActive && !IsQuestLogOpen()) {
				SetMenuStatus(!isMenuOpen);
			}
			// If you want to lock the cursor during gameplay, add ShowCursorOnConversation to the Player,
			// and uncomment the code below:
			//if (!DialogueManager.IsConversationActive && !isMenuOpen && !IsQuestLogOpen ()) {
			//	Screen.lockCursor = true;
			//}
		}
		
		void OnGUI() {
			if (isMenuOpen) {
				if (guiSkin != null) {
					GUI.skin = guiSkin;
				}
				windowRect = GUI.Window(0, windowRect, WindowFunction, "Menu");
			}
		}
		
		private void WindowFunction(int windowID) {
			if (GUI.Button(new Rect(10, 60, windowRect.width - 20, 48), "Quest Log")) {
				SetMenuStatus(false);
				OpenQuestLog();
			}
			if (GUI.Button(new Rect(10, 110, windowRect.width - 20, 48), "Save Game")) {
				SetMenuStatus(false);
				SaveGame();
			}
			if (GUI.Button(new Rect(10, 160, windowRect.width - 20, 48), "Load Game")) {
				SetMenuStatus(false);
				LoadGame();
			}
			if (GUI.Button(new Rect(10, 210, windowRect.width - 20, 48), "Clear Saved Game")) {
				SetMenuStatus(false);
				ClearSavedGame();
			}
			if (GUI.Button(new Rect(10, 260, windowRect.width - 20, 48), "Close Menu")) {
				SetMenuStatus(false);
			}
		}
		
		private void SetMenuStatus(bool open) {
			isMenuOpen = open;
			if (open) windowRect = scaledRect.GetPixelRect();
			Time.timeScale = open ? 0 : 1;
		}
		
		private bool IsQuestLogOpen() {
			return (questLogWindow != null) && questLogWindow.IsOpen;
		}
		
		private void OpenQuestLog() {
			if ((questLogWindow != null) && !IsQuestLogOpen()) {
				questLogWindow.Open();
			}
		}
		
		private void SaveGame() {
			string saveData = PersistentDataManager.GetSaveData();
			PlayerPrefs.SetString("SavedGame", saveData);
			Debug.Log("Save Game Data: " + saveData);
			DialogueManager.ShowAlert("Game Saved to PlayerPrefs");
		}
	
		private void LoadGame() {
			if (PlayerPrefs.HasKey("SavedGame")) {
				string saveData = PlayerPrefs.GetString("SavedGame");
				Debug.Log("Load Game Data: " + saveData);
				LevelManager levelManager = GetComponentInChildren<LevelManager>();
				if (levelManager != null) {
					levelManager.LoadGame(saveData);
				} else {
					PersistentDataManager.ApplySaveData(saveData);
				}
				DialogueManager.ShowAlert("Game Loaded from PlayerPrefs");
			} else {
				DialogueManager.ShowAlert("Save a game first");
			}
		}
		

		private void ClearSavedGame() {
			if (PlayerPrefs.HasKey("SavedGame")) {
				PlayerPrefs.DeleteKey("SavedGame");
				Debug.Log("Cleared saved game data");
			}
			DialogueManager.ShowAlert("Saved Game Cleared From PlayerPrefs");
		}

	}

}
