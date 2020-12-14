/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RememberTrigger.cs"
 * 
 *	This script is attached to Trigger objects in the scene
 *	whose on/off state we wish to save. 
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class RememberTrigger : Remember
	{
		
		public AC_OnOff startState = AC_OnOff.On;
		
		
		public void Awake ()
		{
			if (GameIsPlaying () && GetComponent <AC_Trigger>())
			{
				if (startState == AC_OnOff.On)
				{
					GetComponent <AC_Trigger>().TurnOn ();
				}
				else
				{
					GetComponent <AC_Trigger>().TurnOff ();
				}
			}
		}
		
		
		public override string SaveData ()
		{
			TriggerData triggerData = new TriggerData ();
			triggerData.objectID = constantID;

			if (GetComponent <Collider>())
			{
				triggerData.isOn = GetComponent <Collider>().enabled;
			}
			else if (GetComponent <Collider2D>())
			{
				triggerData.isOn = GetComponent <Collider2D>().enabled;
			}
			else
			{
				triggerData.isOn = false;
			}

			return Serializer.SaveScriptData <TriggerData> (triggerData);
		}
		
		
		public override void LoadData (string stringData)
		{
			TriggerData data = Serializer.LoadScriptData <TriggerData> (stringData);
			if (data == null) return;

			if (GetComponent <Collider>())
			{
				GetComponent <Collider>().enabled = data.isOn;
			}
			else if (GetComponent <Collider2D>())
			{
				GetComponent <Collider2D>().enabled = data.isOn;
			}
		}

	}


	[System.Serializable]
	public class TriggerData : RememberData
	{
		public bool isOn;
		public TriggerData () { }
	}

}