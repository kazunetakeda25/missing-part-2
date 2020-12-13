using UnityEngine;
using System.Collections;

public class HintPopUp : MonoBehaviour 
{
	public GameObject guiElements;
	public UILabel hintLabel;
	private ActionPopUp actionParent;

	private static HintPopUp instance;
	public static HintPopUp Instance
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

	public void ShowHint(ActionPopUp actionToReportTo, string content)
	{
		NGUITools.SetActive(guiElements.gameObject, true);
		this.hintLabel.text = content;
		this.actionParent = actionToReportTo;
		AC.KickStarter.stateHandler.gameState = AC.GameState.Normal;
	}
}
