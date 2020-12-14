/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"SettingsManager.cs"
 * 
 *	This script handles the "Settings" tab of the main wizard.
 *	It is used to define the player, and control methods of the game.
 * 
 */

using UnityEngine;
#if UNITY_5
using UnityEngine.Audio;
#endif
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	[System.Serializable]
	public class SettingsManager : ScriptableObject
	{
		
		#if UNITY_EDITOR
		private static GUIContent
			deleteContent = new GUIContent("-", "Delete item");
		
		private static GUILayoutOption
			buttonWidth = GUILayout.MaxWidth (20f);
		#endif
		
		// Save settings
		public string saveFileName = "";
		public SaveTimeDisplay saveTimeDisplay = SaveTimeDisplay.DateOnly;
		public bool takeSaveScreenshots;
		
		// Cutscene settings
		public ActionListAsset actionListOnStart;
		public bool blackOutWhenSkipping = false;
		
		// Character settings
		public PlayerSwitching playerSwitching = PlayerSwitching.DoNotAllow;
		public Player player;
		public List<PlayerPrefab> players = new List<PlayerPrefab>();
		public bool shareInventory = false;
		
		// Interface settings
		public MovementMethod movementMethod = MovementMethod.PointAndClick;
		public InputMethod inputMethod = InputMethod.MouseAndKeyboard;
		public AC_InteractionMethod interactionMethod = AC_InteractionMethod.ContextSensitive;
		public CancelInteractions cancelInteractions = CancelInteractions.CursorLeavesMenuOrHotspot;
		public SeeInteractions seeInteractions = SeeInteractions.ClickOnHotspot;
		public SelectInteractions selectInteractions = SelectInteractions.ClickingMenu;
		public bool stopPlayerOnClickHotspot = false;
		public bool cycleInventoryCursors = true;
		public bool autoCycleWhenInteract = false;
		public bool hideLockedCursor = false;
		public bool lockCursorOnStart = false;
		public bool disableFreeAimWhenDragging = false;
		public bool runConversationsWithKeys = false;

		// Inventory settings
		public bool inventoryDragDrop = false;
		public bool inventoryDropLook = false;
		public bool useOuya = false;
		public InventoryInteractions inventoryInteractions = InventoryInteractions.Single;
		public InventoryActiveEffect inventoryActiveEffect = InventoryActiveEffect.Simple;
		public bool activeWhenHover = false;
		public bool inventoryDisableLeft = true;
		public float inventoryPulseSpeed = 1f;
		public bool activeWhenUnhandled = true;
		public bool canReorderItems = false;
		public bool hideSelectedFromMenu = false;
		public RightClickInventory rightClickInventory = RightClickInventory.DeselectsItem;
		public bool reverseInventoryCombinations = false;
		public bool canMoveWhenActive = true;
		public bool selectInvWithUnhandled = false;
		public int selectInvWithIconID = 0;
		public bool giveInvWithUnhandled = false;
		public int giveInvWithIconID = 0;
		
		// Movement settings
		public Transform clickPrefab;
		public DirectMovementType directMovementType = DirectMovementType.RelativeToCamera;
		public LimitDirectMovement limitDirectMovement = LimitDirectMovement.NoLimit;
		public bool directMovementPerspective = false;
		public float destinationAccuracy = 0.9f;
		public float walkableClickRange = 0.5f;
		public DragAffects dragAffects = DragAffects.Movement;
		public float verticalReductionFactor = 0.7f;
		public bool doubleClickMovement = false;
		public float jumpSpeed = 4f;
		public bool singleTapStraight = false;
		public bool singleTapStraightPathfind = false;
		public bool magnitudeAffectsDirect = false;
		public bool onlyInteractWhenCursorUnlocked = false; // UFPS only

		// Input settings
		public List<ActiveInput> activeInputs = new List<ActiveInput>();
		
		// Drag settings
		public float freeAimTouchSpeed = 0.01f;
		public float dragWalkThreshold = 5f;
		public float dragRunThreshold = 20f;
		public bool drawDragLine = false;
		public float dragLineWidth = 3f;
		public Color dragLineColor = Color.white;
		
		// Touch Screen settings
		public bool offsetTouchCursor = false;
		public bool doubleTapHotspots = true;
		
		// Camera settings
		public bool forceAspectRatio = false;
		public float wantedAspectRatio = 1.5f;
		public bool landscapeModeOnly = true;
		public CameraPerspective cameraPerspective = CameraPerspective.ThreeD;
		private int cameraPerspective_int;
		#if UNITY_EDITOR
		private string[] cameraPerspective_list = { "2D", "2.5D", "3D" };
		#endif
		public MovingTurning movingTurning = MovingTurning.Unity2D;
		
		// Hotspot settings
		public HotspotDetection hotspotDetection = HotspotDetection.MouseOver;
		public HotspotsInVicinity hotspotsInVicinity = HotspotsInVicinity.NearestOnly;
		public HotspotIconDisplay hotspotIconDisplay = HotspotIconDisplay.Never;
		public HotspotIcon hotspotIcon;
		public Texture2D hotspotIconTexture = null;
		public float hotspotIconSize = 0.04f;
		public bool playerFacesHotspots = false;
		public bool scaleHighlightWithMouseProximity = false;
		public float highlightProximityFactor = 4f;
		public bool occludeIcons = false;
		
		// Raycast settings
		public float navMeshRaycastLength = 100f;
		public float hotspotRaycastLength = 100f;
		public float moveableRaycastLength = 30f;
		
		// Layer names
		public string hotspotLayer = "Default";
		public string navMeshLayer = "NavMesh";
		public string backgroundImageLayer = "BackgroundImage";
		public string deactivatedLayer = "Ignore Raycast";
		
		// Loading screen
		public bool useLoadingScreen = false;
		public ChooseSceneBy loadingSceneIs = ChooseSceneBy.Number;
		public string loadingSceneName = "";
		public int loadingScene = 0;

		// Sound settings
		#if UNITY_5
		public VolumeControl volumeControl = VolumeControl.AudioSources;
		public AudioMixerGroup musicMixerGroup = null;
		public AudioMixerGroup sfxMixerGroup = null;
		public AudioMixerGroup speechMixerGroup = null;
		public string musicAttentuationParameter = "musicVolume";
		public string sfxAttentuationParameter = "sfxVolume";
		public string speechAttentuationParameter = "speechVolume";
		#endif

		// Options data
		public int defaultLanguage = 0;
		public bool defaultShowSubtitles = false;
		public float defaultSfxVolume = 0.9f;
		public float defaultMusicVolume = 0.6f;
		public float defaultSpeechVolume = 1f;

		#if UNITY_EDITOR
		private OptionsData optionsData = new OptionsData ();
		private string ppKey = "Options";
		private string optionsBinary = "";

		// Debug
		public bool showActiveActionLists = false;
		public bool showHierarchyIcons = true;

		
		public void ShowGUI ()
		{
			EditorGUILayout.LabelField ("Save game settings", EditorStyles.boldLabel);
			
			if (saveFileName == "")
			{
				saveFileName = SaveSystem.SetProjectName ();
			}
			saveFileName = EditorGUILayout.TextField ("Save filename:", saveFileName);
			#if !UNITY_WEBPLAYER && !UNITY_ANDROID
			saveTimeDisplay = (SaveTimeDisplay) EditorGUILayout.EnumPopup ("Time display:", saveTimeDisplay);
			takeSaveScreenshots = EditorGUILayout.ToggleLeft ("Take screenshot when saving?", takeSaveScreenshots);
			#else
			EditorGUILayout.HelpBox ("Save-game screenshots are disabled for WebPlayer and Android platforms.", MessageType.Info);
			takeSaveScreenshots = false;
			#endif
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Cutscene settings:", EditorStyles.boldLabel);
			
			actionListOnStart = ActionListAssetMenu.AssetGUI ("ActionList on start game:", actionListOnStart);
			blackOutWhenSkipping = EditorGUILayout.Toggle ("Black out when skipping?", blackOutWhenSkipping);
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Character settings:", EditorStyles.boldLabel);
			
			CreatePlayersGUI ();
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Interface settings", EditorStyles.boldLabel);
			
			movementMethod = (MovementMethod) EditorGUILayout.EnumPopup ("Movement method:", movementMethod);
			if (movementMethod == MovementMethod.UltimateFPS && !UltimateFPSIntegration.IsDefinePresent ())
			{
				EditorGUILayout.HelpBox ("The 'UltimateFPSIsPresent' preprocessor define must be declared in the Player Settings.", MessageType.Warning);
			}

			inputMethod = (InputMethod) EditorGUILayout.EnumPopup ("Input method:", inputMethod);
			interactionMethod = (AC_InteractionMethod) EditorGUILayout.EnumPopup ("Interaction method:", interactionMethod);
			
			if (inputMethod != InputMethod.TouchScreen)
			{
				useOuya = EditorGUILayout.ToggleLeft ("Playing on OUYA platform?", useOuya);
				if (useOuya && !OuyaIntegration.IsDefinePresent ())
				{
					EditorGUILayout.HelpBox ("The 'OUYAIsPresent' preprocessor define must be declared in the Player Settings.", MessageType.Warning);
				}
				if (interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
				{
					selectInteractions = (SelectInteractions) EditorGUILayout.EnumPopup ("Select Interactions by:", selectInteractions);
					if (selectInteractions != SelectInteractions.CyclingCursorAndClickingHotspot)
					{
						seeInteractions = (SeeInteractions) EditorGUILayout.EnumPopup ("See Interactions with:", seeInteractions);
						if (seeInteractions == SeeInteractions.ClickOnHotspot)
						{
							stopPlayerOnClickHotspot = EditorGUILayout.ToggleLeft ("Stop player moving when click Hotspot?", stopPlayerOnClickHotspot);
						}
					}

					if (selectInteractions == SelectInteractions.CyclingCursorAndClickingHotspot)
					{
						autoCycleWhenInteract = EditorGUILayout.ToggleLeft ("Auto-cycle after an Interaction?", autoCycleWhenInteract);
					}
				
					if (SelectInteractionMethod () == SelectInteractions.ClickingMenu)
					{
						cancelInteractions = (CancelInteractions) EditorGUILayout.EnumPopup ("Close interactions with:", cancelInteractions);
					}
					else
					{
						cancelInteractions = CancelInteractions.CursorLeavesMenu;
					}
				}
			}
			if (interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
			{
				autoCycleWhenInteract = EditorGUILayout.ToggleLeft ("Reset cursor after an Interaction?", autoCycleWhenInteract);
			}
			lockCursorOnStart = EditorGUILayout.ToggleLeft ("Lock cursor in screen's centre when game begins?", lockCursorOnStart);
			hideLockedCursor = EditorGUILayout.ToggleLeft ("Hide cursor when locked in screen's centre?", hideLockedCursor);
			onlyInteractWhenCursorUnlocked = EditorGUILayout.ToggleLeft ("Disallow Interactions if cursor is unlocked?", onlyInteractWhenCursorUnlocked);
			if (IsInFirstPerson ())
			{
				disableFreeAimWhenDragging = EditorGUILayout.ToggleLeft ("Disable free-aim when dragging?", disableFreeAimWhenDragging);
			}
			if (inputMethod != InputMethod.TouchScreen)
			{
				runConversationsWithKeys = EditorGUILayout.ToggleLeft ("Dialogue options can be selected with number keys?", runConversationsWithKeys);
			}

			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Inventory settings", EditorStyles.boldLabel);

			if (interactionMethod != AC_InteractionMethod.ContextSensitive)
			{
				inventoryInteractions = (InventoryInteractions) EditorGUILayout.EnumPopup ("Inventory interactions:", inventoryInteractions);

				if (interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
				{
					if (selectInteractions == SelectInteractions.CyclingCursorAndClickingHotspot)
					{
						cycleInventoryCursors = EditorGUILayout.ToggleLeft ("Include Inventory items in Interaction cycles?", cycleInventoryCursors);
					}
					else
					{
						cycleInventoryCursors = EditorGUILayout.ToggleLeft ("Include Inventory items in Interaction menus?", cycleInventoryCursors);
					}
				}

				if (inventoryInteractions == InventoryInteractions.Multiple && CanSelectItems (false))
				{
					selectInvWithUnhandled = EditorGUILayout.ToggleLeft ("Select item if Interaction is unhandled?", selectInvWithUnhandled);
					if (selectInvWithUnhandled)
					{
						CursorManager cursorManager = AdvGame.GetReferences ().cursorManager;
						if (cursorManager != null && cursorManager.cursorIcons != null && cursorManager.cursorIcons.Count > 0)
						{
							selectInvWithIconID = GetIconID ("Select with unhandled:", selectInvWithIconID, cursorManager);
						}
						else
						{
							EditorGUILayout.HelpBox ("No Interaction cursors defined - please do so in the Cursor Manager.", MessageType.Info);
						}
					}

					giveInvWithUnhandled = EditorGUILayout.ToggleLeft ("Give item if Interaction is unhandled?", giveInvWithUnhandled);
					if (giveInvWithUnhandled)
					{
						CursorManager cursorManager = AdvGame.GetReferences ().cursorManager;
						if (cursorManager != null && cursorManager.cursorIcons != null && cursorManager.cursorIcons.Count > 0)
						{
							giveInvWithIconID = GetIconID ("Give with unhandled:", giveInvWithIconID, cursorManager);
						}
						else
						{
							EditorGUILayout.HelpBox ("No Interaction cursors defined - please do so in the Cursor Manager.", MessageType.Info);
						}
					}
				}
			}

			if (interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction && selectInteractions != SelectInteractions.ClickingMenu && inventoryInteractions == InventoryInteractions.Multiple)
			{}
			else
			{
				reverseInventoryCombinations = EditorGUILayout.ToggleLeft ("Combine interactions work in reverse?", reverseInventoryCombinations);
			}

			//if (interactionMethod != AC_InteractionMethod.ChooseHotspotThenInteraction || inventoryInteractions == InventoryInteractions.Single)
			if (CanSelectItems (false))
			{
				inventoryDragDrop = EditorGUILayout.ToggleLeft ("Drag and drop Inventory interface?", inventoryDragDrop);
				if (!inventoryDragDrop)
				{
					if (interactionMethod == AC_InteractionMethod.ContextSensitive || inventoryInteractions == InventoryInteractions.Single)
					{
						rightClickInventory = (RightClickInventory) EditorGUILayout.EnumPopup ("Right-click active item:", rightClickInventory);
					}
				}
				else if (inventoryInteractions == AC.InventoryInteractions.Single)
				{
					inventoryDropLook = EditorGUILayout.ToggleLeft ("Can drop an Item onto itself to Examine it?", inventoryDropLook);
				}
			}

			if (CanSelectItems (false) && !inventoryDragDrop)
			{
				inventoryDisableLeft = EditorGUILayout.ToggleLeft ("Left-click deselects active item?", inventoryDisableLeft);
				
				if (movementMethod == MovementMethod.PointAndClick && !inventoryDisableLeft)
				{
					canMoveWhenActive = EditorGUILayout.ToggleLeft ("Can move player if an Item is active?", canMoveWhenActive);
				}
			}

			inventoryActiveEffect = (InventoryActiveEffect) EditorGUILayout.EnumPopup ("Active cursor FX:", inventoryActiveEffect);
			if (inventoryActiveEffect == InventoryActiveEffect.Pulse)
			{
				inventoryPulseSpeed = EditorGUILayout.Slider ("Active FX pulse speed:", inventoryPulseSpeed, 0.5f, 2f);
			}

			activeWhenUnhandled = EditorGUILayout.ToggleLeft ("Show Active FX when an Interaction is unhandled?", activeWhenUnhandled);
			canReorderItems = EditorGUILayout.ToggleLeft ("Items can be re-ordered in Menu?", canReorderItems);
			hideSelectedFromMenu = EditorGUILayout.ToggleLeft ("Hide currently active Item in Menu?", hideSelectedFromMenu);
			activeWhenHover = EditorGUILayout.ToggleLeft ("Show Active FX when Cursor hovers over Item in Menu?", activeWhenHover);

			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Required inputs:", EditorStyles.boldLabel);
			EditorGUILayout.HelpBox ("The following inputs are available for the chosen interface settings:" + GetInputList (), MessageType.Info);
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Movement settings", EditorStyles.boldLabel);
			
			if ((inputMethod == InputMethod.TouchScreen && movementMethod != MovementMethod.PointAndClick) || movementMethod == MovementMethod.Drag)
			{
				dragWalkThreshold = EditorGUILayout.FloatField ("Walk threshold:", dragWalkThreshold);
				dragRunThreshold = EditorGUILayout.FloatField ("Run threshold:", dragRunThreshold);
				
				if (inputMethod == InputMethod.TouchScreen && movementMethod == MovementMethod.FirstPerson)
				{
					freeAimTouchSpeed = EditorGUILayout.FloatField ("Freelook speed:", freeAimTouchSpeed);
				}
				
				drawDragLine = EditorGUILayout.Toggle ("Draw drag line?", drawDragLine);
				if (drawDragLine)
				{
					dragLineWidth = EditorGUILayout.FloatField ("Drag line width:", dragLineWidth);
					dragLineColor = EditorGUILayout.ColorField ("Drag line colour:", dragLineColor);
				}
			}
			else if (movementMethod == MovementMethod.Direct)
			{
				magnitudeAffectsDirect = EditorGUILayout.ToggleLeft ("Input magnitude affects speed?", magnitudeAffectsDirect);
				directMovementType = (DirectMovementType) EditorGUILayout.EnumPopup ("Direct-movement type:", directMovementType);
				if (directMovementType == DirectMovementType.RelativeToCamera)
				{
					limitDirectMovement = (LimitDirectMovement) EditorGUILayout.EnumPopup ("Movement limitation:", limitDirectMovement);
					if (cameraPerspective == CameraPerspective.ThreeD)
					{
						directMovementPerspective = EditorGUILayout.ToggleLeft ("Account for player's position on screen?", directMovementPerspective);
					}
				}
			}
			else if (movementMethod == MovementMethod.PointAndClick)
			{
				clickPrefab = (Transform) EditorGUILayout.ObjectField ("Click marker:", clickPrefab, typeof (Transform), false);
				walkableClickRange = EditorGUILayout.Slider ("NavMesh search %:", walkableClickRange, 0f, 1f);
				doubleClickMovement = EditorGUILayout.Toggle ("Double-click to move?", doubleClickMovement);
			}
			if (movementMethod == MovementMethod.StraightToCursor)
			{
				dragRunThreshold = EditorGUILayout.FloatField ("Run threshold:", dragRunThreshold);
				singleTapStraight = EditorGUILayout.ToggleLeft ("Single-clicking also moves player?", singleTapStraight);
				if (singleTapStraight)
				{
					singleTapStraightPathfind = EditorGUILayout.ToggleLeft ("Pathfind when single-clicking?", singleTapStraightPathfind);
				}
			}
			if (movementMethod == MovementMethod.FirstPerson && inputMethod == InputMethod.TouchScreen)
			{
				dragAffects = (DragAffects) EditorGUILayout.EnumPopup ("Touch-drag affects:", dragAffects);
			}
			if ((movementMethod == MovementMethod.Direct || movementMethod == MovementMethod.FirstPerson) && inputMethod != InputMethod.TouchScreen)
			{
				jumpSpeed = EditorGUILayout.Slider ("Jump speed:", jumpSpeed, 1f, 10f);
			}
			
			destinationAccuracy = EditorGUILayout.Slider ("Destination accuracy:", destinationAccuracy, 0f, 1f);
			
			if (inputMethod == InputMethod.TouchScreen)
			{
				EditorGUILayout.Space ();
				EditorGUILayout.LabelField ("Touch Screen settings", EditorStyles.boldLabel);
				
				offsetTouchCursor = EditorGUILayout.Toggle ("Drag cursor with touch?", offsetTouchCursor);
				doubleTapHotspots = EditorGUILayout.Toggle ("Double-tap Hotspots?", doubleTapHotspots);
			}
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Camera settings", EditorStyles.boldLabel);
			
			cameraPerspective_int = (int) cameraPerspective;
			cameraPerspective_int = EditorGUILayout.Popup ("Camera perspective:", cameraPerspective_int, cameraPerspective_list);
			cameraPerspective = (CameraPerspective) cameraPerspective_int;
			if (movementMethod == MovementMethod.FirstPerson)
			{
				cameraPerspective = CameraPerspective.ThreeD;
			}
			if (cameraPerspective == CameraPerspective.TwoD)
			{
				movingTurning = (MovingTurning) EditorGUILayout.EnumPopup ("Moving and turning:", movingTurning);
				if (movingTurning == MovingTurning.TopDown || movingTurning == MovingTurning.Unity2D)
				{
					verticalReductionFactor = EditorGUILayout.Slider ("Vertical movement factor:", verticalReductionFactor, 0.1f, 1f);
				}
			}
			
			forceAspectRatio = EditorGUILayout.Toggle ("Force aspect ratio?", forceAspectRatio);
			if (forceAspectRatio)
			{
				wantedAspectRatio = EditorGUILayout.FloatField ("Aspect ratio:", wantedAspectRatio);
				#if UNITY_IPHONE
				landscapeModeOnly = EditorGUILayout.Toggle ("Landscape-mode only?", landscapeModeOnly);
				#endif
			}
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Hotpot settings", EditorStyles.boldLabel);
			
			hotspotDetection = (HotspotDetection) EditorGUILayout.EnumPopup ("Hotspot detection method:", hotspotDetection);
			if (hotspotDetection == HotspotDetection.PlayerVicinity && (movementMethod == MovementMethod.Direct || IsInFirstPerson ()))
			{
				hotspotsInVicinity = (HotspotsInVicinity) EditorGUILayout.EnumPopup ("Hotspots in vicinity:", hotspotsInVicinity);
			}
			else if (hotspotDetection == HotspotDetection.MouseOver)
			{
				scaleHighlightWithMouseProximity = EditorGUILayout.ToggleLeft ("Highlight Hotspots based on cursor proximity?", scaleHighlightWithMouseProximity);
				if (scaleHighlightWithMouseProximity)
				{
					highlightProximityFactor = EditorGUILayout.FloatField ("Cursor proximity factor:", highlightProximityFactor);
				}
			}
			
			if (cameraPerspective != CameraPerspective.TwoD)
			{
				playerFacesHotspots = EditorGUILayout.Toggle ("Player turns head to active?", playerFacesHotspots);
			}
			
			hotspotIconDisplay = (HotspotIconDisplay) EditorGUILayout.EnumPopup ("Display Hotspot icon:", hotspotIconDisplay);
			if (hotspotIconDisplay != HotspotIconDisplay.Never)
			{
				if (cameraPerspective != CameraPerspective.TwoD)
				{
					occludeIcons = EditorGUILayout.Toggle ("Don't show behind Colliders?", occludeIcons);
				}
				hotspotIcon = (HotspotIcon) EditorGUILayout.EnumPopup ("Hotspot icon type:", hotspotIcon);
				if (hotspotIcon == HotspotIcon.Texture)
				{
					hotspotIconTexture = (Texture2D) EditorGUILayout.ObjectField ("Hotspot icon texture:", hotspotIconTexture, typeof (Texture2D), false);
				}
				hotspotIconSize = EditorGUILayout.FloatField ("Hotspot icon size:", hotspotIconSize);
			}

			#if UNITY_5
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Audio settings", EditorStyles.boldLabel);
			volumeControl = (VolumeControl) EditorGUILayout.EnumPopup ("Volume controlled by:", volumeControl);
			if (volumeControl == VolumeControl.AudioMixerGroups)
			{
				musicMixerGroup = (AudioMixerGroup) EditorGUILayout.ObjectField ("Music mixer:", musicMixerGroup, typeof (AudioMixerGroup), false);
				sfxMixerGroup = (AudioMixerGroup) EditorGUILayout.ObjectField ("SFX mixer:", sfxMixerGroup, typeof (AudioMixerGroup), false);
				speechMixerGroup = (AudioMixerGroup) EditorGUILayout.ObjectField ("Speech mixer:", speechMixerGroup, typeof (AudioMixerGroup), false);
				musicAttentuationParameter = EditorGUILayout.TextField ("Music atten. parameter:", musicAttentuationParameter);
				sfxAttentuationParameter = EditorGUILayout.TextField ("SFX atten. parameter:", sfxAttentuationParameter);
				speechAttentuationParameter = EditorGUILayout.TextField ("Speech atten. parameter:", speechAttentuationParameter);
			}
			#endif

			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Raycast settings", EditorStyles.boldLabel);
			navMeshRaycastLength = EditorGUILayout.FloatField ("NavMesh ray length:", navMeshRaycastLength);
			hotspotRaycastLength = EditorGUILayout.FloatField ("Hotspot ray length:", hotspotRaycastLength);
			moveableRaycastLength = EditorGUILayout.FloatField ("Moveable ray length:", moveableRaycastLength);
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Layer names", EditorStyles.boldLabel);
			
			hotspotLayer = EditorGUILayout.TextField ("Hotspot:", hotspotLayer);
			navMeshLayer = EditorGUILayout.TextField ("Nav mesh:", navMeshLayer);
			if (cameraPerspective == CameraPerspective.TwoPointFiveD)
			{
				backgroundImageLayer = EditorGUILayout.TextField ("Background image:", backgroundImageLayer);
			}
			deactivatedLayer = EditorGUILayout.TextField ("Deactivated:", deactivatedLayer);
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Loading scene", EditorStyles.boldLabel);
			useLoadingScreen = EditorGUILayout.Toggle ("Use loading screen?", useLoadingScreen);
			if (useLoadingScreen)
			{
				loadingSceneIs = (ChooseSceneBy) EditorGUILayout.EnumPopup ("Choose loading scene by:", loadingSceneIs);
				if (loadingSceneIs == ChooseSceneBy.Name)
				{
					loadingSceneName = EditorGUILayout.TextField ("Loading scene name:", loadingSceneName);
				}
				else
				{
					loadingScene = EditorGUILayout.IntField ("Loading screen scene:", loadingScene);
				}
			}
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Options data", EditorStyles.boldLabel);
			
			if (!PlayerPrefs.HasKey (ppKey))
			{
				optionsData = new OptionsData ();
				optionsBinary = Serializer.SerializeObjectBinary (optionsData);
				PlayerPrefs.SetString (ppKey, optionsBinary);
			}
			
			optionsBinary = PlayerPrefs.GetString (ppKey);
			if (optionsBinary.Length > 0)
			{
				optionsData = Serializer.DeserializeObjectBinary <OptionsData> (optionsBinary);
			}
			else
			{
				optionsData = new OptionsData ();
			}
			
			defaultSpeechVolume = optionsData.speechVolume = EditorGUILayout.Slider ("Speech volume:", optionsData.speechVolume, 0f, 1f);
			defaultMusicVolume = optionsData.musicVolume = EditorGUILayout.Slider ("Music volume:", optionsData.musicVolume, 0f, 1f);
			defaultSfxVolume = optionsData.sfxVolume = EditorGUILayout.Slider ("SFX volume:", optionsData.sfxVolume, 0f, 1f);
			defaultShowSubtitles = optionsData.showSubtitles = EditorGUILayout.Toggle ("Show subtitles?", optionsData.showSubtitles);
			defaultLanguage = optionsData.language = EditorGUILayout.IntField ("Language:", optionsData.language);
			
			optionsBinary = Serializer.SerializeObjectBinary (optionsData);
			PlayerPrefs.SetString (ppKey, optionsBinary);
			
			if (GUILayout.Button ("Reset options data"))
			{
				PlayerPrefs.DeleteKey ("Options");
				optionsData = new OptionsData ();
				Debug.Log ("PlayerPrefs cleared");
			}

			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Debug settings", EditorStyles.boldLabel);
			showActiveActionLists = EditorGUILayout.ToggleLeft ("List active ActionLists in Game window?", showActiveActionLists);
			showHierarchyIcons = EditorGUILayout.ToggleLeft ("Show icons in Hierarchy window?", showHierarchyIcons);

			
			if (GUI.changed)
			{
				EditorUtility.SetDirty (this);
			}
		}
		
		#endif
		
		
		private string GetInputList ()
		{
			string result = "";
			
			if (inputMethod != InputMethod.TouchScreen)
			{
				result += "\n";
				result += "- InteractionA (Button)";
				result += "\n";
				result += "- InteractionB (Button)";
				result += "\n";
				result += "- CursorHorizontal (Axis)";
				result += "\n";
				result += "- CursorVertical (Axis)";
			}
			
			if (movementMethod != MovementMethod.PointAndClick && movementMethod != MovementMethod.StraightToCursor)
			{
				result += "\n";
				result += "- ToggleCursor (Button)";
			}
			
			if (movementMethod == MovementMethod.Direct || movementMethod == MovementMethod.FirstPerson || inputMethod == InputMethod.KeyboardOrController)
			{
				if (inputMethod != InputMethod.TouchScreen)
				{
					result += "\n";
					result += "- Horizontal (Axis)";
					result += "\n";
					result += "- Vertical (Axis)";
					
					if (movementMethod == MovementMethod.Direct || movementMethod == MovementMethod.FirstPerson)
					{
						result += "\n";
						result += "- Run (Button/Axis)";
						result += "\n";
						result += "- ToggleRun (Button)";
						result += "\n";
						result += "- Jump (Button)";
					}
				}
				
				if (movementMethod == MovementMethod.FirstPerson && inputMethod == InputMethod.MouseAndKeyboard)
				{
					result += "\n";
					result += "- MouseScrollWheel (Axis)";
					result += "\n";
					result += "- CursorHorizontal (Axis)";
					result += "\n";
					result += "- CursorVertical (Axis)";
				}
				
				if ((movementMethod == MovementMethod.Direct || movementMethod == MovementMethod.FirstPerson)
				    && (hotspotDetection == HotspotDetection.PlayerVicinity && hotspotsInVicinity == HotspotsInVicinity.CycleMultiple))
				{
					result += "\n";
					result += "- CycleHotspotsLeft (Button)";
					result += "\n";
					result += "- CycleHotspotsRight (Button)";
					result += "\n";
					result += "- CycleHotspots (Axis)";
				}
			}
			
			if (SelectInteractionMethod () == SelectInteractions.CyclingMenuAndClickingHotspot)
			{
				result += "\n";
				result += "- CycleInteractionsLeft (Button)";
				result += "\n";
				result += "- CycleInteractionsRight (Button)";
				result += "\n";
				result += "- CycleInteractions (Axis)";
			}
			else if (interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
			{
				result += "\n";
				result += "- CycleCursors (Button)";
			}
			
			result += "\n";
			result += "- FlashHotspots (Button)";
			result += "\n";
			result += "- Menu (Button)";
			result += "\n";
			result += "- EndCutscene (Button)";
			result += "\n";
			result += "- ThrowMoveable (Button)";
			result += "\n";
			result += "- RotateMoveable (Button)";
			result += "\n";
			result += "- RotateMoveableToggle (Button)";
			result += "\n";
			result += "- ZoomMoveable (Axis)";
			
			return result;
		}
		
		
		public bool ActInScreenSpace ()
		{
			if ((movingTurning == MovingTurning.ScreenSpace || movingTurning == MovingTurning.Unity2D) && cameraPerspective == CameraPerspective.TwoD)
			{
				return true;
			}
			return false;
		}
		
		
		public bool IsUnity2D ()
		{
			if (movingTurning == MovingTurning.Unity2D && cameraPerspective == CameraPerspective.TwoD)
			{
				return true;
			}
			return false;
		}
		
		
		public bool IsTopDown ()
		{
			if (movingTurning == MovingTurning.TopDown && cameraPerspective == CameraPerspective.TwoD)
			{
				return true;
			}
			return false;
		}
		
		
		public bool IsFirstPersonDragRotation ()
		{
			if (movementMethod == MovementMethod.FirstPerson && inputMethod == InputMethod.TouchScreen && dragAffects == DragAffects.Rotation)
			{
				return true;
			}
			return false;
		}
		
		
		public bool IsFirstPersonDragMovement ()
		{
			if (movementMethod == MovementMethod.FirstPerson && inputMethod == InputMethod.TouchScreen && dragAffects == DragAffects.Movement)
			{
				return true;
			}
			return false;
		}
		
		
		
		#if UNITY_EDITOR
		
		private void CreatePlayersGUI ()
		{
			playerSwitching = (PlayerSwitching) EditorGUILayout.EnumPopup ("Player switching:", playerSwitching);
			if (playerSwitching == PlayerSwitching.DoNotAllow)
			{
				player = (Player) EditorGUILayout.ObjectField ("Player:", player, typeof (Player), false);
			}
			else
			{
				shareInventory = EditorGUILayout.Toggle ("Share same Inventory?", shareInventory);
				
				foreach (PlayerPrefab _player in players)
				{
					EditorGUILayout.BeginHorizontal ();
					
					_player.playerOb = (Player) EditorGUILayout.ObjectField ("Player " + _player.ID + ":", _player.playerOb, typeof (Player), false);
					
					if (_player.isDefault)
					{
						GUILayout.Label ("DEFAULT", EditorStyles.boldLabel, GUILayout.Width (80f));
					}
					else
					{
						if (GUILayout.Button ("Make default", GUILayout.Width (80f)))
						{
							SetDefaultPlayer (_player);
						}
					}
					
					if (GUILayout.Button (deleteContent, EditorStyles.miniButtonRight, buttonWidth))
					{
						Undo.RecordObject (this, "Delete player reference");
						players.Remove (_player);
						break;
					}
					
					EditorGUILayout.EndHorizontal ();
				}
				
				if (GUILayout.Button("Add new player"))
				{
					Undo.RecordObject (this, "Add player");
					
					PlayerPrefab newPlayer = new PlayerPrefab (GetPlayerIDArray ());
					players.Add (newPlayer);
				}
			}
		}


		private int GetIconID (string label, int iconID, CursorManager cursorManager)
		{
			int iconInt = cursorManager.GetIntFromID (iconID);
			iconInt = EditorGUILayout.Popup (label, iconInt, cursorManager.GetLabelsArray (iconInt));
			iconID = cursorManager.cursorIcons[iconInt].id;
			return iconID;
		}

		#endif
		
		
		private int[] GetPlayerIDArray ()
		{
			// Returns a list of id's in the list
			
			List<int> idArray = new List<int>();
			
			foreach (PlayerPrefab player in players)
			{
				idArray.Add (player.ID);
			}
			
			idArray.Sort ();
			return idArray.ToArray ();
		}
		
		
		public int GetDefaultPlayerID ()
		{
			if (playerSwitching == PlayerSwitching.DoNotAllow)
			{
				return 0;
			}
			
			foreach (PlayerPrefab _player in players)
			{
				if (_player.isDefault)
				{
					return _player.ID;
				}
			}
			
			return 0;
		}


		public int GetEmptyPlayerID ()
		{
			if (playerSwitching == PlayerSwitching.DoNotAllow)
			{
				return 0;
			}
			
			foreach (PlayerPrefab _player in players)
			{
				if (_player.playerOb == null)
				{
					return _player.ID;
				}
			}
			
			return 0;
		}
		
		
		public bool CanClickOffInteractionMenu ()
		{
			if (cancelInteractions == CancelInteractions.ClickOffMenu || inputMethod == InputMethod.TouchScreen)
			{
				return true;
			}
			return false;
		}
		
		
		public bool MouseOverForInteractionMenu ()
		{
			if (seeInteractions == SeeInteractions.CursorOverHotspot)
			{
				return true;
			}
			return false;
		}
		
		
		public Player GetDefaultPlayer ()
		{
			if (playerSwitching == PlayerSwitching.DoNotAllow)
			{
				return player;
			}
			
			foreach (PlayerPrefab _player in players)
			{
				if (_player.isDefault)
				{
					if (_player.playerOb != null)
					{
						return _player.playerOb;
					}
					
					Debug.LogWarning ("Default Player has no prefab!");
					return null;
				}
			}
			
			Debug.LogWarning ("Cannot find default player!");
			return null;
		}
		
		
		private void SetDefaultPlayer (PlayerPrefab defaultPlayer)
		{
			foreach (PlayerPrefab _player in players)
			{
				if (_player == defaultPlayer)
				{
					_player.isDefault = true;
				}
				else
				{
					_player.isDefault = false;
				}
			}
		}
		
		
		private bool DoPlayerAnimEnginesMatch ()
		{
			AnimationEngine animationEngine = AnimationEngine.Legacy;
			bool foundFirst = false;
			
			foreach (PlayerPrefab _player in players)
			{
				if (_player.playerOb != null)
				{
					if (!foundFirst)
					{
						foundFirst = true;
						animationEngine = _player.playerOb.animationEngine;
					}
					else
					{
						if (_player.playerOb.animationEngine != animationEngine)
						{
							return false;
						}
					}
				}
			}
			
			return true;
		}
		
		
		public SelectInteractions SelectInteractionMethod ()
		{
			if (inputMethod != InputMethod.TouchScreen && interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
			{
				return selectInteractions;
			}
			return SelectInteractions.ClickingMenu;
		}
		
		
		public bool IsInLoadingScene ()
		{
			if (useLoadingScreen)
			{
				if (loadingSceneIs == ChooseSceneBy.Name)
				{
					if (Application.loadedLevelName != "" && Application.loadedLevelName == loadingSceneName)
					{
						return true;
					}
				}
				else if (loadingSceneIs == ChooseSceneBy.Number)
				{
					if (Application.loadedLevelName != "" && Application.loadedLevel == loadingScene)
					{
						return true;
					}
				}
			}
			return false;
		}
		
		
		private bool IsOnOuya ()
		{
			if (inputMethod != InputMethod.TouchScreen && useOuya)
			{
				return true;
			}
			return false;
		}


		public bool IsInFirstPerson ()
		{
			if (movementMethod == MovementMethod.FirstPerson || movementMethod == MovementMethod.UltimateFPS)
			{
				return true;
			}
			return false;
		}


		public bool CanGiveItems ()
		{
			if (interactionMethod != AC_InteractionMethod.ContextSensitive && CanSelectItems (false))
			{
				return true;
			}
			return false;
		}


		public bool CanSelectItems (bool showError)
		{
			if (interactionMethod != AC_InteractionMethod.ChooseHotspotThenInteraction || selectInteractions != SelectInteractions.CyclingCursorAndClickingHotspot)
			{
				return true;
			}
			if (!cycleInventoryCursors)
			{
				return true;
			}
			if (showError)
			{
				Debug.LogWarning ("Inventory items cannot be selected with this combination of settings - they are included in Interaction cycles instead.");
			}
			return false;
		}
		
	}


	[System.Serializable]
	public class ActiveInput
	{

		public string inputName;
		public GameState gameState;
		public ActionListAsset actionListAsset;


		public ActiveInput ()
		{
			inputName = "";
			gameState = GameState.Normal;
			actionListAsset = null;
		}

	}
	
}