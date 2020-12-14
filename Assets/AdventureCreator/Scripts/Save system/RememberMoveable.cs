/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RememberTransform.cs"
 * 
 *	This script, when attached to Moveable objects in the scene,
 *	will record appropriate positional data
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{
	
	public class RememberMoveable : Remember
	{

		public AC_OnOff startState = AC_OnOff.On;
		
		
		public void Awake ()
		{
			if (KickStarter.settingsManager && GameIsPlaying ())
			{
				if (GetComponent <DragBase>())
				{
					if (startState == AC_OnOff.On)
					{
						GetComponent <DragBase>().TurnOn ();
					}
					else
					{
						GetComponent <DragBase>().TurnOff ();
					}
				}

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
			MoveableData moveableData = new MoveableData ();
			
			moveableData.objectID = constantID;

			if (gameObject.layer == LayerMask.NameToLayer (KickStarter.settingsManager.hotspotLayer))
			{
				moveableData.isOn = true;
			}
			else
			{
				moveableData.isOn = false;
			}

			if (GetComponent <Moveable_Drag>())
			{
				Moveable_Drag moveable_Drag = GetComponent <Moveable_Drag>();
				moveableData.trackValue = moveable_Drag.trackValue;
				moveableData.revolutions = moveable_Drag.revolutions;
			}
			
			moveableData.LocX = transform.position.x;
			moveableData.LocY = transform.position.y;
			moveableData.LocZ = transform.position.z;
			
			moveableData.RotX = transform.eulerAngles.x;
			moveableData.RotY = transform.eulerAngles.y;
			moveableData.RotZ = transform.eulerAngles.z;
			
			moveableData.ScaleX = transform.localScale.x;
			moveableData.ScaleY = transform.localScale.y;
			moveableData.ScaleZ = transform.localScale.z;
			
			return Serializer.SaveScriptData <MoveableData> (moveableData);
		}
		
		
		public override void LoadData (string stringData)
		{
			MoveableData data = Serializer.LoadScriptData <MoveableData> (stringData);
			if (data == null) return;

			if (GetComponent <DragBase>())
			{
				if (data.isOn)
				{
					GetComponent <DragBase>().TurnOn ();
				}
				else
				{
					GetComponent <DragBase>().TurnOff ();
				}
			}

			if (data.isOn)
			{
				gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.hotspotLayer);
			}
			else
			{
				gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.deactivatedLayer);
			}

			transform.position = new Vector3 (data.LocX, data.LocY, data.LocZ);
			transform.eulerAngles = new Vector3 (data.RotX, data.RotY, data.RotZ);
			transform.localScale = new Vector3 (data.ScaleX, data.ScaleY, data.ScaleZ);

			if (GetComponent <Moveable_Drag>())
			{
				Moveable_Drag moveable_Drag = GetComponent <Moveable_Drag>();
				moveable_Drag.isHeld = false;
				if (moveable_Drag.dragMode == DragMode.LockToTrack && moveable_Drag.track != null)
				{
					moveable_Drag.trackValue = data.trackValue;
					moveable_Drag.revolutions = data.revolutions;
					moveable_Drag.StopAutoMove ();
					moveable_Drag.track.SetPositionAlong (data.trackValue, moveable_Drag);
				}
			}
		}
		
	}
	
	
	[System.Serializable]
	public class MoveableData : RememberData
	{
		
		public bool isOn;

		public float trackValue;
		public int revolutions;

		public float LocX;
		public float LocY;
		public float LocZ;
		
		public float RotX;
		public float RotY;
		public float RotZ;
		
		public float ScaleX;
		public float ScaleY;
		public float ScaleZ;
		
		public MoveableData () { }
		
	}
	
}