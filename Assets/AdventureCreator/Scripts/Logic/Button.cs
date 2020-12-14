/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Button.cs"
 * 
 *	This script is a container class for interactions
 *	that are linked to Hotspots and NPCs.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	[System.Serializable]
	public class Button
	{
		
		public Interaction interaction = null;
		public ActionListAsset assetFile = null;

		public GameObject customScriptObject = null;
		public string customScriptFunction = "";
		
		public bool isDisabled = false;
		public int invID = 0;
		public int iconID = -1;
		public SelectItemMode selectItemMode = SelectItemMode.Use;

		public PlayerAction playerAction = PlayerAction.DoNothing;

		public bool setProximity = false;
		public float proximity = 1f;
		public bool faceAfter = false;
		public bool isBlocking = false;

		
		public Button ()
		{ }


		public bool IsButtonModified ()
		{
			if (interaction != null ||
			    assetFile != null ||
			    customScriptObject != null ||
			    customScriptFunction != "" ||
			    isDisabled != false ||
			    playerAction != PlayerAction.DoNothing ||
			    setProximity != false ||
			    proximity != 1f ||
			    faceAfter != false ||
			    isBlocking != false)
			{
				return true;
			}
			return false;
		}


		public void CopyButton (Button _button)
		{
			interaction = _button.interaction;
			assetFile = _button.assetFile;
			customScriptObject = _button.customScriptObject;
			customScriptFunction = _button.customScriptFunction;
			isDisabled = _button.isDisabled;
			invID = _button.invID;
			iconID = _button.iconID;
			playerAction = _button.playerAction;
			setProximity = _button.setProximity;
			proximity = _button.proximity;
			faceAfter = _button.faceAfter;
			isBlocking = _button.isBlocking;
		}
		
	}

}