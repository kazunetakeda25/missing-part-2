/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"References.cs"
 * 
 *	This script stores references to each of the managers that store the main game data.
 *	Each of the references need to be assigned for the game to work,
 *	and an asset file of this script must be placed in the Resources folder.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	[System.Serializable]
	public class References : ScriptableObject
	{
		public ActionsManager actionsManager;
		public SceneManager sceneManager;
		public SettingsManager settingsManager;
		public InventoryManager inventoryManager;
		public VariablesManager variablesManager;
		public SpeechManager speechManager;
		public CursorManager cursorManager;
		public MenuManager menuManager;
	}

}