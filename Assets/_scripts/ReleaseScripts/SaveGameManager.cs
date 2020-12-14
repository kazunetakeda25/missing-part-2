using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.IO;

namespace MissingComplete
{
	public class SaveGameManager : MonoBehaviour 
	{
		[System.Serializable]
		public class SaveGame
		{
			public SaveGame()
			{
				//Debug.Log("Creating Saved Game!");
			}

			public const int TOTAL_CHECKPOINTS = 69;

			public string profileName = null;
			public float playTime = 0.0f;
			public int checkPoint = 0;
			public int aarCheckpoint = 0;
			public string sessionDataJSON = "";
			public string badgeTrackerJSON = "";
			public bool gameCompleted = false;
			public DateTime dateCompleted;


			public void UpdateSessionData(string json)
			{
				this.sessionDataJSON = json;
			}

			public void UpdateTime(float timeToUpdate) {
				playTime += timeToUpdate;
			}

			public int GetPercentageComplete()
			{
				if(checkPoint == TOTAL_CHECKPOINTS)
					return 100;

				float percentage = ((float) (checkPoint - 1) / (float) TOTAL_CHECKPOINTS) * 100.0f;
				return Mathf.RoundToInt(percentage);
			}
		}

		private static SaveGameManager instance;
		public static SaveGameManager Instance { get { return instance; } }

		public const int NUMBER_OF_SAVE_GAMES = 30;
		private const string PROFILE_KEY_NAME = "PNAME";
		private const string PROFILE_KEY_TIME = "PTIME";
		private const string PROFILE_KEY_CHECKPOINT = "PCHECKPOINT";
		private const string PROFILE_SESSION_DATA = "PSESSIONDATA";
		private const string PROFILE_BADGETRACKER = "PBADGETRACKER";

		private const string SAVE_PATH = "Saves";
		private const string SAVE_FILE_NAME = "Missing.sav";

		private int loadedSaveGame = -1;

		[SerializeField] SaveGame[] saveGames;
		[SerializeField] bool deleteSaveFile;

		public SaveGame GetCurrentSaveGame()
		{
			if(loadedSaveGame == -1) {
				Debug.LogWarning("No Saved Game Loaded!!");
				return null;
			}

			return saveGames[loadedSaveGame];
		}

		public void CreateSaveGame(int index, string profileName)
		{
			Debug.Log("Create Save Game: " + index);

			if(CheckForDuplicateName(profileName)) {
				MenuNavigator.Instance.OnErrorPopUp();
				return;
			}

			SessionManager.Instance.StartNewSession(profileName, Sex.UNSPECIFIED);

			SaveGame game = new SaveGame();
			game.profileName = profileName;
			game.checkPoint = 1;
			SessionManager.Instance.Reset();
			game.sessionDataJSON = SessionManager.Instance.GetGameDataJSON();
			saveGames[index] = game;
			SaveGameInSlot(index, game);
			LoadSaveGame(index);
		}

		public void UnloadSavedGame()
		{
			loadedSaveGame = -1;
			//SessionManager.Instance.Reset();
		}

		public void SaveCurrentGame()
		{
			if(loadedSaveGame == -1) {
				Debug.LogWarning("No Save Game Loaded..");
				return;
			}

			//Retrieve JSON
			GetCurrentSaveGame().sessionDataJSON = SessionManager.Instance.GetGameDataJSON();
			GetCurrentSaveGame().badgeTrackerJSON = BadgeTracker.Instance.ExportBadgeScores();

			PlayerLogGenerator.SavePlayerLog(GetCurrentSaveGame());

			SaveGameInSlot(loadedSaveGame, GetCurrentSaveGame());
		}

		public void SaveGameInSlot(int index, SaveGame save)
		{
			Debug.Log("Saving Game - " + save.profileName + " - in slot: " + index);
			WriteSaveGames();
		}

		public void LoadSaveGame(int index)
		{
			Debug.Log("Loading: " + index);
			loadedSaveGame = index;

			//Kickstart New GameLoad
			GameLoader.Instance.Load(saveGames[index].checkPoint);
			SessionManager.Instance.SetGameDataJSON(saveGames[index].sessionDataJSON);
			BadgeTracker.Instance.LoadBadgeScores(saveGames[index].badgeTrackerJSON);
			//
		}

		public void DeleteSaveGame(int index)
		{
			if(loadedSaveGame == index) {
				loadedSaveGame = -1;
			}

			saveGames[index] = null;
			WriteSaveGames();
			RefreshGUI();
		}

		public SaveGame GetSaveGameData(int index)
		{
			
			return saveGames[index];
		}

		private void LoadSaveFile()
		{
			string filePath = Application.persistentDataPath + "/" + SAVE_PATH + "/" + SAVE_FILE_NAME;
			Debug.Log("Checking If Save File Exists: " + filePath);
			if(File.Exists(filePath)) {
				if(deleteSaveFile) {
					DeleteSaveFile(filePath);
					return;
				}
				LoadFile(filePath);
			} else {
				CreateNewFile(filePath);
			}
		}

		private void DeleteSaveFile(string path)
		{
			Debug.Log("Resetting Save File");
			File.Delete(path);
			CreateNewFile(path);
		}

		private void LoadFile(string path)
		{
			string json = File.ReadAllText(path);
			Debug.Log("File Found: " + File.ReadAllText(path));
			saveGames = JsonConvert.DeserializeObject<SaveGame[]>(json);
		}

		private void CreateNewFile(string path)
		{
			Debug.Log("No Save File Found, creating new File...");
			Directory.CreateDirectory(Application.persistentDataPath + "/" + SAVE_PATH);

			saveGames = new SaveGame[NUMBER_OF_SAVE_GAMES];
			WriteSaveGames();
		}

		private void WriteSaveGames()
		{
			Debug.Log("Writing Save Games...");
			string filePath = Application.persistentDataPath + "/" + SAVE_PATH + "/" + SAVE_FILE_NAME;

			string json = JsonConvert.SerializeObject(saveGames);
			Debug.Log(json);
			File.WriteAllText(filePath, json);
		}

		private void RefreshGUI()
		{
			if(ProfilePopulator.Instance != null) {
				ProfilePopulator.Instance.PopulateSavedGames();
			}
		}

		private bool CheckForDuplicateName(string nameToCheck)
		{
			foreach(SaveGame save in saveGames) {
                if(save == null)
					continue;
				
				if (save.profileName == null)
                    continue;

				if(String.Equals(save.profileName, nameToCheck, StringComparison.InvariantCultureIgnoreCase))
					return true;
			}

			return false;
		}

		private void Awake()
		{
			GameObject.DontDestroyOnLoad(this.gameObject);
			instance = this;
		}

		private void Start()
		{
			LoadSaveFile();
			RefreshGUI();
		}

		private void Update()
		{

			if(loadedSaveGame == -1)
				return;

			saveGames[loadedSaveGame].UpdateTime(Time.deltaTime);
		}
	}

}