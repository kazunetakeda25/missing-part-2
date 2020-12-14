/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuLink.cs"
 * 
 *	This script connects to a Menu Element defined
 *  in the Menu Manager, allowing for 3D, scene-based menus
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class MenuLink : MonoBehaviour
	{

		public string menuName = "";
		public string elementName = "";
		public int slot = 0;
		public bool setTextLabels = false;

		private AC.Menu menu;
		private MenuElement element;
	

		private void Start ()
		{
			if (menuName == "" || elementName == "")
			{
				return;
			}

			try
			{
				menu = PlayerMenus.GetMenuWithName (menuName);
				element = PlayerMenus.GetElementWithName (menuName, elementName);
			}
			catch
			{
				Debug.LogWarning ("Cannot find Menu Element with name: " + elementName + " on Menu: " + menuName);
			}
		}


		private void FixedUpdate ()
		{
			if (element && setTextLabels)
			{
				int languageNumber = Options.GetLanguage ();
				if (GetComponent <GUIText>())
				{
					GetComponent <GUIText>().text = GetLabel (languageNumber);
				}
				if (GetComponent <TextMesh>())
				{
					GetComponent <TextMesh>().text = GetLabel (languageNumber);
				}
			}
		}


		public string GetLabel (int languageNumber)
		{
			if (element)
			{
				return element.GetLabel (slot, languageNumber);
			}

			return "";
		}


		public bool IsVisible ()
		{
			if (element && menu)
			{
				if (!menu.IsVisible ())
				{
					return false;
				}

				return element.isVisible;
			}

			return false;
		}


		public void Interact ()
		{
			if (element)
			{
				if (!element.isClickable)
				{
					Debug.Log ("Cannot click on " + elementName);
				}

				PlayerMenus.SimulateClick (menuName, element, slot);
			}
		}


		private void OnDestroy ()
		{
			element = null;
			menu = null;
		}

	}

}