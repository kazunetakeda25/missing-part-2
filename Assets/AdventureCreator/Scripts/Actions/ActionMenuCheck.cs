/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionMenuCheck.cs"
 * 
 *	This Action checks the visibility states of menus and elements
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
	public class ActionMenuCheck : ActionCheck
	{
		
		public enum MenuCheckType { MenuIsVisible, MenuIsLocked, ElementIsVisible };
		public MenuCheckType checkType = MenuCheckType.MenuIsVisible;

		public string menuToCheck = "";
		public int menuToCheckParameterID = -1;
		
		public string elementToCheck = "";
		public int elementToCheckParameterID = -1;

		
		public ActionMenuCheck ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Menu;
			title = "Check state";
			description = "Queries the visibility of menu elements, and the enabled or locked state of menus.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			menuToCheck = AssignString (parameters, menuToCheckParameterID, menuToCheck);
			elementToCheck = AssignString (parameters, elementToCheckParameterID, elementToCheck);
		}


		override public bool CheckCondition ()
		{
			AC.Menu _menu = PlayerMenus.GetMenuWithName (menuToCheck);
			if (_menu != null)
			{
				if (checkType == MenuCheckType.MenuIsVisible)
				{
					return _menu.IsVisible ();
				}
				else if (checkType == MenuCheckType.MenuIsLocked)
				{
					return _menu.isLocked;
				}
				else if (checkType == MenuCheckType.ElementIsVisible)
				{
					MenuElement _element = PlayerMenus.GetElementWithName (menuToCheck, elementToCheck);
					if (_element != null)
					{
						return _element.isVisible;
					}
				}
			}

			return false;
		}
		

		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			checkType = (MenuCheckType) EditorGUILayout.EnumPopup ("State to check:", checkType);
			
			if (checkType == MenuCheckType.MenuIsVisible || checkType == MenuCheckType.MenuIsLocked)
			{
				menuToCheckParameterID = Action.ChooseParameterGUI ("Menu to check:", parameters, menuToCheckParameterID, ParameterType.String);
				if (menuToCheckParameterID < 0)
				{
					menuToCheck = EditorGUILayout.TextField ("Menu to check:", menuToCheck);
				}
			}
			else if (checkType == MenuCheckType.ElementIsVisible)
			{
				menuToCheckParameterID = Action.ChooseParameterGUI ("Menu containing element:", parameters, menuToCheckParameterID, ParameterType.String);
				if (menuToCheckParameterID < 0)
				{
					menuToCheck = EditorGUILayout.TextField ("Menu containing element:", menuToCheck);
				}

				elementToCheckParameterID = Action.ChooseParameterGUI ("Element to check:", parameters, elementToCheckParameterID, ParameterType.String);
				if (elementToCheckParameterID < 0)
				{
					elementToCheck = EditorGUILayout.TextField ("Element to check:", elementToCheck);
				}
			}
		}
		
		
		public override string SetLabel ()
		{
			string labelAdd = " (" + checkType.ToString () + " '" + menuToCheck;
			if (checkType == MenuCheckType.ElementIsVisible)
			{
				labelAdd += " " + elementToCheck;
			}
			labelAdd += "')";
			return labelAdd;
		}
		
		#endif
		
	}

}