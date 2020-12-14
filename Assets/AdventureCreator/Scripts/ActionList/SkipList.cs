/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"SkipList.cs"
 * 
 *	This is a container for ActionList objets and assets than can be skipped or resumed at a later time.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class SkipList
	{
		
		public ActionList actionList;
		public ActionListAsset actionListAsset;
		public int startIndex;
		
		
		public SkipList ()
		{
			actionList = null;
			actionListAsset = null;
			startIndex = 0;
		}
		
		
		public SkipList (SkipList _skipList)
		{
			actionList = _skipList.actionList;
			actionListAsset = _skipList.actionListAsset;
			startIndex = _skipList.startIndex;
		}
		
		
		public SkipList (ActionList _actionList, int _startIndex)
		{
			actionList = _actionList;
			startIndex = _startIndex;
			
			if (_actionList is RuntimeActionList)
			{
				RuntimeActionList runtimeActionList = (RuntimeActionList) _actionList;
				actionListAsset = runtimeActionList.assetSource;
			}
			else
			{
				actionListAsset = null;
			}
		}
		
		
		public void Resume ()
		{
			if (actionListAsset != null)
			{
				// Destroy old list, but don't go through ActionListManager's Reset code, to bypass changing GameState etc
				KickStarter.actionListManager.DestroyAssetList (actionListAsset);
				actionList = AdvGame.RunActionListAsset (actionListAsset, startIndex, true);
			}
			else if (actionList != null)
			{
				actionList.Interact (startIndex, true);
			}
		}
		
		
		public void Skip ()
		{
			if (actionListAsset != null)
			{
				// Destroy old list, but don't go through ActionListManager's Reset code, to bypass changing GameState etc
				KickStarter.actionListManager.DestroyAssetList (actionListAsset);
				actionList = AdvGame.SkipActionListAsset (actionListAsset, startIndex);
			}
			else if (actionList != null)
			{
				actionList.Skip (startIndex);
			}
		}
		
		
		public string GetName ()
		{
			if (actionListAsset != null)
			{
				return actionListAsset.name;
			}
			if (actionList != null)
			{
				return actionList.gameObject.name;
			}
			return "";
		}
		
	}

}