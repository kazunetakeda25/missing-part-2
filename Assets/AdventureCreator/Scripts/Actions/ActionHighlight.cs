/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionHighlight.cs"
 * 
 *	This action manually highlights objects and Inventory items
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionHighlight : Action
	{

		public int parameterID = -1;
		public int constantID = 0;
		public enum WhatToHighlight { SceneObject, InventoryItem };
		public WhatToHighlight whatToHighlight = WhatToHighlight.SceneObject;
		public HighlightType highlightType = HighlightType.Enable;
		public bool isInstant = false;

		public Highlight highlightObject;

		public int invID;
		private int invNumber;
		
		private InventoryManager inventoryManager;

		
		public ActionHighlight ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Object;
			title = "Highlight";
			description = "Gives a glow effect to any mesh object with the Highlight script component attached to it. Can also be used to make Inventory items glow, making it useful for tutorial sections.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			if (whatToHighlight == WhatToHighlight.SceneObject)
			{
				highlightObject = AssignFile <Highlight> (parameters, parameterID, constantID, highlightObject);
			}
			else
			{
				invID = AssignInvItemID (parameters, parameterID, invID);
			}
		}
		
		
		override public float Run ()
		{
			if (whatToHighlight == WhatToHighlight.SceneObject && highlightObject == null)
			{
				return 0f;
			}

			if (whatToHighlight == WhatToHighlight.SceneObject)
			{
				if (highlightType == HighlightType.Enable)
				{
					if (isInstant)
					{
						highlightObject.HighlightOnInstant ();
					}
					else
					{
						highlightObject.HighlightOn ();
					}
				}
				else if (highlightType == HighlightType.Disable)
				{
					if (isInstant)
					{
						highlightObject.HighlightOffInstant ();
					}
					else
					{
						highlightObject.HighlightOff ();
					}
				}
				else if (highlightType == HighlightType.PulseOnce)
				{
					highlightObject.Flash ();
				}
				else if (highlightType == HighlightType.PulseContinually)
				{
					highlightObject.Pulse ();
				}
			}

			else
			{
				if (KickStarter.runtimeInventory)
				{
					if (highlightType == HighlightType.Enable && isInstant)
					{
						KickStarter.runtimeInventory.HighlightItemOnInstant (invID);
						return 0f;
					}
					else if (highlightType == HighlightType.Disable && isInstant)
					{
						KickStarter.runtimeInventory.HighlightItemOffInstant ();
						return 0f;
					}
					KickStarter.runtimeInventory.HighlightItem (invID, highlightType);
				}
			}

			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			whatToHighlight = (WhatToHighlight) EditorGUILayout.EnumPopup ("What to highlight:", whatToHighlight);

			if (whatToHighlight == WhatToHighlight.SceneObject)
			{
				parameterID = Action.ChooseParameterGUI ("Object to highlight:", parameters, parameterID, ParameterType.GameObject);
				if (parameterID >= 0)
				{
					constantID = 0;
					highlightObject = null;
				}
				else
				{
					highlightObject = (Highlight) EditorGUILayout.ObjectField ("Object to highlight:", highlightObject, typeof (Highlight), true);
					
					constantID = FieldToID <Highlight> (highlightObject, constantID);
					highlightObject = IDToField <Highlight> (highlightObject, constantID, false);
				}
			}
			else if (whatToHighlight == WhatToHighlight.InventoryItem)
			{
				if (!inventoryManager)
				{
					inventoryManager = AdvGame.GetReferences ().inventoryManager;
				}

				if (inventoryManager)
				{
					// Create a string List of the field's names (for the PopUp box)
					List<string> labelList = new List<string>();
					
					int i = 0;
					if (parameterID == -1)
					{
						invNumber = -1;
					}
					
					if (inventoryManager.items.Count > 0)
					{
						foreach (InvItem _item in inventoryManager.items)
						{
							labelList.Add (_item.label);
							if (_item.id == invID)
							{
								invNumber = i;
							}
							i++;
						}
						
						if (invNumber == -1)
						{
							Debug.LogWarning ("Previously chosen item no longer exists!");
							invNumber = 0;
							invID = 0;
						}

						//
						parameterID = Action.ChooseParameterGUI ("Inventory item:", parameters, parameterID, ParameterType.InventoryItem);
						if (parameterID >= 0)
						{
							invNumber = Mathf.Min (invNumber, inventoryManager.items.Count-1);
							invID = -1;
						}
						else
						{
							invNumber = EditorGUILayout.Popup ("Inventory item:", invNumber, labelList.ToArray());
							invID = inventoryManager.items[invNumber].id;
						}
						//
					}
					
					else
					{
						EditorGUILayout.HelpBox ("No inventory items exist!", MessageType.Info);
						invID = -1;
						invNumber = -1;
					}
				}
			}

			highlightType = (HighlightType) EditorGUILayout.EnumPopup ("Highlight type:", highlightType);
			if (highlightType == HighlightType.Enable || highlightType == HighlightType.Disable)
			{
				isInstant = EditorGUILayout.Toggle ("Is instant?", isInstant);
			}

			AfterRunningOption ();
		}
		
		
		public override string SetLabel ()
		{
			if (highlightObject != null)
			{
				if (whatToHighlight == WhatToHighlight.SceneObject)
				{
					return (" (" + highlightType.ToString () + " " + highlightObject.gameObject.name + ")");
				}
				return (" (" + highlightType.ToString () + " Inventory item)");
			}

			return "";
		}
		
		#endif
		
	}

}