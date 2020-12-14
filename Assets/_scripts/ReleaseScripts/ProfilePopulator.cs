using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace MissingComplete
{
	public class ProfilePopulator : MonoBehaviour 
	{
		private static ProfilePopulator instance;
		public static ProfilePopulator Instance { get { return instance; } }

		[SerializeField] GridLayoutGroup createGrid;
		[SerializeField] GridLayoutGroup loadGrid;

		[SerializeField] ProfileButton[] createButtons;
		[SerializeField] ProfileButton[] loadButtons;

		[SerializeField] ProfileButton profileButtonTemplate;

		public void PopulateSavedGames()
		{
			Debug.Log("Refreshing Saved Games");
			PopulateSet(true);
			PopulateSet(false);
		}

		private void Awake()
		{
			instance = this;
		}

		private void ClearButtons(ProfileButton[] buttonsToClear)
		{
			for(int i = 0; i < createButtons.Length; i++) {
				GameObject.Destroy(buttonsToClear[i].gameObject);
			}
		}

		//TODO
		private void PopulateSet(bool create)
		{
			if(create == true && createButtons.Length > 0) {
				ClearButtons(createButtons);
				createButtons = null;
			} else if(create == false && loadButtons.Length > 0) {
				ClearButtons(loadButtons);
				loadButtons = null;
			}

			var createButtonList = new List<ProfileButton>();
			var loadButtonList = new List<ProfileButton>();

			for(int i = 0; i < SaveGameManager.NUMBER_OF_SAVE_GAMES; i++) {
				SaveGameManager.SaveGame save = SaveGameManager.Instance.GetSaveGameData(i);

				ProfileButton spawnedButton = GameObject.Instantiate(profileButtonTemplate) as ProfileButton;

				if(create) {
					spawnedButton.transform.SetParent(createGrid.transform, false);
					createButtonList.Add(spawnedButton);
				} else {
					spawnedButton.transform.SetParent(loadGrid.transform, false);
					loadButtonList.Add(spawnedButton);
				}

				//spawnedButton.transform.localScale = Vector3.one;

				if(save == null || save.profileName == null) {
					SetupButtonForNull(spawnedButton, i);
					spawnedButton.selfButton.interactable = create;
					continue;
				}

				SetupButton(create, save, spawnedButton, i);
			}

			if(create == true) {
				createButtons = createButtonList.ToArray();
			} else if(create == false) {
				loadButtons = loadButtonList.ToArray();
			}
		}

		private void SetupButtonForNull(ProfileButton button, int index)
		{
			SetButtons(button, true);
			button.deleteButton.gameObject.SetActive(false);
			button.selfButton.onClick.AddListener(() => MenuNavigator.Instance.PopUpProfileInput(index));
		}

		private const string PROGRESS_SUFFIX = "% COMPLETE";
		private const string TIME_PREFIX = "TIME: ";

		private void SetupButton(bool create, SaveGameManager.SaveGame save, ProfileButton button, int index)
		{
			SetButtons(button, false);
			button.profileNameLabel.text = save.profileName;
			button.progressLabel.text = save.GetPercentageComplete().ToString() + PROGRESS_SUFFIX;

			TimeSpan time = TimeSpan.FromSeconds(save.playTime);
			string answer = string.Format("{0:D2}h:{1:D2}m:{2:D2}s",
				                time.Hours,
				                time.Minutes,
				                time.Seconds);

			button.timeLabel.text = answer;

			button.deleteButton.onClick.AddListener(() => MenuNavigator.Instance.OnDeletePopUp(index));
			button.selfButton.onClick.AddListener(() => MenuNavigator.Instance.OnLoadGame(index));

			if(create == true) {
				button.selfButton.interactable = false;
			} else {
				button.selfButton.interactable = true;
			}
		}
			
		private void SetButtons(ProfileButton button, bool empty)
		{
			button.profileLabel.gameObject.SetActive(!empty);
			button.profileNameLabel.gameObject.SetActive(!empty);
			button.progressLabel.gameObject.SetActive(!empty);
			button.timeLabel.gameObject.SetActive(!empty);
			button.emptyLabel.gameObject.SetActive(empty);
		}
	}
}
