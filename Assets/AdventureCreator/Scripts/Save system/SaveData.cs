/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"SaveData.cs"
 * 
 *	This script contains all the non-scene-specific data we wish to save.
 * 
 */

using System.Collections.Generic;

namespace AC
{

	[System.Serializable]
	public class SaveData
	{

		public MainData mainData;
		public List<PlayerData> playerData = new List<PlayerData>();
		public SaveData() { }

	}


	[System.Serializable]
	public struct MainData
	{

		public int currentPlayerID;
		public float timeScale;
		
		public string runtimeVariablesData;

		public string menuLockData;
		public string menuVisibilityData;
		public string menuElementVisibilityData;
		public string menuJournalData;

		public int activeArrows;
		public int activeConversation;

		public int selectedInventoryID;
		public bool isGivingItem;

		public bool cursorIsOff;
		public bool inputIsOff;
		public bool interactionIsOff;
		public bool menuIsOff;
		public bool movementIsOff;
		public bool cameraIsOff;
		public bool triggerIsOff;
		public bool playerIsOff;

		public int movementMethod;

	}


	[System.Serializable]
	public struct PlayerData
	{

		public int playerID;
		public int currentScene;
		public int previousScene;
		public string currentSceneName;
		public string previousSceneName;
		
		public float playerLocX;
		public float playerLocY;
		public float playerLocZ;		
		public float playerRotY;
		
		public float playerWalkSpeed;
		public float playerRunSpeed;
		
		public string playerIdleAnim;
		public string playerWalkAnim;
		public string playerTalkAnim;
		public string playerRunAnim;

		public string playerWalkSound;
		public string playerRunSound;
		public string playerPortraitGraphic;
		public string playerSpeechLabel;

		public int playerTargetNode;
		public int playerPrevNode;
		public string playerPathData;
		public bool playerIsRunning;
		public bool playerLockedPath;
		public int playerActivePath;
		public bool playerPathAffectY;

		public int lastPlayerTargetNode;
		public int lastPlayerPrevNode;
		public int lastPlayerActivePath;

		public bool playerUpLock;
		public bool playerDownLock;
		public bool playerLeftlock;
		public bool playerRightLock;
		public int playerRunLock;
		public bool playerFreeAimLock;
		public bool playerIgnoreGravity;
		
		public bool playerLockDirection;
		public string playerSpriteDirection;
		public bool playerLockScale;
		public float playerSpriteScale;
		public bool playerLockSorting;
		public int playerSortingOrder;
		public string playerSortingLayer;
		
		public string inventoryData;

		public bool playerLockHotspotHeadTurning;
		public bool isHeadTurning;
		public float headTargetX;
		public float headTargetY;
		public float headTargetZ;

		public int gameCamera;
		public int lastNavCamera;
		public int lastNavCamera2;
		
		public float mainCameraLocX;
		public float mainCameraLocY;
		public float mainCameraLocZ;
		public float mainCameraRotX;
		public float mainCameraRotY;
		public float mainCameraRotZ;

		public bool isSplitScreen;
		public bool isTopLeftSplit;
		public bool splitIsVertical;
		public int splitCameraID;
		public float splitAmountMain;
		public float splitAmountOther;


		public void CopyData (PlayerData originalData)
		{
			playerID = originalData.playerID;
			currentScene = originalData.currentScene;
			previousScene = originalData.previousScene;
			currentSceneName = originalData.currentSceneName;
			previousSceneName = originalData.previousSceneName;
			
			playerLocX = originalData.playerLocX;
			playerLocY = originalData.playerLocY;
			playerLocZ = originalData.playerLocZ;
			playerRotY = originalData.playerRotY;
			
			playerWalkSpeed = originalData.playerWalkSpeed;
			playerRunSpeed = originalData.playerRunSpeed;
			
			playerIdleAnim = originalData.playerIdleAnim;
			playerWalkAnim = originalData.playerWalkAnim;
			playerTalkAnim = originalData.playerTalkAnim;
			playerRunAnim = originalData.playerRunAnim;
			
			playerWalkSound = originalData.playerWalkSound;
			playerRunSound = originalData.playerRunSound;
			playerPortraitGraphic = originalData.playerPortraitGraphic;
			playerSpeechLabel = originalData.playerSpeechLabel;
			
			playerTargetNode = originalData.playerTargetNode;
			playerPrevNode = originalData.playerPrevNode;
			playerPathData = originalData.playerPathData;
			playerIsRunning = originalData.playerIsRunning;
			playerLockedPath = originalData.playerLockedPath;
			playerActivePath = originalData.playerActivePath;
			playerPathAffectY = originalData.playerPathAffectY;
			
			lastPlayerTargetNode = originalData.lastPlayerTargetNode;
			lastPlayerPrevNode = originalData.playerPrevNode;
			lastPlayerActivePath = originalData.playerActivePath;
			
			playerUpLock = originalData.playerUpLock;
			playerDownLock = originalData.playerDownLock;
			playerLeftlock = originalData.playerLeftlock;
			playerRightLock = originalData.playerRightLock;
			playerRunLock = originalData.playerRunLock;
			playerFreeAimLock = originalData.playerFreeAimLock;
			playerIgnoreGravity = originalData.playerIgnoreGravity;
			
			playerLockDirection = originalData.playerLockDirection;
			playerSpriteDirection = originalData.playerSpriteDirection;
			playerLockScale = originalData.playerLockScale;
			playerSpriteScale = originalData.playerSpriteScale;
			playerLockSorting = originalData.playerLockSorting;
			playerSortingOrder = originalData.playerSortingOrder;
			playerSortingLayer = originalData.playerSortingLayer;
			
			inventoryData = originalData.inventoryData;
			
			isHeadTurning = originalData.isHeadTurning;
			headTargetX = originalData.headTargetX;
			headTargetY = originalData.headTargetY;
			headTargetZ = originalData.headTargetZ;
			
			gameCamera = originalData.gameCamera;
			lastNavCamera = originalData.lastNavCamera;
			lastNavCamera2 = originalData.lastNavCamera2;
			mainCameraLocX = originalData.mainCameraLocX;
			mainCameraLocY = originalData.mainCameraLocY;
			mainCameraLocZ = originalData.mainCameraLocZ;
			
			mainCameraRotX = originalData.mainCameraRotX;
			mainCameraRotY = originalData.mainCameraRotY;
			mainCameraRotZ = originalData.mainCameraRotZ;
			
			isSplitScreen = originalData.isSplitScreen;
			isTopLeftSplit = originalData.isTopLeftSplit;
			splitIsVertical = originalData.splitIsVertical;
			splitCameraID = originalData.splitCameraID;
			splitAmountMain = originalData.splitAmountMain;
			splitAmountOther = originalData.splitAmountOther;

		}

	}

}