using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using PixelCrushers.DialogueSystem;

public class AAR2Gen : MonoBehaviour 
{
	[Serializable]
	public class AARNode
	{
		public delegate bool ConditionOperator<T>(T left, T right);

		[Serializable]
		public class OutgoingLink
		{
			public int link;
			public string text;
		}

		[Serializable]
		public class Condition
		{
			public ConditionOperator<int> condition;
			public string left;
			public string right;
		}

		public static ConditionOperator<int> ParseConditionOperator(string opString) {
			switch(opString) {
			case "==":
				ConditionOperator<int> doesEqual = (x, y) => (x == y); 
				//Debug.Log("Found Operator ==");
				return doesEqual;
			case "~=":
			case "!=":
			case "=!":
			case "=~":
				ConditionOperator<int> doesNotEqual = (x, y) => (x != y);
				//Debug.Log("Found Operator !=");
				return doesNotEqual;
			case ">=":
			case "=>":
				ConditionOperator<int> greaterThanEqual = (x, y) => (x >= y);
				//Debug.Log("Found Operator >=");
				return greaterThanEqual;
			case "<=":
			case "=<":
				ConditionOperator<int> lessThanEqual = (x, y) => (x <= y);
				//Debug.Log("Found Operator <=");
				return lessThanEqual;
			case "<":
				ConditionOperator<int> lessThan = (x, y) => (x < y);
				//Debug.Log("Found Operator <");
				return lessThan;
			case ">":
				ConditionOperator<int> greaterThan = (x, y) => (x > y);
				//Debug.Log("Found Operator >");
				return greaterThan;
			}

			Debug.LogError("Unrecognized Operator!!");
			return null;
		}

		public int ID;
		public OutgoingLink[] outgoingLinks;
		public AARSlideType slideType;

		public string parsedTitle;
		public string[] titleVars;
		public string parsedBody;
		public string[] bodyVars;

		public string image;
		public string video;
		public string audioFile;
		public bool fireworks;
		public Narrator narrator;

		public string quizVariable;
		public int[] quizSliderRanges;

		public Condition[] myConditions;
		public ConditionPriority conditionPriority;
	}

	private const string BREAK_TAG = "[BREAK]";
	private const string FIREWORKS = "[FIREWORKS]";
	private const string RADIO = "[RADIO]";
	private const string SLIDER = "[SLIDER]";
	private const string WHEEL = "[WHEEL]";
	private const string EM1_OPEN_TAG = "[em1]";
	private const string EM1_CONVERTED_OPEN_TAG = "[b][FF0000]";
	private const string EM1_CLOSE_TAG = "[/em1]";
	private const string EM1_CONVERTED_CLOSE_TAG = "[/b][FFFFFF]";
	private const string EM2_OPEN_TAG = "[em2]";
	private const string EM2_CONVERTED_OPEN_TAG = "[b]";
	private const string EM2_CLOSE_TAG = "[/em2]";
	private const string EM2_CONVERTED_CLOSE_TAG = "[/b]";
	private const string EM3_OPEN_TAG = "[em3]";
	private const string EM3_CONVERTED_OPEN_TAG = "[i]";
	private const string EM3_CLOSE_TAG = "[/em3]";
	private const string EM3_CONVERTED_CLOSE_TAG = "[/i]";
	private const string EM4_OPEN_TAG = "[em4]";
	private const string EM4_CONVERTED_OPEN_TAG = "[b][i]";
	private const string EM4_CLOSE_TAG = "[/em4]";
	private const string EM4_CONVERTED_CLOSE_TAG = "[/b][/i]";

	[SerializeField] private Episode episode;
	[SerializeField] private DialogueDatabase dialogueDB;
	[SerializeField] private AARDebugger aarDebugger;
	[SerializeField] private AARNGUI gui;
	[SerializeField] private AARDriver driver;

	private PixelCrushers.DialogueSystem.Conversation myConversation;

	public AARNode[] nodes;
	private int currentID = 0;

	private MissingComplete.SaveGameManager saveGameManager;

	public bool inputValsStart;

	private void Start()
	{
		saveGameManager = MissingComplete.SaveGameManager.Instance;

		if(inputValsStart == false) {
			InitializeAAR();
			return;
		}

		if(aarDebugger != null && Debug.isDebugBuild == true) {
			ProcessAARDebugger();
			SessionManager.Instance.StartNewSession("AAR TEST SUBJECT", Sex.UNSPECIFIED);
			return;
		}

		InitializeAAR();
	}

	private void OnWheelBroughtIn()
	{
		//gui.wheel.SpinWheel();
	}

	public int GetConversationID()
	{
		switch(episode) {
		case Episode.NEWAAR1:
			return 1;
		case Episode.NEWAAR2:
			return 2;
		case Episode.NEWAAR3:
			return 3;
		}

		Debug.LogError("Episode not set to correct AAR episode!!");
		return -1;
	}

	public void InitializeAAR()
	{
		//Debug.Log("Initializing AAR");
		//Pull all of the slides out of Dialogue DB, put them in an array.
		//Start navigating through the AAR

		myConversation = dialogueDB.GetConversation(GetConversationID());
		DialogueEntry[] rawDialogs = myConversation.dialogueEntries.ToArray();

		List<AARNode> aarNodes = new List<AARNode>();
		foreach(DialogueEntry entry in rawDialogs) {
			AARNode newNode = ConvertDialogueEntryToNodes(entry);
			if(newNode != null) {
				aarNodes.Add(newNode);
			}
		}

		nodes = aarNodes.ToArray();
		RunAAR();
	}

	private void RunAAR()
	{
		int startNode = 0;

		if(saveGameManager != null) {
			if(saveGameManager.GetCurrentSaveGame() == null) {
				UnityEngine.SceneManagement.SceneManager.LoadScene("MAIN_MENU");
			}
			startNode = saveGameManager.GetCurrentSaveGame().aarCheckpoint;
		}

		if(startNode == -1) {
			driver.PrepareMedalsSlide();
			return;
		}

		currentID = nodes[startNode].ID;
		driver.DisplaySlide(nodes[startNode]);
	}

	public void NextSlide(int nextSlide)
	{
		currentID = nextSlide;
		Debug.Log("Queuing Next Slide: " + nextSlide + " - ID: " + currentID);

		if(nextSlide == -1) {
			driver.PrepareMedalsSlide();
			if(saveGameManager != null) {
				saveGameManager.GetCurrentSaveGame().aarCheckpoint = -1;
				saveGameManager.SaveCurrentGame();
			}
			return;
		}

		int nextNodeIndex = GetNodeIndexByID(currentID);

		if(saveGameManager != null) {
			saveGameManager.GetCurrentSaveGame().aarCheckpoint = nextNodeIndex;
			saveGameManager.SaveCurrentGame();
		}

		driver.DisplaySlide(nodes[nextNodeIndex]);
	}

	public AARNode GetNodeByID(int id) 
	{
		foreach(AARNode node in nodes) {
			if(node.ID == id) {
				return node;
			}
		}

		Debug.LogError("Node ID: " + id + " not found!");
		return null;
	}

	public int GetNodeIndexByID(int id)
	{
		for(int i = 0; i < nodes.Length; i++) {
			if(nodes[i].ID == id) {
				return i;
			}
		}

		Debug.LogError("Node ID: " + id + " not found!");
		return -1;
	}

	public void WrapAAR()
	{
		if(Debug.isDebugBuild)
			Application.Quit();

		SessionManager.Instance.GotoNextLevel();
	}

	private bool GetRemainingMoviesToPlay(int dialogueIndex, out string newMoviePath)
	{
		newMoviePath = null;

		if(nodes[dialogueIndex].video == null)
			return false;

		newMoviePath = nodes[dialogueIndex].video;

		if(nodes[dialogueIndex].outgoingLinks.Length > 1)
			return true;

		return false;
	}

	private AARNode ConvertDialogueEntryToNodes(DialogueEntry de)
	{
		//Debug.Log("Converting Dialogue Entry: " + de.id);
		//Root Node, useless to us.

		//Debug.Log("Setting Up Node: " + de.id);

		//Setup Video File Node
		if(de.VideoFile.Length > 0) {
			//Debug.Log("Creating Movie Node for Movie: " + de.VideoFile);
			return CreateMovieNode(de);
		}

		//Debug.Log("Converting Dialogue Entry: " + de.id);

		AARNode newNode = new AARNode();
	
		newNode.ID = de.id;

		newNode.narrator = DetermineNarrator(de);

		newNode.parsedTitle = de.MenuText;
		newNode.titleVars = GetVariables(ref newNode.parsedTitle);
		newNode.parsedBody = de.DialogueText;
		newNode.bodyVars = GetVariables(ref newNode.parsedBody);

		newNode.audioFile = de.AudioFiles;
		newNode.audioFile = newNode.audioFile.Replace("[", "");
		newNode.audioFile = newNode.audioFile.Replace("]", "");
		newNode.audioFile = newNode.audioFile.Replace(".wav", "");
		newNode.audioFile = newNode.audioFile.Replace(".WAV", "");
		newNode.audioFile = newNode.audioFile.Replace(".ogg", "");
		newNode.audioFile = newNode.audioFile.Replace(".OGG", "");
		newNode.audioFile = newNode.audioFile.Replace(".MP3", "");
		newNode.audioFile = newNode.audioFile.Replace(".mp3", "");

		//Do we have Fireworks?
		newNode.fireworks = newNode.parsedBody.Contains(FIREWORKS);
		newNode.parsedBody = newNode.parsedBody.Replace(FIREWORKS, "");

		//Put in BBCOde for NGUI
		newNode.parsedBody = ReplaceAllEMs(newNode.parsedBody);

		newNode.image = Field.LookupValue(de.fields, "Pictures");
		newNode.image = newNode.image.Replace(@"[", string.Empty);
		newNode.image = newNode.image.Replace(@"]", string.Empty);
		newNode.image = newNode.image.Replace(".jpg", string.Empty);
		newNode.image = newNode.image.Replace(".png", string.Empty);
		newNode.image = newNode.image.Replace(".psd", string.Empty);

		newNode.slideType = FindSlideType(newNode.parsedBody, (newNode.image == null));
		newNode.parsedBody = newNode.parsedBody.Replace(RADIO, "");
		newNode.parsedBody = newNode.parsedBody.Replace(SLIDER, "");
		newNode.parsedBody = newNode.parsedBody.Replace(WHEEL, "");


		//Debug.Log("String sent: " + newNode.parsedBody);
		string parsedString = StripAllTags(newNode.parsedBody); 
		newNode.parsedBody = parsedString;

		//Debug.Log("Node is type: " + newNode.slideType);

		newNode.outgoingLinks = GetOutgoingLinks(de);

		if(IsQuizSlide(newNode.slideType)) {
			newNode.quizVariable = GetQuizVariableString(de.conditionsString);
		} else {
			newNode.myConditions = GetConditions(de.conditionsString);
		}

		if(IsQuizSlider(newNode.slideType)) {
			newNode.quizSliderRanges = GetQuizSliderRanges(de);
		}

		newNode.conditionPriority = de.conditionPriority;

		return newNode;
	}

	private AARNode.OutgoingLink[] GetOutgoingLinks(DialogueEntry de)
	{
		AARNode.OutgoingLink[] outgoingLinks = new AARNode.OutgoingLink[de.outgoingLinks.Count];
		for(int i = 0; i < de.outgoingLinks.Count; i++) {
			outgoingLinks[i] = new AARNode.OutgoingLink();
			outgoingLinks[i].link = de.outgoingLinks[i].destinationDialogueID;
			outgoingLinks[i].text = myConversation.GetDialogueEntry(de.outgoingLinks[i].destinationDialogueID).DialogueText;
		}

		return outgoingLinks;
	}

	private int[] GetQuizSliderRanges(DialogueEntry de)
	{
		string parenthetical = Field.LookupValue(de.fields, "Parenthetical");

		//Debug.Log("Fetching Slider: " + parenthetical);
		List<int> quizSlideInts = new List<int>();

		string[] splitParenthetical = parenthetical.Split(',');

		foreach(string rangeString in splitParenthetical) {
			int parentheticalNumber;
			bool parentheticalOK =int.TryParse(rangeString, out parentheticalNumber);

			if(parentheticalOK == true) {
				quizSlideInts.Add(parentheticalNumber);
			} else {
				Debug.LogError("Bad Parenthetical Data at: " + de.id);
				quizSlideInts.Add(1);
			}
		}

		return quizSlideInts.ToArray();
	}

	private bool IsQuizSlide(AARSlideType type)
	{
		switch(type) {
		case AARSlideType.QuizRadios:
		case AARSlideType.QuizRadiosImage:
		case AARSlideType.QuizSlider:
		case AARSlideType.QuizSliderImage:
			return true;
		}

		return false;
	}

	private bool IsQuizSlider(AARSlideType type)
	{
		switch(type) {
		case AARSlideType.QuizSlider:
		case AARSlideType.QuizSliderImage:
			return true;
		}

		return false;
	}

	private string GetQuizVariableString(string condition)
	{
		string startString = "Variable[\"";
		string endString = "\"]";
		condition = condition.Replace(startString, "");
		condition = condition.Replace(endString, "");

		return condition;
	}

	private AARNode.Condition[] GetConditions(string conditions)
	{
		if(conditions.Length == 0)
			return null;

		List<AARNode.Condition> nodeConditions = new List<AARNode.Condition>();
	
		string[] allConditions;
		//Split Conditions
		if(conditions.Contains(" AND ")) {
			allConditions = conditions.Split(new string[] { " AND " }, StringSplitOptions.None);
		} else if(conditions.Contains(" and ")) {
			allConditions = conditions.Split(new string[] { " and " }, StringSplitOptions.None);
		} else if(conditions.Contains(" && ")) {
			allConditions = conditions.Split(new string[] { " && " }, StringSplitOptions.None);
		} else {
			allConditions = new string[] { conditions };
		}

		foreach(string s in allConditions) {
			AARNode.Condition newCondition = new AARNode.Condition();
			//Debug.Log("Splitting: " + s);
			string[] splitBySpace = s.Split(' ');

			if(splitBySpace.Length < 3) {
				Debug.LogError("Bad Parse from Conditions in string: " + s);
			}

			newCondition.left  = splitBySpace[0];
			newCondition.right = splitBySpace[2];

			newCondition.condition = AARNode.ParseConditionOperator(splitBySpace[1]);

			nodeConditions.Add(newCondition);
		}

		return nodeConditions.ToArray();
	}
		
	private AARSlideType FindSlideType(string bodyText, bool hasImage)
	{
		if(bodyText.Contains(WHEEL)) {
			return AARSlideType.Spinner;
		}

		if(bodyText.Contains(RADIO)) {
			if(hasImage == true)
				return AARSlideType.QuizRadiosImage;

			return AARSlideType.QuizRadios;
		}

		if(bodyText.Contains(SLIDER)) {
			if(hasImage == true)
				return AARSlideType.QuizSliderImage;

			return AARSlideType.QuizSlider;
		}

		if(hasImage) {
			return AARSlideType.BasicImage;
		}

		return AARSlideType.Basic;
	}

	private Narrator DetermineNarrator(DialogueEntry de)
	{
		switch(de.ActorID) {
		case 1:
			return Narrator.SME;
		case 2:
			return Narrator.Generic;
		case 3:
			return Narrator.Generic;
		case 4:
			return Narrator.SME2;
		case 5:
			return Narrator.Computer;
		case 6:
			return Narrator.Anchoring;
		case 7:
			return Narrator.Projection;
		case 8:
			return Narrator.Represenative;
		}

		return Narrator.Generic;
	}

	private string ExtractTitle(string bodyText)
	{
		return "PLACEHOLDER";
	}

	private string[] GetVariables(ref string sourceText)
	{

		List<string> matched = new List<string>();

		string startString = "[var=";
		string endString = "]";

		//Are there variables in this text?
		if(sourceText.Contains(startString) == false) {
			return matched.ToArray();
		}

		int startIndex = 0;
		int endIndex = 0;

		bool exit = false;
		int varIndex = 0;
		while(exit == false) {
			//Where is the first instance of the start string?
			startIndex = sourceText.IndexOf(startString);
			//Where is the first isntance of the end string?
			endIndex = sourceText.IndexOf(endString);
			//if both the start and end string are present, run the algorithm
			if(startIndex != -1 && endIndex != -1) {
				//Add a matched string to the array between the two values.
				matched.Add(sourceText.Substring(startIndex + startString.Length, endIndex - startIndex - startString.Length));
				//Replace the removed text with a variable for formatting later
				//Debug.Log("Just added to vars: " + matched[varIndex]);
				sourceText = sourceText.Replace((startString + matched[varIndex] + endString), ("{" + varIndex.ToString() + "}"));
				//Debug.Log("Replace String: " + (startString + matched[varIndex] + endString));
				//Debug.Log("New String: " + sourceText);
				varIndex++;
				//sourceText = sourceText.Substring(endIndex + endString.Length);
				exit = false;
			} else {
				exit = true;
			}
		}

		return matched.ToArray();
	}
		

	private string ReplaceAllEMs(string bodyText) {
		bodyText = bodyText.Replace(EM1_OPEN_TAG, EM1_CONVERTED_OPEN_TAG);
		bodyText = bodyText.Replace(EM1_CLOSE_TAG, EM1_CONVERTED_CLOSE_TAG);
		bodyText = bodyText.Replace(EM2_OPEN_TAG, EM2_CONVERTED_OPEN_TAG);
		bodyText = bodyText.Replace(EM2_CLOSE_TAG, EM2_CONVERTED_CLOSE_TAG);
		bodyText = bodyText.Replace(EM3_OPEN_TAG, EM3_CONVERTED_OPEN_TAG);
		bodyText = bodyText.Replace(EM3_CLOSE_TAG, EM3_CONVERTED_CLOSE_TAG);
		bodyText = bodyText.Replace(EM4_OPEN_TAG, EM4_CONVERTED_OPEN_TAG);
		bodyText = bodyText.Replace(EM4_CLOSE_TAG, EM4_CONVERTED_CLOSE_TAG);

		return bodyText;
	}

	private string StripAllTags(string stringToStrip)
	{

		char[] array = new char[stringToStrip.Length];
		int arrayIndex = 0;
		bool inside = false;

		for(int i = 0; i < stringToStrip.Length; i++) {
			char let = stringToStrip[i];

			//Get rid of line breaks at beginning of string
			if(let == '\n' || let == '\r') {
				if(arrayIndex == 0)
					continue;
			}

			if(let == '<') {
				inside = true;
				continue;
			}

			if(let == '>') {
				inside = false;
				continue;
			}

			if(inside == false) {
				array[arrayIndex] = let;
				arrayIndex++;
			}
		}

		string cleanString = new string(array, 0, arrayIndex);

		return cleanString;
	}

	private AARNode CreateMovieNode(DialogueEntry de)
	{
		AARNode myNode = new AARNode();
		myNode.ID = de.id;
		myNode.slideType = AARSlideType.Movie;
		myNode.video = de.VideoFile;

		myNode.video = myNode.video.Replace(".mov", "");
		myNode.video = myNode.video.Replace(".mp4", "");
		myNode.video = myNode.video.Replace("\\", "/");

		myNode.outgoingLinks = GetOutgoingLinks(de);

		return myNode;
	}
		
	private void ProcessAARDebugger()
	{
		aarDebugger.GatherDebugInfo();
	}

}
