using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using AC;

[CustomEditor(typeof(Container))]
public class ContainerEditor : Editor
{

	private Container _target;
	private int itemNumber;
	private int sideItem;
	private InventoryManager inventoryManager;
	private Texture2D sideIcon;
	

	public void OnEnable()
	{
		_target = (Container) target;

		if (AdvGame.GetReferences () && AdvGame.GetReferences ().inventoryManager)
		{
			inventoryManager = AdvGame.GetReferences ().inventoryManager;
		}
	}
	
	
	public override void OnInspectorGUI()
	{
		if (_target == null || inventoryManager == null)
		{
			OnEnable ();
			return;
		}

		if (!sideIcon)
		{
			sideIcon = (Texture2D) AssetDatabase.LoadAssetAtPath ("Assets/AdventureCreator/Graphics/Textures/inspector-use.png", typeof (Texture2D));
		}

		EditorGUILayout.LabelField ("Stored Inventory items", EditorStyles.boldLabel);
		if (_target.items.Count > 0)
		{
			EditorGUILayout.BeginVertical ("Button");
			for (int i=0; i<_target.items.Count; i++)
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Item name:", GUILayout.Width (80f));
				if (inventoryManager.CanCarryMultiple (_target.items[i].linkedID))
				{
					EditorGUILayout.LabelField (inventoryManager.GetLabel (_target.items[i].linkedID), EditorStyles.boldLabel, GUILayout.Width (135f));
					EditorGUILayout.LabelField ("Count:", GUILayout.Width (50f));
					_target.items[i].count = EditorGUILayout.IntField (_target.items[i].count, GUILayout.Width (44f));
				}
				else
				{
					EditorGUILayout.LabelField (inventoryManager.GetLabel (_target.items[i].linkedID), EditorStyles.boldLabel);
					_target.items[i].count = 1;
				}

				if (GUILayout.Button (sideIcon, GUILayout.Width (20f), GUILayout.Height (15f)))
				{
					SideMenu (_target.items[i]);
				}

				EditorGUILayout.EndHorizontal ();
				GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
			}
			EditorGUILayout.EndVertical ();
		}
		else
		{
			EditorGUILayout.HelpBox ("This Container has no items", MessageType.Info);
		}

		EditorGUILayout.Space ();

		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("Item to create:", GUILayout.MaxWidth (130f));
		itemNumber = EditorGUILayout.Popup (itemNumber, CreateItemList ());
		if (GUILayout.Button ("Add new item"))
		{
			ContainerItem newItem = new ContainerItem (CreateItemID (itemNumber), _target.GetIDArray ());
			_target.items.Add (newItem);
		}
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.Space ();

		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
	}


	private void SideMenu (ContainerItem item)
	{
		GenericMenu menu = new GenericMenu ();
		sideItem = _target.items.IndexOf (item);
		
		if (_target.items.Count > 0)
		{
			menu.AddItem (new GUIContent ("Delete"), false, Callback, "Delete");
		}
		if (sideItem > 0 || sideItem < _target.items.Count-1)
		{
			menu.AddSeparator ("");
		}
		if (sideItem > 0)
		{
			menu.AddItem (new GUIContent ("Move up"), false, Callback, "Move up");
		}
		if (sideItem < _target.items.Count-1)
		{
			menu.AddItem (new GUIContent ("Move down"), false, Callback, "Move down");
		}
		
		menu.ShowAsContext ();
	}
	
	
	private void Callback (object obj)
	{
		if (sideItem >= 0)
		{
			ContainerItem tempItem = _target.items[sideItem];
			
			switch (obj.ToString ())
			{
			case "Delete":
				Undo.RecordObject (_target, "Delete item");
				_target.items.RemoveAt (sideItem);
				break;
				
			case "Move up":
				Undo.RecordObject (this, "Move item up");
				_target.items.RemoveAt (sideItem);
				_target.items.Insert (sideItem-1, tempItem);
				break;
				
			case "Move down":
				Undo.RecordObject (this, "Move item down");
				_target.items.RemoveAt (sideItem);
				_target.items.Insert (sideItem+1, tempItem);
				break;
			}
		}
		
		sideItem = -1;
	}
	
	
	private string[] CreateItemList ()
	{
		List<string> itemList = new List<string>();
		
		foreach (InvItem item in inventoryManager.items)
		{
			itemList.Add (item.label);
		}

		return itemList.ToArray ();
	}


	private int CreateItemID (int i)
	{
		return (inventoryManager.items[i].id);
	}

}
