/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuSystem.cs"
 *	This script can be used to add extra functionality to menus.
 *	When a menu is enabled, OnMenuEnable is called,
 *  and when an element is clicked on, OnElementClick is (optionally) called.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	public class MenuSystem : MonoBehaviour
	{

		public static void OnMenuEnable (AC.Menu _menu)
		{
			// This function is called whenever a menu is enabled.

			if (_menu.title == "Pause")
			{
				MenuElement saveButton = _menu.GetElementWithName ("SaveButton");
				
				if (saveButton)
				{
					saveButton.isVisible = !PlayerMenus.IsSavingLocked ();
				}
				
				_menu.Recalculate ();
			}
		}
		

		public static void OnElementClick (AC.Menu _menu, MenuElement _element, int _slot, int _buttonPressed)
		{
			// This function is called whenever a clickable element has a click type of "Custom Script".
		}
		
	}

}