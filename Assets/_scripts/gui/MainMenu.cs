using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour 
{
	public ShowInputControls showInputControls;

	private void Start()
	{
		showInputControls.ShowMainMenu();
	}
}
