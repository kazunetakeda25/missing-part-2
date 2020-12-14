/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RememberHotspot.cs"
 * 
 *	This script is attached to hotspot objects in the scene
 *	whose on/off state we wish to save. 
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class RememberHotspot : Remember
	{

		public AC_OnOff startState = AC_OnOff.On;


		public void Awake ()
		{
			if (KickStarter.settingsManager && GameIsPlaying ())
			{
				if (startState == AC_OnOff.On)
				{
					this.gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.hotspotLayer);
				}
				else
				{
					this.gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.deactivatedLayer);
				}
			}
		}


		public override string SaveData ()
		{
			HotspotData hotspotData = new HotspotData ();
			hotspotData.objectID = constantID;

			if (gameObject.layer == LayerMask.NameToLayer (KickStarter.settingsManager.hotspotLayer))
			{
				hotspotData.isOn = true;
			}
			else
			{
				hotspotData.isOn = false;
			}
			
			if (GetComponent <Hotspot>())
			{
				Hotspot _hotspot = GetComponent <Hotspot>();
				hotspotData.buttonStates = ButtonStatesToString (_hotspot);

				hotspotData.hotspotName = _hotspot.GetName (0);
				hotspotData.displayLineID = _hotspot.displayLineID;
			}
			
			return Serializer.SaveScriptData <HotspotData> (hotspotData);
		}


		public override void LoadData (string stringData)
		{
			HotspotData data = Serializer.LoadScriptData <HotspotData> (stringData);
			if (data == null) return;

			if (data.isOn)
			{
				gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.hotspotLayer);
			}
			else
			{
				gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.deactivatedLayer);
			}
			
			if (GetComponent <Hotspot>())
			{
				Hotspot _hotspot = GetComponent <Hotspot>();

				StringToButtonStates (_hotspot, data.buttonStates);

				if (data.hotspotName != "")
				{
					_hotspot.SetName (data.hotspotName, data.displayLineID);
				}
			}
		}


		private void StringToButtonStates (Hotspot hotspot, string stateString)
		{
			if (stateString.Length == 0)
			{
				return;
			}

			string[] typesArray = stateString.Split ("|"[0]);
			
			if (KickStarter.settingsManager == null || KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
			{
				// Look interactions
				if (hotspot.provideLookInteraction && hotspot.lookButton != null)
				{
					hotspot.lookButton.isDisabled = SetButtonDisabledValue (typesArray [0]);
				}
			}

			if (hotspot.provideUseInteraction)
			{
				string[] usesArray = typesArray[1].Split (","[0]);
				
				for (int i=0; i<usesArray.Length; i++)
				{
					if (hotspot.useButtons.Count < i+1)
					{
						break;
					}
					hotspot.useButtons[i].isDisabled = SetButtonDisabledValue (usesArray [i]);
				}
			}

			// Inventory interactions
			if (hotspot.provideInvInteraction && typesArray.Length > 2)
			{
				string[] invArray = typesArray[2].Split (","[0]);
				
				for (int i=0; i<invArray.Length; i++)
				{
					if (hotspot.invButtons.Count < i+1)
					{
						break;
					}
					
					hotspot.invButtons[i].isDisabled = SetButtonDisabledValue (invArray [i]);
				}
			}
		}
		
		
		private string ButtonStatesToString (Hotspot hotspot)
		{
			System.Text.StringBuilder stateString = new System.Text.StringBuilder ();
			
			if (KickStarter.settingsManager == null || KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
			{
				// Single-use and Look interaction
				if (hotspot.provideLookInteraction)
				{
					stateString.Append (GetButtonDisabledValue (hotspot.lookButton));
				}
				else
				{
					stateString.Append ("0");
				}
			}

			stateString.Append ("|");

			// Multi-use interactions
			if (hotspot.provideUseInteraction)
			{
				foreach (AC.Button button in hotspot.useButtons)
				{
					stateString.Append (GetButtonDisabledValue (button));
					
					if (hotspot.useButtons.IndexOf (button) < hotspot.useButtons.Count-1)
					{
						stateString.Append (",");
					}
				}
			}
				
			stateString.Append ("|");

			// Inventory interactions
			if (hotspot.provideInvInteraction)
			{
				foreach (AC.Button button in hotspot.invButtons)
				{
					stateString.Append (GetButtonDisabledValue (button));
					
					if (hotspot.invButtons.IndexOf (button) < hotspot.invButtons.Count-1)
					{
						stateString.Append (",");
					}
				}
			}
			
			return stateString.ToString ();
		}


		private string GetButtonDisabledValue (AC.Button button)
		{
			if (button != null && !button.isDisabled)
			{
				return ("1");
			}
			
			return ("0");
		}
		
		
		private bool SetButtonDisabledValue (string text)
		{
			if (text == "1")
			{
				return false;
			}
			
			return true;
		}

	}


	[System.Serializable]
	public class HotspotData : RememberData
	{
		public bool isOn;
		public string buttonStates;
		public int displayLineID;
		public string hotspotName;

		public HotspotData () { }
	}

}