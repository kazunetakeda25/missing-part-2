using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Collections.Generic;

namespace AC
{
	
	[CustomEditor (typeof (Hotspot))]
	public class HotspotEditor : Editor
	{
		
		private Hotspot _target;
		private int sideIndex;
		private Texture2D sideIcon;

		private InventoryManager inventoryManager;
		private SettingsManager settingsManager;
		private CursorManager cursorManager;
		
		private static GUIContent
			deleteContent = new GUIContent("-", "Delete this interaction"),
			addContent = new GUIContent("+", "Create this interaction");
		
		private static GUILayoutOption
			autoWidth = GUILayout.MaxWidth (90f),
			buttonWidth = GUILayout.Width (20f);
		
		
		private void OnEnable ()
		{
			_target = (Hotspot) target;
		}
		
		
		public override void OnInspectorGUI()
		{
			if (!sideIcon)
			{
				sideIcon = (Texture2D) AssetDatabase.LoadAssetAtPath ("Assets/AdventureCreator/Graphics/Textures/inspector-use.png", typeof (Texture2D));
			}

			if (AdvGame.GetReferences () == null)
			{
				Debug.LogError ("A References file is required - please use the Adventure Creator window to create one.");
				EditorGUILayout.LabelField ("No References file found!");
			}
			else
			{
				if (AdvGame.GetReferences ().inventoryManager)
				{
					inventoryManager = AdvGame.GetReferences ().inventoryManager;
				}
				if (AdvGame.GetReferences ().cursorManager)
				{
					cursorManager = AdvGame.GetReferences ().cursorManager;
				}
				if (AdvGame.GetReferences ().settingsManager)
				{
					settingsManager = AdvGame.GetReferences ().settingsManager;
				}
				
				if (_target.lineID > -1)
				{
					EditorGUILayout.LabelField ("Speech Manager ID:", _target.lineID.ToString ());
				}
				
				_target.interactionSource = (InteractionSource) EditorGUILayout.EnumPopup ("Interaction source:", _target.interactionSource);
				_target.hotspotName = EditorGUILayout.TextField ("Label (if not name):", _target.hotspotName);
				_target.highlight = (Highlight) EditorGUILayout.ObjectField ("Object to highlight:", _target.highlight, typeof (Highlight), true);

				EditorGUILayout.BeginHorizontal ();
				_target.centrePoint = (Transform) EditorGUILayout.ObjectField ("Centre point (override):", _target.centrePoint, typeof (Transform), true);

				if (_target.centrePoint == null)
				{
					if (GUILayout.Button ("Create", autoWidth))
					{
						string prefabName = "Hotspot centre: " + _target.gameObject.name;
						GameObject go = SceneManager.AddPrefab ("Navigation", "HotspotCentre", true, false, false);
						go.name = prefabName;
						go.transform.position = _target.transform.position;
						_target.centrePoint = go.transform;
						if (GameObject.Find ("_Markers"))
						{
							go.transform.parent = GameObject.Find ("_Markers").transform;
						}
					}
				}
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.BeginHorizontal ();
				_target.walkToMarker = (Marker) EditorGUILayout.ObjectField ("Walk-to marker:", _target.walkToMarker, typeof (Marker), true);

				if (_target.walkToMarker == null)
				{
					if (GUILayout.Button ("Create", autoWidth))
					{
						string prefabName = "Marker";
						if (settingsManager && settingsManager.IsUnity2D ())
						{
							prefabName += "2D";
						}
						Marker newMarker = SceneManager.AddPrefab ("Navigation", prefabName, true, false, true).GetComponent <Marker>();
						newMarker.gameObject.name += (": " + _target.gameObject.name);
						newMarker.transform.position = _target.transform.position;
						_target.walkToMarker = newMarker;
					}
				}
				EditorGUILayout.EndHorizontal ();

				_target.drawGizmos = EditorGUILayout.Toggle ("Draw yellow cube?", _target.drawGizmos);
				
				if (settingsManager != null && settingsManager.interactionMethod != AC_InteractionMethod.ContextSensitive)
				{
					_target.oneClick = EditorGUILayout.Toggle ("Single 'Use' Interaction?", _target.oneClick);
				}
				EditorGUILayout.Space ();
				
				UseInteractionGUI ();
				
				if (settingsManager == null || settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
				{
					EditorGUILayout.Space ();
					LookInteractionGUI ();
				}
				
				EditorGUILayout.Space ();
				InvInteractionGUI ();
			}
			
			if (GUI.changed)
			{
				EditorUtility.SetDirty (_target);
			}
		}
		
		
		private void LookInteractionGUI ()
		{
			EditorGUILayout.BeginVertical("Button");
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Examine interaction", EditorStyles.boldLabel);
			
			if (!_target.provideLookInteraction)
			{
				if (GUILayout.Button (addContent, EditorStyles.miniButtonRight, buttonWidth))
				{
					Undo.RecordObject (_target, "Create examine interaction");
					_target.provideLookInteraction = true;
				}
			}
			else
			{
				if (GUILayout.Button (deleteContent, EditorStyles.miniButtonRight, buttonWidth))
				{
					Undo.RecordObject (_target, "Delete examine interaction");
					_target.provideLookInteraction = false;
				}
			}
			
			EditorGUILayout.EndHorizontal ();
			if (_target.provideLookInteraction)
			{
				ButtonGUI (_target.lookButton, "Look", _target.interactionSource);
			}
			EditorGUILayout.EndVertical ();
		}
		
		
		private void UseInteractionGUI ()
		{
			if (settingsManager && settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
			{
				if (_target.UpgradeSelf ())
				{
					EditorUtility.SetDirty (_target);
				}
			}
			
			EditorGUILayout.BeginVertical("Button");
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Use interactions", EditorStyles.boldLabel);
			
			if (GUILayout.Button (addContent, EditorStyles.miniButtonRight, buttonWidth))
			{
				Undo.RecordObject (_target, "Create use interaction");
				_target.useButtons.Add (new Button ());
				_target.provideUseInteraction = true;
			}
			EditorGUILayout.EndHorizontal();
			
			if (_target.provideUseInteraction)
			{
				if (cursorManager)
				{
					// Create a string List of the field's names (for the PopUp box)
					List<string> labelList = new List<string>();
					int iconNumber;
					
					if (cursorManager.cursorIcons.Count > 0)
					{
						foreach (CursorIcon _icon in cursorManager.cursorIcons)
						{
							labelList.Add (_icon.label);
						}
						
						foreach (Button useButton in _target.useButtons)
						{
							iconNumber = -1;
							
							int j = 0;
							foreach (CursorIcon _icon in cursorManager.cursorIcons)
							{
								// If an item has been removed, make sure selected variable is still valid
								if (_icon.id == useButton.iconID)
								{
									iconNumber = j;
									break;
								}
								j++;
							}
							
							if (iconNumber == -1)
							{
								// Wasn't found (item was deleted?), so revert to zero
								iconNumber = 0;
								useButton.iconID = 0;
							}
							
							EditorGUILayout.Space ();
							EditorGUILayout.BeginHorizontal ();
							
							iconNumber = EditorGUILayout.Popup ("Cursor:", iconNumber, labelList.ToArray());
							
							// Re-assign variableID based on PopUp selection
							useButton.iconID = cursorManager.cursorIcons[iconNumber].id;
							string iconLabel = cursorManager.cursorIcons[iconNumber].label;
							
							if (GUILayout.Button (sideIcon, EditorStyles.miniButtonRight, buttonWidth))
							{
								SideMenu ("Use", _target.useButtons.Count, _target.useButtons.IndexOf (useButton));
							}
							
							EditorGUILayout.EndHorizontal ();
							ButtonGUI (useButton, iconLabel, _target.interactionSource);
							GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
						}
					}					
					else
					{
						EditorGUILayout.LabelField ("No cursor icons exist!");
						iconNumber = -1;
						
						for (int i=0; i<_target.useButtons.Count; i++)
						{
							_target.useButtons[i].iconID = -1;
						}
					}
				}
				else
				{
					Debug.LogWarning ("A CursorManager is required to run the game properly - please open the Adventure Creator wizard and set one.");
				}
			}
			
			EditorGUILayout.EndVertical ();
		}
		
		
		private void InvInteractionGUI ()
		{
			EditorGUILayout.BeginVertical("Button");
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Inventory interactions", EditorStyles.boldLabel);
			
			if (GUILayout.Button (addContent, EditorStyles.miniButtonRight, buttonWidth))
			{
				Undo.RecordObject (_target, "Create inventory interaction");
				_target.invButtons.Add (new Button ());
				_target.provideInvInteraction = true;
			}
			EditorGUILayout.EndHorizontal();
			
			if (_target.provideInvInteraction)
			{
				if (inventoryManager)
				{
					// Create a string List of the field's names (for the PopUp box)
					List<string> labelList = new List<string>();
					int invNumber;
					
					if (inventoryManager.items.Count > 0)
					{
						
						foreach (InvItem _item in inventoryManager.items)
						{
							labelList.Add (_item.label);
						}
						
						foreach (Button invButton in _target.invButtons)
						{
							invNumber = -1;
							
							int j = 0;
							string invName = "";
							foreach (InvItem _item in inventoryManager.items)
							{
								// If an item has been removed, make sure selected variable is still valid
								if (_item.id == invButton.invID)
								{
									invNumber = j;
									invName = _item.label;
									break;
								}
								
								j++;
							}
							
							if (invNumber == -1)
							{
								// Wasn't found (item was deleted?), so revert to zero
								Debug.Log ("Previously chosen item no longer exists!");
								invNumber = 0;
								invButton.invID = 0;
							}
							
							EditorGUILayout.Space ();
							EditorGUILayout.BeginHorizontal ();
							
							invNumber = EditorGUILayout.Popup ("Inventory item:", invNumber, labelList.ToArray());
							
							// Re-assign variableID based on PopUp selection
							invButton.invID = inventoryManager.items[invNumber].id;

							if (_target.GetComponent <Char>() && settingsManager != null && settingsManager.CanGiveItems ())
							{
								invButton.selectItemMode = (SelectItemMode) EditorGUILayout.EnumPopup (invButton.selectItemMode, GUILayout.Width (70f));
							}

							if (GUILayout.Button (sideIcon, EditorStyles.miniButtonRight, buttonWidth))
							{
								SideMenu ("Inv", _target.invButtons.Count, _target.invButtons.IndexOf (invButton));
							}

							
							EditorGUILayout.EndHorizontal ();
							if (invName != "")
							{
								string label = invName;
								if (_target.GetComponent <Char>() && settingsManager != null && settingsManager.CanGiveItems ())
								{
									label = invButton.selectItemMode.ToString () + " " + label;
								}
								ButtonGUI (invButton, label, _target.interactionSource);
							}
							else
							{
								ButtonGUI (invButton, "Inventory", _target.interactionSource);
							}
							GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
						}
						
					}					
					else
					{
						EditorGUILayout.LabelField ("No inventory items exist!");
						invNumber = -1;
						
						for (int i=0; i<_target.invButtons.Count; i++)
						{
							_target.invButtons[i].invID = -1;
						}
					}
				}
				else
				{
					Debug.LogWarning ("An InventoryManager is required to run the game properly - please open the Adventure Creator wizard and set one.");
				}
			}
			
			EditorGUILayout.EndVertical ();
		}
		
		
		private void ButtonGUI (Button button, string suffix, InteractionSource source)
		{
			bool isEnabled = !button.isDisabled;
			isEnabled = EditorGUILayout.Toggle ("Enabled:", isEnabled);
			button.isDisabled = !isEnabled;
			
			if (source == InteractionSource.AssetFile)
			{
				button.assetFile = (ActionListAsset) EditorGUILayout.ObjectField ("Interaction:", button.assetFile, typeof (ActionListAsset), false);
			}
			else if (source == InteractionSource.CustomScript)
			{
				button.customScriptObject = (GameObject) EditorGUILayout.ObjectField ("Object with script:", button.customScriptObject, typeof (GameObject), true);
				button.customScriptFunction = EditorGUILayout.TextField ("Message to send:", button.customScriptFunction);
			}
			else if (source == InteractionSource.InScene)
			{
				EditorGUILayout.BeginHorizontal ();
				button.interaction = (Interaction) EditorGUILayout.ObjectField ("Interaction:", button.interaction, typeof (Interaction), true);
				
				if (button.interaction == null)
				{
					if (GUILayout.Button ("Create", autoWidth))
					{
						Undo.RecordObject (_target, "Create Interaction");
						Interaction newInteraction = SceneManager.AddPrefab ("Logic", "Interaction", true, false, true).GetComponent <Interaction>();
						
						string hotspotName = _target.gameObject.name;
						if (_target.hotspotName != "")
						{
							hotspotName = _target.hotspotName;
						}
						
						newInteraction.gameObject.name = AdvGame.UniqueName (hotspotName + ": " + suffix);
						button.interaction = newInteraction;
					}
				}
				EditorGUILayout.EndHorizontal ();
			}
			
			button.playerAction = (PlayerAction) EditorGUILayout.EnumPopup ("Player action:", button.playerAction);
			
			if (button.playerAction == PlayerAction.WalkTo || button.playerAction == PlayerAction.WalkToMarker)
			{
				if (KickStarter.settingsManager == null || KickStarter.settingsManager.movementMethod != MovementMethod.UltimateFPS)
				{
					button.isBlocking = EditorGUILayout.Toggle ("Cutscene while moving?", button.isBlocking);
				}
				button.faceAfter = EditorGUILayout.Toggle ("Face after moving?", button.faceAfter);
				
				if (button.playerAction == PlayerAction.WalkTo)
				{
					button.setProximity = EditorGUILayout.Toggle ("Set minimum distance?", button.setProximity);
					if (button.setProximity)
					{
						button.proximity = EditorGUILayout.FloatField ("Proximity:", button.proximity);
					}
				}
			}
		}


		private void SideMenu (string suffix, int listSize, int index)
		{
			GenericMenu menu = new GenericMenu ();
			sideIndex = index;

			menu.AddItem (new GUIContent ("Insert after"), false, Callback, "Insert " + suffix);
			if (listSize > 0)
			{
				menu.AddItem (new GUIContent ("Delete"), false, Callback, "Delete " + suffix);
			}
			if (index > 0 || index < listSize-1)
			{
				menu.AddSeparator ("");
			}
			if (index > 0)
			{
				menu.AddItem (new GUIContent ("Move up"), false, Callback, "Move up " + suffix);
			}
			if (index < listSize-1)
			{
				menu.AddItem (new GUIContent ("Move down"), false, Callback, "Move down " + suffix);
			}
			
			menu.ShowAsContext ();
		}
		
		
		private void Callback (object obj)
		{
			if (sideIndex >= 0)
			{
				switch (obj.ToString ())
				{
				case "Insert after Use":
					Undo.RecordObject (_target, "Insert Interaction");
					_target.useButtons.Insert (sideIndex+1, new Button ());
					break;
					
				case "Delete Use":
					Undo.RecordObject (_target, "Delete Interaction");
					_target.useButtons.RemoveAt (sideIndex);
					if (_target.useButtons.Count == 0)
					{
						_target.provideUseInteraction = false;
					}
					break;

				case "Move up Use":
					Undo.RecordObject (_target, "Move Interaction up");
					Button tempButton = _target.useButtons [sideIndex];
					_target.useButtons.RemoveAt (sideIndex);
					_target.useButtons.Insert (sideIndex-1, tempButton);
					break;
					
				case "Move down Use":
					Undo.RecordObject (_target, "Move Interaction down");
					Button tempButton2 = _target.useButtons [sideIndex];
					_target.useButtons.RemoveAt (sideIndex);
					_target.useButtons.Insert (sideIndex+1, tempButton2);
					break;
				
				case "Insert after Inv":
					Undo.RecordObject (_target, "Insert Interaction");
					_target.invButtons.Insert (sideIndex+1, new Button ());
					break;
				
				case "Delete Inv":
					Undo.RecordObject (_target, "Delete Interaction");
					_target.invButtons.RemoveAt (sideIndex);
					if (_target.invButtons.Count == 0)
					{
						_target.provideInvInteraction = false;
					}
					break;
				
				case "Move up Inv":
					Undo.RecordObject (_target, "Move Interaction up");
					Button tempButton3 = _target.invButtons [sideIndex];
					_target.invButtons.RemoveAt (sideIndex);
					_target.invButtons.Insert (sideIndex-1, tempButton3);
					break;
				
				case "Move down Inv":
					Undo.RecordObject (_target, "Move Interaction down");
					Button tempButton4 = _target.invButtons [sideIndex];
					_target.invButtons.RemoveAt (sideIndex);
					_target.invButtons.Insert (sideIndex+1, tempButton4);
					break;
					
				}
				
			}
		}

	}
	
}
