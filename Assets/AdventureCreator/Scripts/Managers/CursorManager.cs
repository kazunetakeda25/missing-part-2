/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"CursorManager.cs"
 * 
 *	This script handles the "Cursor" tab of the main wizard.
 *	It is used to define cursor icons and the method in which
 *	interactions are triggered by the player.
 * 
 */

using UnityEngine;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class CursorManager : ScriptableObject
	{

		public CursorRendering cursorRendering = CursorRendering.Software;
		public CursorDisplay cursorDisplay = CursorDisplay.Always;
		public bool allowMainCursor = false;

		public bool allowWalkCursor = false;
		public bool addWalkPrefix = false;
		public HotspotPrefix walkPrefix = new HotspotPrefix ("Walk to");

		public bool addHotspotPrefix = false;
		public bool allowInteractionCursor = false;
		public bool allowInteractionCursorForInventory = false;
		public bool cycleCursors = false;
		public bool leftClickExamine = false;
		public bool onlyWalkWhenOverNavMesh = false;
		public bool onlyShowInventoryLabelOverHotspots = false;
		
		public float inventoryCursorSize = 0.06f;

		public CursorIconBase waitIcon = new CursorIcon ();
		public CursorIconBase pointerIcon = new CursorIcon ();
		public CursorIconBase walkIcon = new CursorIcon ();
		public CursorIconBase mouseOverIcon = new CursorIcon ();

		public InventoryHandling inventoryHandling = InventoryHandling.ChangeCursor;
		public HotspotPrefix hotspotPrefix1 = new HotspotPrefix ("Use");
		public HotspotPrefix hotspotPrefix2 = new HotspotPrefix ("on");
		public HotspotPrefix hotspotPrefix3 = new HotspotPrefix ("Give");
		public HotspotPrefix hotspotPrefix4 = new HotspotPrefix ("to");

		public List<CursorIcon> cursorIcons = new List<CursorIcon>();
		public List<ActionListAsset> unhandledCursorInteractions = new List<ActionListAsset>();

		public LookUseCursorAction lookUseCursorAction = LookUseCursorAction.DisplayBothSideBySide;
		public int lookCursor_ID = 0;
		public int lookCursor_int = 0;

		private SettingsManager settingsManager;
		
		
		#if UNITY_EDITOR
		
		private static GUIContent
			insertContent = new GUIContent("+", "Insert variable"),
			deleteContent = new GUIContent("-", "Delete variable");

		private static GUILayoutOption
			buttonWidth = GUILayout.MaxWidth (20f);

		
		public void ShowGUI ()
		{
			settingsManager = AdvGame.GetReferences().settingsManager;

			cursorRendering = (CursorRendering) EditorGUILayout.EnumPopup ("Cursor rendering:", cursorRendering);
			EditorGUILayout.Space ();
			
			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Main cursor", EditorStyles.boldLabel);
			cursorDisplay = (CursorDisplay) EditorGUILayout.EnumPopup ("Display cursor:", cursorDisplay);
			allowMainCursor = EditorGUILayout.Toggle ("Replace mouse cursor?", allowMainCursor);
			if (allowMainCursor || (settingsManager && settingsManager.inputMethod == InputMethod.KeyboardOrController))
			{
				IconBaseGUI ("Main cursor:", pointerIcon);
			}
			EditorGUILayout.EndVertical ();
			
			EditorGUILayout.Space ();
			
			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Walk settings", EditorStyles.boldLabel);
			if (allowMainCursor)
			{
				allowWalkCursor = EditorGUILayout.Toggle ("Provide walk cursor?", allowWalkCursor);
				if (allowWalkCursor)
				{
					if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
					{
						EditorGUILayout.LabelField ("Input button:", "Icon_Walk");
					}
					IconBaseGUI ("Walk cursor:", walkIcon);
					onlyWalkWhenOverNavMesh = EditorGUILayout.ToggleLeft ("Only show 'Walk' Cursor when over NavMesh?", onlyWalkWhenOverNavMesh);
				}
			}
			addWalkPrefix = EditorGUILayout.Toggle ("Prefix cursor labels?", addWalkPrefix);
			if (addWalkPrefix)
			{
				walkPrefix.label = EditorGUILayout.TextField ("Walk prefix:", walkPrefix.label);
			}
			EditorGUILayout.EndVertical ();
			
			EditorGUILayout.Space ();

			EditorGUILayout.BeginVertical ("Button");
				EditorGUILayout.LabelField ("Hotspot settings", EditorStyles.boldLabel);
				addHotspotPrefix = EditorGUILayout.Toggle ("Prefix cursor labels?", addHotspotPrefix);
				IconBaseGUI ("Mouse-over cursor:", mouseOverIcon);
			EditorGUILayout.EndVertical ();

			EditorGUILayout.Space ();
			
			EditorGUILayout.BeginVertical ("Button");
				EditorGUILayout.LabelField ("Inventory cursor", EditorStyles.boldLabel);
				inventoryHandling = (InventoryHandling) EditorGUILayout.EnumPopup ("When inventory selected:", inventoryHandling);
				if (inventoryHandling != InventoryHandling.ChangeCursor)
				{
					onlyShowInventoryLabelOverHotspots = EditorGUILayout.ToggleLeft ("Only show label when over Hotspots and Inventory?", onlyShowInventoryLabelOverHotspots);
				}
				if (inventoryHandling != InventoryHandling.ChangeHotspotLabel)
				{
					inventoryCursorSize = EditorGUILayout.FloatField ("Inventory cursor size:", inventoryCursorSize);
				}
				EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Use syntax:", GUILayout.Width (100f));
					hotspotPrefix1.label = EditorGUILayout.TextField (hotspotPrefix1.label, GUILayout.MaxWidth (80f));
					EditorGUILayout.LabelField ("(item)", GUILayout.MaxWidth (40f));
					hotspotPrefix2.label = EditorGUILayout.TextField (hotspotPrefix2.label, GUILayout.MaxWidth (80f));
					EditorGUILayout.LabelField ("(hotspot)", GUILayout.MaxWidth (55f));
				EditorGUILayout.EndHorizontal ();
				if (AdvGame.GetReferences ().settingsManager && AdvGame.GetReferences ().settingsManager.CanGiveItems ())
				{
					EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField ("Give syntax:", GUILayout.Width (100f));
						hotspotPrefix3.label = EditorGUILayout.TextField (hotspotPrefix3.label, GUILayout.MaxWidth (80f));
						EditorGUILayout.LabelField ("(item)", GUILayout.MaxWidth (40f));
						hotspotPrefix4.label = EditorGUILayout.TextField (hotspotPrefix4.label, GUILayout.MaxWidth (80f));
						EditorGUILayout.LabelField ("(hotspot)", GUILayout.MaxWidth (55f));
					EditorGUILayout.EndHorizontal ();
				}
			EditorGUILayout.EndVertical ();
			
			EditorGUILayout.Space ();
			
			EditorGUILayout.BeginVertical ("Button");
				EditorGUILayout.LabelField ("Interaction icons", EditorStyles.boldLabel);
				
				if (settingsManager == null || settingsManager.interactionMethod != AC_InteractionMethod.ChooseHotspotThenInteraction)
				{
					allowInteractionCursor = EditorGUILayout.ToggleLeft ("Change cursor based on Interaction?", allowInteractionCursor);
					if (allowInteractionCursor && (settingsManager == null || settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive))
					{
						allowInteractionCursorForInventory = EditorGUILayout.ToggleLeft ("Change when over Inventory items too?", allowInteractionCursorForInventory);
					}
					if (settingsManager && settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
					{
						cycleCursors = EditorGUILayout.ToggleLeft ("Cycle Interactions with right-click?", cycleCursors);
					}
				}
				
				IconsGUI ();
			
				EditorGUILayout.Space ();
			
				if (settingsManager == null || settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
				{
					LookIconGUI ();
				}
			EditorGUILayout.EndVertical ();

			EditorGUILayout.Space ();

			EditorGUILayout.BeginVertical ("Button");
				IconBaseGUI ("Wait cursor", waitIcon);
			EditorGUILayout.EndVertical ();

			if (GUI.changed)
			{
				EditorUtility.SetDirty (this);
			}
		}
		
		
		private void IconsGUI ()
		{
			// Make sure unhandledCursorInteractions is the same length as cursorIcons
			while (unhandledCursorInteractions.Count < cursorIcons.Count)
			{
				unhandledCursorInteractions.Add (null);
			}
			while (unhandledCursorInteractions.Count > cursorIcons.Count)
			{
				unhandledCursorInteractions.RemoveAt (unhandledCursorInteractions.Count + 1);
			}

			// List icons
			foreach (CursorIcon _cursorIcon in cursorIcons)
			{
				int i = cursorIcons.IndexOf (_cursorIcon);
				GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Icon ID:", GUILayout.MaxWidth (145));
				EditorGUILayout.LabelField (_cursorIcon.id.ToString (), GUILayout.MaxWidth (120));

				if (GUILayout.Button (insertContent, EditorStyles.miniButtonLeft, buttonWidth))
				{
					Undo.RecordObject (this, "Add icon");
					cursorIcons.Insert (i+1, new CursorIcon (GetIDArray ()));
					unhandledCursorInteractions.Insert (i+1, null);
					break;
				}
				if (GUILayout.Button (deleteContent, EditorStyles.miniButtonRight, buttonWidth))
				{
					Undo.RecordObject (this, "Delete icon: " + _cursorIcon.label);
					cursorIcons.Remove (_cursorIcon);
					unhandledCursorInteractions.RemoveAt (i);
					break;
				}
				EditorGUILayout.EndHorizontal ();

				_cursorIcon.label = EditorGUILayout.TextField ("Label:", _cursorIcon.label);
				if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
				{
					EditorGUILayout.LabelField ("Input button:", _cursorIcon.GetButtonName ());
				}
				_cursorIcon.ShowGUI (true, cursorRendering);

				if (settingsManager && settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
				{
					unhandledCursorInteractions[i] = ActionListAssetMenu.AssetGUI ("Unhandled interaction", unhandledCursorInteractions[i]);
					_cursorIcon.dontCycle = EditorGUILayout.Toggle ("Leave out of Cursor cycle?", _cursorIcon.dontCycle);
				}
			}

			if (GUILayout.Button("Create new icon"))
			{
				Undo.RecordObject (this, "Add icon");
				cursorIcons.Add (new CursorIcon (GetIDArray ()));
			}
		}
		
		
		private void LookIconGUI ()
		{
			if (cursorIcons.Count > 0)
			{
				lookCursor_int = GetIntFromID (lookCursor_ID);
				lookCursor_int = EditorGUILayout.Popup ("Examine icon:", lookCursor_int, GetLabelsArray (lookCursor_int));
				lookCursor_ID = cursorIcons[lookCursor_int].id;

				EditorGUILayout.LabelField ("When Use and Examine interactions are both available:");
				lookUseCursorAction = (LookUseCursorAction) EditorGUILayout.EnumPopup (" ", lookUseCursorAction);

				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Left-click to examine when no use interaction exists?", GUILayout.Width (300f));
				leftClickExamine = EditorGUILayout.Toggle (leftClickExamine);
				EditorGUILayout.EndHorizontal ();
			}
		}


		private void IconBaseGUI (string fieldLabel, CursorIconBase icon)
		{
			EditorGUILayout.LabelField (fieldLabel, EditorStyles.boldLabel);
			icon.ShowGUI (true, cursorRendering);
			GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
		}

		#endif
		
		
		public string[] GetLabelsArray (int requestedInt)
		{
			// Create a string List of the field's names (for the PopUp box)
			List<string> iconLabels = new List<string>();
			
			foreach (CursorIcon cursorIcon in cursorIcons)
			{
				iconLabels.Add (cursorIcon.label);
			}
		
			return (iconLabels.ToArray());
		}
		
		
		public string GetLabelFromID (int _ID, int languageNumber)
		{
			foreach (CursorIcon cursorIcon in cursorIcons)
			{
				if (cursorIcon.id == _ID)
				{
					return (SpeechManager.GetTranslation (cursorIcon.label, cursorIcon.lineID, languageNumber) + " ");
				}
			}
			
			return ("");
		}
		
		
		public CursorIcon GetCursorIconFromID (int _ID)
		{
			foreach (CursorIcon cursorIcon in cursorIcons)
			{
				if (cursorIcon.id == _ID)
				{
					return (cursorIcon);
				}
			}
			
			return (null);
		}
		
		
		public int GetIntFromID (int _ID)
		{
			int i = 0;
			int requestedInt = -1;
			
			foreach (CursorIcon cursorIcon in cursorIcons)
			{
				if (cursorIcon.id == _ID)
				{
					requestedInt = i;
				}
				
				i++;
			}
			
			if (requestedInt == -1)
			{
				// Wasn't found (icon was deleted?), so revert to zero
				requestedInt = 0;
			}
		
			return (requestedInt);
		}


		public ActionListAsset GetUnhandledInteraction (int _ID)
		{
			foreach (CursorIcon cursorIcon in cursorIcons)
			{
				if (cursorIcon.id == _ID)
				{
					int i = cursorIcons.IndexOf (cursorIcon);
					if (unhandledCursorInteractions.Count > i)
					{
						return unhandledCursorInteractions [i];
					}
					return null;
				}
			}
			return null;
		}
		
		
		private int[] GetIDArray ()
		{
			// Returns a list of id's in the list
			
			List<int> idArray = new List<int>();
			
			foreach (CursorIcon cursorIcon in cursorIcons)
			{
				idArray.Add (cursorIcon.id);
			}
			
			idArray.Sort ();
			return idArray.ToArray ();
		}

	}

}