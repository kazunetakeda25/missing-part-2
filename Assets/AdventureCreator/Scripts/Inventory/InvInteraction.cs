/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"InvInteraction.cs"
 * 
 *	This script is a container class for inventory interactions.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	[System.Serializable]
	public class InvInteraction
	{

		public ActionListAsset actionList;
		public CursorIcon icon;


		public InvInteraction (CursorIcon _icon)
		{
			icon = _icon;
			actionList = null;
		}

	}

}