using UnityEngine;
using System.Collections;

public class ACCommunicator : MonoBehaviour {

	private static ACCommunicator instance;
	public static ACCommunicator Instance { get { return instance; } }

	public static bool FidelityActive = false;

	public ShowInputControls InputControls;
	private AC.SettingsManager settingsManager;
	public bool isMessageAlive = false;
	public bool bshowDialog = false;
	public bool bshowOption = false;
	public bool bshowSlider = false;
	public bool bshowText = false;
	public bool bshowCount = false;
	public bool bprimingshow = false;
	public float messageTimer = 0.0f;
	public float Length;
	public int sliderValue = -1;
	public int counterValue = 0;
	public Quiz quiz;

	private AC.Char currentCharTalking;
	private bool currentClipIsBackground = false;

	private void Awake()
	{
		instance = this;
	}

	void Start () {
		isMessageAlive = false;
		if (AC.AdvGame.GetReferences () == null)
		{
			Debug.LogError ("A References file is required - please use the Adventure Creator window to create one.");
		}
		else
		{
			settingsManager = AC.AdvGame.GetReferences ().settingsManager;
		}

		if (AC.KickStarter.runtimeVariables) {
			//AC.KickStarter.runtimeVariables.SetProperty("priming", Settings.Instance.Priming);
		}

	}

	private bool CheckIfMessageIsEligibleToSkip()
	{
		if(Debug.isDebugBuild == true)
			return true;

		if(bshowDialog == false)
			return false;

		if(messageTimer < Length / 2)
			return false;

		return true;
	}

	// Update is called once per frame
	void Update () {

		if (Input.GetMouseButtonDown (0)) 
		{
			if(CheckIfMessageIsEligibleToSkip())
				messageTimer = Length + 0.5f;
		}

		if (bshowDialog) {
			messageTimer += Time.deltaTime;
			if(messageTimer > Length + 0.5f)
			{
				messageTimer = 0.0f;
				bshowDialog = false;
				isMessageAlive = false;
				InputControls.HideDialogBox();

				if(currentCharTalking != null && currentClipIsBackground == false) {
					AudioSource charAudio = currentCharTalking.GetComponent<AudioSource>();

					if(charAudio != null)
						charAudio.Stop();

					Animation charAnim = currentCharTalking.GetComponent<Animation>();

					if(charAnim != null) {
						foreach(AnimationState animState in charAnim) {
							if(animState.layer == (int)AC.AnimLayer.Mouth) {
								//charAnim.Blend(animState.name, 0.0f, 0.01f);
								charAnim.GetComponent<Animation>()[animState.name].time = 0.0f;
								charAnim.GetComponent<Animation>().Sample();
								charAnim.GetComponent<Animation>()[animState.name].enabled = false;
							}
						}
					}
					
					currentCharTalking = null;
				}
			}
		}
		if (bshowText) {
			messageTimer += Time.deltaTime;
			if(messageTimer > Length + 0.5f)
			{
				messageTimer = 0.0f;
				bshowText = false;
				isMessageAlive = false;
			}
		}
	
	}
	public void ShowDialog(int lineNumber, AC.Char speakerName, Vector3 position, float Height, float Width, string message, bool isBackground)
	{
		currentClipIsBackground = isBackground;
		//Debug.Log ("with pos with line number");


		
		Length = PlayAudio (lineNumber, speakerName);
		//message =  message;
		if (Length <= 0) {
			Length = 5.0f;
		}

		if(isBackground == true)
			return;

		InputControls.ShowDialogBox (position, Height, Width);
		InputControls.AddDialog (message);
		messageTimer = 0.0f;
		bshowDialog = true;
		isMessageAlive = true;
	}
	public void ShowDialog( Vector3 position, float Height, float Width, string message)
	{
		Debug.Log ("with pos but no line number");
		InputControls.ShowDialogBox (position, Height, Width);
		InputControls.AddDialog (message);
		Length = 10.0f;
		messageTimer = 0.0f;
		isMessageAlive = true;
		bshowDialog = true;
		
	}
	public void ShowDialog( Vector3 position, float Height, float Width, string message,bool priming)
	{
		Debug.Log ("with pos but no line number");
		InputControls.ShowDialogBox (position, Height, Width);
		InputControls.AddDialog (message);
		//Length = 10.0f;
		//messageTimer = 0.0f;
		//isMessageAlive = true;
		//bshowDialog = true;
		bprimingshow = priming;
	}
	public void ShowTextMessage(string message, float timer)
	{
		InputControls.ShowTextMessage (message,timer);
		isMessageAlive = true;
		Length = timer;
		messageTimer = 0.0f;
		bshowText = true;
	}
	float PlayAudio(int lineNumber, AC.Char speakerName)
	{
		if (lineNumber > -1 && speakerName.name != "" /*&& settingsManager.searchAudioFiles*/) {
			string filename = "Speech/" + speakerName.name + "/" + speakerName.name + lineNumber;
			//Debug.Log(filename);
			
			AudioClip clipObj = Resources.Load(filename) as AudioClip;
			if (clipObj)
			{
				AC.Char _speakerChar = speakerName;
				if (_speakerChar.GetComponent<AudioSource>())
				{
					_speakerChar.GetComponent <AudioSource>().volume = AC.KickStarter.options.optionsData.speechVolume;
					
					_speakerChar.GetComponent <AudioSource>().clip = clipObj;
					_speakerChar.GetComponent <AudioSource>().Play();

				}
				else
				{
					Debug.LogWarning (_speakerChar.name + " has no audio source component!");
				}

				currentCharTalking = speakerName;
				
				return clipObj.length;
			}
		}
			return 0.0f;
	}
	public void Stop()
	{
		if (isMessageAlive) 
		{
			InputControls.RemoveCurrentDialog ();
			InputControls.HideDialogBox ();
			
			isMessageAlive = false;
		}
	}
	[HideInInspector]
	public ConversationDaikon _convo;
	private string[] labelsForMul;
	public void CreateList(string[] labels,Vector3 _pos,float _width, ConversationDaikon cD, Quiz _quiz)
	{
		quiz = _quiz;
		//InputControls.ShowMultipleChoice (labels);
		InputControls.ShowMultipleChoice (labels, _pos, _width);
		ReportEvent.DisplayingPlayerPrompt (labels);
		labelsForMul = labels;
		_convo = cD;
	}
	public void Answer(int chosen)
	{

		if (_convo != null) {
			ReportEvent.PlayerSelectedChoice(labelsForMul[chosen],chosen);
			_convo.RunOption (chosen);
			_convo = null;
			}
		if (quiz != Quiz.NONE) {
			SessionManager.Instance.SaveQuizAnswer (quiz, chosen);
			quiz = Quiz.NONE;
			chosen = -1;
		}
		if (bprimingshow) 
		{
			InputControls.HideDialogBox();
			bprimingshow = false;
		}
	}
	public void ShowSlider(int lowRange, int highRange,Quiz _quiz)
	{
		InputControls.ShowSlider (lowRange, highRange, 1);
		isMessageAlive = true;
		bshowSlider = true;
		quiz = _quiz;
	}
	public void ShowSlider(int lowRange, int highRange, string message, int anchorValue, Quiz _quiz)
	{
		InputControls.ShowSlider (lowRange, highRange, anchorValue);
		InputControls.ShowSliderText (message);
		isMessageAlive = true;
		bshowSlider = true;
		quiz = _quiz;
	}
	public void ShowSliderIncrement(int lowRange, int highRange, string message, Quiz _quiz)
	{
		InputControls.ShowSliderWithIncrements (lowRange, highRange, 1);
		InputControls.ShowSliderText (message);
		isMessageAlive = true;
		bshowSlider = true;
		quiz = _quiz;
	}
	public void AnswerS(int chosen)
	{
		sliderValue = chosen;
		isMessageAlive = false;
		bshowSlider = false;
		if (quiz != Quiz.NONE) {
			SessionManager.Instance.SaveQuizAnswer(quiz, (float) sliderValue);
			quiz = Quiz.NONE;
		}
	}
	public void ShowCounter(Vector3 _screenPos,string message, Quiz _quiz)
	{
		quiz = _quiz;
		InputControls.ShowBulletCount (_screenPos, message);
		isMessageAlive = true;
		bshowCount = true;
	}
	public void GetCounterValue(int count)
	{
		isMessageAlive = false;
		bshowSlider = false;
		if (quiz != Quiz.NONE) {
			SessionManager.Instance.SaveQuizAnswer (quiz, (float) count);
			quiz = Quiz.NONE;			
		}
	}
	public int GetValue()
	{
		return sliderValue;
	}
	public void LoadNextLevel()
	{
		SessionManager.Instance.GotoNextLevel ();
	}
	public void MessupFont()
	{
		if(Settings.Instance.Fidelity)
			FidelityActive = true;
	}
	public void ReturnFont()
	{
		FidelityActive = false;
	}
}
