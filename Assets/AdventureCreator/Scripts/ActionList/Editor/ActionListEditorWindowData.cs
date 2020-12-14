using UnityEngine;
using System.Collections;
using AC;

[System.Serializable]
public class ActionListEditorWindowData
{

	public bool isLocked;
	public int targetID;
	private ActionList _target;
	public ActionListAsset targetAsset;


	public ActionListEditorWindowData ()
	{
		isLocked = false;
		targetID = 0;
		_target = null;
		targetAsset = null;
	}


	public ActionListEditorWindowData (ActionList actionList)
	{
		targetAsset = null;
		_target = actionList;

		if (actionList != null)
		{
			isLocked = true;
			targetID = actionList.GetInstanceID ();
		}
		else
		{
			isLocked = false;
			targetID = 0;
		}
	}


	public ActionListEditorWindowData (ActionListAsset actionListAsset)
	{
		targetID = 0;
		_target = null;
		targetAsset = actionListAsset;

		if (actionListAsset != null)
		{
			isLocked = true;
		}
		else
		{
			isLocked = false;
		}
	}


	public ActionList target
	{
		get
		{
			if (_target != null)
			{
				return _target;
			}
			if (targetID != 0)
			{
				ActionList[] actionLists = GameObject.FindObjectsOfType (typeof (ActionList)) as ActionList[];
				foreach (ActionList actionList in actionLists)
				{
					if (actionList.GetInstanceID () == targetID)
					{
						_target = actionList;
						return _target;
					}
				}
			}

			return null;
		}
		set
		{
			if (value != null)
			{
				_target = value;
				targetID = value.GetInstanceID ();
			}
		}
	}

}
