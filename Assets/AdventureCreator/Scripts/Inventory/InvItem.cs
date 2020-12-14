/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"InvItem.cs"
 * 
 *	This script is a container class for individual inventory items.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

namespace AC
{

	[System.Serializable]
	public class InvItem
	{

		public int count;
		public Texture2D tex;
		public Texture2D activeTex;
		public bool carryOnStart;
		public bool carryOnStartNotDefault;
		public int carryOnStartID;
		public bool canCarryMultiple;
		public bool useSeparateSlots;
		public string label;
		public string altLabel;
		public int id;
		public int lineID = -1;
		public int useIconID = 0;
		public int binID;
		public bool isEditing = false;
		public int recipeSlot = -1;
		public int lastInteractionIndex = 0;

		public bool overrideUseSyntax = false;
		public HotspotPrefix hotspotPrefix1 = new HotspotPrefix ("Use");
		public HotspotPrefix hotspotPrefix2 = new HotspotPrefix ("on");

		public ActionListAsset useActionList;
		public ActionListAsset lookActionList;
		public List<InvInteraction> interactions = new List<InvInteraction>();
		public List<ActionListAsset> combineActionList = new List<ActionListAsset>();
		public ActionListAsset unhandledActionList;
		public ActionListAsset unhandledCombineActionList;
		public List<int> combineID = new List<int>();


		public InvItem (int[] idArray)
		{
			count = 0;
			tex = null;
			activeTex = null;
			id = 0;
			binID = -1;
			recipeSlot = -1;
			useSeparateSlots = false;
			carryOnStartNotDefault = false;

			interactions = new List<InvInteraction>();

			combineActionList = new List<ActionListAsset>();
			combineID = new List<int>();

			overrideUseSyntax = false;
			hotspotPrefix1 = new HotspotPrefix ("Use");
			hotspotPrefix2 = new HotspotPrefix ("on");

			// Update id based on array
			foreach (int _id in idArray)
			{
				if (id == _id)
					id ++;
			}

			label = "Inventory item " + (id + 1).ToString ();
			altLabel = "";
		}
		
		
		public InvItem (InvItem assetItem)
		{
			count = assetItem.count;
			tex = assetItem.tex;
			activeTex = assetItem.activeTex;
			carryOnStart = assetItem.carryOnStart;
			carryOnStartNotDefault = assetItem.carryOnStartNotDefault;
			carryOnStartID = assetItem.carryOnStartID;
			canCarryMultiple = assetItem.canCarryMultiple;
			label = assetItem.label;
			altLabel = assetItem.altLabel;
			id = assetItem.id;
			lineID = assetItem.lineID;
			useIconID = assetItem.useIconID;
			binID = assetItem.binID;
			useSeparateSlots = assetItem.useSeparateSlots;
			isEditing = false;
			recipeSlot = -1;

			overrideUseSyntax = assetItem.overrideUseSyntax;
			hotspotPrefix1 = assetItem.hotspotPrefix1;
			hotspotPrefix2 = assetItem.hotspotPrefix2;
			
			useActionList = assetItem.useActionList;
			lookActionList = assetItem.lookActionList;
			interactions = assetItem.interactions;
			combineActionList = assetItem.combineActionList;
			unhandledActionList = assetItem.unhandledActionList;
			unhandledCombineActionList = assetItem.unhandledCombineActionList;
			combineID = assetItem.combineID;
		}


		public bool DoesHaveInventoryInteraction (InvItem invItem)
		{
			if (invItem != null)
			{
				foreach (int invID in combineID)
				{
					if (invID == invItem.id)
					{
						return true;
					}
				}
			}
			
			return false;
		}


		public string GetLabel (int languageNumber)
		{
			if (languageNumber > 0)
			{
				return (SpeechManager.GetTranslation (label, lineID, languageNumber));
			}
			else
			{
				if (altLabel != "")
				{
					return altLabel;
				}
				return label;
			}
		}


		public int GetNextInteraction (int i, int numInvInteractions)
		{
			if (i < interactions.Count)
			{
				i ++;

				if (i >= interactions.Count + numInvInteractions)
				{
					return 0;
				}
				else
				{
					return i;
				}
			}
			else if (i == interactions.Count - 1 + numInvInteractions)
			{
				return 0;
			}
			
			return (i+1);
		}
		
		
		public int GetPreviousInteraction (int i, int numInvInteractions)
		{
			if (i > interactions.Count && numInvInteractions > 0)
			{
				return (i-1);
			}
			else if (i == 0)
			{
				return GetNumInteractions (numInvInteractions) - 1;
			}
			else if (i <= interactions.Count)
			{
				i --;

				if (i < 0)
				{
					return GetNumInteractions (numInvInteractions) - 1;
				}
				else
				{
					return i;
				}
			}
			
			return (i-1);
		}


		private int GetNumInteractions (int numInvInteractions)
		{
			return (interactions.Count + numInvInteractions);
		}

	}

}