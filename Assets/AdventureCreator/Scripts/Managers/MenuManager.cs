/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuManager.cs"
 * 
 *	This script handles the "Menu" tab of the main wizard.
 *	It is used to define the menus that make up the game's GUI.
 * 
 */

using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class MenuManager : ScriptableObject
	{

		public EventSystem eventSystem;
		public List<Menu> menus = new List<Menu>();
		public bool drawOutlines = true;
		public bool drawInEditor = false;
		public int globalDepth;
		public Texture2D pauseTexture = null;

		#if UNITY_EDITOR

		public static Menu copiedMenu = null;
		public static MenuElement copiedElement = null;

		private Texture2D icon;
		private Menu selectedMenu = null;
		private MenuElement selectedMenuElement = null;
		private int sideMenu = -1;
		private int sideElement = -1;

		private bool oldVisibility;
		private int typeNumber = 0;
		private string[] elementTypes = { "Button", "Crafting", "Cycle", "DialogList", "Drag", "Graphic", "Input", "Interaction", "InventoryBox", "Journal", "Label", "SavesList", "Slider", "Timer", "Toggle" };


		public void OnEnable ()
		{
			if (menus == null)
			{
				menus = new List<Menu>();
			}
		}
		
		
		public void ShowGUI ()
		{
			if (icon == null)
			{
				icon = (Texture2D) AssetDatabase.LoadAssetAtPath ("Assets/AdventureCreator/Graphics/Textures/inspector-use.png", typeof (Texture2D));
			}

			EditorGUILayout.BeginVertical ("Button");
				drawInEditor = EditorGUILayout.Toggle ("Test in Game Window?", drawInEditor);
				drawOutlines = EditorGUILayout.Toggle ("Draw outlines?", drawOutlines);
				EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Pause background texture:", GUILayout.Width (255f));
					pauseTexture = (Texture2D) EditorGUILayout.ObjectField (pauseTexture, typeof (Texture2D), false, GUILayout.Width (70f), GUILayout.Height (30f));
				EditorGUILayout.EndHorizontal ();
				globalDepth = EditorGUILayout.IntField ("GUI depth:", globalDepth);
				eventSystem = (EventSystem) EditorGUILayout.ObjectField ("Event system prefab:", eventSystem, typeof (EventSystem), false);
			
				if (drawInEditor && KickStarter.menuPreview == null)
				{	
					EditorGUILayout.HelpBox ("A GameEngine prefab is required to display menus while editing - please click Organise Room Objects within the Scene Manager.", MessageType.Warning);
				}
				else if (Application.isPlaying)
				{
					EditorGUILayout.HelpBox ("Changes made to the menus will not be registed by the game until the game is restarted.", MessageType.Info);
				}
			EditorGUILayout.EndVertical ();
			
			EditorGUILayout.Space ();
				
			EditorGUILayout.LabelField ("Menus", EditorStyles.boldLabel);
			CreateMenusGUI ();
			
			if (selectedMenu != null)
			{
				EditorGUILayout.Space ();
				
				string menuTitle = selectedMenu.title;
				if (menuTitle == "")
				{
					menuTitle = "(Untitled)";
				}
				
				EditorGUILayout.LabelField ("Menu " + selectedMenu.id + ": '" + menuTitle + "' properties", EditorStyles.boldLabel);
				EditorGUILayout.BeginVertical ("Button");
					selectedMenu.ShowGUI ();
				EditorGUILayout.EndVertical ();
				
				EditorGUILayout.Space ();
				
				EditorGUILayout.LabelField ("Menu " + selectedMenu.id + ": '" + menuTitle + "' elements", EditorStyles.boldLabel);
				EditorGUILayout.BeginVertical ("Button");
					CreateElementsGUI (selectedMenu);
				EditorGUILayout.EndVertical ();
				
				if (selectedMenuElement != null)
				{
					EditorGUILayout.Space ();
					
					string elementName = selectedMenuElement.title;
					if (elementName == "")
					{
						elementName = "(Untitled)";
					}
					
					string elementType = "";
					foreach (string _elementType in elementTypes)
					{
						if (selectedMenuElement.GetType ().ToString ().Contains (_elementType))
						{
							elementType = _elementType;
							break;
						}
					}

					EditorGUILayout.LabelField (elementType + " " + selectedMenuElement.ID + ": '" + elementName + "' properties", EditorStyles.boldLabel);
					oldVisibility = selectedMenuElement.isVisible;
					selectedMenuElement.ShowGUIStart (selectedMenu.menuSource);
					if (selectedMenuElement.isVisible != oldVisibility)
					{
						if (!Application.isPlaying)
						{
							selectedMenu.Recalculate ();
						}
					}
				}
			}
			
			if (GUI.changed)
			{
				if (!Application.isPlaying)
				{
					SaveAllMenus ();
				}
				EditorUtility.SetDirty (this);
			}
		}
		
		
		private void SaveAllMenus ()
		{
			foreach (AC.Menu menu in menus)
			{
				if (!Application.isPlaying)
				{
					menu.Recalculate ();
				}
			}
		}
		
		
		private void CreateMenusGUI ()
		{
			foreach (AC.Menu _menu in menus)
			{
				if (_menu == null)
				{
					menus.Remove (_menu);
					CleanUpAsset ();
					return;
				}

				EditorGUILayout.BeginHorizontal ();
				
					string buttonLabel = _menu.title;
					if (buttonLabel == "")
					{
						buttonLabel = "(Untitled)";	
					}
					if (GUILayout.Toggle (_menu.isEditing, buttonLabel, "Button"))
					{
						if (selectedMenu != _menu)
						{
							DeactivateAllMenus ();
							ActivateMenu (_menu);
						}
					}

					if (GUILayout.Button (icon, GUILayout.Width (20f), GUILayout.Height (15f)))
					{
						SideMenu (_menu);
					}
			
				EditorGUILayout.EndHorizontal ();
			}

			EditorGUILayout.Space ();

			EditorGUILayout.BeginHorizontal ();
				if (GUILayout.Button("Create new menu"))
				{
					Undo.RecordObject (this, "Add menu");
					
					Menu newMenu = (Menu) CreateInstance <Menu>();
					newMenu.Declare (GetIDArray ());
					menus.Add (newMenu);
					
					DeactivateAllMenus ();
					ActivateMenu (newMenu);
					
					newMenu.hideFlags = HideFlags.HideInHierarchy;
					AssetDatabase.AddObjectToAsset (newMenu, this);
					AssetDatabase.ImportAsset (AssetDatabase.GetAssetPath (newMenu));
					AssetDatabase.SaveAssets ();
					CleanUpAsset ();
				}
				if (MenuManager.copiedMenu == null)
				{
					GUI.enabled = false;
				}
				if (GUILayout.Button ("Paste menu"))
				{
					PasteMenu ();
				}
				GUI.enabled = true;
			EditorGUILayout.EndHorizontal ();
		}


		private void CleanUpAsset ()
		{
			string assetPath = AssetDatabase.GetAssetPath (this);
			Object[] objects = AssetDatabase.LoadAllAssetsAtPath (assetPath);

			foreach (Object _object in objects)
			{
				if (_object is Menu)
				{
					AC.Menu _menu = (Menu) _object;

					bool found = false;
					foreach (AC.Menu menu in menus)
					{
						if (menu == _menu)
						{
							_object.hideFlags = HideFlags.HideInHierarchy;
							found = true;
							break;
						}
					}

					if (!found)
					{
						Debug.Log ("Deleted unset menu: " + _menu.title);
						DestroyImmediate (_object, true);
					}
				}
			}

			foreach (Object _object in objects)
			{
				if (_object is MenuElement)
				{
					MenuElement _element = (MenuElement) _object;
					
					bool found = false;
					foreach (AC.Menu menu in menus)
					{
						foreach (MenuElement element in menu.elements)
						{
							if (element == _element)
							{
								_object.hideFlags = HideFlags.HideInHierarchy;
								found = true;
								break;
							}
						}
					}

					if (!found)
					{
						Debug.Log ("Deleted unset element: " + _element.title);
						DestroyImmediate (_object, true);
					}
				}
			}

			AssetDatabase.SaveAssets ();
		}


		public void SelectElementFromPreview (AC.Menu _menu, MenuElement _element)
		{
			if (_menu.elements.Contains (_element))
			{
				if (selectedMenuElement != _element)
				{
					DeactivateAllElements (_menu);
					ActivateElement (_element);
				}
			}
		}
		
		
		private void CreateElementsGUI (AC.Menu _menu)
		{	
			if (_menu.elements != null && _menu.elements.Count > 0)
			{
				foreach (MenuElement _element in _menu.elements)
				{
					if (_element != null)
					{
						string elementName = _element.title;
						
						if (elementName == "")
						{
							elementName = "(Untitled)";
						}
						
						EditorGUILayout.BeginHorizontal ();
						
							if (GUILayout.Toggle (_element.isEditing, elementName, "Button"))
							{
								if (selectedMenuElement != _element)
								{
									DeactivateAllElements (_menu);
									ActivateElement (_element);
								}
							}

							if (GUILayout.Button (icon, GUILayout.Width (20f), GUILayout.Height (15f)))
							{
								SideMenu (_menu, _element);
							}
					
						EditorGUILayout.EndHorizontal ();
					}
				}
			}

			EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Element type:", GUILayout.Width (80f));
				typeNumber = EditorGUILayout.Popup (typeNumber, elementTypes);
				
				if (GUILayout.Button ("Add new"))
				{
					AddElement (elementTypes[typeNumber], _menu);
				}

				if (copiedElement != null)
				{
					if (GUILayout.Button ("Paste"))
					{
						PasteElement (menus.IndexOf (_menu), _menu.elements.Count -1);
					}
				}
			EditorGUILayout.EndHorizontal ();
		}
		
		
		private void ActivateMenu (AC.Menu menu)
		{
			menu.isEditing = true;
			selectedMenu = menu;
		}
		
		
		private void DeactivateAllMenus ()
		{
			foreach (AC.Menu menu in menus)
			{
				menu.isEditing = false;
			}
			selectedMenu = null;
			selectedMenuElement = null;
		}
		
		
		private void ActivateElement (MenuElement menuElement)
		{
			menuElement.isEditing = true;
			selectedMenuElement = menuElement;
		}
		
		
		private void DeleteAllElements (AC.Menu menu)
		{
			foreach (MenuElement menuElement in menu.elements)
			{
				UnityEngine.Object.DestroyImmediate (menuElement, true);
				AssetDatabase.SaveAssets();
			}
			CleanUpAsset ();
		}
		
		
		private void DeactivateAllElements (AC.Menu menu)
		{
			foreach (MenuElement menuElement in menu.elements)
			{
				menuElement.isEditing = false;
			}
		}


		private int[] GetElementIDArray (int i)
		{
			// Returns a list of id's in the list
			List<int> idArray = new List<int>();
			
			foreach (MenuElement _element in menus[i].elements)
			{
				if (_element != null)
				{
					idArray.Add (_element.ID);
				}
			}
			
			idArray.Sort ();
			return idArray.ToArray ();
		}
					
		
		private int[] GetIDArray ()
		{
			// Returns a list of id's in the list
			List<int> idArray = new List<int>();
			
			foreach (AC.Menu menu in menus)
			{
				if (menu != null)
				{
					idArray.Add (menu.id);
				}
			}
			
			idArray.Sort ();
			return idArray.ToArray ();
		}
		
		
		private void AddElement (string className, AC.Menu _menu)
		{
			Undo.RecordObject (_menu, "Add element");

			List<int> idArray = new List<int>();
			
			foreach (MenuElement _element in _menu.elements)
			{
				if (_element != null)
				{
					idArray.Add (_element.ID);
				}
			}
			idArray.Sort ();
			
			className = "Menu" + className;
			MenuElement newElement = (MenuElement) CreateInstance (className);
			newElement.Declare ();
			newElement.title = className.Substring (4);
			
			// Update id based on array
			foreach (int _id in idArray.ToArray())
			{
				if (newElement.ID == _id)
				{
					newElement.ID ++;
				}
			}
			
			_menu.elements.Add (newElement);
			if (!Application.isPlaying)
			{
				_menu.Recalculate ();
			}
			DeactivateAllElements (_menu);
			newElement.isEditing = true;
			selectedMenuElement = newElement;

			newElement.hideFlags = HideFlags.HideInHierarchy;
			AssetDatabase.AddObjectToAsset (newElement, this);
			AssetDatabase.ImportAsset (AssetDatabase.GetAssetPath (newElement));
			AssetDatabase.SaveAssets ();

			CleanUpAsset ();
		}


		private void PasteMenu ()
		{
			PasteMenu (menus.Count-1);
		}


		private void PasteMenu (int i)
		{
			if (MenuManager.copiedMenu != null)
			{
				Undo.RecordObject (this, "Paste menu");
				
				Menu newMenu = (Menu) CreateInstance <Menu>();
				newMenu.Declare (GetIDArray ());
				int newMenuID = newMenu.id;
				newMenu.Copy (MenuManager.copiedMenu);
				newMenu.id = newMenuID;
				newMenu.title += " (Copy)";

				menus.Insert (i+1, newMenu);
				
				DeactivateAllMenus ();
				ActivateMenu (newMenu);

				newMenu.hideFlags = HideFlags.HideInHierarchy;
				AssetDatabase.AddObjectToAsset (newMenu, this);
				AssetDatabase.ImportAsset (AssetDatabase.GetAssetPath (newMenu));

				foreach (MenuElement newElement in newMenu.elements)
				{
					newElement.hideFlags = HideFlags.HideInHierarchy;
					AssetDatabase.AddObjectToAsset (newElement, this);
					AssetDatabase.ImportAsset (AssetDatabase.GetAssetPath (newElement));
				}

				AssetDatabase.SaveAssets ();
				CleanUpAsset ();
			}
		}


		private void PasteElement (int i, int j)
		{
			if (MenuManager.copiedElement != null)
			{
				Undo.RecordObject (this, "Paste element");

				int[] idArray = GetElementIDArray (i);

				MenuElement newElement = MenuManager.copiedElement.DuplicateSelf ();
				newElement.title += " (Copy)";
				newElement.UpdateID (idArray);
				newElement.lineID = -1;
				newElement.hideFlags = HideFlags.HideInHierarchy;
				menus[i].elements.Insert (j+1, newElement);

				AssetDatabase.AddObjectToAsset (newElement, this);
				AssetDatabase.ImportAsset (AssetDatabase.GetAssetPath (newElement));
				AssetDatabase.SaveAssets ();

				CleanUpAsset ();
			}
		}


		private void SideMenu (AC.Menu _menu)
		{
			GenericMenu menu = new GenericMenu ();
			sideMenu = menus.IndexOf (_menu);

			menu.AddItem (new GUIContent ("Insert after"), false, MenuCallback, "Insert after");
			if (menus.Count > 0)
			{
				menu.AddItem (new GUIContent ("Delete"), false, MenuCallback, "Delete");
			}

			menu.AddSeparator ("");
			menu.AddItem (new GUIContent ("Copy"), false, MenuCallback, "Copy");
			if (MenuManager.copiedMenu != null)
			{
				menu.AddItem (new GUIContent ("Paste after"), false, MenuCallback, "Paste after");
			}

			if (sideMenu > 0 || sideMenu < menus.Count-1)
			{
				menu.AddSeparator ("");
				if (sideMenu > 0)
				{
					menu.AddItem (new GUIContent ("Move up"), false, MenuCallback, "Move up");
				}
				if (sideMenu < menus.Count-1)
				{
					menu.AddItem (new GUIContent ("Move down"), false, MenuCallback, "Move down");
				}
			}
			
			menu.ShowAsContext ();
		}


		private void MenuCallback (object obj)
		{
			if (sideMenu >= 0)
			{
				switch (obj.ToString ())
				{
				case "Copy":
					MenuManager.copiedMenu = menus[sideMenu];
					break;

				case "Paste after":
					PasteMenu (sideMenu);
					break;

				case "Insert after":
					Undo.RecordObject (this, "Insert menu");
					Menu newMenu = (Menu) CreateInstance <Menu>();
					newMenu.Declare (GetIDArray ());
					menus.Insert (sideMenu+1, newMenu);
					
					DeactivateAllMenus ();
					ActivateMenu (newMenu);

					newMenu.hideFlags = HideFlags.HideInHierarchy;
					AssetDatabase.AddObjectToAsset (newMenu, this);
					AssetDatabase.ImportAsset (AssetDatabase.GetAssetPath (newMenu));
					break;
					
				case "Delete":
					Undo.RecordObject (this, "Delete menu");
					if (menus[sideMenu] == selectedMenu)
					{
						DeactivateAllElements (menus[sideMenu]);
						DeleteAllElements (menus[sideMenu]);
						selectedMenuElement = null;
					}
					DeactivateAllMenus ();
					Menu tempMenu = menus[sideMenu];
					foreach (MenuElement element in tempMenu.elements)
					{
						UnityEngine.Object.DestroyImmediate (element, true);
					}
					menus.RemoveAt (sideMenu);
					UnityEngine.Object.DestroyImmediate (tempMenu, true);
					AssetDatabase.SaveAssets ();
					CleanUpAsset ();
					break;
					
				case "Move up":
					Undo.RecordObject (this, "Move menu up");
					menus = SwapMenus (menus, sideMenu, sideMenu-1);
					menus[sideMenu].ResetVisibleElements ();
					AssetDatabase.SaveAssets ();
					break;
					
				case "Move down":
					Undo.RecordObject (this, "Move menu down");
					menus = SwapMenus (menus, sideMenu, sideMenu+1);
					menus[sideMenu].ResetVisibleElements ();
					AssetDatabase.SaveAssets ();
					break;
				}
			}
			
			sideMenu = -1;
			sideElement = -1;
			SaveAllMenus ();
		}


		private void SideMenu (AC.Menu _menu, MenuElement _element)
		{
			GenericMenu menu = new GenericMenu ();
			sideElement = _menu.elements.IndexOf (_element);
			sideMenu = menus.IndexOf (_menu);
			
			if (_menu.elements.Count > 0)
			{
				menu.AddItem (new GUIContent ("Delete"), false, ElementCallback, "Delete");
			}

			menu.AddSeparator ("");
			menu.AddItem (new GUIContent ("Copy"), false, ElementCallback, "Copy");
			if (MenuManager.copiedElement != null)
			{
				menu.AddItem (new GUIContent ("Paste after"), false, ElementCallback, "Paste after");
			}
			if (sideElement > 0 || sideElement < _menu.elements.Count-1)
			{
				menu.AddSeparator ("");
			}

			if (sideElement > 0)
			{
				menu.AddItem (new GUIContent ("Move up"), false, ElementCallback, "Move up");
			}
			if (sideElement < _menu.elements.Count-1)
			{
				menu.AddItem (new GUIContent ("Move down"), false, ElementCallback, "Move down");
			}
			
			menu.ShowAsContext ();
		}
		
		
		private void ElementCallback (object obj)
		{
			if (sideElement >= 0 && sideMenu >= 0)
			{
				switch (obj.ToString ())
				{
				case "Copy":
					MenuManager.copiedElement = menus[sideMenu].elements[sideElement];
					break;
					
				case "Paste after":
					PasteElement (sideMenu, sideElement);
					break;

				case "Delete":
					Undo.RecordObject (this, "Delete menu element");
					DeactivateAllElements (menus[sideMenu]);
					selectedMenuElement = null;
					MenuElement tempElement = menus[sideMenu].elements[sideElement];
					menus[sideMenu].elements.RemoveAt (sideElement);
					UnityEngine.Object.DestroyImmediate (tempElement, true);
					AssetDatabase.SaveAssets();
					CleanUpAsset ();
					break;
					
				case "Move up":
					Undo.RecordObject (this, "Move menu element up");
					menus[sideMenu].elements = SwapElements (menus[sideMenu].elements, sideElement, sideElement-1);
					menus[sideMenu].ResetVisibleElements ();
					AssetDatabase.SaveAssets();
					break;
					
				case "Move down":
					Undo.RecordObject (this, "Move menu element down");
					menus[sideMenu].elements = SwapElements (menus[sideMenu].elements, sideElement, sideElement+1);
					menus[sideMenu].ResetVisibleElements ();
					AssetDatabase.SaveAssets();
					break;
				}
			}
			
			sideMenu = -1;
			sideElement = -1;
			SaveAllMenus ();
		}
		

		private List<Menu> SwapMenus (List<Menu> list, int a1, int a2)
		{
			Menu tempMenu = list[a1];
			list[a1] = list[a2];
			list[a2] = tempMenu;
			return (list);
		}

		
		private List<MenuElement> SwapElements (List<MenuElement> list, int a1, int a2)
		{
			MenuElement tempElement = list[a1];
			list[a1] = list[a2];
			list[a2] = tempElement;
			return (list);
		}
		
		
		public Menu GetSelectedMenu ()
		{
			return selectedMenu;
		}
		
		
		public MenuElement GetSelectedElement ()
		{
			return selectedMenuElement;
		}


		public static MenuElement GetElementWithName (string menuName, string menuElementName)
		{
			if (AdvGame.GetReferences () && AdvGame.GetReferences ().menuManager)
			{
				foreach (AC.Menu menu in AdvGame.GetReferences ().menuManager.menus)
				{
					if (menu.title == menuName)
					{
						foreach (MenuElement menuElement in menu.elements)
						{
							if (menuElement.title == menuElementName)
							{
								return menuElement;
							}
						}
					}
				}
			}
			
			return null;
		}

		#endif
		
	}

}