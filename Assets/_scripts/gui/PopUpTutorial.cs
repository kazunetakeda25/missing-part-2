using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PopUpTutorial : MonoBehaviour 
{
	public GameObject guiElements;
	public Text leadinText;
	private ActionPopUpTutorial actionParent;

	private static PopUpTutorial instance;
	public static PopUpTutorial Instance
	{
		get { return instance; }
	}

	private void Awake()
	{
		instance = this;
	}

	public void OnOKClicked()
	{
		Debug.Log ("On OK Clicked.");
		NGUITools.SetActive(guiElements.gameObject, false);
		actionParent.DismissHint();
	}

	public void ShowTutorial(ActionPopUpTutorial actionToReportTo, string leadin)
	{
		leadinText.text = leadin;
		NGUITools.SetActive(guiElements.gameObject, true);
		this.actionParent = actionToReportTo;
		AC.KickStarter.stateHandler.gameState = AC.GameState.Normal;
	}
}
