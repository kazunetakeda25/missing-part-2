/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"RememberNPC.cs"
 * 
 *	This script is attached to NPCs in the scene
 *	with path and transform data we wish to save.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class RememberNPC : Remember
	{

		public AC_OnOff startState = AC_OnOff.On;

		
		public void Awake ()
		{
			if (KickStarter.settingsManager && GetComponent <RememberHotspot>() == null && GameIsPlaying ())
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
			NPCData npcData = new NPCData();

			npcData.objectID = constantID;
			
			if (gameObject.layer == LayerMask.NameToLayer (KickStarter.settingsManager.hotspotLayer))
			{
				npcData.isOn = true;
			}
			else
			{
				npcData.isOn = false;
			}
			
			npcData.LocX = transform.position.x;
			npcData.LocY = transform.position.y;
			npcData.LocZ = transform.position.z;
			
			npcData.RotX = transform.eulerAngles.x;
			npcData.RotY = transform.eulerAngles.y;
			npcData.RotZ = transform.eulerAngles.z;
			
			npcData.ScaleX = transform.localScale.x;
			npcData.ScaleY = transform.localScale.y;
			npcData.ScaleZ = transform.localScale.z;
			
			if (GetComponent <NPC>())
			{
				NPC npc = GetComponent <NPC>();

				if (npc.animationEngine == AnimationEngine.Sprites2DToolkit || npc.animationEngine == AnimationEngine.SpritesUnity)
				{
					npcData.idleAnim = npc.idleAnimSprite;
					npcData.walkAnim = npc.walkAnimSprite;
					npcData.talkAnim = npc.talkAnimSprite;
					npcData.runAnim = npc.runAnimSprite;
				}
				else if (npc.animationEngine == AnimationEngine.Legacy)
				{
					npcData.idleAnim = AssetLoader.GetAssetInstanceID (npc.idleAnim);
					npcData.walkAnim = AssetLoader.GetAssetInstanceID (npc.walkAnim);
					npcData.runAnim = AssetLoader.GetAssetInstanceID (npc.runAnim);
					npcData.talkAnim = AssetLoader.GetAssetInstanceID (npc.talkAnim);
				}
				else if (npc.animationEngine == AnimationEngine.Mecanim)
				{
					npcData.walkAnim = npc.moveSpeedParameter;
					npcData.talkAnim = npc.talkParameter;
					npcData.runAnim = npc.turnParameter;
				}

				npcData.walkSound = AssetLoader.GetAssetInstanceID (npc.walkSound);
				npcData.runSound = AssetLoader.GetAssetInstanceID (npc.runSound);

				npcData.speechLabel = npc.speechLabel;
				npcData.portraitGraphic = AssetLoader.GetAssetInstanceID (npc.portraitIcon.texture);

				npcData.walkSpeed = npc.walkSpeedScale;
				npcData.runSpeed = npc.runSpeedScale;

				// Rendering
				npcData.lockDirection = npc.lockDirection;
				npcData.lockScale = npc.lockScale;
				if (npc.spriteChild && npc.spriteChild.GetComponent <FollowSortingMap>())
				{
					npcData.lockSorting = npc.spriteChild.GetComponent <FollowSortingMap>().lockSorting;
				}
				else if (npc.GetComponent <FollowSortingMap>())
				{
					npcData.lockSorting = npc.GetComponent <FollowSortingMap>().lockSorting;
				}
				else
				{
					npcData.lockSorting = false;
				}
				npcData.spriteDirection = npc.spriteDirection;
				npcData.spriteScale = npc.spriteScale;
				if (npc.spriteChild && npc.spriteChild.GetComponent <Renderer>())
				{
					npcData.sortingOrder = npc.spriteChild.GetComponent <Renderer>().sortingOrder;
					npcData.sortingLayer = npc.spriteChild.GetComponent <Renderer>().sortingLayerName;
				}
				else if (npc.GetComponent <Renderer>())
				{
					npcData.sortingOrder = npc.GetComponent <Renderer>().sortingOrder;
					npcData.sortingLayer = npc.GetComponent <Renderer>().sortingLayerName;
				}

				npcData.pathID = 0;
				npcData.lastPathID = 0;
				if (npc.GetPath (true))
				{
					npcData.targetNode = npc.GetTargetNode (true);
					npcData.prevNode = npc.GetPrevNode (true);
					npcData.isRunning = npc.isRunning;
					npcData.pathAffectY = npc.activePath.affectY;
					
					if (npc.GetPath (true) == GetComponent <Paths>())
					{
						npcData.pathData = Serializer.CreatePathData (GetComponent <Paths>());
					}
					else
					{
						if (npc.GetPath (true).GetComponent <ConstantID>())
						{
							npcData.pathID = npc.GetPath (true).GetComponent <ConstantID>().constantID;
						}
						else
						{
							Debug.LogWarning ("Want to save path data for " + name + " but path has no ID!");
						}
					}
				}

				if (npc.GetPath (false))
				{
					npcData.lastTargetNode = npc.GetTargetNode (false);
					npcData.lastPrevNode = npc.GetPrevNode (false);

					if (npc.GetPath (false).GetComponent <ConstantID>())
					{
						npcData.lastPathID = npc.GetPath (false).GetComponent <ConstantID>().constantID;
					}
					else
					{
						Debug.LogWarning ("Want to save previous path data for " + name + " but path has no ID!");
					}
				}
		
				if (npc.followTarget)
				{
					if (!npc.followTargetIsPlayer)
					{
						if (npc.followTarget.GetComponent <ConstantID>())
						{
							npcData.followTargetID = npc.followTarget.GetComponent <ConstantID>().constantID;
							npcData.followTargetIsPlayer = npc.followTargetIsPlayer;
							npcData.followFrequency = npc.followFrequency;
							npcData.followDistance = npc.followDistance;
							npcData.followDistanceMax= npc.followDistanceMax;
						}
						else
						{
							Debug.LogWarning ("Want to save follow data for " + name + " but " + npc.followTarget.name + " has no ID!");
						}
					}
					else
					{
						npcData.followTargetID = 0;
						npcData.followTargetIsPlayer = npc.followTargetIsPlayer;
						npcData.followFrequency = npc.followFrequency;
						npcData.followDistance = npc.followDistance;
						npcData.followDistanceMax = npc.followDistanceMax;
					}
				}
				else
				{
					npcData.followTargetID = 0;
					npcData.followTargetIsPlayer = false;
					npcData.followFrequency = 0f;
					npcData.followDistance = 0f;
					npcData.followDistanceMax = 0f;
				}

				if (npc.headFacing == HeadFacing.Manual)
				{
					npcData.isHeadTurning = true;
					npcData.headTargetX = npc.headTurnTarget.x;
					npcData.headTargetY = npc.headTurnTarget.y;
					npcData.headTargetZ = npc.headTurnTarget.z;
				}
				else
				{
					npcData.isHeadTurning = false;
					npcData.headTargetX = 0f;
					npcData.headTargetY = 0f;
					npcData.headTargetZ = 0f;
				}
			}
			
			return Serializer.SaveScriptData <NPCData> (npcData);
		}
		
		
		public override void LoadData (string stringData)
		{
			NPCData data = Serializer.LoadScriptData <NPCData> (stringData);
			if (data == null) return;

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
			
			if (GetComponent <NPC>())
			{
				NPC npc = GetComponent <NPC>();

				npc.EndPath ();
				
				if (npc.animationEngine == AnimationEngine.Sprites2DToolkit || npc.animationEngine == AnimationEngine.SpritesUnity)
				{
					npc.idleAnimSprite = data.idleAnim;
					npc.walkAnimSprite = data.walkAnim;
					npc.talkAnimSprite = data.talkAnim;
					npc.runAnimSprite = data.runAnim;
				}
				else if (npc.animationEngine == AnimationEngine.Legacy)
				{
					npc.idleAnim = AssetLoader.RetrieveAsset (npc.idleAnim, data.idleAnim);
					npc.walkAnim = AssetLoader.RetrieveAsset (npc.walkAnim, data.walkAnim);
					npc.runAnim = AssetLoader.RetrieveAsset (npc.runAnim, data.talkAnim);
					npc.talkAnim = AssetLoader.RetrieveAsset (npc.talkAnim, data.runAnim);
				}
				else if (npc.animationEngine == AnimationEngine.Mecanim)
				{
					npc.moveSpeedParameter = data.walkAnim;
					npc.talkParameter = data.talkAnim;
					npc.turnParameter = data.runAnim;;
				}

				npc.walkSound = AssetLoader.RetrieveAsset (npc.walkSound, data.walkSound);
				npc.runSound = AssetLoader.RetrieveAsset (npc.runSound, data.runSound);
				npc.speechLabel = data.speechLabel;
				npc.portraitIcon.texture = AssetLoader.RetrieveAsset (npc.portraitIcon.texture, data.portraitGraphic);
					
				npc.walkSpeedScale = data.walkSpeed;
				npc.runSpeedScale = data.runSpeed;

				// Rendering
				npc.lockDirection = data.lockDirection;
				npc.lockScale = data.lockScale;
				if (npc.spriteChild && npc.spriteChild.GetComponent <FollowSortingMap>())
				{
					npc.spriteChild.GetComponent <FollowSortingMap>().lockSorting = data.lockSorting;
				}
				else if (npc.GetComponent <FollowSortingMap>())
				{
					npc.GetComponent <FollowSortingMap>().lockSorting = data.lockSorting;
				}
				else
				{
					npc.ReleaseSorting ();
				}
				
				if (data.lockDirection)
				{
					npc.spriteDirection = data.spriteDirection;
				}
				if (data.lockScale)
				{
					npc.spriteScale = data.spriteScale;
				}
				if (data.lockSorting)
				{
					if (npc.spriteChild && npc.spriteChild.GetComponent <Renderer>())
					{
						npc.spriteChild.GetComponent <Renderer>().sortingOrder = data.sortingOrder;
						npc.spriteChild.GetComponent <Renderer>().sortingLayerName = data.sortingLayer;
					}
					else if (npc.GetComponent <Renderer>())
					{
						npc.GetComponent <Renderer>().sortingOrder = data.sortingOrder;
						npc.GetComponent <Renderer>().sortingLayerName = data.sortingLayer;
					}
				}
			
				AC.Char charToFollow = null;
				if (data.followTargetID != 0)
				{
					RememberNPC followNPC = Serializer.returnComponent <RememberNPC> (data.followTargetID);
					if (followNPC.GetComponent <AC.Char>())
					{
						charToFollow = followNPC.GetComponent <AC.Char>();
					}
				}
				
				npc.FollowAssign (charToFollow, data.followTargetIsPlayer, data.followFrequency, data.followDistance, data.followDistanceMax);
				npc.Halt ();
				
				if (data.pathData != null && data.pathData != "" && GetComponent <Paths>())
				{
					Paths savedPath = GetComponent <Paths>();
					savedPath = Serializer.RestorePathData (savedPath, data.pathData);
					npc.SetPath (savedPath, data.targetNode, data.prevNode, data.pathAffectY);
					npc.isRunning = data.isRunning;
				}
				else if (data.pathID != 0)
				{
					Paths pathObject = Serializer.returnComponent <Paths> (data.pathID);
					
					if (pathObject != null)
					{
						npc.SetPath (pathObject, data.targetNode, data.prevNode);
					}
					else
					{
						Debug.LogWarning ("Trying to assign a path for NPC " + this.name + ", but the path was not found - was it deleted?");
					}
				}

				if (data.lastPathID != 0)
				{
					Paths pathObject = Serializer.returnComponent <Paths> (data.lastPathID);
					
					if (pathObject != null)
					{
						npc.SetLastPath (pathObject, data.lastTargetNode, data.lastPrevNode);
					}
					else
					{
						Debug.LogWarning ("Trying to assign the previous path for NPC " + this.name + ", but the path was not found - was it deleted?");
					}
				}

				// Head target
				if (data.isHeadTurning)
				{
					npc.SetHeadTurnTarget (new Vector3 (data.headTargetX, data.headTargetY, data.headTargetZ), true);
				}
				else
				{
					npc.ClearHeadTurnTarget (true);
				}
			}
		}

	}


	[System.Serializable]
	public class NPCData : RememberData
	{
		
		public bool isOn;
		
		public float LocX;
		public float LocY;
		public float LocZ;
		
		public float RotX;
		public float RotY;
		public float RotZ;
		
		public float ScaleX;
		public float ScaleY;
		public float ScaleZ;
		
		public string idleAnim;
		public string walkAnim;
		public string talkAnim;
		public string runAnim;

		public string walkSound;
		public string runSound;
		public string portraitGraphic;

		public float walkSpeed;
		public float runSpeed;

		public bool lockDirection;
		public string spriteDirection;
		public bool lockScale;
		public float spriteScale;
		public bool lockSorting;
		public int sortingOrder;
		public string sortingLayer;
		
		public int pathID;
		public int targetNode;
		public int prevNode;
		public string pathData;
		public bool isRunning;
		public bool pathAffectY;

		public int lastPathID;
		public int lastTargetNode;
		public int lastPrevNode;

		public int followTargetID = 0;
		public bool followTargetIsPlayer = false;
		public float followFrequency = 0f;
		public float followDistance = 0f;
		public float followDistanceMax = 0f;

		public bool isHeadTurning = false;
		public float headTargetX = 0f;
		public float headTargetY = 0f;
		public float headTargetZ = 0f;

		public string speechLabel;
		
		public NPCData () { }

	}

}