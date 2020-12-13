using UnityEngine;
using System.Collections;

public class CreateSession : MonoBehaviour {

	public dfButton
		male, female,
		fidelityOn, fidelityOff,
		scoringOn, scoringOff,
		hintsOn, hintsOff;

	public dfTextbox userID;
	public dfListbox templateList;

	public Color32 activeColor, inactiveColor;

	public bool
		isMale = true,
		useFidelity = true,
		useScoring = true,
		useHints = true;

	private ShowInputControls guiManager;

	void Start()
	{
		guiManager = GameObject.Find("GuiManager").GetComponent<ShowInputControls>();


		// Todo: 
		// 	Get template files
		// 	Add clicktofade for each template in the list
		// 	When new templates are made, add clicktofade as well


		RefreshLabels();
	}

	#region OnClick event handlers

	public void ChangeToMale(){ isMale = true; RefreshLabels(); }
	public void ChangeToFemale(){ isMale = false; RefreshLabels(); }

	public void TurnFidelityOn(){ useFidelity = true; RefreshLabels(); }
	public void TurnFidelityOff(){ useFidelity = false; RefreshLabels(); }

	public void TurnScoringOn(){ useScoring = true; RefreshLabels(); }
	public void TurnScoringOff(){ useScoring = false; RefreshLabels(); }

	public void TurnHintsOn(){ useHints = true; RefreshLabels(); }
	public void TurnHintsOff(){ useHints = false; RefreshLabels(); }

	public void HideUsernamePlaceholder()
	{
		if( userID.Text == "Your Name" )
			userID.Text = "";
	}
	public void CheckUsername()
	{
		if( userID.Text == "" )
			userID.Text = "Your Name";
	}

	#endregion

	/// <summary>
	/// Resets the game options to their default values.
	/// </summary>
	public void ResetToDefaults()
	{
		templateList.SelectedValue = "none";
		isMale = true;
		useFidelity = true;
		useScoring = true;
		useHints = true;
		RefreshLabels();
	}

	public void LoadTemplate()
	{
		Debug.Log( "Loading Template: " + templateList.SelectedValue );
		SetTemplate(templateList.SelectedValue);
		FadeRecipient fade = templateList.transform.parent.GetComponent<FadeRecipient>();
		fade.Fade();
	}

	/// <summary>
	/// Refreshs the color of text labels depending on the values the user has selected.
	/// </summary>
	void RefreshLabels()
	{
		// Gender
		if( isMale )
		{
			male.TextColor = activeColor;
			female.TextColor = inactiveColor;
		}
		else
		{
			male.TextColor = inactiveColor;
			female.TextColor = activeColor;
		}

		// Fidelity
		if( useFidelity )
		{
			fidelityOn.TextColor = activeColor;
			fidelityOff.TextColor = inactiveColor;
		}
		else
		{
			fidelityOn.TextColor = inactiveColor;
			fidelityOff.TextColor = activeColor;
		}

		// Scoring
		if( useScoring )
		{
			scoringOn.TextColor = activeColor;
			scoringOff.TextColor = inactiveColor;
		}
		else
		{
			scoringOn.TextColor = inactiveColor;
			scoringOff.TextColor = activeColor;
		}

		// Hints
		if( useHints )
		{
			hintsOn.TextColor = activeColor;
			hintsOff.TextColor = inactiveColor;
		}
		else
		{
			hintsOn.TextColor = inactiveColor;
			hintsOff.TextColor = activeColor;
		}
	}

	/// <summary>
	/// Create a session based on the values selected.
	/// </summary>
	public void Submit()
	{
		Debug.Log("User ID: " + userID.Text + " -- Gender is male?: " + isMale + ", Fidelity?: " + useFidelity + ", Scoring?: " + 
		          useScoring + ", Hints?: " + useHints + ", Template: " + templateList.SelectedValue);

		Sex sex = isMale ? Sex.MALE : Sex.FEMALE;

		SessionManager.Instance.StartNewSession(userID.Text, sex);
		Settings.Instance.SetGameIVs(isMale, useHints, useFidelity, useScoring);
		ReportEvent.ReportPlayerInfo(sex, userID.Text);
		SessionManager.Instance.GotoNextLevel();
	}

	public void BackToMainMenu()
	{
		// Remove create session screen
		FadeRecipient fade = transform.parent.gameObject.GetComponent<FadeRecipient>();
		fade.DestroyObjectOnFadeOut( transform.parent.gameObject );
		fade.Fade();

		guiManager.ShowMainMenu();
	}

	private void SetTemplate(string template)
	{
		switch(template) {
		case "C1_F0R0P0":
			useFidelity = false;
			useScoring = false;
			useHints = false;
			break;
		case "C2_F0R0P1":
			useFidelity = false;
			useScoring = false;
			useHints = true;
			break;
		case "C3_F0R1P0":
			useFidelity = false;
			useScoring = true;
			useHints = false;
			break;
		case "C4_F0R1P1":
			useFidelity = false;
			useScoring = true;
			useHints = true;
			break;
		case "C5_F1R0P0":
			useFidelity = true;
			useScoring = false;
			useHints = false;
			break;
		case "C6_F1R0P1":
			useFidelity = true;
			useScoring = false;
			useHints = true;
			break;
		case "C7_F1R1P0":
			useFidelity = true;
			useScoring = true;
			useHints = false;
			break;
		case "C8_F1R1P1":
			useFidelity = true;
			useScoring = true;
			useHints = true;
			break;
		}

		RefreshLabels();
	}
}
