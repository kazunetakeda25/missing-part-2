using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

namespace MissingComplete
{
	public class MenuNavigator : MonoBehaviour 
	{
		private static MenuNavigator instance;
		public static MenuNavigator Instance { get { return instance; } }

		[SerializeField] GameObject menu;
		[SerializeField] private float menuDistance;
		[SerializeField] private float animationTime;

		[SerializeField] Button[] mainButtons;

		private enum Menu
		{
			MAIN,
			ABOUT,
			LOAD,
			CREATE
		}

		private Menu currentMenu = Menu.MAIN;
		private bool tweening;
		private Tweener menuAnimator;

		private int currentCreateIndex = -1;

		[SerializeField] private GameObject profileNameInput;
		[SerializeField] private InputField nameInputfield;
		[SerializeField] private Image inputMask;
		[SerializeField] private Button submitButton;
		[SerializeField] private GameObject errorPopUp;
		[SerializeField] private GameObject notActivatedErrorPopUp;
		private const float NAME_INPUT_ANIMATION_TIME = 0.5f;

		[SerializeField] private GameObject deleteConfirmationBox;
		[SerializeField] Text profileName;
		private int currentDeleteIndex = -1;

		private void Update()
		{
			if(Input.GetKeyUp(KeyCode.Escape)) {
				ProcessBackHit();
			}
		}

		private void ProcessBackHit()
		{
			if(profileNameInput.activeSelf == true) {
				OnProfileNameCancelled(nameInputfield);
				return;
			}

			if(deleteConfirmationBox.activeSelf == true) {
				DismissDelete();
				return;
			}

			if(errorPopUp.activeSelf == true) {
				DismissErrorPopUp();
			}

			if(currentMenu != Menu.MAIN) {

				ToMainMenu();
			}
		}

		public void OnDeletePopUp(int index)
		{
			ShowInputMask();
			currentDeleteIndex = index;
			profileName.text = SaveGameManager.Instance.GetSaveGameData(index).profileName;
			deleteConfirmationBox.SetActive(true);
			deleteConfirmationBox.transform.localScale = Vector3.zero;
			deleteConfirmationBox.transform.DOScale(Vector3.one, NAME_INPUT_ANIMATION_TIME);
			DeactivateButtons();
		}

		public void OnDeleteConfirmed()
		{
			SaveGameManager.Instance.DeleteSaveGame(currentDeleteIndex);
			DismissDelete();
		}

		public void OnGameNotActivated()
		{
			notActivatedErrorPopUp.gameObject.SetActive(true);
			notActivatedErrorPopUp.transform.localScale = Vector3.zero;
			notActivatedErrorPopUp.transform.DOScale(Vector3.one, NAME_INPUT_ANIMATION_TIME);
		}

		public void DismissActivatedErrorPopUp()
		{
			notActivatedErrorPopUp.gameObject.SetActive(false);
			notActivatedErrorPopUp.transform.DOScale(Vector3.zero, NAME_INPUT_ANIMATION_TIME).OnComplete(()=> errorPopUp.SetActive(false));
		}

		public void OnErrorPopUp()
		{
			OnProfileNameCancelled(nameInputfield);
			ShowInputMask();
			errorPopUp.gameObject.SetActive(true);
			errorPopUp.transform.localScale = Vector3.zero;
			errorPopUp.transform.DOScale(Vector3.one, NAME_INPUT_ANIMATION_TIME);
		}

		public void DismissErrorPopUp()
		{
			HideInputMask();
			errorPopUp.gameObject.SetActive(false);
			errorPopUp.transform.DOScale(Vector3.zero, NAME_INPUT_ANIMATION_TIME).OnComplete(()=> errorPopUp.SetActive(false));
		}

		public void DismissDelete()
		{
			currentDeleteIndex = -1;
			HideInputMask();

			deleteConfirmationBox.transform.DOScale(Vector3.zero, NAME_INPUT_ANIMATION_TIME).OnComplete(()=> deleteConfirmationBox.SetActive(false));
			ActivateButtons();
		}

		public void OnTextEntered(InputField textBox)
		{

			if(textBox.text.Length <= 0) {
				submitButton.interactable = false;
			} else {
				submitButton.interactable = true;
			}
		}

		public void OnLoadGame(int index)
		{
			SaveGameManager.Instance.LoadSaveGame(index);
		}

		public void PopUpProfileInput(int saveIndex)
		{
			ShowInputMask();

			currentCreateIndex = saveIndex;
			profileNameInput.SetActive(true);
			profileNameInput.transform.localScale = Vector3.zero;
			profileNameInput.transform.DOScale(Vector3.one, NAME_INPUT_ANIMATION_TIME);
			DeactivateButtons();
		}

		public void OnProfileNameSubmitted(Text name)
		{
			SaveGameManager.Instance.CreateSaveGame(currentCreateIndex, name.text);
		}

		public void OnProfileNameCancelled(InputField name)
		{
			HideInputMask();

			name.text = "";

			currentCreateIndex = -1;
			profileNameInput.transform.DOScale(Vector3.zero, NAME_INPUT_ANIMATION_TIME).OnComplete(()=> profileNameInput.SetActive(false));
			ActivateButtons();
		}

		public void ToMainMenu()
		{
			if(CheckToggleTween() == false) {
				return;
			}

			currentMenu = Menu.MAIN;
			menuAnimator = menu.transform.DOMoveX(0f, animationTime);
			menuAnimator = menu.transform.DOMoveY(0f, animationTime);
			ProcessTween(menuAnimator);
		}

		public void MainMenuToAbout()
		{
			if(CheckToggleTween() == false) {
				return;
			}

			currentMenu = Menu.ABOUT;
			menuAnimator = menu.transform.DOMoveX(menuDistance, animationTime);
			ProcessTween(menuAnimator);

		}

		public void MainMenuToLoad()
		{
			if(TurboActivateChecker.Instance != null && TurboActivateChecker.Instance.Activated == false) {
				OnGameNotActivated();
				return;
			}

			if(CheckToggleTween() == false) {
				return;
			}

			currentMenu = Menu.LOAD;
			menuAnimator = menu.transform.DOMoveX(-menuDistance, animationTime);
			ProcessTween(menuAnimator);
		}

		public void MainMenuToCreate()
		{
			if(TurboActivateChecker.Instance != null && TurboActivateChecker.Instance.Activated == false) {
				OnGameNotActivated();
				return;
			}

			if(CheckToggleTween() == false) {
				return;
			}

			currentMenu = Menu.CREATE;
			menuAnimator = menu.transform.DOMoveY(-menuDistance, animationTime);
			ProcessTween(menuAnimator);
		}

		public void CreateToMainMenu()
		{
			if(CheckToggleTween() == false) {
				return;
			}

			currentMenu = Menu.MAIN;
			menuAnimator = menu.transform.DOMoveY(0, animationTime);
			ProcessTween(menuAnimator);
		}

		private bool CheckToggleTween() 
		{
			if(tweening == true) {
				return false;
			}

			tweening = true;
			return true;
		}

		private void ProcessTween(Tweener tween)
		{
			tween.OnComplete(OnTweenComplete);
			DeactivateButtons();
			MenuSoundBox.Instance.PlayClick();
		}

		private void OnTweenComplete()
		{
			tweening = false;
			ActivateButtons();
		}

		private void ActivateButtons()
		{
			inputMask.gameObject.SetActive(false);
		}

		private void DeactivateButtons()
		{
			inputMask.color = new Color(inputMask.color.r, inputMask.color.g, inputMask.color.b, 0.0f);
			inputMask.gameObject.SetActive(true);
		}

		private void ShowInputMask()
		{
			if(fadeTweener != null) fadeTweener.Kill();
			inputMask.gameObject.SetActive(true);
			inputMask.DOFade(0.4f, NAME_INPUT_ANIMATION_TIME);
		}

		private Tweener fadeTweener;
		private void HideInputMask()
		{
			Debug.Log ("Hide");
			if(fadeTweener != null) fadeTweener.Kill();
			fadeTweener = inputMask.DOFade(0.0f, NAME_INPUT_ANIMATION_TIME).OnComplete(() => inputMask.gameObject.SetActive(false));
		}

		private void Awake()
		{
			instance = this;
		}

		private void Start()
		{
			SaveGameManager.Instance.UnloadSavedGame();
			Time.timeScale = 1.0f;
		}
	}
}


