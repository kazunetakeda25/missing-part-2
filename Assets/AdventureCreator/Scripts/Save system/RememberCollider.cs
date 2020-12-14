/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RememberCollider.cs"
 * 
 *	This script is attached to Colliders in the scene
 *	whose on/off state we wish to save. 
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class RememberCollider : Remember
	{
		
		public AC_OnOff startState = AC_OnOff.On;
		
		
		public void Awake ()
		{
			if (KickStarter.settingsManager && GameIsPlaying ())
			{
				bool isOn = false;
				if (startState == AC_OnOff.On)
				{
					isOn = true;
				}

				if (GetComponent <Collider>())
				{
					GetComponent <Collider>().enabled = isOn;
				}

				else if (GetComponent <Collider2D>())
				{
					GetComponent <Collider2D>().enabled = isOn;
				}
			}
		}
		
		
		public override string SaveData ()
		{
			ColliderData colliderData = new ColliderData ();

			colliderData.objectID = constantID;
			colliderData.isOn = false;

			if (GetComponent <Collider>())
			{
				colliderData.isOn = GetComponent <Collider>().enabled;
			}
			else if (GetComponent <Collider2D>())
			{
				colliderData.isOn = GetComponent <Collider2D>().enabled;
			}

			return Serializer.SaveScriptData <ColliderData> (colliderData);
		}
		
		
		public override void LoadData (string stringData)
		{
			ColliderData data = Serializer.LoadScriptData <ColliderData> (stringData);
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
	public class ColliderData : RememberData
	{
		public bool isOn;
		public ColliderData () { }
	}

}